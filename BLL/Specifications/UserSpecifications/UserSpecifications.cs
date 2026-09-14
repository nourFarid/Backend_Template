using DAL.Model;
using Shared.SpecificationsQueryParams;

namespace BLL.Specifications.UserSpecifications;

public class UserSpecifications : BaseSpecifications<User>
{
    public UserSpecifications(UserQueryParams queryParams, List<string>? userIdsInRole)
        : base(u =>
            (string.IsNullOrWhiteSpace(queryParams.FullName) || u.FullName.ToLower().Contains(queryParams.FullName.ToLower()) || u.UserCode.ToLower().Contains(queryParams.FullName.ToLower()))
            && (string.IsNullOrWhiteSpace(queryParams.UserCode) || u.UserCode.ToLower().Contains(queryParams.UserCode.ToLower()))
            && (!queryParams.IsActive.HasValue || u.IsActive == queryParams.IsActive.Value)
            && (userIdsInRole == null || userIdsInRole.Contains(u.Id)))
    {
        if (!string.IsNullOrEmpty(queryParams.Sort))
        {
            switch (queryParams.Sort.ToLower())
            {
                case "fullname":
                    if (!queryParams.IsAscending)
                        AddOrderByDescending(u => u.FullName);
                    else
                        AddOrderBy(u => u.FullName);
                    break;
                case "usercode":
                    if (!queryParams.IsAscending)
                        AddOrderByDescending(u => u.UserCode);
                    else
                        AddOrderBy(u => u.UserCode);
                    break;
                case "email":
                    if (!queryParams.IsAscending)
                        AddOrderByDescending(u => u.Email!);
                    else
                        AddOrderBy(u => u.Email!);
                    break;
                case "username":
                    if (!queryParams.IsAscending)
                        AddOrderByDescending(u => u.UserName!);
                    else
                        AddOrderBy(u => u.UserName!);
                    break;
                default:
                    AddOrderBy(u => u.FullName);
                    break;
            }
        }
        else
        {
            AddOrderBy(u => u.FullName);
        }

        ApplyPagination(queryParams.PageSize, queryParams.PageIndex);
    }
}
