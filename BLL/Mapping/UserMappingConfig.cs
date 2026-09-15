using DAL.Model;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Shared.DTO.RoleDtos;
using Shared.DTO.UserDtos;

namespace BLL.Mapping;

public class UserMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserDto>()
            .Map(dest => dest.UserId, src => src.Id)
            .Map(dest => dest.Roles, src => src.Roles != null 
                ? src.Roles.Select(r => new RoleDto { RoleId = r.RoleId, RoleName = r.RoleName }).ToList() 
                : null);

        config.NewConfig<NewUserDto, User>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Roles!);

        config.NewConfig<EditUserDto, User>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Roles!);

        config.NewConfig<IdentityRole, RoleDto>()
            .Map(dest => dest.RoleId, src => src.Id)
            .Map(dest => dest.RoleName, src => src.Name ?? string.Empty);

        config.NewConfig<Role, RoleDto>()
            .Map(dest => dest.RoleId, src => src.RoleId)
            .Map(dest => dest.RoleName, src => src.RoleName);
    }
}
