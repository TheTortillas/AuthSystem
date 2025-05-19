using backend.DTOs.Auth;
using backend.Services.Auth;
using backend.Services.Email;
using backend.Services.JWT;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace backend.Controllers.Email
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailVerificationController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;
        private readonly jwtService _jwtService;

        public EmailVerificationController(IAuthService authService, IEmailService emailService, jwtService jwtService)
        {
            _authService = authService;
            _emailService = emailService;
            _jwtService = jwtService;
        }

        [HttpPost("send-verification")]
        public async Task<IActionResult> SendVerificationEmail([FromBody] EmailRequestDTO request)
        {
            try
            {
                var user = await _authService.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                // Generate verification token
                var token = _jwtService.CreateEmailVerificationToken(user.Id, user.Email);

                // Send email
                var frontendUrl = Request.Headers["Origin"].ToString();
                await _emailService.SendVerificationEmail(user.Email, token, frontendUrl);

                return Ok(new { message = "Correo de verificación enviado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al enviar correo: {ex.Message}" });
            }
        }

        [HttpGet("verify")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            try
            {
                // Validate token
                var tokenHandler = new JwtSecurityTokenHandler();
                if (!tokenHandler.CanReadToken(token))
                {
                    return BadRequest(new { message = "Token inválido" });
                }

                var jwtToken = tokenHandler.ReadJwtToken(token);

                // Extract claims
                var userId = int.Parse(jwtToken.Claims.First(c => c.Type == "userId").Value);
                var tokenType = jwtToken.Claims.First(c => c.Type == "type").Value;

                if (tokenType != "VerifyEmail")
                {
                    return BadRequest(new { message = "Tipo de token incorrecto" });
                }

                // Verify email in database
                var success = await _authService.VerifyEmailAsync(userId);
                if (!success)
                {
                    return BadRequest(new { message = "No se pudo verificar el email" });
                }

                return Ok(new { message = "Email verificado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al verificar email: {ex.Message}" });
            }
        }
    }
}