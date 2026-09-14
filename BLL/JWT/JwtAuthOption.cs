using Jose;

namespace BLL.JWT;

public class JwtAuthOption : JwtOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
    public double DurationInMinutes { get; set; }
}
