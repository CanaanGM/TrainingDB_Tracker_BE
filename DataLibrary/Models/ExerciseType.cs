using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class ExerciseType
{
    public int Id { get; set; }

    public int? ExerciseId { get; set; }

    public int? TrainingTypeId { get; set; }

    public virtual Exercise? Exercise { get; set; }

    public virtual TrainingType? TrainingType { get; set; }
}
