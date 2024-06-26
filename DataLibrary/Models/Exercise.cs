using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class Exercise
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<BlockExercise> BlockExercises { get; set; } = new List<BlockExercise>();

    public virtual ICollection<ExerciseMuscle> ExerciseMuscles { get; set; } = new List<ExerciseMuscle>();

    public virtual ICollection<ExerciseRecord> ExerciseRecords { get; set; } = new List<ExerciseRecord>();

    public virtual ICollection<ExerciseType> ExerciseTypes { get; set; } = new List<ExerciseType>();
}
