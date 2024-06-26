using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class Muscle
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Function { get; set; }

    public string? WikiPageUrl { get; set; }

    public virtual ICollection<EquipmentMuscle> EquipmentMuscles { get; set; } = new List<EquipmentMuscle>();

    public virtual ICollection<ExerciseMuscle> ExerciseMuscles { get; set; } = new List<ExerciseMuscle>();

    public virtual ICollection<MuscleGroupMuscle> MuscleGroupMuscles { get; set; } = new List<MuscleGroupMuscle>();
}
