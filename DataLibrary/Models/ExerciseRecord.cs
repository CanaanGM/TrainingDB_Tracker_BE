using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class ExerciseRecord
{
    public int Id { get; set; }

    public int? ExerciseId { get; set; }

    public int? UserId { get; set; }

    public int Repetitions { get; set; }

    public int Mood { get; set; }

    public int? TimerInSeconds { get; set; }

    public double? WeightUsedKg { get; set; }

    public double? RateOfPerceivedExertion { get; set; }

    public int? RestInSeconds { get; set; }

    public int? KcalBurned { get; set; }

    public int? DistanceInMeters { get; set; }

    public string? Notes { get; set; }

    public int? Incline { get; set; }

    public int? Speed { get; set; }

    public int? HeartRateAvg { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Exercise? Exercise { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<TrainingSession> TrainingSessions { get; set; } = new List<TrainingSession>();
}
