using AikidoLive.DataModels;
using System.Threading.Tasks;

namespace AikidoLive.Services.Authentication
{
    public interface IAuthService
    {
        Task<User> AuthenticateAsync(string email, string password);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        Task<User> RegisterUserAsync(RegisterModel model);
        Task<User> GetUserByEmailAsync(string email);
        Task<bool> UpdateUserAsync(string originalEmail, User updatedUser);
    }
}