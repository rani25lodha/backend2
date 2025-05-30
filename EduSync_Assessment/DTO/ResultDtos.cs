namespace EduSync_Assessment.DTO
{
    public class ResultCreateDto
    {
        public Guid AssessmentId { get; set; }
        public Guid UserId { get; set; }
        public int Score { get; set; }
        public DateTime AttemptDate { get; set; }
    }

    public class ResultReadDto
    {
        public Guid ResultId { get; set; }
        public Guid? AssessmentId { get; set; }
        public Guid? UserId { get; set; }
        public int Score { get; set; }
        public DateTime AttemptDate { get; set; }
    }
}