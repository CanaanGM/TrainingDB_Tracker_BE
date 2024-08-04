using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class TrainingSessionExerciseRecord
{
    public int TrainingSessionId { get; set; }

    public int ExerciseRecordId { get; set; }

    public double? LastWeightUsedKg { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ExerciseRecord ExerciseRecord { get; set; } = null!;

    public virtual TrainingSession TrainingSession { get; set; } = null!;
}
