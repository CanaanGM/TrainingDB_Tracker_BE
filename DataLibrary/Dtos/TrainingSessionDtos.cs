using DataLibrary.Helpers;

namespace DataLibrary.Dtos;

public class TrainingSessionReadDto
{
    public int Id { get; set; }
    public double DurationInMinutes { get; set; }
    public int? TotalCaloriesBurned { get; set; }
    public string? Notes { get; set; }
    public int? Mood { get; set; }
    public double TotalKgMoved { get; set; }
    public double TotalRepetitions { get; set; }
    public double AverageRateOfPreceivedExertion { get; set; }
    public DateTime? CreatedAt { get; set; }
    public ICollection<ExerciseRecordReadDto> ExerciseRecords { get; set; } = new List<ExerciseRecordReadDto>();
    public ICollection<TrainingTypeReadDto> TrainingTypes { get; set; } = new List<TrainingTypeReadDto>();
}

public class TrainingSessionWriteDto
{
    public string Feeling { get; set; } = "good";
    public int DurationInMinutes { get; set; } = 1;
    public int? TotalCaloriesBurned { get; set; } = 1;
    public string? Notes { get; set; }
    public int Mood { get; set; } = 1;
    public ICollection<ExerciseRecordWriteDto> ExerciseRecords { get; set; } = [];
    public string CreatedAt { get; set; } = DateTime.Now.ToString("M-d-yyyy HH:mm:ss");
}

public class ExerciseRecordReadDto
{
    private string _exerciseName { get; set; }

    public string ExerciseName
    {
        get => _exerciseName;
        set => _exerciseName = Utils.NormalizeString(value);
    }
    public int Id { get; set; }
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
}

public class ExerciseRecordWriteDto
{
    //TODO: timer in seconds helper method
    private string _exerciseName;
    private int _timerInSeconds;
    public int Repetitions { get; set; } = 1;
    public int Mood { get; set; } = 5;

    //TODO: change this AFTER you create the sessions 
    // not gonna mess with a 12k lines json for this change now. . .
    public int? TimerInSeconds
    {
        get => _timerInSeconds;
        set => _timerInSeconds = (int) value ; // =(int) Utils.DurationSecondsFromMinutes(value);
    }
    public double WeightUsedKg { get; set; } = 0;
    public int RateOfPerceivedExertion { get; set; } = 1;
    public int? RestInSeconds { get; set; }
    public int KcalBurned { get; set; } = 1;
    public int? DistanceInMeters { get; set; }
    public string? Notes { get; set; }
    public int? Incline { get; set; }
    public int? Speed { get; set; }
    public int? HeartRateAvg { get; set; }


    public required string ExerciseName
    {
        get => _exerciseName;
        set => _exerciseName = Utils.NormalizeString(value);
    }
}