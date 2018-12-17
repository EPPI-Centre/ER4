using ERIdentityProvider.UserServices;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class CustomIdentityServerBuilderExtensions
    {//also appears isn UserServices\IdentityExtensions.cs
        public static IIdentityServerBuilder AddCustomTokenStore(this IIdentityServerBuilder builder)
        {
            builder.Services.AddSingleton<IUserRepository, UserRepository>();
            builder.AddProfileService<EppiUserProfileService>();
            builder.AddResourceOwnerValidator<CustomResourceOwnerPasswordValidator>();
            return builder;
        }
    }
}