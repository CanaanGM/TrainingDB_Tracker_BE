using System;
using System.Collections.Generic;

namespace DataLibrary.Models;

public partial class ExerciseRecord
{
    public int Id { get; set; }
    public int? ExerciseId { get; set; }
    public int? Repetitions { get; set; }
    public int? TimerInSeconds { get; set; } //TODO: this need to be inputed in mins then converted 
    public int? DistanceInMeters { get; set; }
    public double? WeightUsedKg { get; set; }
    public string? Notes { get; set; }
    public int? RestInSeconds { get; set; }
    public int? Incline { get; set; }
    public int? Speed { get; set; }
    public int? HeartRateAvg { get; set; }
    public int? KcalBurned { get; set; }
    public int? RateOfPerceivedExertion { get; set; } 
    public DateTime? CreatedAt { get; set; }
    public  Exercise? Exercise { get; set; }
    public  ICollection<TrainingSessionExerciseRecord> TrainingSessionExerciseRecords { get; set; } =
        new List<TrainingSessionExerciseRecord>();
}