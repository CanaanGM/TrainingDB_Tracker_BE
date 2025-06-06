﻿using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class Exercise
{
    public int Id { get; set; }

    public int? Difficulty { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? HowTo { get; set; }

    public virtual ICollection<BlockExercise> BlockExercises { get; set; } = new List<BlockExercise>();

    public virtual ICollection<ExerciseHowTo> ExerciseHowTos { get; set; } = new List<ExerciseHowTo>();

    public virtual ICollection<ExerciseImage> ExerciseImages { get; set; } = new List<ExerciseImage>();

    public virtual ICollection<ExerciseMuscle> ExerciseMuscles { get; set; } = new List<ExerciseMuscle>();

    public virtual ICollection<ExerciseRecord> ExerciseRecords { get; set; } = new List<ExerciseRecord>();

    public virtual ICollection<UserExercise> UserExercises { get; set; } = new List<UserExercise>();

    public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();

    public virtual ICollection<TrainingType> TrainingTypes { get; set; } = new List<TrainingType>();
}
