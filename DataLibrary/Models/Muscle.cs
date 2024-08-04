using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class Muscle
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string MuscleGroup { get; set; } = null!;

    public string? Function { get; set; }

    public string? WikiPageUrl { get; set; }

    public virtual ICollection<ExerciseMuscle> ExerciseMuscles { get; set; } = new List<ExerciseMuscle>();

    public virtual ICollection<UserMuscle> UserMuscles { get; set; } = new List<UserMuscle>();
}
