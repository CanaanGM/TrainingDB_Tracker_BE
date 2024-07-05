using System;
using System.Collections.Generic;

namespace DataLibrary.ModelsV2;

public partial class Exercise
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? HowTo { get; set; }

    public int? Difficulty { get; set; }

    public virtual ICollection<BlockExercise> BlockExercises { get; set; } = new List<BlockExercise>();

    public virtual ICollection<ExerciseHowTo> ExerciseHowTos { get; set; } = new List<ExerciseHowTo>();

    public virtual ICollection<ExerciseMuscle> ExerciseMuscles { get; set; } = new List<ExerciseMuscle>();

    public virtual ICollection<ExerciseRecord> ExerciseRecords { get; set; } = new List<ExerciseRecord>();

    public virtual ICollection<TrainingType> TrainingTypes { get; set; } = new List<TrainingType>();
}
