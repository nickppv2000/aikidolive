using AikidoLive.DataModels;

namespace AikidoLive.Services
{
    public interface IAuthService
    {
        Task<bool> RegisterUser(User user);
        Task<User> AuthenticateUser(string email, string password);
    }
}