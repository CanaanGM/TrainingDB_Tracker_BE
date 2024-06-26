using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class TrainingSession
{
    public int Id { get; set; }

    public int? Duration { get; set; }

    public int? Calories { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<TrainingSessionExerciseRecord> TrainingSessionExerciseRecords { get; set; } = new List<TrainingSessionExerciseRecord>();

    public virtual ICollection<TrainingSessionType> TrainingSessionTypes { get; set; } = new List<TrainingSessionType>();
}
