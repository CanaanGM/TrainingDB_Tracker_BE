using AutoMapper;
using DataLibrary.Context;
using DataLibrary.Core;
using DataLibrary.Dtos;
using DataLibrary.Helpers;
using DataLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataLibrary.Services;

public class TrainingSessionService
{
    private readonly SqliteContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<TrainingSessionService> _logger;

    public TrainingSessionService(SqliteContext context, IMapper mapper, ILogger<TrainingSessionService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result> CreateSessionAsync(int userId, TrainingSessionWriteDto sessionDto,
        CancellationToken cancellationToken)
    {
        var stringValidationResult = Validation.ValidateDtoStrings(sessionDto);
        if (!stringValidationResult.IsSuccess)
            return Result.Failure(stringValidationResult.ErrorMessage);

        var collectionValidationResult = Validation.ValidateDtoICollections(sessionDto);
        if (!collectionValidationResult.IsSuccess)
            return Result.Failure(collectionValidationResult.ErrorMessage);

        try
        {
            var user = await _context.Users
                .Include(x => x.ExerciseRecords)
                .ThenInclude(exerciseRecord => exerciseRecord.Exercise)
                .Include(x => x.TrainingSessions)
                .Include(x => x.UserExercises)
                .ThenInclude(userExercise => userExercise.Exercise)
                .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            
            if (user is null) return Result.Failure("User was not found");

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var relatedExercisesResult =
                await GetRelatedExercisesDictionary(
                    sessionDto.ExerciseRecords
                        .Select(x => x.ExerciseName)
                        .Distinct()
                        .ToList(),
                    cancellationToken);

            if (!relatedExercisesResult.IsSuccess)
                return Result.Failure(relatedExercisesResult.ErrorMessage!);
            var relatedExercises = relatedExercisesResult.Value;

            var newSessionDate = Utils.ParseDate(sessionDto.CreatedAt) ?? DateTime.Now;
            var exerciseRecords =
                CreateExerciseRecords(sessionDto.ExerciseRecords.ToList(), relatedExercises!, newSessionDate);
            var newSession = CreateTrainingSession(user, sessionDto, newSessionDate, exerciseRecords);

            await _context.TrainingSessions.AddAsync(newSession, cancellationToken);

            UpdateUserExercises(user, exerciseRecords, newSessionDate);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success("Training session created successfully ~!");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error has occured in {nameof(CreateSessionAsync)}: {ex}");
            return Result.Failure("Could not create session");
        }
    }

public async Task<Result> CreateSessionsBulkAsync(int userId, List<TrainingSessionWriteDto> newSessions,
    CancellationToken cancellationToken)
{
    var stringValidationErrors = "";
    var collectionValidatoinErrors = "";
    foreach (var newSession in newSessions)
    {
        var stringValidationResult = Validation.ValidateDtoStrings(newSession);
        if (!stringValidationResult.IsSuccess)
            stringValidationErrors += stringValidationResult.ErrorMessage;

        var collectionValidationResult = Validation.ValidateDtoICollections(newSession);
        if (!collectionValidationResult.IsSuccess)
            collectionValidatoinErrors += collectionValidationResult.ErrorMessage;
    }

    if(stringValidationErrors.Length > 0 || collectionValidatoinErrors.Length >0)
        return Result.Failure($"these errors were encountered whist creating bulk exercises:\n{stringValidationErrors}\n{collectionValidatoinErrors}");
    try
    {
        var user = await _context.Users
            .Include(x => x.ExerciseRecords)
            .ThenInclude(exerciseRecord => exerciseRecord.Exercise)
            .Include(x => x.TrainingSessions)
            .Include(x => x.UserExercises)
            .ThenInclude(userExercise => userExercise.Exercise)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user == null) return Result.Failure("User was not found");

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        var relatedExercisesResult = await GetRelatedExercisesDictionary(
            newSessions.SelectMany(x => x.ExerciseRecords)
                .Select(x => x.ExerciseName)
                .Distinct()
                .ToList(),
            cancellationToken);

        if (!relatedExercisesResult.IsSuccess)
            return Result.Failure(relatedExercisesResult.ErrorMessage!);
        var relatedExercises = relatedExercisesResult.Value;
        
        foreach (var sessionDto in newSessions)
        {
            var newSessionDate = Utils.ParseDate(sessionDto.CreatedAt) ?? DateTime.Now;
            var exerciseRecords = CreateExerciseRecords(sessionDto.ExerciseRecords
                .ToList(), relatedExercises, newSessionDate);

            var newSession = CreateTrainingSession(user, sessionDto, newSessionDate, exerciseRecords);


            await _context.TrainingSessions.AddAsync(newSession, cancellationToken);
            UpdateUserExercises(user, exerciseRecords, newSessionDate);
        }

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success("Bulk training sessions created successfully!");
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error has occurred in {nameof(CreateSessionsBulkAsync)}: {ex}");
        return Result.Failure("Could not create sessions");
    }
}

    
    private void UpdateUserExercises(User user, List<ExerciseRecord> exerciseRecords, DateTime newSessionDate)
    {
        foreach (var exerciseRecord in exerciseRecords)
        {
            var existingUserExercise =
                user.UserExercises.FirstOrDefault(x => x.Exercise.Name == exerciseRecord.Exercise.Name);
            if (existingUserExercise is null)
            {
                user.UserExercises.Add(
                    new UserExercise()
                    {
                        Exercise = exerciseRecord.Exercise,
                        User = user,
                        CreatedAt = newSessionDate,
                        LastUsedWeightKg = exerciseRecord.WeightUsedKg ?? 0,
                        AverageWeight = exerciseRecord.WeightUsedKg ?? 0,
                        BestWeight = exerciseRecord.WeightUsedKg ?? 0,
                        AverageHeartRate = exerciseRecord.HeartRateAvg ?? 0,
                        AverageTimerInSeconds = exerciseRecord.TimerInSeconds ?? 0,
                        AverageDistance = exerciseRecord.DistanceInMeters ?? 0,
                        AverageKCalBurned = exerciseRecord.KcalBurned ?? 0,
                        AverageSpeed = exerciseRecord.Speed ?? 0,
                        AverageRateOfPreceivedExertion = exerciseRecord.RateOfPerceivedExertion ?? 0,
                        UseCount = 1
                    });
            }
            else
            {
                var userExerciseRecords = user.ExerciseRecords
                    .Where(x => x.Exercise.Name == exerciseRecord.Exercise.Name);

                existingUserExercise.UseCount++;
                existingUserExercise.AverageWeight = userExerciseRecords.Average(x => x.WeightUsedKg ?? 0);
                existingUserExercise.BestWeight = userExerciseRecords.Max(x => x.WeightUsedKg ?? 0);
                existingUserExercise.LastUsedWeightKg = userExerciseRecords.OrderBy(x => x.CreatedAt).First().WeightUsedKg ?? 0;
                existingUserExercise.AverageDistance = userExerciseRecords.Average(x => x.DistanceInMeters ?? 0);
                existingUserExercise.AverageSpeed = userExerciseRecords.Average(x => x.Speed ?? 0);
                existingUserExercise.AverageHeartRate = userExerciseRecords.Average(x => x.HeartRateAvg ?? 0);
                existingUserExercise.AverageKCalBurned = userExerciseRecords.Average(x => x.KcalBurned ?? 0);
                existingUserExercise.AverageTimerInSeconds = userExerciseRecords.Average(x => x.TimerInSeconds ?? 0);
                existingUserExercise.AverageRateOfPreceivedExertion = userExerciseRecords.Average(x => x.RateOfPerceivedExertion ?? 0);
            }

            user.ExerciseRecords.Add(exerciseRecord);
        }
    }

