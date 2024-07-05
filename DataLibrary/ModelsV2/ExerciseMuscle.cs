using System;
using System.Collections.Generic;

namespace DataLibrary.ModelsV2;

public partial class ExerciseMuscle
{
    public bool? IsPrimary { get; set; }

    public int MuscleId { get; set; }

    public int ExerciseId { get; set; }

    public virtual Exercise Exercise { get; set; } = null!;

    public virtual Muscle Muscle { get; set; } = null!;
}
