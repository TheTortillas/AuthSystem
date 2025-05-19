
namespace backend.Services.Email
{
    public interface IEmailService
    {
        Task SendEmail(string emailReceptor, string subject, string body);
    }
}