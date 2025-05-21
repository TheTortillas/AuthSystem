namespace backend.DTOs.Users
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        //public string GivenNames { get; set; } = string.Empty;
        //public string PSurname { get; set; } = string.Empty;
        //public string? MSurname { get; set; }
        //public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public int FailedAttempts { get; set; }

    }
}
