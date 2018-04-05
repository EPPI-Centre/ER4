using ERIdentityProvider.UserServices;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class CustomIdentityServerBuilderExtensions
    {//also appears isn TokenServices\TokenExtensions.cs
        public static IIdentityServerBuilder AddCustomUserStore(this IIdentityServerBuilder builder)
        {
            builder.Services.AddSingleton<IUserRepository, UserRepository>();
            builder.AddProfileService<EppiUserProfileService>();
            builder.AddResourceOwnerValidator<CustomResourceOwnerPasswordValidator>();

            return builder;
        }
		public static IIdentityServerBuilder AddInMemoryDevUserStore(this IIdentityServerBuilder builder)
		{
			builder.Services.AddSingleton<IUserRepository, InMemoryDevUserRepository>();
			builder.AddProfileService<EppiUserProfileService>();
			builder.AddResourceOwnerValidator<CustomResourceOwnerPasswordValidator>();

			return builder;
		}
	}
}