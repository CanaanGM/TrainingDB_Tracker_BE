using DataLibrary.Models;
using DataLibrary.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SharedLibrary.Dtos;
using SharedLibrary.Helpers;
using TrainingTests.helpers;

namespace TrainingTests.ServicesTests;

public class TrainingSessionServiceTests : BaseIntegrationTestClass
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
        var newSession = TrainingSessionDtoFactory.CreateLegsSessionDto();

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
            await _service.CreateSessionAsync(1, TrainingSessionDtoFactory.createMissingExerciseSessionWriteDto(),
                new CancellationToken());
        Assert.False(result.IsSuccess);
        Assert.Equal($"Some exercises are not in the database:\n alphard\n dante\n canaan", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateSessionAsync_Success()
    {
        ProductionDatabaseHelpers.SeedProductionData(_context);
        ProductionDatabaseHelpers.SeedDummyUsers(_context);
        var newSession = TrainingSessionDtoFactory.CreateCorrectSessionDtoMixedCardio();

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
        ValidateMixedCardioSession(trainingSession, newSession, user);
    }

    private static void ValidateMixedCardioSession(TrainingSession trainingSession, TrainingSessionWriteDto newSession,
        User user)
    {
        Assert.Equal(6, trainingSession.ExerciseRecords.Count);
        Assert.Equal(
            newSession.TotalCaloriesBurned + newSession.ExerciseRecords.Sum(x => x.KcalBurned)
            , trainingSession.Calories); // cause the default is 1 ; 3 times not explicitly added + 546
        Assert.NotNull(trainingSession.CreatedAt);
        Assert.IsType<DateTime>(trainingSession.CreatedAt);
        Assert.NotNull(trainingSession.ExerciseRecords.First().CreatedAt);
        Assert.IsType<DateTime>(trainingSession.ExerciseRecords.First().CreatedAt);

        Assert.Equal(newSession.Feeling, trainingSession.Feeling);
        Assert.Equal(newSession.Mood, trainingSession.Mood);
        Assert.Equal(newSession.Notes, trainingSession.Notes);
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
        Assert.Equal(5, fastWalkingExercise.AverageRateOfPerceivedExertion);
        Assert.Equal(1, fastWalkingExercise.UseCount);
        Assert.Equal(122, fastWalkingExercise.AverageHeartRate);
        Assert.Equal(10, fastWalkingExercise.AverageSpeed);
        Assert.Equal(0, fastWalkingExercise.AverageWeight);
        Assert.Equal(0, fastWalkingExercise.LastUsedWeightKg);
        Assert.Equal(0, fastWalkingExercise.BestWeight);
        Assert.Equal(0, fastWalkingExercise.AverageTimerInSeconds);
        Assert.Equal(1, fastWalkingExercise.AverageKcalBurned);

        Assert.Single(user.UserExercises.Where(x => x.Exercise.Name == "dragon flag"));
        var dragonFlagExercise = user.UserExercises.First(x => x.Exercise.Name == "dragon flag");
        Assert.Equal(6.5, dragonFlagExercise.AverageRateOfPerceivedExertion);
    }

    [Fact]
    public async Task CreateSessionAsync_Success_Secundous()
    {
        ProductionDatabaseHelpers.SeedProductionData(_context);
        ProductionDatabaseHelpers.SeedDummyUsers(_context);
        var newSession = TrainingSessionDtoFactory.CreateLegsSessionDto();
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

        ValidateLegsSession(trainingSession, newSession, user);
        // the user exercises
        var userExerciseRecords = user.UserExercises;
        Assert.Equal(5, userExerciseRecords.Count());

        var sumoDeadLiftUserExercise = userExerciseRecords.First(x => x.Exercise.Name == "sumo deadlifts");
        Assert.Equal(9, sumoDeadLiftUserExercise.UseCount);
        Assert.Equal(90, sumoDeadLiftUserExercise.BestWeight);
        Assert.Equal(90, sumoDeadLiftUserExercise.LastUsedWeightKg);
    }

    private void ValidateLegsSession(TrainingSession trainingSession, TrainingSessionWriteDto newSession, User user)
    {
        Assert.Equal(30, trainingSession.ExerciseRecords.Count);
        Assert.Equal((489 + 366 + 29), trainingSession.Calories);
        Assert.Equal(newSession.Feeling, trainingSession.Feeling);
        Assert.Equal(newSession.Mood, trainingSession.Mood);
        Assert.IsType<DateTime>(trainingSession.CreatedAt);
        Assert.Equal(newSession.DurationInMinutes * 60, trainingSession.DurationInSeconds);
        // the training records,
        Assert.Equal(30, trainingSession.ExerciseRecords.Count);
        Assert.Equal(267, trainingSession.ExerciseRecords.Sum(x => x.Repetitions));
        Assert.Equal(1310, trainingSession.ExerciseRecords.Sum(x => x.WeightUsedKg));
        Assert.Equal(1, trainingSession.ExerciseRecords.Average(x => x.RateOfPerceivedExertion));
        Assert.Equal(157, trainingSession.ExerciseRecords.Average(x => x.HeartRateAvg));
        // record meta data

        // 1. how many kg moved in this exercise
        Assert.Equal((9 * 50), trainingSession.ExerciseRecords
            .Where(x => x.Exercise.Name == "front squat - barbell")
            .Sum(x => x.WeightUsedKg));
        // 2. how many reps for this exercise
        Assert.Equal(80, trainingSession.ExerciseRecords
            .Where(x => x.Exercise.Name == "front squat - barbell")
            .Sum(x => x.Repetitions));
        // 3. maximum weight used
        Assert.Equal(50, trainingSession.ExerciseRecords
            .Where(x => x.Exercise.Name == "front squat - barbell")
            .Max(x => x.WeightUsedKg));

        // 3. see the default weight is set properly 
        Assert.Equal(8, trainingSession.ExerciseRecords.Count(x => x.Exercise.Name == "sissy squat - dumbbell"));
        Assert.Equal(40, trainingSession.ExerciseRecords
            .Where(x => x.Exercise.Name == "sissy squat - dumbbell")
            .Sum(x => x.WeightUsedKg));
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
            TrainingSessionDtoFactory.CreateLegsSessionDto(),
            TrainingSessionDtoFactory.CreateCorrectSessionDtoMixedCardio()
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
        Assert.Equal(36, user.ExerciseRecords.Count);

        var userTrainingSessions = user.TrainingSessions.ToList();
        ValidateLegsSession(userTrainingSessions[0], sessionDtos[0], user);
        ValidateMixedCardioSession(userTrainingSessions[1], sessionDtos[1], user);
    }

    [Fact]
    public async Task CreateSessionBulkAsync_UserNotFound_Failure()
    {
        var sessionDtos = new List<TrainingSessionWriteDto>
        {
            TrainingSessionDtoFactory.CreateLegsSessionDto(),
            TrainingSessionDtoFactory.CreateCorrectSessionDtoMixedCardio()
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
            TrainingSessionDtoFactory.createInvalidSessionWriteDto(),
            TrainingSessionDtoFactory.CreateCorrectSessionDtoMixedCardio()
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

        var user = _context.Users
            .Include(x => x.TrainingSessions)
            .First(x => x.Id == 1);
        Assert.Equal(user.TrainingSessions.Count, trainingSessions.Count);
        Assert.Equal(user.TrainingSessions.Count, createdSessions.Count);
    }


    [Fact]
    public async Task UpdateAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
        var updateDto = TrainingSessionDtoFactory.CreateLegsSessionDto();

        // Act
        var result = await _service.UpdateTrainingSession(999, 1, updateDto, new CancellationToken());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User was not found", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_SessionNotFound_ReturnsFailure()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
        ProductionDatabaseHelpers.SeedDummyUsers(_context);
        var updateDto = TrainingSessionDtoFactory.CreateUpdateDto();

        // Act
        var result = await _service.UpdateTrainingSession(1, 999, updateDto, new CancellationToken());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal($"Training Session of id:`{999}` was not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_ValidUpdate_ReturnsSuccess()
    {
        ProductionDatabaseHelpers.SeedProductionData(_context);
        ProductionDatabaseHelpers.SeedDummyUsers(_context);

        var sessionDto = TrainingSessionDtoFactory.CreateLegsSessionDto();
        var creationResult = await _service.CreateSessionAsync(1, sessionDto, new CancellationToken());
        Assert.True(creationResult.IsSuccess);

        var existingSession = _context.TrainingSessions.First();
        var updateDto = TrainingSessionDtoFactory.CreateUpdateDto();


        var result = await _service.UpdateTrainingSession(1, existingSession.Id, updateDto, new CancellationToken());


        Assert.True(result.IsSuccess);
        Assert.Equal($"Updated session of id:'{existingSession.Id}' successfully", result.SuccessMessage);

        var updatedSession = _context.TrainingSessions
            .Include(ts => ts.ExerciseRecords)
            .FirstOrDefault(ts => ts.Id == existingSession.Id);

        Assert.NotNull(updatedSession);
        Assert.Equal(updateDto.Feeling, updatedSession.Feeling);
        Assert.Equal(updateDto.Notes, updatedSession.Notes);
        Assert.Equal(updateDto.Mood, updatedSession.Mood);
        Assert.Equal(updateDto.ExerciseRecords.Count, updatedSession.ExerciseRecords.Count);
        Assert.Equal(
            updateDto.TotalCaloriesBurned
            + updateDto.ExerciseRecords.Sum(x => x.KcalBurned),
            updatedSession.Calories);

        var user = await _context.Users
            .Include(x => x.ExerciseRecords)
            .ThenInclude(exerciseRecord => exerciseRecord.Exercise)
            .Include(x => x.TrainingSessions)
            .ThenInclude(x => x.ExerciseRecords)
            .Include(x => x.UserExercises)
            .ThenInclude(userExercise => userExercise.Exercise)
            .FirstOrDefaultAsync(x => x.Id == 1, new CancellationToken());

        Assert.NotNull(user);
        var userExerciseRecords = user.UserExercises.ToList();
        var sissySquatDumbbell = userExerciseRecords
            // it is there cause of the creation earlier, otherwise something IS wrong.
            .FirstOrDefault(x => x.Exercise.Name == "sissy squat - dumbbell");
        Assert.Null(sissySquatDumbbell);
        Assert.Equal(2, user.UserExercises.Count);
    }

    [Fact]
    public async Task UpdateAsync_Success_Secondus()
    {
        ProductionDatabaseHelpers.SeedProductionData(_context);
        ProductionDatabaseHelpers.SeedDummyUsers(_context);

        var sessionDto = TrainingSessionDtoFactory.CreateLegsSessionDto();
        var creationResult = await _service.CreateSessionAsync(1, sessionDto, new CancellationToken());
        Assert.True(creationResult.IsSuccess);

        var oldLegDay = _context.TrainingSessions.First();
        var updateLegDto = TrainingSessionDtoFactory.CreateUpdateLegsSessionDto();

        var mixedDto = TrainingSessionDtoFactory.CreateCorrectSessionDtoMixedCardio();

        var messThingsUpSession = await _service.CreateSessionAsync(1, mixedDto, new CancellationToken());
        var mixedSession = _context.TrainingSessions.FirstOrDefault(x => x.Id == 2);

        var oldDeadLiftMetadata = _context.UserExercises
            .FirstOrDefault(x => x.Exercise.Name == "sumo deadlifts" && x.UserId == 1);

        Assert.NotNull(oldDeadLiftMetadata);
        Assert.Equal(90, oldDeadLiftMetadata.BestWeight);
        Assert.Equal(90, oldDeadLiftMetadata.LastUsedWeightKg);
        Assert.Equal(90, oldDeadLiftMetadata.AverageWeight);


        var result = await _service.UpdateTrainingSession(1, oldLegDay.Id, updateLegDto, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.Equal($"Updated session of id:'{oldLegDay.Id}' successfully", result.SuccessMessage);


        var updatedLegDay = _context.TrainingSessions.FirstOrDefault();
        Assert.NotNull(updateLegDto);
        var updatedLegExerciseRecords = updatedLegDay.ExerciseRecords;
        Assert.Equal(5, updatedLegExerciseRecords.Count(x => x.Exercise.Name == "sissy squat - dumbbell"));
        Assert.Equal(3, updatedLegExerciseRecords.Count(x => x.Exercise.Name == "dumbbell split squat - glutes"));
        Assert.Equal(3, updatedLegExerciseRecords.Count(x => x.Exercise.Name == "sumo deadlifts"));
        Assert.Equal(1, updatedLegExerciseRecords.Count(x => x.Exercise.Name == "rope jumping"));
        Assert.Equal(1, updatedLegExerciseRecords.Count(x => x.Exercise.Name == "dragon flag"));

        var firstSissySquatRecord = updatedLegExerciseRecords.First(x => x.Exercise.Name == "sissy squat - dumbbell");
        Assert.Equal("these are awesome", firstSissySquatRecord.Notes);
        Assert.Equal(9, firstSissySquatRecord.RateOfPerceivedExertion);


        var user = await _context.Users
            .Include(x => x.ExerciseRecords)
            .ThenInclude(exerciseRecord => exerciseRecord.Exercise)
            .Include(x => x.TrainingSessions)
            .ThenInclude(x => x.ExerciseRecords)
            .Include(x => x.UserExercises)
            .ThenInclude(userExercise => userExercise.Exercise)
            .FirstOrDefaultAsync(x => x.Id == 1, new CancellationToken());

        Assert.Equal(8, user.UserExercises.Count());


        var newDeadLiftMetadata = user.UserExercises.FirstOrDefault(x => x.Exercise.Name == "sumo deadlifts");
        Assert.Equal(190, newDeadLiftMetadata.BestWeight);
        Assert.Equal(90, newDeadLiftMetadata.LastUsedWeightKg);
        Assert.Equal(140, newDeadLiftMetadata.AverageWeight);
        //TODO: add in more tests for this, im bored now
    }

    [Fact]
    public async Task DeleteTrainingSessionAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);

        // Act
        var result = await _service.DeleteTrainingSessionAsync(999, 1, new CancellationToken());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User was not found", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteTrainingSessionAsync_SessionNotFound_ReturnsFailure()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
        ProductionDatabaseHelpers.SeedDummyUsers(_context);

        // Act
        var result = await _service.DeleteTrainingSessionAsync(1, 999, new CancellationToken());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal($"Training Session of id:`{999}` was not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteTrainingSessionAsync_SuccessfullyDeletesSession()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
        ProductionDatabaseHelpers.SeedDummyUsers(_context);

        var sessionDto = TrainingSessionDtoFactory.CreateLegsSessionDto();
        var creationResult = await _service.CreateSessionAsync(1, sessionDto, new CancellationToken());
        Assert.True(creationResult.IsSuccess);

        var trainingSession = _context.TrainingSessions.First();

        // Act
        var result = await _service.DeleteTrainingSessionAsync(1, trainingSession.Id, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal($"Deleted session of id:'{trainingSession.Id}' successfully", result.SuccessMessage);

        var user = _context.Users
            .Include(u => u.TrainingSessions)
            .ThenInclude(ts => ts.ExerciseRecords)
            .Include(u => u.UserExercises)
            .Include(user => user.ExerciseRecords)
            .FirstOrDefault(u => u.Id == 1);

        Assert.NotNull(user);
        Assert.Empty(user.TrainingSessions);
        Assert.Empty(user.ExerciseRecords);

        var userExerciseRecords = user.UserExercises.ToList();
        Assert.Empty(userExerciseRecords);
    }

    [Fact]
    public async Task DeleteTrainingSessionAsync_RemovesOnlyTargetedSessionAndUpdatesMetadata()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
        ProductionDatabaseHelpers.SeedDummyUsers(_context);

        var sessionDto1 = TrainingSessionDtoFactory.CreateLegsSessionDto();
        var sessionDto2 = TrainingSessionDtoFactory.CreateCorrectSessionDtoMixedCardio();
        await _service.CreateSessionAsync(1, sessionDto1, new CancellationToken());
        await _service.CreateSessionAsync(1, sessionDto2, new CancellationToken());

        var trainingSessionToDelete = _context.TrainingSessions.First();
        var user = await _context.Users
            .Include(u => u.TrainingSessions)
            .ThenInclude(ts => ts.ExerciseRecords)
            .Include(u => u.UserExercises)
            .FirstOrDefaultAsync(u => u.Id == 1);

        var initialUserExerciseCount = user.UserExercises.Count;

        // Act
        var result = await _service.DeleteTrainingSessionAsync(1, trainingSessionToDelete.Id, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal($"Deleted session of id:'{trainingSessionToDelete.Id}' successfully", result.SuccessMessage);

        user = await _context.Users
            .Include(u => u.TrainingSessions)
            .ThenInclude(ts => ts.ExerciseRecords)
            .Include(u => u.UserExercises)
            .FirstOrDefaultAsync(u => u.Id == 1);

        Assert.Single(user.TrainingSessions);
        Assert.True(user.UserExercises.Count <= initialUserExerciseCount); // The count could decrease but not increase.
    }

    [Fact]
    public async Task GetTrainingSessionByIdAsync_ReturnsCorrectSession()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
        ProductionDatabaseHelpers.SeedDummyUsers(_context);

        var sessionDto = TrainingSessionDtoFactory.CreateLegsSessionDto();
        var creationResult = await _service.CreateSessionAsync(1, sessionDto, new CancellationToken());
        Assert.True(creationResult.IsSuccess);

        var trainingSession = _context.TrainingSessions.First();

        // Act
        var result = await _service.GetTrainingSessionByIdAsync(1, trainingSession.Id, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        var retrievedSession = result.Value;

        // Validate the session details
        Assert.Equal(trainingSession.Id, retrievedSession.Id);
        Assert.Equal(trainingSession.Notes, retrievedSession.Notes);
        Assert.Equal(trainingSession.Mood, retrievedSession.Mood);
        Assert.Equal(trainingSession.CreatedAt, retrievedSession.CreatedAt);

        // Validate calculated fields
        Assert.Equal(trainingSession.Calories, retrievedSession.TotalCaloriesBurned);
        Assert.Equal(trainingSession.DurationInSeconds / 60, retrievedSession.DurationInMinutes);
        Assert.Equal(trainingSession.TotalRepetitions, retrievedSession.TotalRepetitions);
        Assert.Equal(trainingSession.TotalKgMoved, retrievedSession.TotalKgMoved);
        Assert.Equal(trainingSession.AverageRateOfPerceivedExertion, retrievedSession.AverageRateOfPreceivedExertion);

        // Validate the exercise records
        Assert.Equal(trainingSession.ExerciseRecords.Count, retrievedSession.ExerciseRecords.Count);

        for (int i = 0; i < sessionDto.ExerciseRecords.Count; i++)
        {
            var expectedExercise = trainingSession.ExerciseRecords.ElementAt(i);
            var actualExercise = retrievedSession.ExerciseRecords.ElementAt(i);

            Assert.Equal(expectedExercise.Exercise.Name, actualExercise.ExerciseName);
            Assert.Equal(expectedExercise.Repetitions, actualExercise.Repetitions);
            Assert.Equal(expectedExercise.WeightUsedKg, actualExercise.WeightUsedKg);
            Assert.Equal(expectedExercise.TimerInSeconds, actualExercise.TimerInSeconds);
            Assert.Equal(expectedExercise.KcalBurned, actualExercise.KcalBurned);
            Assert.Equal(expectedExercise.RateOfPerceivedExertion, actualExercise.RateOfPerceivedExertion);
            Assert.Equal(expectedExercise.RestInSeconds, actualExercise.RestInSeconds);
            Assert.Equal(expectedExercise.Notes, actualExercise.Notes);
            Assert.Equal(expectedExercise.Incline, actualExercise.Incline);
            Assert.Equal(expectedExercise.Speed, actualExercise.Speed);
            Assert.Equal(expectedExercise.HeartRateAvg, actualExercise.HeartRateAvg);
        }
    }

    [Fact]
    public async Task GetPaginatedTrainingSessionsAsync_ReturnsCorrectSessions()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(_context);
        ProductionDatabaseHelpers.SeedDummyUsers(_context);

        var legsDto = TrainingSessionDtoFactory.CreateLegsSessionDto();
        var mixedCardioDto = TrainingSessionDtoFactory.CreateCorrectSessionDtoMixedCardio();

        await _service.CreateSessionAsync(1, legsDto, new CancellationToken());
        await _service.CreateSessionAsync(1, mixedCardioDto, new CancellationToken());

        // Act
        var result = await _service.GetPaginatedTrainingSessionsAsync(1, 1, 2, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        var paginatedSessions = result.Value;

        Assert.Equal(2, paginatedSessions.Items.Count);
        Assert.Equal(2, paginatedSessions.Metadata.TotalCount);
        Assert.Equal(1, paginatedSessions.Metadata.CurrentPage);
        Assert.Equal(1, paginatedSessions.Metadata.TotalPages);
        Assert.Equal(2, paginatedSessions.Metadata.PageSize);
        Assert.False(paginatedSessions.Metadata.HasPrevious);
        Assert.False(paginatedSessions.Metadata.HasNext);

        var retrievedMixedCardioSession = paginatedSessions.Items[0];
        var retrievedLegsSession = paginatedSessions.Items[1];

        // Validate Mixed Cardio Session
        Assert.Equal(mixedCardioDto.DurationInMinutes, retrievedMixedCardioSession.DurationInMinutes);
        Assert.Equal((mixedCardioDto.TotalCaloriesBurned ?? 0) + mixedCardioDto.ExerciseRecords.Sum(x => x.KcalBurned),
            retrievedMixedCardioSession.TotalCaloriesBurned);
        Assert.Equal(mixedCardioDto.Notes, retrievedMixedCardioSession.Notes);
        Assert.Equal(mixedCardioDto.Mood, retrievedMixedCardioSession.Mood);
        Assert.Equal(mixedCardioDto.ExerciseRecords.Sum(x => x.Repetitions),
            retrievedMixedCardioSession.TotalRepetitions);
        Assert.Equal(mixedCardioDto.ExerciseRecords.Sum(x => x.WeightUsedKg), retrievedMixedCardioSession.TotalKgMoved);
        Assert.Equal(mixedCardioDto.ExerciseRecords.Average(x => x.RateOfPerceivedExertion),
            retrievedMixedCardioSession.AverageRateOfPreceivedExertion);

        foreach (var (expected, actual) in mixedCardioDto.ExerciseRecords.Zip(
                     retrievedMixedCardioSession.ExerciseRecords, (expected, actual) => (expected, actual)))
        {
            Assert.Equal(expected.ExerciseName, actual.ExerciseName);
            Assert.Equal(expected.Repetitions, actual.Repetitions);

            if (expected.WeightUsedKg != null)
                Assert.Equal(expected.WeightUsedKg, actual.WeightUsedKg);

            if (expected.TimerInSeconds != null)
                Assert.Equal(expected.TimerInSeconds, actual.TimerInSeconds);

            if (expected.KcalBurned != null)
                Assert.Equal(expected.KcalBurned, actual.KcalBurned);

            if (expected.RateOfPerceivedExertion != null)
                Assert.Equal(expected.RateOfPerceivedExertion, actual.RateOfPerceivedExertion);

            if (expected.RestInSeconds != null)
                Assert.Equal(expected.RestInSeconds, actual.RestInSeconds);

            Assert.Equal(expected.Notes, actual.Notes);

            if (expected.Incline != null)
                Assert.Equal(expected.Incline, actual.Incline);

            if (expected.Speed != null)
                Assert.Equal(expected.Speed, actual.Speed);

            if (expected.HeartRateAvg != null)
                Assert.Equal(expected.HeartRateAvg, actual.HeartRateAvg);
        }

        // Validate Legs Session
        Assert.Equal(legsDto.DurationInMinutes, retrievedLegsSession.DurationInMinutes);
        Assert.Equal((legsDto.TotalCaloriesBurned ?? 0) + legsDto.ExerciseRecords.Sum(x => x.KcalBurned),
            retrievedLegsSession.TotalCaloriesBurned);
        Assert.Equal(legsDto.Notes, retrievedLegsSession.Notes);
        Assert.Equal(legsDto.Mood, retrievedLegsSession.Mood);
        Assert.Equal(legsDto.ExerciseRecords.Sum(x => x.Repetitions), retrievedLegsSession.TotalRepetitions);
        Assert.Equal(legsDto.ExerciseRecords.Sum(x => x.WeightUsedKg), retrievedLegsSession.TotalKgMoved);
        Assert.Equal(legsDto.ExerciseRecords.Average(x => x.RateOfPerceivedExertion),
            retrievedLegsSession.AverageRateOfPreceivedExertion);

        foreach (var (expected, actual) in legsDto.ExerciseRecords.Zip(retrievedLegsSession.ExerciseRecords,
                     (expected, actual) => (expected, actual)))
        {
            Assert.Equal(expected.ExerciseName, actual.ExerciseName);
            Assert.Equal(expected.Repetitions, actual.Repetitions);

            if (expected.WeightUsedKg != null)
                Assert.Equal(expected.WeightUsedKg, actual.WeightUsedKg);

            if (expected.TimerInSeconds != null)
                Assert.Equal(expected.TimerInSeconds, actual.TimerInSeconds);

            if (expected.KcalBurned != null)
                Assert.Equal(expected.KcalBurned, actual.KcalBurned);

            if (expected.RateOfPerceivedExertion != null)
                Assert.Equal(expected.RateOfPerceivedExertion, actual.RateOfPerceivedExertion);

            if (expected.RestInSeconds != null)
                Assert.Equal(expected.RestInSeconds, actual.RestInSeconds);

            Assert.Equal(expected.Notes, actual.Notes);

            if (expected.Incline != null)
                Assert.Equal(expected.Incline, actual.Incline);

            if (expected.Speed != null)
                Assert.Equal(expected.Speed, actual.Speed);

            if (expected.HeartRateAvg != null)
                Assert.Equal(expected.HeartRateAvg, actual.HeartRateAvg);
        }
    }
}