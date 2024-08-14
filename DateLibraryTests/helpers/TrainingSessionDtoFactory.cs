using DataLibrary.Dtos;

namespace DateLibraryTests.helpers;

public static class TrainingSessionDtoFactory
{
    public static TrainingSessionWriteDto CreateMixedDto()
    {
        return new TrainingSessionWriteDto
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
                    { ExerciseName = "barbell front squat", Repetitions = 15, WeightUsedKg = 50 },
                new ExerciseRecordWriteDto
                    { ExerciseName = "barbell front squat", Repetitions = 15, WeightUsedKg = 50 },
                new ExerciseRecordWriteDto
                    { ExerciseName = "barbell front squat", Repetitions = 15, WeightUsedKg = 50 },
                new ExerciseRecordWriteDto { ExerciseName = "barbell front squat", Repetitions = 5, WeightUsedKg = 50 },
                new ExerciseRecordWriteDto { ExerciseName = "barbell front squat", Repetitions = 6, WeightUsedKg = 50 },
                new ExerciseRecordWriteDto { ExerciseName = "barbell front squat", Repetitions = 6, WeightUsedKg = 50 },
                new ExerciseRecordWriteDto { ExerciseName = "barbell front squat", Repetitions = 6, WeightUsedKg = 50 },
                new ExerciseRecordWriteDto { ExerciseName = "barbell front squat", Repetitions = 6, WeightUsedKg = 50 },
                new ExerciseRecordWriteDto { ExerciseName = "barbell front squat", Repetitions = 6, WeightUsedKg = 50 },
                new ExerciseRecordWriteDto
                    { ExerciseName = "rope jumping", TimerInSeconds = 1380, HeartRateAvg = 157, KcalBurned = 366 }
            }
        };
    }

    public static TrainingSessionWriteDto CreateCorrectSessionDtoMixedExerciseTypes(string date = "")
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
}