using API.Middleware;
using BLL;
using BLL.JWT;
using DAL;
using DAL.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Shared.DTO;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// 1. Controllers & JSON Serialization
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// 2. Custom Model State Validation Response Envelope
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var localizer = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizer<Messages>>();
        var errorItems = new List<ApiErrorItem>();

        foreach (var (field, entry) in context.ModelState)
        {
            foreach (var error in entry.Errors)
            {
                var localizedDesc = localizer[error.ErrorMessage];
                var description = (!localizedDesc.ResourceNotFound && !string.IsNullOrEmpty(localizedDesc.Value))
                    ? localizedDesc.Value
                    : error.ErrorMessage;

                errorItems.Add(new ApiErrorItem(field, description, "Validation"));
            }
        }

        string topMessage = string.Join(" | ", errorItems.Select(e => e.Description));
        var response = ApiResponse<object>.Fail(topMessage, errorItems, StatusCodes.Status400BadRequest);
        return new BadRequestObjectResult(response);
    };
});

// 3. Layer Dependencies (Clean Architecture Extension Methods)
builder.Services.AddDalDependencies(builder.Configuration);
builder.Services.AddBllDependencies(builder.Configuration);

// 4. Localization Configuration
builder.Services.AddLocalization(opts => opts.ResourcesPath = string.Empty);

// 5. Global Exception Handling (IExceptionHandler)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var supportedCultures = new[] { "en", "ar", "ar-EG" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

builder.Services.AddSingleton(localizationOptions);

// 6. JWT Authentication & Bearer Pipeline
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSection = builder.Configuration.GetSection("Jwt");
    var signingKey = jwtSection.GetValue<string>("SigningKey");

    if (string.IsNullOrWhiteSpace(signingKey))
        throw new ArgumentNullException("Jwt:SigningKey", "SigningKey cannot be null or empty in appsettings.json");

    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSection.GetValue<string>("Issuer"),
        ValidateAudience = true,
        ValidAudience = jwtSection.GetValue<string>("Audience"),
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Strictly extract token from HttpOnly cookie only (no Authorization header fallback)
            context.Token = context.Request.Cookies.TryGetValue("jwt", out var token) ? token : string.Empty;
            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
            var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            var tokenStamp = context.Principal?.FindFirstValue("AspNet.Identity.SecurityStamp");

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tokenStamp))
            {
                context.Fail("Invalid token claims");
                return;
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive || user.SecurityStamp != tokenStamp)
            {
                context.Fail("Token has been revoked or user is deactivated");
            }
        }
    };
});

builder.Services.AddAuthorization();

// 7. OpenAPI & Scalar API Documentation
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "Enterprise Backend API Template";
        document.Info.Version = "v1";

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter your JWT token"
        };

        var schemeReference = new OpenApiSecuritySchemeReference("Bearer", document);
        document.Security =
        [
            new OpenApiSecurityRequirement
            {
                [schemeReference] = []
            }
        ];

        return Task.CompletedTask;
    });
});

// 8. CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
        if (allowedOrigins != null && allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins);
        }
        else
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:4000", "http://localhost:5173");
        }

        policy.AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// 9. Rate Limiting Configuration (DoS Protection)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        var localizer = context.HttpContext.RequestServices.GetService<IStringLocalizer<Messages>>();
        string messageKey = "General.RateLimitExceeded";
        string defaultMsg = "Too many requests. Please try again later.";
        string message = defaultMsg;

        if (localizer != null)
        {
            var localized = localizer[messageKey];
            if (!localized.ResourceNotFound && !string.IsNullOrEmpty(localized.Value))
            {
                message = localized.Value;
            }
        }

        context.HttpContext.Response.ContentType = "application/json";
        var errorItem = new ApiErrorItem(messageKey, message, "RateLimit");
        var response = ApiResponse<object>.Fail(message, [errorItem], StatusCodes.Status429TooManyRequests);
        await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken: token);
    };

    // A. Global Limiter: 100 requests per minute per IP address
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: clientIp,
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 4,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    // B. Strict Auth Limiter: 10 requests per minute for sensitive endpoints
    options.AddFixedWindowLimiter("AuthLimiter", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

var app = builder.Build();

// 9. HTTP Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Request Localization must execute before exception handling and controllers
app.UseRequestLocalization(app.Services.GetRequiredService<RequestLocalizationOptions>());

app.UseExceptionHandler();

app.UseRouting();
app.UseCors("AllowFrontend");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
