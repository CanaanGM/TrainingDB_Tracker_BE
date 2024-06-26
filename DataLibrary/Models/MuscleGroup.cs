using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class MuscleGroup
{
    public int Id { get; set; }

    public string ScientificName { get; set; } = null!;

    public string? CommonName { get; set; }

    public string? Function { get; set; }

    public string? WikiPageUrl { get; set; }

    public virtual ICollection<MuscleGroupMuscle> MuscleGroupMuscles { get; set; } = new List<MuscleGroupMuscle>();

    public virtual ICollection<TrainingDayMuscleGroup> TrainingDayMuscleGroups { get; set; } = new List<TrainingDayMuscleGroup>();
}
