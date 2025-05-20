using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Auth
{
    public class ResetPasswordRequestDTO
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}