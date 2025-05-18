using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Auth
{
    public class RegisterRequestDTO
    {
        [Required]
        [MinLength(6)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string GivenNames { get; set; } = string.Empty;

        [Required]
        public string PSurname { get; set; } = string.Empty;

        public string? MSurname { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}
