using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class TrainingWeek
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int OrderNumber { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? TrainingPlanId { get; set; }

    public virtual ICollection<TrainingDay> Days { get; set; } = new List<TrainingDay>();

    public virtual TrainingPlan? TrainingPlan { get; set; }
}
