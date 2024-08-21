using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class TrainingSession
{
    public int Id { get; set; }

    public int Calories { get; set; }

    public double DurationInSeconds { get; set; }

    public int Mood { get; set; }

    public string Feeling { get; set; } = null!;

    public double? TotalKgMoved { get; set; }

    public double? TotalRepetitions { get; set; }

    public double? AverageRateOfPerceivedExertion { get; set; }

    public string? Notes { get; set; }

    public int? UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<ExerciseRecord> ExerciseRecords { get; set; } = new List<ExerciseRecord>();
}
