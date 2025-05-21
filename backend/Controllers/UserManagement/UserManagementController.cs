using backend.DTOs.Auth;
using backend.Services.Auth;
using backend.Services.Email;
using backend.Services.JWT;
using Microsoft.AspNetCore.Identity;
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
        private readonly IPasswordHasher _passwordHasher;

        public UserManagementController(IAuthService authService, jwtService jwtService, IEmailService emailService, IPasswordHasher passwordHasher)
        {
            _authService = authService;
            _jwtService = jwtService;
            IEmailService = emailService;
            _passwordHasher = passwordHasher;
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
                // Validar la solicitud
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Generate password hash (salt is embedded in the hash)
                var passwordHash = _passwordHasher.Hash(request.Password);

                // Create a token containing all user data
                var registrationToken = _jwtService.CreateRegistrationToken(
                    request.Username,
                    request.Email,
                    //request.GivenNames,
                    //request.PSurname,
                    //request.MSurname,
                    //request.PhoneNumber,
                    passwordHash
                );

                // Send verification email
                var frontendUrl = Request.Headers["Origin"].ToString();
                await IEmailService.SendVerificationEmail(request.Email, registrationToken, frontendUrl);

                return Ok(new
                {
                    message = "Se ha enviado un correo de verificación. Por favor, verifica tu correo para completar el registro."
                });
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
                if (ex.Message.Contains("expirado"))
                {
                    // 401 para tokens expirados
                    return Unauthorized(new { message = ex.Message });
                }
                else if (ex.Message.Contains("inválida"))
                {
                    // 403 para tokens con firma inválida (posible manipulación)
                    return StatusCode(403, new { message = ex.Message });
                }
                else
                {
                    // 401 para otros errores
                    return Unauthorized(new { message = ex.Message });
                }
            }
        }
    }
}