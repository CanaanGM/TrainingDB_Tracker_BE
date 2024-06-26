using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class TrainingType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<ExerciseType> ExerciseTypes { get; set; } = new List<ExerciseType>();

    public virtual ICollection<TrainingPlanType> TrainingPlanTypes { get; set; } = new List<TrainingPlanType>();

    public virtual ICollection<TrainingSessionType> TrainingSessionTypes { get; set; } = new List<TrainingSessionType>();
}
