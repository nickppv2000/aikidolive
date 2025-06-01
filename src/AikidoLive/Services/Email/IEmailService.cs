using System.Threading.Tasks;

namespace AikidoLive.Services.Email
{
    public interface IEmailService
    {
        Task<bool> SendConfirmationEmailAsync(string toEmail, string firstName, string confirmationLink);
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string firstName, string resetLink);
    }
}