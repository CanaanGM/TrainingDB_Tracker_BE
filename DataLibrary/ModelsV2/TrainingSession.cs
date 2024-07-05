using System;
using System.Collections.Generic;

namespace DataLibrary.ModelsV2;

public partial class TrainingSession
{
    public int Id { get; set; }

    public int? DurationInSeconds { get; set; }

    public int? Calories { get; set; }

    public string? Notes { get; set; }

    public int? Mood { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<TrainingSessionExerciseRecord> TrainingSessionExerciseRecords { get; set; } = new List<TrainingSessionExerciseRecord>();

    public virtual ICollection<TrainingType> TrainingTypes { get; set; } = new List<TrainingType>();
}
