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
            .Map(dest => dest.UserId, src => src.Id);

        config.NewConfig<NewUserDto, User>()
            .Ignore(dest => dest.Id);

        config.NewConfig<EditUserDto, User>()
            .Ignore(dest => dest.Id);

        config.NewConfig<IdentityRole, RoleDto>()
            .Map(dest => dest.RoleId, src => src.Id)
            .Map(dest => dest.RoleName, src => src.Name ?? string.Empty);
    }
}
