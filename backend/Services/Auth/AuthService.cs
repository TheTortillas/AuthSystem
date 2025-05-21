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
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<bool> RegisterUserAsync(RegisterRequestDTO request)
        {
            // Hash the password with our new PasswordHasher
            string passwordHash = _passwordHasher.Hash(request.Password);

            var user = new UserDTO
            {
                Username = request.Username,
                GivenNames = request.GivenNames,
                PSurname = request.PSurname,
                MSurname = request.MSurname,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = passwordHash,
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

            // Verify password using the new passwordHasher
            if (!_passwordHasher.Verify(request.Password, user.Password))
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

            // Verify password using the new passwordHasher
            if (!_passwordHasher.Verify(request.Password, user.Password))
            {
                // Increment failed attempts counter
                await _userRepository.IncrementFailedAttemptsAsync(user.Id);

                int remainingAttempts = SecurityConstants.MaxFailedAttempts - (user.FailedAttempts + 1);
                return (null, $"Contraseña incorrecta. Intentos restantes: {remainingAttempts}", 401);
            }

            return (user, null, 200);
        }

        public async Task<bool> ResetPasswordAsync(int userId, string newPassword)
        {
            // Hash the new password with our PasswordHasher
            string passwordHash = _passwordHasher.Hash(newPassword);

            // The salt is embedded in the password hash, so we pass empty string
            return await _userRepository.ResetPasswordAsync(userId, passwordHash);
        }

        // Other methods remain unchanged...
        public async Task<bool> IncrementFailedAttemptsAsync(int userId)
        {
            return await _userRepository.IncrementFailedAttemptsAsync(userId);
        }

        public async Task<bool> UpdateLoginSuccessAsync(int userId)
        {
            return await _userRepository.UpdateLoginSuccessAsync(userId);
        }

        public async Task<bool> VerifyEmailAsync(int userId)
        {
            return await _userRepository.VerifyEmailAsync(userId);
        }

        public async Task<UserDTO?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetUserByEmailAsync(email);
        }

        public async Task<bool> RegisterVerifiedUserAsync(string username, string email, string givenNames, string pSurname, string mSurname, string phoneNumber, string passwordHash)
        {
            // Call the repository method to register the verified user
            return await _userRepository.RegisterVerifiedUserAsync(username, email, givenNames, pSurname, mSurname, phoneNumber, passwordHash);
        }
    }
}