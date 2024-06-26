using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class TrainingWeek
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? Mesocycle { get; set; }

    public int OrderNumber { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<TrainingPlanWeek> TrainingPlanWeeks { get; set; } = new List<TrainingPlanWeek>();

    public virtual ICollection<TrainingWeekDay> TrainingWeekDays { get; set; } = new List<TrainingWeekDay>();
}