    private TrainingSession CreateTrainingSession(User user, TrainingSessionWriteDto sessionDto,
        DateTime newSessionDate, List<ExerciseRecord> exerciseRecords)
    {
        return new TrainingSession()
        {
            Calories = (sessionDto.TotalCaloriesBurned ?? 0)
                       + sessionDto.ExerciseRecords.Sum(x => x.KcalBurned),
            Feeling = sessionDto.Feeling,
            Notes = sessionDto.Notes,
            Mood = sessionDto.Mood,
            DurationInSeconds =
                Utils.DurationSecondsFromMinutes(sessionDto.DurationInMinutes) ?? 0
                + sessionDto.ExerciseRecords.Sum(x => x.TimerInSeconds ?? 1),
            CreatedAt = newSessionDate,
            ExerciseRecords = exerciseRecords,
            User = user,
            TotalRepetitions = sessionDto.ExerciseRecords.Sum(x => x.Repetitions),
            TotalKgMoved = sessionDto.ExerciseRecords.Sum(x => x.WeightUsedKg),
            AverageRateOfPreceivedExertion = sessionDto.ExerciseRecords.Average(x => x.RateOfPerceivedExertion)
        };
    }
    
    private List<ExerciseRecord> CreateExerciseRecords(List<ExerciseRecordWriteDto> sessionDtoExerciseRecords,
        Dictionary<string, Exercise> relatedExercises,
        DateTime newSessionDate)
    {
        return sessionDtoExerciseRecords.Select(exerciseDto => new ExerciseRecord()
        {
            Exercise = relatedExercises![exerciseDto.ExerciseName],
            CreatedAt = newSessionDate,
            Notes = exerciseDto.Notes,
            KcalBurned = exerciseDto.KcalBurned,
            WeightUsedKg = exerciseDto.WeightUsedKg,
            Incline = exerciseDto.Incline,
            Speed = exerciseDto.Speed,
            TimerInSeconds = exerciseDto.TimerInSeconds,
            DistanceInMeters = exerciseDto.DistanceInMeters,
            RestInSeconds = exerciseDto.RestInSeconds,
            HeartRateAvg = exerciseDto.HeartRateAvg,
            Repetitions = exerciseDto.Repetitions,
            RateOfPerceivedExertion = exerciseDto.RateOfPerceivedExertion,
            Mood = exerciseDto.Mood,
            
        }).ToList();
    }

    private async Task<Result<Dictionary<string, Exercise>>> GetRelatedExercisesDictionary(List<string> exerciseNames,
        CancellationToken cancellationToken)
    {
        var result = await _context.Exercises
            .Where(x => exerciseNames
                .Contains(x.Name))
            .ToDictionaryAsync(x => x.Name, x => x, cancellationToken);
        if (result.Count != exerciseNames.Count)
        {
            var missingExercises = exerciseNames
                .Where(x => !result.Keys.Contains(x))
                .Distinct(); // don't want 'barbell static lunge' more than once.
            var errorMessage = string.Join("\n ", missingExercises);
            _logger.LogError($"[ERROR]: these exercises were not in the database:\n{errorMessage}");
            return Result<Dictionary<string, Exercise>>.Failure(
                $"Some exercises are not in the database: {errorMessage}");
        }

        return Result<Dictionary<string, Exercise>>.Success(result);
    }
}