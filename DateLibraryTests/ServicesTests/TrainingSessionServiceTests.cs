using DataLibrary.Dtos;
using DataLibrary.Helpers;
using DataLibrary.Services;
using DateLibraryTests.helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace DateLibraryTests.ServicesTests;

public class TrainingSessionServiceTests : BaseTestClass
{
    private Mock<ILogger<TrainingSessionService>> _logger;
    private readonly TrainingSessionService _service;

    public TrainingSessionServiceTests()
    {
        _logger = new Mock<ILogger<TrainingSessionService>>();
        _service = new TrainingSessionService(_context, _mapper, _logger.Object);
    }

    [Fact]
    public async Task CreateSessionAsync_NoUser_Failure()
    {
        var newSession = TrainingSessionDtoFactory.CreateMixedDto();

        var result = await _service.CreateSessionAsync(1, newSession, new CancellationToken());
        Assert.False(result.IsSuccess);
        Assert.Equal("User was not found", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateSessionAsync_MissingExercises_Failure()
    {
        ProductionDatabaseHelpers.SeedProductionData(_context);
        ProductionDatabaseHelpers.SeedDummyUsers(_context);
        var result =
            await _service.CreateSessionAsync(1, createMissingExerciseSessionWriteDto(), new CancellationToken());
        Assert.False(result.IsSuccess);
        Assert.Equal($"Some exercises are not in the database: alphard\n dante\n canaan", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateSessionAsync_Success()
    {
        ProductionDatabaseHelpers.SeedProductionData(_context);
        ProductionDatabaseHelpers.SeedDummyUsers(_context);
        var newSession = TrainingSessionDtoFactory.CreateCorrectSessionDtoMixedExerciseTypes();

        var result = await _service.CreateSessionAsync(1, newSession, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.Equal("Training session created successfully ~!", result.SuccessMessage);

        var user = _context.Users
            .Include(x => x.TrainingSessions)
            .ThenInclude(y => y.ExerciseRecords).ThenInclude(t => t.Exercise)
            .Include(x => x.ExerciseRecords)
            .ThenInclude(u => u.Exercise).Include(user => user.UserExercises)
            .ThenInclude(userExercise => userExercise.Exercise)
            .FirstOrDefault(x => x.Id == 1);

        Assert.NotNull(user);
        Assert.Single(user.TrainingSessions);
        Assert.Equal(4, user.UserExercises.Count);
        Assert.Equal(6, user.ExerciseRecords.Count);

        var trainingSession = user.TrainingSessions.Single();
        Assert.Equal(6, trainingSession.ExerciseRecords.Count);
        Assert.Equal(
            newSession.TotalCaloriesBurned + newSession.ExerciseRecords.Sum(x => x.KcalBurned)
            , trainingSession.Calories); // cause the default is 1 ; 3 times not explicitly added + 546
        Assert.NotNull(trainingSession.CreatedAt);
        Assert.IsType<DateTime>(trainingSession.CreatedAt);
        Assert.NotNull(trainingSession.ExerciseRecords.First().CreatedAt);
        Assert.IsType<DateTime>(trainingSession.ExerciseRecords.First().CreatedAt);

        Assert.Equal(newSession.Feeling, user.TrainingSessions.First().Feeling);
        Assert.Equal(newSession.Mood, user.TrainingSessions.First().Mood);
        Assert.Equal(newSession.Notes, user.TrainingSessions.First().Notes);
        Assert.Equal(newSession.TotalCaloriesBurned + newSession.ExerciseRecords.Sum(x => x.KcalBurned)
            , trainingSession.Calories);
        Assert.Equal(Utils.DurationSecondsFromMinutes(newSession.DurationInMinutes) ?? 0
            + newSession.ExerciseRecords.Sum(x => x.TimerInSeconds ?? 0)
            , trainingSession.DurationInSeconds);

        Assert.Equal(3, user.ExerciseRecords.Count(x => x.Exercise.Name == "dragon flag"));
        Assert.Equal(3, user.UserExercises.First(x => x.Exercise.Name == "dragon flag").UseCount);

        Assert.Single(user.UserExercises.Where(x => x.Exercise.Name == "fast walking"));
        
        var fastWalkingExercise = user.UserExercises.First(x => x.Exercise.Name == "fast walking");
        Assert.Equal(10000, fastWalkingExercise.AverageDistance);
        Assert.Equal(5, fastWalkingExercise.AverageRateOfPreceivedExertion);
        Assert.Equal(1, fastWalkingExercise.UseCount);
        Assert.Equal(122, fastWalkingExercise.AverageHeartRate);
        Assert.Equal(10, fastWalkingExercise.AverageSpeed);
        Assert.Equal(0, fastWalkingExercise.AverageWeight);
        Assert.Equal(0, fastWalkingExercise.LastUsedWeightKg);
        Assert.Equal(0, fastWalkingExercise.BestWeight);
        Assert.Equal(0, fastWalkingExercise.AverageTimerInSeconds);
        Assert.Equal(1, fastWalkingExercise.AverageKCalBurned);

        Assert.Single(user.UserExercises.Where(x => x.Exercise.Name == "dragon flag"));
        var dragonFlagExercise = user.UserExercises.First(x => x.Exercise.Name == "dragon flag");
        Assert.Equal(6.5, dragonFlagExercise.AverageRateOfPreceivedExertion);
        
    }

    [Fact]
    public async Task CreateSessionAsync_Success_Secundous()
    {
        ProductionDatabaseHelpers.SeedProductionData(_context);
        ProductionDatabaseHelpers.SeedDummyUsers(_context);
        var newSession = TrainingSessionDtoFactory.CreateMixedDto();
        var result = await _service.CreateSessionAsync(1, newSession, new CancellationToken());

        Assert.True(result.IsSuccess);
        Assert.Equal("Training session created successfully ~!", result.SuccessMessage);


        var user = _context.Users
            .Include(x => x.TrainingSessions)
            .ThenInclude(y => y.ExerciseRecords).ThenInclude(t => t.Exercise)
            .Include(x => x.ExerciseRecords)
            .ThenInclude(u => u.Exercise).Include(user => user.UserExercises)
            .ThenInclude(userExercise => userExercise.Exercise)
            .FirstOrDefault(x => x.Id == 1);

        // the training session
        var trainingSession = user.TrainingSessions.First();
        Assert.Equal(30, trainingSession.ExerciseRecords.Count);
        Assert.Equal((489 + 366 + 29), trainingSession.Calories);
        Assert.Equal(newSession.Feeling, trainingSession.Feeling);
        Assert.Equal(newSession.Mood, trainingSession.Mood);
        Assert.IsType<DateTime>(trainingSession.CreatedAt);
        Assert.Equal(newSession.DurationInMinutes * 60, trainingSession.DurationInSeconds);
        Assert.Equal(1, _context.TrainingSessions.Count());
        // the training records,
        Assert.Equal(30, trainingSession.ExerciseRecords.Count);
        Assert.Equal(267, trainingSession.ExerciseRecords.Sum(x => x.Repetitions));
        Assert.Equal(1310, trainingSession.ExerciseRecords.Sum(x => x.WeightUsedKg));
        Assert.Equal(1, trainingSession.ExerciseRecords.Average(x => x.RateOfPerceivedExertion));
        Assert.Equal(157, trainingSession.ExerciseRecords.Average(x => x.HeartRateAvg));
        // record meta data

        // 1. how many kg moved in this exercise
        Assert.Equal((9 * 50), trainingSession.ExerciseRecords
            .Where(x => x.Exercise.Name == "barbell front squat")
            .Sum(x => x.WeightUsedKg));
        // 2. how many reps for this exercise
        Assert.Equal(80, trainingSession.ExerciseRecords
            .Where(x => x.Exercise.Name == "barbell front squat")
            .Sum(x => x.Repetitions));
        // 3. maximum weight used
        Assert.Equal(50, trainingSession.ExerciseRecords
            .Where(x => x.Exercise.Name == "barbell front squat")
            .Max(x => x.WeightUsedKg));

        // 3. see the default weight is set properly 
        Assert.Equal(8, trainingSession.ExerciseRecords.Count(x => x.Exercise.Name == "sissy squat - dumbbell"));
        Assert.Equal(40, trainingSession.ExerciseRecords
            .Where(x => x.Exercise.Name == "sissy squat - dumbbell")
            .Sum(x => x.WeightUsedKg));


        // the user exercises
        var userExerciseRecords = user.UserExercises;
        Assert.Equal(5, userExerciseRecords.Count());

        var sumoDeadLiftUserExercise = userExerciseRecords.First(x => x.Exercise.Name == "sumo deadlifts");
        Assert.Equal(9, sumoDeadLiftUserExercise.UseCount);
        Assert.Equal(90, sumoDeadLiftUserExercise.BestWeight);
        Assert.Equal(90, sumoDeadLiftUserExercise.LastUsedWeightKg);
    }

    [Fact]
    public async Task CreateSessionAsync_NullExerciseRecords_Failure()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
        ProductionDatabaseHelpers.SeedDummyUsers(_context);

        var sessionDto = new TrainingSessionWriteDto()
        {
            Feeling = "Good",
            Mood = 5,
            Notes = "Test with null exercise records",
            DurationInMinutes = 50,
            TotalCaloriesBurned = 546,
            ExerciseRecords = null // This is the edge case we're testing
        };

        // Act
        var result = await _service.CreateSessionAsync(1, sessionDto, new CancellationToken());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("ExerciseRecords cannot be null or empty", result.ErrorMessage);
    }


    [Fact]
    public async Task CreateSessionBulkAsync_Success()
    {
        ProductionDatabaseHelpers.SeedProductionData(_context);
        ProductionDatabaseHelpers.SeedDummyUsers(_context);
    
        var sessionDtos = new List<TrainingSessionWriteDto>
        {
            TrainingSessionDtoFactory.CreateMixedDto(),
            TrainingSessionDtoFactory.CreateCorrectSessionDtoMixedExerciseTypes()
        };
    
        var result = await _service.CreateSessionsBulkAsync(1, sessionDtos, new CancellationToken());
    
        Assert.True(result.IsSuccess);
        Assert.Equal("Bulk training sessions created successfully!", result.SuccessMessage);
    
        var user = _context.Users
            .Include(x => x.TrainingSessions)
            .ThenInclude(y => y.ExerciseRecords)
            .Include(x => x.ExerciseRecords)
            .FirstOrDefault(x => x.Id == 1);
    
        Assert.NotNull(user);
        Assert.Equal(2, user.TrainingSessions.Count);
        Assert.Equal(4, user.ExerciseRecords.Count);
    }
    
    [Fact]
    public async Task CreateSessionBulkAsync_UserNotFound_Failure()
    {
        var sessionDtos = new List<TrainingSessionWriteDto>
        {
            TrainingSessionDtoFactory.CreateMixedDto(),
            TrainingSessionDtoFactory.CreateCorrectSessionDtoMixedExerciseTypes()
        };
    
        var result = await _service.CreateSessionsBulkAsync(999, sessionDtos, new CancellationToken());
    
        Assert.False(result.IsSuccess);
        Assert.Equal("User was not found", result.ErrorMessage);
    }
    
    [Fact]
    public async Task CreateSessionBulkAsync_InvalidDto_Failure()
    {
        ProductionDatabaseHelpers.SeedProductionData(_context);
        ProductionDatabaseHelpers.SeedDummyUsers(_context);
    
        var sessionDtos = new List<TrainingSessionWriteDto>
        {
            createInvalidSessionWriteDto(), // Assuming this method creates an invalid DTO
            TrainingSessionDtoFactory.CreateCorrectSessionDtoMixedExerciseTypes()
        };
    
        var result = await _service.CreateSessionsBulkAsync(1, sessionDtos, new CancellationToken());
    
        Assert.False(result.IsSuccess);
        Assert.Contains("Feeling cannot be empty", result.ErrorMessage); // Replace with the actual validation error
    }
    
    [Fact]
    public async Task CreateSessionBulkAsync_ShouldCreateSessionsFromJson()
    {
        ProductionDatabaseHelpers.SeedProductionData(_context);
        ProductionDatabaseHelpers.SeedDummyUsers(_context);
        
        // Arrange
        var jsonContent = File.ReadAllText("../../../../trainingSessionBulkRequest.json");
        var trainingSessions = JsonConvert.DeserializeObject<List<TrainingSessionWriteDto>>(jsonContent);
    
        // Act
        var result = await _service.CreateSessionsBulkAsync(1, trainingSessions, new CancellationToken());
    
        // Assert
        Assert.True(result.IsSuccess);
        var createdSessions = _context.TrainingSessions.Include(ts => ts.ExerciseRecords).ToList();
    
        Assert.Equal(trainingSessions.Count, createdSessions.Count);
    
        // Further assertions to verify each session and its exercise records match the JSON input
    }
    //
    //    
    //
    // [Fact]
    //    public async Task UpdateAsync_UserNotFound_ReturnsFailure()
    //    {
    //        // Arrange
    //        ProductionDatabaseHelpers.SeedProductionData(_context);
    //        var updateDto = createCorrectSessionWriteDto();
    //
    //        // Act
    //        var result = await _service.UpdateAsync(999, 1, updateDto, new CancellationToken());
    //
    //        // Assert
    //        Assert.False(result.IsSuccess);
    //        Assert.Equal("User not found", result.ErrorMessage);
    //    }
    //
    //    [Fact]
    //    public async Task UpdateAsync_SessionNotFound_ReturnsFailure()
    //    {
    //        // Arrange
    //        ProductionDatabaseHelpers.SeedProductionData(_context);
    //        ProductionDatabaseHelpers.SeedDummyUsers(_context);
    //        var updateDto = createUpdatedSessionWriteDto();
    //
    //        // Act
    //        var result = await _service.UpdateAsync(1, 999, updateDto, new CancellationToken());
    //
    //        // Assert
    //        Assert.False(result.IsSuccess);
    //        Assert.Equal("Training session not found", result.ErrorMessage);
    //    }
    //
    //    [Fact]
    //    public async Task UpdateAsync_ValidUpdate_ReturnsSuccess()
    //    {
    //        // Arrange
    //        ProductionDatabaseHelpers.SeedProductionData(_context);
    //        ProductionDatabaseHelpers.SeedDummyUsers(_context);
    //
    //        var sessionDto = createCorrectSessionWriteDto();
    //        var creationResult = await _service.CreateSessionAsync(1, sessionDto, new CancellationToken());
    //        Assert.True(creationResult.IsSuccess);
    //
    //        var existingSession = _context.TrainingSessions.First();
    //        var updateDto = createUpdatedSessionWriteDto();
    //
    //        // Act
    //        var result = await _service.UpdateAsync(1, existingSession.Id, updateDto, new CancellationToken());
    //
    //        // Assert
    //        Assert.True(result.IsSuccess);
    //        Assert.Equal("Training session updated successfully!", result.SuccessMessage);
    //
    //        var updatedSession = _context.TrainingSessions
    //            .Include(ts => ts.ExerciseRecords)
    //            .FirstOrDefault(ts => ts.Id == existingSession.Id);
    //
    //        Assert.NotNull(updatedSession);
    //        Assert.Equal(updateDto.Feeling, updatedSession.Feeling);
    //        Assert.Equal(updateDto.Notes, updatedSession.Notes);
    //        Assert.Equal(updateDto.Mood, updatedSession.Mood);
    //        Assert.Equal(updateDto.TotalCaloriesBurned + updateDto.ExerciseRecords.Sum(x => x.KcalBurned), updatedSession.Calories);
    //        Assert.Equal(updateDto.ExerciseRecords.Count, updatedSession.ExerciseRecords.Count);
    //    }
    private TrainingSessionWriteDto createInvalidSessionWriteDto()
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


    private TrainingSessionWriteDto createUpdatedSessionWriteDto()
    {
        return new TrainingSessionWriteDto()
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
    }


    private TrainingSessionWriteDto createMissingExerciseSessionWriteDto()
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