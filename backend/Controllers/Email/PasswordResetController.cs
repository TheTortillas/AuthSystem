using backend.DTOs.Auth;
using backend.Services.Auth;
using backend.Services.Email;
using backend.Services.JWT;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace backend.Controllers.UserManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordResetController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;
        private readonly jwtService _jwtService;

        public PasswordResetController(IAuthService authService, IEmailService emailService, jwtService jwtService)
        {
            _authService = authService;
            _emailService = emailService;
            _jwtService = jwtService;
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] EmailRequestDTO request)
        {
            try
            {
                var user = await _authService.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    // For security, don't reveal if email exists or not
                    return Ok(new { message = "Si el correo existe, recibirás un enlace para restablecer tu contraseña" });
                }

                // Generate password reset token (create this method in JWT service)
                var token = _jwtService.CreatePasswordResetToken(user.Id, user.Email);

                // Send email
                var frontendUrl = Request.Headers["Origin"].ToString();
                await _emailService.SendPasswordResetEmail(user.Email, token, frontendUrl);

                return Ok(new { message = "Si el correo existe, recibirás un enlace para restablecer tu contraseña" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO request)
        {
            try
            {
                // Validate token
                var tokenHandler = new JwtSecurityTokenHandler();
                if (!tokenHandler.CanReadToken(request.Token))
                {
                    return BadRequest(new { message = "Token inválido" });
                }

                var jwtToken = tokenHandler.ReadJwtToken(request.Token);

                // Extract claims
                var userId = int.Parse(jwtToken.Claims.First(c => c.Type == "userId").Value);
                var tokenType = jwtToken.Claims.First(c => c.Type == "type").Value;

                if (tokenType != "ResetPassword")
                {
                    return BadRequest(new { message = "Tipo de token incorrecto" });
                }

                // Reset password in database
                var success = await _authService.ResetPasswordAsync(userId, request.NewPassword);
                if (!success)
                {
                    return BadRequest(new { message = "No se pudo restablecer la contraseña" });
                }

                return Ok(new { message = "Contraseña restablecida exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
            }
        }
    }
}