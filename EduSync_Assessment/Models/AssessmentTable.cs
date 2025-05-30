using System;
using System.Collections.Generic;

namespace EduSync_Assessment.Models;

public partial class AssessmentTable
{
    public Guid AssessmentId { get; set; }

    public Guid? CourseId { get; set; }

    public string Title { get; set; } = null!;

    public string Questions { get; set; } = null!;

    public int MaxScore { get; set; }

    public virtual CourseTable? Course { get; set; }

    public virtual ICollection<ResultTable> ResultTables { get; set; } = new List<ResultTable>();
}
