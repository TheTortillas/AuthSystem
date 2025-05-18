using backend.Constants;
using backend.DTOs.Auth;
using backend.DTOs.Users;
using backend.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace backend.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<RegisterRequestDTO> _passwordHasher;

        public AuthService(IUserRepository userRepository, IPasswordHasher<RegisterRequestDTO> passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        // Method to generate a salt
        private string GenerateSalt(int size = SecurityConstants.DefaultSaltSize)
        {
            var saltBytes = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        public async Task<bool> RegisterUserAsync(RegisterRequestDTO request)
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

            return await _userRepository.CreateUserAsync(user);
        }

        public async Task<(UserDTO? User, string? ErrorMessage, int StatusCode)> AuthenticateUserAsync(LoginRequestDTO request)
        {
            // Find user by email
            var user = await _userRepository.GetUserByEmailAsync(request.Email);

            if (user == null)
            {
                return (null, "Usuario no encontrado", 401);
            }

            // Check if account is locked
            if (user.FailedAttempts >= SecurityConstants.MaxFailedAttempts)
            {
                return (null, "Cuenta bloqueada por múltiples intentos fallidos. Contacte a soporte.", 423);
            }

            // Verify password
            var saltedPassword = request.Password + user.Salt;
            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(null, user.Password, saltedPassword);

            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                // Increment failed attempts counter
                await _userRepository.IncrementFailedAttemptsAsync(user.Id);

                int remainingAttempts = SecurityConstants.MaxFailedAttempts - (user.FailedAttempts + 1);
                return (null, $"Contraseña incorrecta. Intentos restantes: {remainingAttempts}", 401);
            }

            return (user, null, 200);
        }

        public async Task<(UserDTO? User, string? ErrorMessage, int StatusCode)> AuthenticateByUsernameAsync(UsernameLoginRequestDTO request)
        {
            // Find user by username
            var user = await _userRepository.GetUserByUsernameAsync(request.Username);

            if (user == null)
            {
                return (null, "Usuario no encontrado", 401);
            }

            // Check if account is locked
            if (user.FailedAttempts >= SecurityConstants.MaxFailedAttempts)
            {
                return (null, "Cuenta bloqueada por múltiples intentos fallidos. Contacte a soporte.", 423);
            }

            // Verify password
            var saltedPassword = request.Password + user.Salt;
            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(null, user.Password, saltedPassword);

            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                // Increment failed attempts counter
                await _userRepository.IncrementFailedAttemptsAsync(user.Id);

                int remainingAttempts = SecurityConstants.MaxFailedAttempts - (user.FailedAttempts + 1);
                return (null, $"Contraseña incorrecta. Intentos restantes: {remainingAttempts}", 401);
            }

            return (user, null, 200);
        }

        public async Task<bool> IncrementFailedAttemptsAsync(int userId)
        {
            return await _userRepository.IncrementFailedAttemptsAsync(userId);
        }

        public async Task<bool> UpdateLoginSuccessAsync(int userId)
        {
            return await _userRepository.UpdateLoginSuccessAsync(userId);
        }
    }
}
