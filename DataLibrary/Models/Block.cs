using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class Block
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? Sets { get; set; }

    public int? RestInSeconds { get; set; }

    public string? Instrcustions { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<BlockExercise> BlockExercises { get; set; } = new List<BlockExercise>();

    public virtual ICollection<TrainingDayBlock> TrainingDayBlocks { get; set; } = new List<TrainingDayBlock>();
}
