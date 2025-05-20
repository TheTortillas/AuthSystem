using backend.DTOs.Users;

namespace backend.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> CreateUserAsync(UserDTO user);
        Task<UserDTO?> GetUserByEmailAsync(string email);
        Task<UserDTO?> GetUserByUsernameAsync(string username);
        Task<bool> IncrementFailedAttemptsAsync(int userId);
        Task<bool> UpdateLoginSuccessAsync(int userId);
        Task<bool> VerifyEmailAsync(int userId);
        Task<bool> ResetPasswordAsync(int userId, string passwordHash, string salt);
        Task<bool> RegisterVerifiedUserAsync(
            string username, string email, string givenNames,
            string pSurname, string mSurname, string phoneNumber,
            string passwordHash, string passwordSalt);
        }
}
