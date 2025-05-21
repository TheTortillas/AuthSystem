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
                // Validar token  
                var (jwtToken, errorMessage) = _jwtService.ValidateToken(token);

                // Verificar si es valido  
                if (jwtToken == null)
                {
                    return BadRequest(new { message = errorMessage ?? "Error al válidar el token" });
                }

                // Verificar tipo de token  
                var tokenType = jwtToken.Claims.FirstOrDefault(c => c.Type == "type")?.Value;
                if (tokenType != "Registration")
                {
                    return BadRequest(new { message = "Tipo de token incorrecto" });
                }

                var expiry = jwtToken.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
                if (expiry != null)
                {
                    var expiryTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiry));
                    if (expiryTime < DateTimeOffset.UtcNow)
                    {
                        return BadRequest(new { message = "El enlace de verificación ha expirado. Por favor, solicita uno nuevo." });
                    }
                }

                // Extraer datos de usuario del token  
                var username = jwtToken.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
                var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
                var givenNames = jwtToken.Claims.FirstOrDefault(c => c.Type == "givenNames")?.Value;
                var pSurname = jwtToken.Claims.FirstOrDefault(c => c.Type == "pSurname")?.Value;
                var mSurname = jwtToken.Claims.FirstOrDefault(c => c.Type == "mSurname")?.Value;
                var phoneNumber = jwtToken.Claims.FirstOrDefault(c => c.Type == "phoneNumber")?.Value;
                var passwordHash = jwtToken.Claims.FirstOrDefault(c => c.Type == "passwordHash")?.Value;

                // Validar que todos los datos necesarios estén presentes  
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) ||
                    string.IsNullOrEmpty(givenNames) || string.IsNullOrEmpty(pSurname) ||
                    string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(passwordHash) 
                )
                {
                    return BadRequest(new { message = "Datos de registro incompletos en el token" });
                }

                // Registrar al usuario con los datos del token  
                var success = await _authService.RegisterVerifiedUserAsync(
                    username, email, givenNames, pSurname, mSurname,
                    phoneNumber, passwordHash);

                if (!success)
                {
                    return BadRequest(new { message = "No se pudo completar el registro" });
                }

                return Ok(new { message = "Registro completado exitosamente. Ya puedes iniciar sesión." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al verificar email: {ex.Message}" });
            }
        }
    }
}