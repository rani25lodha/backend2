using System;
using System.Collections.Generic;

namespace EduSync_Assessment.Models;

public partial class UserTable
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public string? Email { get; set; }

    public string Role { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public virtual ICollection<CourseTable> CourseTables { get; set; } = new List<CourseTable>();

    public virtual ICollection<ResultTable> ResultTables { get; set; } = new List<ResultTable>();
}
