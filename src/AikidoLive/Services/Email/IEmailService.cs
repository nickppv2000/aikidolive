using System.Threading.Tasks;

namespace AikidoLive.Services.Email
{
    public interface IEmailService
    {
        Task<bool> SendNewUserNotificationToAdminsAsync(string newUserFirstName, string newUserLastName, string newUserEmail);
    }
}