using SharedLibrary.Dtos;

namespace TrainingTests.helpers;

public static class TrainingSessionDtoFactory
{
    public static TrainingSessionWriteDto CreateLegsSessionDto() => new TrainingSessionWriteDto
    {
        DurationInMinutes = 50,
        TotalCaloriesBurned = 489,
        Notes = "Legs",
        Mood = 5,
        CreatedAt = "8-11-2024 2:34:00",

        ExerciseRecords = new List<ExerciseRecordWriteDto>
        {
            new ExerciseRecordWriteDto
                { ExerciseName = "sissy squat - dumbbell", Repetitions = 15, WeightUsedKg = 10 },
            new ExerciseRecordWriteDto
                { ExerciseName = "sissy squat - dumbbell", Repetitions = 15, WeightUsedKg = 10 },
            new ExerciseRecordWriteDto
                { ExerciseName = "sissy squat - dumbbell", Repetitions = 15, WeightUsedKg = 10 },
            new ExerciseRecordWriteDto
                { ExerciseName = "sissy squat - dumbbell", Repetitions = 15, WeightUsedKg = 10 },
            new ExerciseRecordWriteDto
                { ExerciseName = "sissy squat - dumbbell", Repetitions = 12, WeightUsedKg = 0 },
            new ExerciseRecordWriteDto
                { ExerciseName = "sissy squat - dumbbell", Repetitions = 12 },
            new ExerciseRecordWriteDto
                { ExerciseName = "sissy squat - dumbbell", Repetitions = 12 },
            new ExerciseRecordWriteDto
                { ExerciseName = "sissy squat - dumbbell", Repetitions = 12 },
            new ExerciseRecordWriteDto
                { ExerciseName = "dumbbell split squat - glutes", Repetitions = 12, WeightUsedKg = 10 },
            new ExerciseRecordWriteDto
                { ExerciseName = "dumbbell split squat - glutes", Repetitions = 12, WeightUsedKg = 10 },
            new ExerciseRecordWriteDto
                { ExerciseName = "dumbbell split squat - glutes", Repetitions = 12, WeightUsedKg = 10 },
            new ExerciseRecordWriteDto { ExerciseName = "sumo deadlifts", Repetitions = 15, WeightUsedKg = 90 },
            new ExerciseRecordWriteDto { ExerciseName = "sumo deadlifts", Repetitions = 15, WeightUsedKg = 90 },
            new ExerciseRecordWriteDto { ExerciseName = "sumo deadlifts", Repetitions = 2, WeightUsedKg = 90 },
            new ExerciseRecordWriteDto { ExerciseName = "sumo deadlifts", Repetitions = 1, WeightUsedKg = 90 },
            new ExerciseRecordWriteDto { ExerciseName = "sumo deadlifts", Repetitions = 1, WeightUsedKg = 90 },
            new ExerciseRecordWriteDto { ExerciseName = "sumo deadlifts", Repetitions = 1, WeightUsedKg = 90 },
            new ExerciseRecordWriteDto { ExerciseName = "sumo deadlifts", Repetitions = 2, WeightUsedKg = 90 },
            new ExerciseRecordWriteDto { ExerciseName = "sumo deadlifts", Repetitions = 1, WeightUsedKg = 90 },
            new ExerciseRecordWriteDto { ExerciseName = "sumo deadlifts", Repetitions = 4, WeightUsedKg = 70 },
            new ExerciseRecordWriteDto
                { ExerciseName = "front squat - barbell", Repetitions = 15, WeightUsedKg = 50 },
            new ExerciseRecordWriteDto
                { ExerciseName = "front squat - barbell", Repetitions = 15, WeightUsedKg = 50 },
            new ExerciseRecordWriteDto
                { ExerciseName = "front squat - barbell", Repetitions = 15, WeightUsedKg = 50 },
            new ExerciseRecordWriteDto { ExerciseName = "front squat - barbell", Repetitions = 5, WeightUsedKg = 50 },
            new ExerciseRecordWriteDto { ExerciseName = "front squat - barbell", Repetitions = 6, WeightUsedKg = 50 },
            new ExerciseRecordWriteDto { ExerciseName = "front squat - barbell", Repetitions = 6, WeightUsedKg = 50 },
            new ExerciseRecordWriteDto { ExerciseName = "front squat - barbell", Repetitions = 6, WeightUsedKg = 50 },
            new ExerciseRecordWriteDto { ExerciseName = "front squat - barbell", Repetitions = 6, WeightUsedKg = 50 },
            new ExerciseRecordWriteDto { ExerciseName = "front squat - barbell", Repetitions = 6, WeightUsedKg = 50 },
            new ExerciseRecordWriteDto
                { ExerciseName = "rope jumping", TimerInSeconds = 1380, HeartRateAvg = 157, KcalBurned = 366 }
        }
    };

