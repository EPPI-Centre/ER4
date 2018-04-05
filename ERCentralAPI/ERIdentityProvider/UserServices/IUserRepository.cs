using System.Threading.Tasks;

namespace ERIdentityProvider.UserServices
{
    public interface IUserRepository
    {
        Task<CustomUser> ValidateCredentialsAsync(string username, string password);

		Task<CustomUser> FindBySubjectIdAsync(string subjectId);

//        CustomUser FindByUsername(string username);
    }
}
