using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class UserTrainingPlan
{
    public int UserId { get; set; }

    public int TrainingPlanId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsFinished { get; set; }

    public DateTime? EnrolledDate { get; set; }
}
