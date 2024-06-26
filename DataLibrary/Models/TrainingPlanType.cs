using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class TrainingPlanType
{
    public int Id { get; set; }

    public int? TrainingPlanId { get; set; }

    public int? TrainingTypeId { get; set; }

    public virtual TrainingPlan? TrainingPlan { get; set; }

    public virtual TrainingType? TrainingType { get; set; }
}
