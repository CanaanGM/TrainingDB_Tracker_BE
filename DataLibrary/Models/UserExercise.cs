using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class UserExercise
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? ExerciseId { get; set; }

    public int? UseCount { get; set; }

    public double? BestWeight { get; set; }

    public double? AverageWeight { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Exercise? Exercise { get; set; }

    public virtual ICollection<ExerciseRecord> ExerciseRecords { get; set; } = new List<ExerciseRecord>();

    public virtual User? User { get; set; }
}
