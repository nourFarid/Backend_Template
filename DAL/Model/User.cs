using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Model;

public class User : IdentityUser
{
    [Column(TypeName = "nvarchar(255)"), MaxLength(255)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "nvarchar(255)"), MaxLength(255)]
    public string UserCode { get; set; } = string.Empty;

    [Required, DefaultValue(true)]
    public bool IsActive { get; set; } = true;

    [Required, DefaultValue(true)]
    public bool NewUser { get; set; } = true;

    public string? RefreshToken { get; set; } = string.Empty;
    public DateTime? RefreshTokenExpiryTime { get; set; }

    [NotMapped]
    public List<Role>? Roles { get; set; }
}
