namespace EduSync_Assessment.DTO
{
    public class AssessmentCreateDto
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string Questions { get; set; } = null!;
        public int MaxScore { get; set; }

       
    }

    public class AssessmentReadDto
    {
        public Guid AssessmentId { get; set; }
        public string Title { get; set; } = null!;
        public string Questions { get; set; } = null!;
        public int MaxScore { get; set; }
        public Guid? CourseId { get; set; }
    }

    public class AssessmentUpdateDto
    {
        public string Title { get; set; } = null!;
        public string Questions { get; set; } = null!;
        public int MaxScore { get; set; }
    }
}