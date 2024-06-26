using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class TrainingDay
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<TrainingDayBlock> TrainingDayBlocks { get; set; } = new List<TrainingDayBlock>();

    public virtual ICollection<TrainingDayMuscleGroup> TrainingDayMuscleGroups { get; set; } = new List<TrainingDayMuscleGroup>();

    public virtual ICollection<TrainingWeekDay> TrainingWeekDays { get; set; } = new List<TrainingWeekDay>();
}
