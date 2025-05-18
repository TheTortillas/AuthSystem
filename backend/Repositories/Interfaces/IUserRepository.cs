using backend.DTOs.Users;

namespace backend.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> CreateUserAsync(UserDTO user);
        Task<UserDTO?> GetUserByEmailAsync(string email);
        Task<bool> IncrementFailedAttemptsAsync(int userId);
        Task<bool> UpdateLoginSuccessAsync(int userId);
    }
}
