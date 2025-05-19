using backend.Services.Email;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Email
{
    [Route("api/[controller]")]
    [ApiController]

    public class EmailController : ControllerBase
    {
        private readonly IEmailService emailService;

        public EmailController(IEmailService emailService)
        {
            this.emailService = emailService;
        }

        [HttpPost]
        public async Task<ActionResult> Send(string emailReceptor, string subject, string body)
        {
            await emailService.SendEmail(emailReceptor, subject, body);
            return Ok();
        }
    }
}
