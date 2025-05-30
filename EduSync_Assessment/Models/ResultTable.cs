using System;
using System.Collections.Generic;

namespace EduSync_Assessment.Models;

public partial class ResultTable
{
    public Guid ResultId { get; set; }

    public Guid? AssessmentId { get; set; }

    public Guid? UserId { get; set; }

    public int Score { get; set; }

    public DateTime AttemptDate { get; set; }

    public virtual AssessmentTable? Assessment { get; set; }

    public virtual UserTable? User { get; set; }
}
