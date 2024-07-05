﻿using System;
using System.Collections.Generic;

namespace DataLibrary.ModelsV2;

public partial class ExerciseRecord
{
    public int Id { get; set; }

    public int? ExerciseId { get; set; }

    public int? Repetitions { get; set; }

    public int? TimerInSeconds { get; set; }

    public int? DistanceInMeters { get; set; }

    public double? WeightUsedKg { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Exercise? Exercise { get; set; }

    public virtual ICollection<TrainingSessionExerciseRecord> TrainingSessionExerciseRecords { get; set; } = new List<TrainingSessionExerciseRecord>();
}