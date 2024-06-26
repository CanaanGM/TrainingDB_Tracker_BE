using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class TrainingPlanWeek
{
    public int Id { get; set; }

    public int? TrainingPlanId { get; set; }

    public int? TrainingWeekId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual TrainingPlan? TrainingPlan { get; set; }

    public virtual TrainingWeek? TrainingWeek { get; set; }
}
