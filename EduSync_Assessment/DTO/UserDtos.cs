namespace EduSync_Assessment.DTO
{
    public class UserCreateDto
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
    }

    public class UserReadDto
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = null!;
        public string? Email { get; set; }
        public string Role { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
    }
}