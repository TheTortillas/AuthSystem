using backend.DTOs.Auth;
using backend.DTOs.Users;
using backend.Services.JWT;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace backend.Controllers.UserManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly DBManager _dbManager;
        private readonly IPasswordHasher<RegisterRequestDTO> _passwordHasher;

        public UserManagementController(IConfiguration config)
        {
            _config = config;
            _dbManager = new DBManager(_config);
            _passwordHasher = new PasswordHasher<RegisterRequestDTO>();
        }

        // Método para generar un salt
        private string GenerateSalt(int size = 16)
        {
            var saltBytes = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        [HttpPost("SignUp", Name = "PostSignUp")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            try
            {
                var salt = GenerateSalt();
                var saltedPassword = request.Password + salt;

                var user = new UserDTO
                {
                    Username = request.Username,
                    GivenNames = request.GivenNames,
                    PSurname = request.PSurname,
                    MSurname = request.MSurname,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    Password = _passwordHasher.HashPassword(request, saltedPassword),
                    Salt = salt,
                };

                var success = await _dbManager.SignUpAsync(user);

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
                // Buscar el usuario por email
                var user = await _dbManager.FindByEmailAsync(request.Email);

                if (user == null)
                {
                    return Unauthorized(new { message = "Usuario no encontrado" });
                }

                // Verificar si la cuenta está bloqueada
                if (user.FailedAttempts >= 5)
                {
                    return StatusCode(423, new { message = "Cuenta bloqueada por múltiples intentos fallidos. Contacte a soporte." });
                }

                // Verificar la contraseña
                var saltedPassword = request.Password + user.Salt;
                var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(null, user.Password, saltedPassword);

                if (passwordVerificationResult == PasswordVerificationResult.Failed)
                {
                    // Incrementar el contador de intentos fallidos
                    await _dbManager.IncrementFailedAttemptsAsync(user.Id);

                    int remainingAttempts = 5 - (user.FailedAttempts + 1);
                    return Unauthorized(new { message = $"Contraseña incorrecta. Intentos restantes: {remainingAttempts}" });
                }

                // Restablecer intentos fallidos y actualizar último inicio de sesión
                await _dbManager.UpdateLoginSuccessAsync(user.Id);

                // Generar el token JWT
                var jwtService = new jwtService(_config);
                string token = jwtService.CreateToken(user);

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
                var jwtService = new jwtService(_config);
                var newToken = jwtService.RefreshToken(token);
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