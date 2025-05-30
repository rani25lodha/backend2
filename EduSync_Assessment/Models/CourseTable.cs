using System;
using System.Collections.Generic;

namespace EduSync_Assessment.Models;

public partial class CourseTable
{
    public Guid CourseId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public Guid? InstructorId { get; set; }

    public string? MediaUrl { get; set; }

    public virtual ICollection<AssessmentTable> AssessmentTables { get; set; } = new List<AssessmentTable>();

    public virtual UserTable? Instructor { get; set; }
}
