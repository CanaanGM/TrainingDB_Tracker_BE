﻿using System;
using System.Collections.Generic;

namespace DataLibrary.ModelsV2;

public partial class TrainingPlan
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int TrainingWeeks { get; set; }

    public int TrainingDaysPerWeek { get; set; }

    public string? Description { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<TrainingWeek> TrainingWeeksNavigation { get; set; } = new List<TrainingWeek>();

    public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();

    public virtual ICollection<TrainingType> TrainingTypes { get; set; } = new List<TrainingType>();
}
