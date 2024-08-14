using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class UserExercise
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? ExerciseId { get; set; }

    public int UseCount { get; set; }

    public double BestWeight { get; set; }

    public double AverageWeight { get; set; }

    public double LastUsedWeightKg { get; set; }

    public double AverageTimerInSeconds { get; set; }
    public double AverageHeartRate { get; set; }
    public double AverageKCalBurned { get; set; }
    public double AverageDistance { get; set; }
    public double AverageSpeed { get; set; }
    public double AverageRateOfPreceivedExertion { get; set; }
    public DateTime? CreatedAt { get; set; }

    public virtual Exercise? Exercise { get; set; }

    public virtual User? User { get; set; }
}
