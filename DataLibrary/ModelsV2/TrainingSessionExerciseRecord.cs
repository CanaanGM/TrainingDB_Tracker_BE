﻿using System;
using System.Collections.Generic;

namespace DataLibrary.ModelsV2;

public partial class TrainingSessionExerciseRecord
{
    public int Id { get; set; }

    public int? TrainingSessionId { get; set; }

    public int? ExerciseRecordId { get; set; }

    public double? LastWeightUsedKg { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ExerciseRecord? ExerciseRecord { get; set; }

    public virtual TrainingSession? TrainingSession { get; set; }
}