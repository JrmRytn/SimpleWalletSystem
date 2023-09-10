namespace SimpleWalletSystem.WebApi.Mapping;

public class UserMapping : Profile
{
    public UserMapping()
    {
        CreateMap<RegisterUserDto, User>()
            .ConstructUsing((src) => new User(Guid.NewGuid(),src.LoginName,src.Password));
    }
}
