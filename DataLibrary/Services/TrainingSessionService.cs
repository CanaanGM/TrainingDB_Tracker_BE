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


    public async Task<Result<TrainingSessionReadDto>> GetTrainingSessionByIdAsync(int userId, int trainingSessionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.TrainingSessions)
                .ThenInclude(ts => ts.ExerciseRecords).ThenInclude(exerciseRecord => exerciseRecord.Exercise)
                .ThenInclude(exercise => exercise.TrainingTypes)
                .FirstOrDefaultAsync(u => u.Id == userId && u.TrainingSessions.Any(ts => ts.Id == trainingSessionId),
                    cancellationToken);

            if (user == null)
                return Result<TrainingSessionReadDto>.Failure("User or training session not found");

            var trainingSession = user.TrainingSessions.First(ts => ts.Id == trainingSessionId);
            var trainingSessionDto = new TrainingSessionReadDto()
            {
                CreatedAt = trainingSession.CreatedAt,
                ExerciseRecords = trainingSession.ExerciseRecords.Select(exerciseRecord => new ExerciseRecordReadDto()
                {
                    ExerciseName = exerciseRecord.Exercise.Name,
                    Repetitions = exerciseRecord.Repetitions,
                    RateOfPerceivedExertion = exerciseRecord.RateOfPerceivedExertion,
                    Mood = exerciseRecord.Mood,
                    CreatedAt = exerciseRecord.CreatedAt,
                    WeightUsedKg = exerciseRecord.WeightUsedKg,
                    Incline = exerciseRecord.Incline,
                    Speed = exerciseRecord.Speed,
                    TimerInSeconds = exerciseRecord.TimerInSeconds,
                    DistanceInMeters = exerciseRecord.DistanceInMeters,
                    RestInSeconds = exerciseRecord.RestInSeconds,
                    HeartRateAvg = exerciseRecord.HeartRateAvg,
                    Notes = exerciseRecord.Notes,
                    KcalBurned = exerciseRecord.KcalBurned,
                    Id = exerciseRecord.Id
                }).ToList(),
                DurationInMinutes = (int) Utils.DurationMinutesFromSeconds( trainingSession.DurationInSeconds),
                TotalCaloriesBurned = trainingSession.Calories,
                Mood = trainingSession.Mood,
                Notes = trainingSession.Notes,
                TotalRepetitions = trainingSession.TotalRepetitions,
                Id = trainingSession.Id,
                TotalKgMoved = trainingSession.TotalKgMoved,
                AverageRateOfPreceivedExertion = trainingSession.AverageRateOfPreceivedExertion,
                TrainingTypes = trainingSession.ExerciseRecords.SelectMany(x =>
                    x.Exercise.TrainingTypes.Select(x => new TrainingTypeReadDto() { Name = x.Name })).ToList()
            };

            return Result<TrainingSessionReadDto>.Success(trainingSessionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error occurred in {nameof(GetTrainingSessionByIdAsync)}: {ex}");
            return Result<TrainingSessionReadDto>.Failure("Could not retrieve session", ex);
        }
    }

    public async Task<Result<PaginatedList<TrainingSessionReadDto>>> GetPaginatedTrainingSessionsAsync(
    int userId, int pageNumber, int pageSize, CancellationToken cancellationToken)
{
    try
    {
        var user = await _context.Users
            .Include(u => u.TrainingSessions)
            .ThenInclude(ts => ts.ExerciseRecords)
            .ThenInclude(exerciseRecord => exerciseRecord.Exercise)
            .ThenInclude(exercise => exercise.TrainingTypes)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            return Result<PaginatedList<TrainingSessionReadDto>>.Failure("User not found");

        var totalSessionsCount = user.TrainingSessions.Count();

        var sessionsQuery = user.TrainingSessions
            .OrderByDescending(ts => ts.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var sessionDtos = sessionsQuery.Select(trainingSession => new TrainingSessionReadDto
        {
            Id = trainingSession.Id,
            CreatedAt = trainingSession.CreatedAt,
            DurationInMinutes = (int) Utils.DurationMinutesFromSeconds( trainingSession.DurationInSeconds),
            TotalCaloriesBurned = trainingSession.Calories,
            Mood = trainingSession.Mood,
            Notes = trainingSession.Notes,
            TotalRepetitions = trainingSession.TotalRepetitions,
            TotalKgMoved = trainingSession.TotalKgMoved,
            AverageRateOfPreceivedExertion = trainingSession.AverageRateOfPreceivedExertion,
            TrainingTypes = trainingSession.ExerciseRecords
                .SelectMany(x => x.Exercise.TrainingTypes.Select(tt => new TrainingTypeReadDto { Name = tt.Name }))
                .ToList(),
            ExerciseRecords = trainingSession.ExerciseRecords.Select(exerciseRecord => new ExerciseRecordReadDto
            {
                ExerciseName = exerciseRecord.Exercise.Name,
                Repetitions = exerciseRecord.Repetitions,
                RateOfPerceivedExertion = exerciseRecord.RateOfPerceivedExertion,
                Mood = exerciseRecord.Mood,
                CreatedAt = exerciseRecord.CreatedAt,
                WeightUsedKg = exerciseRecord.WeightUsedKg,
                Incline = exerciseRecord.Incline,
                Speed = exerciseRecord.Speed,
                TimerInSeconds = exerciseRecord.TimerInSeconds,
                DistanceInMeters = exerciseRecord.DistanceInMeters,
                RestInSeconds = exerciseRecord.RestInSeconds,
                HeartRateAvg = exerciseRecord.HeartRateAvg,
                Notes = exerciseRecord.Notes,
                KcalBurned = exerciseRecord.KcalBurned,
                Id = exerciseRecord.Id
            }).ToList()
        }).ToList();

        var paginationMetadata = new PaginationMetadata
        {
            TotalCount = totalSessionsCount,
            TotalPages = (int)Math.Ceiling(totalSessionsCount / (double)pageSize),
            CurrentPage = pageNumber,
            PageSize = pageSize
        };

        var paginatedSessions = new PaginatedList<TrainingSessionReadDto>
        {
            Items = sessionDtos,
            Metadata = paginationMetadata
        };

        return Result<PaginatedList<TrainingSessionReadDto>>.Success(paginatedSessions);
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error occurred in {nameof(GetPaginatedTrainingSessionsAsync)}: {ex}");
        return Result<PaginatedList<TrainingSessionReadDto>>.Failure("Could not retrieve paginated sessions", ex);
    }
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
            var user = await GetUser(userId, cancellationToken);
            if (user is null) return Result.Failure("User was not found");

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var relatedExercisesResult =
                await GetRelatedExercisesDictionaryAsync(
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

            CreateOrUpdateUserExercises(user, exerciseRecords, newSessionDate);

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

        if (stringValidationErrors.Length > 0 || collectionValidatoinErrors.Length > 0)
            return Result.Failure(
                $"these errors were encountered whist creating bulk exercises:\n{stringValidationErrors}\n{collectionValidatoinErrors}");
        try
        {
            var user = await GetUser(userId, cancellationToken);
            if (user == null) return Result.Failure("User was not found");

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var relatedExercisesResult = await GetRelatedExercisesDictionaryAsync(
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
                CreateOrUpdateUserExercises(user, exerciseRecords, newSessionDate);
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


    //TODO: to cleanly update a session, delete the old one then re-add it.
    public async Task<Result> UpdateTrainingSession(int userId, int trainingSessionId,
        TrainingSessionWriteDto updateSessionDto,
        CancellationToken cancellationToken)
    {
        var stringValidationResult = Validation.ValidateDtoStrings(updateSessionDto);
        if (!stringValidationResult.IsSuccess)
            return Result.Failure(stringValidationResult.ErrorMessage);

        var collectionValidationResult = Validation.ValidateDtoICollections(updateSessionDto);
        if (!collectionValidationResult.IsSuccess)
            return Result.Failure(collectionValidationResult.ErrorMessage);
        try
        {
            var user = await GetUser(userId, cancellationToken);
            if (user == null) return Result.Failure("User was not found");

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            var trainingSessionToUpdate = user.TrainingSessions
                .FirstOrDefault(x => x.Id == trainingSessionId);
            if (trainingSessionToUpdate is null)
                return Result.Failure($"Training Session of id:`{trainingSessionId}` was not found.");


            var oldExercises = trainingSessionToUpdate.ExerciseRecords.ToList();
            ModifyUsersOldExercises(oldExercises, user, trainingSessionToUpdate);

            _mapper.Map(updateSessionDto, trainingSessionToUpdate);

            var relatedExercisesResult = await GetRelatedExercisesDictionaryAsync(
                updateSessionDto.ExerciseRecords
                    .Select(x => x.ExerciseName).Distinct().ToList(), cancellationToken);

            if (!relatedExercisesResult.IsSuccess)
                return Result.Failure(relatedExercisesResult.ErrorMessage!);

            var newSessionDate = Utils.ParseDate(updateSessionDto.CreatedAt) ?? DateTime.Now;
            var newExerciseRecords =
                CreateExerciseRecords(updateSessionDto.ExerciseRecords.ToList(), relatedExercisesResult.Value!,
                    newSessionDate);
            trainingSessionToUpdate.ExerciseRecords = newExerciseRecords;


            CalculateTrainingSessionMetadata(trainingSessionToUpdate);

            CreateOrUpdateUserExercises(user, newExerciseRecords, newSessionDate);
            user.ExerciseRecords.ToList().AddRange(newExerciseRecords);

            _context.Update(trainingSessionToUpdate);
            _context.Update(user);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result.Success($"Updated session of id:'{trainingSessionId}' successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error has occurred in {nameof(UpdateTrainingSession)}: {ex}");
            return Result.Failure("Could not update session");
        }
    }

    public async Task<Result> DeleteTrainingSessionAsync(int userId, int trainingSessionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await GetUser(userId, cancellationToken);
            if (user == null)
                return Result.Failure("User was not found");

            var trainingSessionToDelete = user.TrainingSessions.FirstOrDefault(ts => ts.Id == trainingSessionId);
            if (trainingSessionToDelete == null)
                return Result.Failure($"Training Session of id:`{trainingSessionId}` was not found.");

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            foreach (var oldExerciseRecord in trainingSessionToDelete.ExerciseRecords.ToList())
            {
                ModifyUserExercisesForRemovedRecord(user, oldExerciseRecord);

                trainingSessionToDelete.ExerciseRecords.Remove(oldExerciseRecord);
                user.ExerciseRecords.Remove(oldExerciseRecord);
                _context.ExerciseRecords.Remove(oldExerciseRecord);
            }

            user.TrainingSessions.Remove(trainingSessionToDelete);
            _context.TrainingSessions.Remove(trainingSessionToDelete);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success($"Deleted session of id:'{trainingSessionId}' successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error occurred in {nameof(DeleteTrainingSessionAsync)}: {ex}");
            return Result.Failure("Could not delete session");
        }
    }

    private void ModifyUserExercisesForRemovedRecord(User user, ExerciseRecord oldExerciseRecord)
    {
        var userExercise = user.UserExercises.FirstOrDefault(ue => ue.ExerciseId == oldExerciseRecord.ExerciseId);
        if (userExercise != null)
        {
            var remainingRecords = user.ExerciseRecords
                .Where(er => er.ExerciseId == userExercise.ExerciseId && er.Id != oldExerciseRecord.Id).ToList();
            if (remainingRecords.Any())
            {
                CalculateUserExerciseMetadata(userExercise, remainingRecords, false);
                _context.UserExercises.Update(userExercise);
            }
            else
            {
                user.UserExercises.Remove(userExercise);
                _context.UserExercises.Remove(userExercise);
            }
        }
    }

    private static void ModifyUsersOldExercises(List<ExerciseRecord> oldExercises, User user,
        TrainingSession trainingSessionToUpdate)
    {
        foreach (var oldExercise in oldExercises)
        {
            var userExercise = user.UserExercises.First(x => x.Exercise.Name == oldExercise.Exercise.Name);
            if (userExercise.UseCount == 1)
            {
                user.UserExercises.Remove(userExercise);
            }
            else
            {
                var updatedUserExerciseRecords =
                    user.ExerciseRecords.Where(x => x.Exercise.Name == oldExercise.Exercise.Name).ToList();
                userExercise.UseCount--;
                userExercise.AverageWeight = updatedUserExerciseRecords.Average(x => x.WeightUsedKg ?? 0);
                userExercise.BestWeight = updatedUserExerciseRecords.Max(x => x.WeightUsedKg ?? 0);
                userExercise.LastUsedWeightKg =
                    updatedUserExerciseRecords.OrderBy(x => x.CreatedAt).First().WeightUsedKg ?? 0;
                userExercise.AverageDistance = updatedUserExerciseRecords.Average(x => x.DistanceInMeters ?? 0);
                userExercise.AverageSpeed = updatedUserExerciseRecords.Average(x => x.Speed ?? 0);
                userExercise.AverageHeartRate = updatedUserExerciseRecords.Average(x => x.HeartRateAvg ?? 0);
                userExercise.AverageKCalBurned = updatedUserExerciseRecords.Average(x => x.KcalBurned ?? 0);
                userExercise.AverageTimerInSeconds = updatedUserExerciseRecords.Average(x => x.TimerInSeconds ?? 0);
                userExercise.AverageRateOfPreceivedExertion =
                    updatedUserExerciseRecords.Average(x => x.RateOfPerceivedExertion ?? 0);
            }

            trainingSessionToUpdate.ExerciseRecords.Remove(oldExercise);
            user.ExerciseRecords.Remove(oldExercise);
        }
    }

    private static void CalculateTrainingSessionMetadata(TrainingSession trainingSessionToUpdate)
    {
        trainingSessionToUpdate.TotalRepetitions = trainingSessionToUpdate.ExerciseRecords.Sum(x => x.Repetitions);
        trainingSessionToUpdate.Calories += trainingSessionToUpdate.ExerciseRecords.Sum(x => x.KcalBurned ?? 1);
        trainingSessionToUpdate.DurationInSeconds +=
            trainingSessionToUpdate.ExerciseRecords.Sum(x => x.TimerInSeconds ?? 1);
        trainingSessionToUpdate.TotalKgMoved += trainingSessionToUpdate.ExerciseRecords.Sum(x => x.WeightUsedKg ?? 0);
        trainingSessionToUpdate.AverageRateOfPreceivedExertion +=
            trainingSessionToUpdate.ExerciseRecords.Average(x => x.RateOfPerceivedExertion ?? 1);
    }

    private static void CalculateUserExerciseMetadata(UserExercise userExercise,
        List<ExerciseRecord> updatedUserExerciseRecords, bool increment = true)
    {
        if (!increment)
            userExercise.UseCount--;
        userExercise.UseCount++;
        userExercise.AverageWeight = updatedUserExerciseRecords.Average(x => x.WeightUsedKg ?? 0);
        userExercise.BestWeight = updatedUserExerciseRecords.Max(x => x.WeightUsedKg ?? 0);
        userExercise.LastUsedWeightKg =
            updatedUserExerciseRecords.OrderBy(x => x.CreatedAt).First().WeightUsedKg ?? 0;
        userExercise.AverageDistance = updatedUserExerciseRecords.Average(x => x.DistanceInMeters ?? 0);
        userExercise.AverageSpeed = updatedUserExerciseRecords.Average(x => x.Speed ?? 0);
        userExercise.AverageHeartRate = updatedUserExerciseRecords.Average(x => x.HeartRateAvg ?? 0);
        userExercise.AverageKCalBurned = updatedUserExerciseRecords.Average(x => x.KcalBurned ?? 0);
        userExercise.AverageTimerInSeconds = updatedUserExerciseRecords.Average(x => x.TimerInSeconds ?? 0);
        userExercise.AverageRateOfPreceivedExertion =
            updatedUserExerciseRecords.Average(x => x.RateOfPerceivedExertion ?? 0);
    }


    private async Task<User?> GetUser(int userId, CancellationToken cancellationToken)
    {
        return await _context.Users
            .Include(x => x.ExerciseRecords)
            .ThenInclude(exerciseRecord => exerciseRecord.Exercise)
            .Include(x => x.TrainingSessions)
            .ThenInclude(x => x.ExerciseRecords)
            .Include(x => x.UserExercises)
            .ThenInclude(userExercise => userExercise.Exercise)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    private void CreateOrUpdateUserExercises(User user, List<ExerciseRecord> exerciseRecords, DateTime newSessionDate)
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

                CalculateUserExerciseMetadata(existingUserExercise, userExerciseRecords.ToList());
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

    private async Task<Result<Dictionary<string, Exercise>>> GetRelatedExercisesDictionaryAsync(
        List<string> exerciseNames,
        CancellationToken cancellationToken)
    {
        var result = await _context.Exercises
            .Where(x => exerciseNames
                .Contains(x.Name))
            .ToDictionaryAsync(x => x.Name, x => x, cancellationToken);
        
        if (result.Count == exerciseNames.Count) 
            return Result<Dictionary<string, Exercise>>.Success(result);
        
        var missingExercises = exerciseNames
            .Where(x => !result.Keys.Contains(x))
            .Distinct(); // don't want 'barbell static lunge' more than once.
        var errorMessage = string.Join("\n ", missingExercises);
        _logger.LogError($"[ERROR]: these exercises were not in the database:\n{errorMessage}");
        return Result<Dictionary<string, Exercise>>.Failure(
            $"Some exercises are not in the database:\n {errorMessage}");
   

    }
}