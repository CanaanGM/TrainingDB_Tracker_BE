using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class ExerciseMuscle
{
    public int Id { get; set; }

    public bool? IsPrimary { get; set; }

    public int? MuscleId { get; set; }

    public int? ExerciseId { get; set; }

    public virtual Exercise? Exercise { get; set; }

    public virtual Muscle? Muscle { get; set; }
}
