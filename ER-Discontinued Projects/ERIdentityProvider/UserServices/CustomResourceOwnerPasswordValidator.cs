using IdentityServer4.Validation;
using IdentityModel;
using System.Threading.Tasks;
using System.Security.Claims;

namespace ERIdentityProvider.UserServices
{
    public class CustomResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly IUserRepository _userRepository;

        public CustomResourceOwnerPasswordValidator(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            //if (_userRepository.ValidateCredentials(context.UserName, context.Password))
            //{
            //    var user = _userRepository.FindByUsername(context.UserName);
            //    context.Result = new GrantValidationResult(user.SubjectId, OidcConstants.AuthenticationMethods.Password);
            //}
            CustomUser user = await _userRepository.ValidateCredentialsAsync(context.UserName, context.Password);
            if (user != null)
            {
				

				context.Result = new GrantValidationResult(user.SubjectId, OidcConstants.AuthenticationMethods.Password);
				//ClaimsIdentity ClI = new ClaimsIdentity(OidcConstants.AuthenticationMethods.Password);
				//ClI.AddClaim(new Claim("ExpiresOn", user.ExpiresOn));
				//context.Result.Subject.AddIdentity(ClI);
			}
			return;// Task.FromResult(0);
        }
    }
}
