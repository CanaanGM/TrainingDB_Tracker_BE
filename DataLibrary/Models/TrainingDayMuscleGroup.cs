using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class TrainingDayMuscleGroup
{
    public int Id { get; set; }

    public int? TrainingDayId { get; set; }

    public int? MuscleGroupId { get; set; }

    public virtual MuscleGroup? MuscleGroup { get; set; }

    public virtual TrainingDay? TrainingDay { get; set; }
}
