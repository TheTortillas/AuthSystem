using backend.DTOs.Auth;
using backend.Services.Auth;
using backend.Services.Email;
using backend.Services.JWT;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace backend.Controllers.UserManagement
{
    /// <summary>
    /// Controller for user authentication and management
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly jwtService _jwtService;
        private readonly IEmailService IEmailService;

        public UserManagementController(IAuthService authService, jwtService jwtService, IEmailService emailService)
        {
            IEmailService = emailService;
            _authService = authService;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="request">Registration information</param>
        /// <returns>Success or error message</returns>
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

                // Envía email de verificación
                var user = await _authService.GetUserByEmailAsync(request.Email);
                if (user != null)
                {
                    var token = _jwtService.CreateEmailVerificationToken(user.Id, user.Email);
                    var frontendUrl = Request.Headers["Origin"].ToString();
                    await IEmailService.SendVerificationEmail(user.Email, token, frontendUrl);
                }

                return Ok(new { message = "Usuario registrado exitosamente, revisa tu correo para verificar tu cuenta" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno del servidor: {ex.Message}" });
            }
        }

        /// <summary>
        /// Authenticates a user and issues a JWT token
        /// </summary>
        /// <param name="request">Login information</param>
        /// <returns>JWT token or error message</returns>
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

        /// <summary>
        /// Authenticates a user with username and issues a JWT token
        /// </summary>
        /// <param name="request">Login information with username</param>
        /// <returns>JWT token or error message</returns>
        [HttpPost("SignInUsername", Name = "PostSignInUsername")]
        public async Task<IActionResult> LoginWithUsername([FromBody] UsernameLoginRequestDTO request)
        {
            try
            {
                var (user, errorMessage, statusCode) = await _authService.AuthenticateByUsernameAsync(request);

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

        /// <summary>
        /// Refreshes the JWT token
        /// </summary>
        /// <returns>New JWT token or error message</returns>
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