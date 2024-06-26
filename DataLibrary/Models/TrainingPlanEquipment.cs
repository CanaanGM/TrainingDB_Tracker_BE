using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class TrainingPlanEquipment
{
    public int Id { get; set; }

    public int? TrainingPlanId { get; set; }

    public int? EquipmentId { get; set; }

    public virtual Equipment? Equipment { get; set; }

    public virtual TrainingPlan? TrainingPlan { get; set; }
}
