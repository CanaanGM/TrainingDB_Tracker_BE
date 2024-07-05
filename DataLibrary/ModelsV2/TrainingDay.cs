﻿using System;
using System.Collections.Generic;

namespace DataLibrary.ModelsV2;

public partial class TrainingDay
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Notes { get; set; }

    public int? OrderNumber { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? TrainingWeekId { get; set; }

    public virtual ICollection<Block> Blocks { get; set; } = new List<Block>();

    public virtual TrainingWeek? TrainingWeek { get; set; }

    public virtual ICollection<Muscle> Muscles { get; set; } = new List<Muscle>();
}