namespace EduSync_Assessment.DTO
{
    public class CourseCreateDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Guid InstructorId { get; set; }
        public string MediaUrl { get; set; } = null!;
    }

    public class CourseReadDto
    {
        public Guid CourseId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public Guid? InstructorId { get; set; }
        public string? MediaUrl { get; set; }
    }

    public class CourseUpdateDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string MediaUrl { get; set; } = null!;
    }
}