using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class TrainingPlan
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int TrainingWeeks { get; set; }

    public int TrainingDaysPerWeek { get; set; }

    public string? Description { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<TrainingPlanEquipment> TrainingPlanEquipments { get; set; } = new List<TrainingPlanEquipment>();

    public virtual ICollection<TrainingPlanType> TrainingPlanTypes { get; set; } = new List<TrainingPlanType>();

    public virtual ICollection<TrainingPlanWeek> TrainingPlanWeeks { get; set; } = new List<TrainingPlanWeek>();
}
