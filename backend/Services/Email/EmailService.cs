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

        public async Task SendVerificationEmail(string email, string token, string frontendUrl)
        {
            string verificationLink = $"{frontendUrl}/verify-email?token={token}";
            string subject = "Verifica tu cuenta en AuthSystem";
            string body = $@"
            <html>
            <body>
                <h2>Verificación de cuenta</h2>
                <p>Gracias por registrarte. Por favor haz clic en el siguiente enlace para verificar tu cuenta:</p>
                <p><a href='{verificationLink}'>Verificar mi cuenta</a></p>
                <p>Este enlace expirará en 24 horas.</p>
                <p>Si no solicitaste esta verificación, puedes ignorar este correo.</p>
            </body>
            </html>";

            await SendEmail(email, subject, body);
        }

        public async Task SendPasswordResetEmail(string email, string token, string frontendUrl)
        {
            string resetLink = $"{frontendUrl}/reset-password?token={token}";
            string subject = "Restablecimiento de contraseña en AuthSystem";
            string body = $@"
            <html>
            <body>
                <h2>Restablecimiento de contraseña</h2>
                <p>Has solicitado restablecer tu contraseña. Por favor haz clic en el siguiente enlace para hacerlo:</p>
                <p><a href='{resetLink}'>Restablecer mi contraseña</a></p>
                <p>Este enlace expirará en 1 hora.</p>
                <p>Si no solicitaste este restablecimiento, puedes ignorar este correo.</p>
            </body>
            </html>";

            await SendEmail(email, subject, body);
        }
    }
}