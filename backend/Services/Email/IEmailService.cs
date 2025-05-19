
namespace backend.Services.Email
{
    public interface IEmailService
    {
        Task SendEmail(string emailReceptor, string subject, string body);
        Task SendVerificationEmail(string email, string token, string frontendUrl);
        Task SendPasswordResetEmail(string email, string token, string frontendUrl);
    }
}