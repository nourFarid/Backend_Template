namespace Shared.SpecificationsQueryParams;

public class UserQueryParams : BaseQueryParams
{
    public string? FullName { get; set; }
    public string? UserCode { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
}