    public static TrainingSessionWriteDto CreateUpdateLegsSessionDto() => new TrainingSessionWriteDto
    {
        DurationInMinutes = 150,
        TotalCaloriesBurned = 500,
        Notes = "Updated Legs",
        Mood = 10,
        CreatedAt = "8-18-2024 12:34:00",

        ExerciseRecords = new List<ExerciseRecordWriteDto>
        {
            new ExerciseRecordWriteDto
                { ExerciseName = "sissy squat - dumbbell", Repetitions = 15, WeightUsedKg = 10, Notes = "these are awesome", RateOfPerceivedExertion = 9},
            new ExerciseRecordWriteDto
                { ExerciseName = "sissy squat - dumbbell", Repetitions = 15, WeightUsedKg = 15 },
            new ExerciseRecordWriteDto
                { ExerciseName = "sissy squat - dumbbell", Repetitions = 15, WeightUsedKg = 10 },
            new ExerciseRecordWriteDto
                { ExerciseName = "sissy squat - dumbbell", Repetitions = 15, WeightUsedKg = 5 },
            new ExerciseRecordWriteDto
                { ExerciseName = "sissy squat - dumbbell", Repetitions = 15, WeightUsedKg = 0 },
            new ExerciseRecordWriteDto
                { ExerciseName = "dumbbell split squat - glutes", Repetitions = 12, WeightUsedKg = 10 },
            new ExerciseRecordWriteDto
                { ExerciseName = "dumbbell split squat - glutes", Repetitions = 12, WeightUsedKg = 10 },
            new ExerciseRecordWriteDto
                { ExerciseName = "dumbbell split squat - glutes", Repetitions = 12, WeightUsedKg = 10 },
            new ExerciseRecordWriteDto { ExerciseName = "sumo deadlifts", Repetitions = 15, WeightUsedKg = 90 },
            new ExerciseRecordWriteDto { ExerciseName = "sumo deadlifts", Repetitions = 15, WeightUsedKg = 190 },
            new ExerciseRecordWriteDto { ExerciseName = "sumo deadlifts", Repetitions = 2, WeightUsedKg = 90 },
            new ExerciseRecordWriteDto
                { ExerciseName = "front squat - barbell", Repetitions = 15, WeightUsedKg = 50 },
            new ExerciseRecordWriteDto { ExerciseName = "front squat - barbell", Repetitions = 5, WeightUsedKg = 50 },
            new ExerciseRecordWriteDto { ExerciseName = "front squat - barbell", Repetitions = 6, WeightUsedKg = 50 },
            new ExerciseRecordWriteDto
                { ExerciseName = "rope jumping", TimerInSeconds = 1380, HeartRateAvg = 157, KcalBurned = 366 },
            new()
            {
                ExerciseName = "dragon flag", Repetitions = 30, RateOfPerceivedExertion = 6, RestInSeconds = 30
            },
        }
    };

    public static TrainingSessionWriteDto CreateCorrectSessionDtoMixedCardio(string date = "")
    {
        return new TrainingSessionWriteDto()
        {
            DurationInMinutes = 50,
            TotalCaloriesBurned = 489,
            Notes = "Mixed cardio session",
            Mood = 5,
            CreatedAt = string.IsNullOrEmpty(date) ? "8-11-2024 2:34:00" : date,
            ExerciseRecords =
            [
                new()
                {
                    ExerciseName = "rope jumping", TimerInSeconds = 1380, HeartRateAvg = 157, KcalBurned = 366,
                    RateOfPerceivedExertion = 8
                },
                new()
                {
                    ExerciseName = "pro shuttle", Repetitions = 6, HeartRateAvg = 162, RateOfPerceivedExertion = 8,
                    DistanceInMeters = 8
                },
                new()
                {
                    ExerciseName = "fast walking", Incline = 4, HeartRateAvg = 122, RateOfPerceivedExertion = 5,
                    DistanceInMeters = 10000, Speed = 10
                },
                new()
                {
                    ExerciseName = "dragon flag", Repetitions = 30, RateOfPerceivedExertion = 6, RestInSeconds = 30
                },
                new()
                {
                    ExerciseName = "dragon flag", Repetitions = 15, RateOfPerceivedExertion = 7, RestInSeconds = 30
                },
                new()
                {
                    ExerciseName = "dragon flag", Repetitions = 5, RateOfPerceivedExertion = 10, RestInSeconds = 30
                },
            ]
        };
    }

    public static TrainingSessionWriteDto CreateUpdateDto() => new TrainingSessionWriteDto()
    {
        Feeling = "Exhausted",
        Mood = 3,
        Notes = "Updated leg day",
        DurationInMinutes = 45,
        TotalCaloriesBurned = 450,
        ExerciseRecords = new List<ExerciseRecordWriteDto>()
        {
            new ExerciseRecordWriteDto()
            {
                ExerciseName = "arnold rotations - slight incline",
                Repetitions = 12,
                WeightUsedKg = 85
            },
            new ExerciseRecordWriteDto()
            {
                ExerciseName = "bus drivers - incline",
                Repetitions = 10,
                WeightUsedKg = 200
            }
        }
    };

    public static TrainingSessionWriteDto createInvalidSessionWriteDto()
    {
        return new TrainingSessionWriteDto()
        {
            Feeling = "", // Invalid because it's empty
            Mood = 5,
            Notes = "Invalid session",
            DurationInMinutes = 30,
            TotalCaloriesBurned = 300,
            ExerciseRecords = new List<ExerciseRecordWriteDto>()
            {
                new ExerciseRecordWriteDto()
                {
                    ExerciseName = "bench press",
                    Repetitions = 10,
                    WeightUsedKg = 60
                }
            }
        };
    }


    public static TrainingSessionWriteDto createMissingExerciseSessionWriteDto()
    {
        return new TrainingSessionWriteDto()
        {
            Feeling = "Death",
            Mood = 5,
            Notes = "ma knees!!",
            DurationInMinutes = 50,
            TotalCaloriesBurned = 546,
            ExerciseRecords =
            [
                new ExerciseRecordWriteDto()
                {
                    ExerciseName = "alphard",
                    Repetitions = 30,
                    RateOfPerceivedExertion = 1
                },
                new ExerciseRecordWriteDto()
                {
                    ExerciseName = "dante",
                    Repetitions = 12,
                    WeightUsedKg = 15,
                    RateOfPerceivedExertion = 6
                },
                new ExerciseRecordWriteDto()
                {
                    ExerciseName = "canaan",
                    Repetitions = 12,
                    WeightUsedKg = 15,
                    RateOfPerceivedExertion = 6
                },
            ]
        };
    }
}