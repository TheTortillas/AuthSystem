using System.Net;
using System.Net.Mail;

namespace backend.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration config;

        public EmailService(IConfiguration config)
        {
            this.config = config;
        }

        public async Task SendEmail(string emailReceptor, string subject, string body)
        {
            var emailSender = config.GetValue<string>("EmailSettings:SenderEmail");
            var password = config.GetValue<string>("EmailSettings:SmtpPassword");
            var server = config.GetValue<string>("EmailSettings:SmtpServer");
            var port = config.GetValue<int>("EmailSettings:SmtpPort");
            var displayName = config.GetValue<string>("EmailSettings:DisplayName") ?? "AuthSystem"; // Nombre personalizado

            // Verificar que los valores se hayan cargado correctamente
            if (string.IsNullOrEmpty(emailSender))
            {
                throw new InvalidOperationException("El email del remitente no está configurado correctamente.");
            }

            var smptClient = new SmtpClient(server, port);
            smptClient.EnableSsl = true;
            smptClient.UseDefaultCredentials = false;

            smptClient.Credentials = new NetworkCredential(emailSender, password);

            // Usar MailAddress con nombre a mostrar personalizado
            var fromAddress = new MailAddress(emailSender, displayName);
            var toAddress = new MailAddress(emailReceptor);

            var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true // Habilita HTML en el correo si lo necesitas
            };

            await smptClient.SendMailAsync(message);
        }
    }
}