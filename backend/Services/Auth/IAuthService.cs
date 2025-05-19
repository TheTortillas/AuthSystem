using backend.DTOs.Auth;
using backend.DTOs.Users;
using Microsoft.AspNetCore.Mvc;

namespace backend.Services.Auth
{
    public interface IAuthService
    {
        Task<bool> RegisterUserAsync(RegisterRequestDTO request);
        Task<(UserDTO? User, string? ErrorMessage, int StatusCode)> AuthenticateUserAsync(LoginRequestDTO request);
        Task<(UserDTO? User, string? ErrorMessage, int StatusCode)> AuthenticateByUsernameAsync(UsernameLoginRequestDTO request);
        Task<bool> IncrementFailedAttemptsAsync(int userId);
        Task<bool> UpdateLoginSuccessAsync(int userId);
        Task<bool> VerifyEmailAsync(int userId);
        Task<UserDTO?> GetUserByEmailAsync(string email);
        Task<bool> ResetPasswordAsync(int userId, string newPassword);
    }
}
