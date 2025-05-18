using backend.DTOs.Auth;
using backend.Services.Auth;
using backend.Services.JWT;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace backend.Controllers.UserManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly jwtService _jwtService;

        public UserManagementController(IAuthService authService, jwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;
        }

        [HttpPost("SignUp", Name = "PostSignUp")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            try
            {
                var success = await _authService.RegisterUserAsync(request);

                if (!success)
                {
                    return BadRequest(new { message = "Error al registrar el usuario" });
                }

                return Ok(new { message = "Usuario registrado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno del servidor: {ex.Message}" });
            }
        }

        [HttpPost("SignIn", Name = "PostSignIn")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            try
            {
                var (user, errorMessage, statusCode) = await _authService.AuthenticateUserAsync(request);

                if (user == null)
                {
                    return StatusCode(statusCode, new { message = errorMessage });
                }

                // Update login success (reset failed attempts and update last login)
                await _authService.UpdateLoginSuccessAsync(user.Id);

                // Generate JWT token
                string token = _jwtService.CreateToken(user);

                return Ok(new AuthResponseDTO { Token = token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno del servidor: {ex.Message}" });
            }
        }

        [HttpPost("RefreshToken", Name = "PostRefreshToken")]
        public IActionResult RefreshToken()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { message = "Token no proporcionado." });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            try
            {
                var newToken = _jwtService.RefreshToken(token);
                return Ok(new { token = newToken });
            }
            catch (SecurityTokenException ex)
            {
                Console.WriteLine($"Error al validar el token: {ex.Message}");
                return Unauthorized(new { message = "Token inválido o expirado." });
            }
        }
    }
}