using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class MuscleGroupMuscle
{
    public int Id { get; set; }

    public int? MuscleGroupId { get; set; }

    public int? MuscleId { get; set; }

    public virtual Muscle? Muscle { get; set; }

    public virtual MuscleGroup? MuscleGroup { get; set; }
}
