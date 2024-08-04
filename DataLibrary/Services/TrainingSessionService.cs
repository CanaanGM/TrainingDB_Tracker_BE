using AutoMapper;

using DataLibrary.Context;
using DataLibrary.Core;
using DataLibrary.Dtos;
using DataLibrary.Helpers;
using DataLibrary.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataLibrary.Services;

public interface ITrainingSessionService
{
    Task<Result<int>> CreateSessionAsync(TrainingSessionWriteDto newSession, CancellationToken cancellationToken);

    Task<Result<bool>> CreateBulkSessionsAsync(List<TrainingSessionWriteDto> newSessions,
        CancellationToken cancellationToken);
    Task<Result<bool>> DeleteSessionAsync(int sessionId, CancellationToken cancellationToken);
    Task<Result<List<TrainingSessionReadDto>>> GetTrainingSessionsAsync(string? startDate, string? endDate, CancellationToken cancellationToken);
    Task<Result<bool>> UpdateSessionAsync(int sessionId, TrainingSessionWriteDto updateDto, CancellationToken cancellationToken);
}

public class TrainingSessionService : ITrainingSessionService
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

    // this sould be by a date range ; get me the sessions from 4-7-2024 to 5-2-2024.
    // no pagination, this needs to be all at once
    // export to something else ? ?
    public async Task<Result<List<TrainingSessionReadDto>>> GetTrainingSessionsAsync(
        string? startDate,
        string? endDate,
        CancellationToken cancellationToken)
    {
        try
        {
            DateTime? start = Utils.ParseDate(startDate);
            DateTime? end = Utils.ParseDate(endDate);
            _logger.LogInformation($"Parsed dates - Start: {start}, End: {end}");

            IQueryable<TrainingSession> query = _context.TrainingSessions
                .AsNoTracking()
                .Include(x => x.TrainingSessionExerciseRecords)
                .ThenInclude(w => w.ExerciseRecord)
                .ThenInclude(w => w.UserExercise)
                .ThenInclude(e => e.Exercise)
                .ThenInclude(e => e.TrainingTypes);


            if (start.HasValue)
            {
                _logger.LogInformation($"Applying start date filter: {start.Value}");
                query = query.Where(x => x.CreatedAt >= start.Value);
            }

            if (end.HasValue)
            {
                _logger.LogInformation($"Applying end date filter: {end.Value}");
                query = query.Where(x => x.CreatedAt <= end.Value);
            }



            List<TrainingSession> sessions = await query.ToListAsync(cancellationToken);
            return Result<List<TrainingSessionReadDto>>.Success(_mapper.Map<List<TrainingSessionReadDto>>(sessions));
        }
        catch (Exception ex)
        {
            return Result<List<TrainingSessionReadDto>>.Failure($"Error retrieving training sessions: {ex.Message}", ex);
        }
    }


    public async Task<Result<int>> CreateSessionAsync(TrainingSessionWriteDto newSession, CancellationToken cancellationToken)
    {
        // get the user ID
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            List<string> normalizedExerciseNames = Utils.NormalizeStringList(
                newSession.ExerciseRecords
                    .Select(x => x.ExerciseName)
                    .Distinct()
                    .ToList()
                );
            List<Exercise> relatedExercises = await GetRelatedExercises(normalizedExerciseNames, cancellationToken);

            List<TrainingType> relatedTrainingTypes = relatedExercises
                .SelectMany(x =>
                {
                    return x.TrainingTypes;
                })
                .Distinct()
                .ToList();

            DateTime sessionCreatedAt = Utils.ParseDate(newSession.CreatedAt) ?? DateTime.UtcNow;
            TrainingSession newTrainingSession = new TrainingSession()
            {
                DurationInSeconds = Utils.DurationSecondsFromMinutes(newSession.DurationInMinutes),
                TotalCaloriesBurned = newSession.TotalCaloriesBurned,
                Notes = newSession.Notes,
                Mood = newSession.Mood,
                CreatedAt = sessionCreatedAt,
                TrainingSessionExerciseRecords = newSession.ExerciseRecords
                    .Select(x => new TrainingSessionExerciseRecord
                    {
                        ExerciseRecord = new ExerciseRecord
                        {
                            Repetitions = x.Repetitions,
                            TimerInSeconds = x.TimerInSeconds,
                            DistanceInMeters = x.DistanceInMeters,
                            WeightUsedKg = x.WeightUsedKg,
                            Notes = x.Notes,
                            UserExercise = new UserExercise()
                            {
                                Exercise = relatedExercises
                                    .FirstOrDefault(e => Utils.NormalizeString(e.Name) == Utils.NormalizeString(x.ExerciseName))
                            },
                            CreatedAt = sessionCreatedAt,
                            
                            RateOfPerceivedExertion =x.RateOfPerceivedExertion,
                            Incline =x.Incline,
                            Speed =x.Speed,
                            KcalBurned =x.KcalBurned,
                            HeartRateAvg =x.HeartRateAvg,
                            RestInSeconds =x.RestInSeconds
                        },
                        LastWeightUsedKg = x.WeightUsedKg,
                        CreatedAt = sessionCreatedAt
                    })
                    .ToList(),
            };

            await _context.TrainingSessions.AddAsync(newTrainingSession, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<int>.Success(newTrainingSession.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<int>.Failure($"Error Creating Session: {ex.Message}", ex);
        }
    }

  public async Task<Result<bool>> CreateBulkSessionsAsync(List<TrainingSessionWriteDto> newSessions, CancellationToken cancellationToken)
{
    using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    try
    {
        if ( newSessions.Count == 0)
        {
            return Result<bool>.Failure("Error creating bulk sessions: The input list is empty.");
        }
        
        // this is important, cause let's say you have 300 in the db, and u want to create 60 but 13 are duplicate,
        // you would want to know which are duplicated without goin to the db, dammit! 
        var exerciseNamesInTheDatabase = await _context.Exercises
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);
        
        var exerciseNamesFromDto = newSessions.SelectMany(x => x.ExerciseRecords)
            .Select(s => Utils.NormalizeString(s.ExerciseName))
            .ToList();
        
        var nonExistingExercises = exerciseNamesFromDto .Where(x => !exerciseNamesInTheDatabase.Contains(x)).ToList();
        if (nonExistingExercises.Any())
        {
            // maybe draw them for cool effect ? 
            return Result<bool>.Failure($"these exercises are not in the database, create them ? {string.Join(", ", nonExistingExercises)}");
        }

        var exerciseNames = newSessions
            .SelectMany(s => s.ExerciseRecords.Select(er => Utils.NormalizeString(er.ExerciseName)))
            .Distinct()
            .ToList();

        var exercises = await _context.Exercises
            .Include(e => e.TrainingTypes)
            .Where(e => exerciseNames.Contains(e.Name))
            .ToListAsync(cancellationToken);

        if (exerciseNames.Count != exercises.Count)
        {
            return Result<bool>.Failure("Error creating bulk sessions: one or more exercises could not be found");
        }

        var sessions = new List<TrainingSession>();

        foreach (var sessionDto in newSessions)
        {
            if (sessionDto.ExerciseRecords == null || !sessionDto.ExerciseRecords.Any())
            {
                return Result<bool>.Failure("Error creating bulk sessions: No exercise records found in one or more sessions.");
            }

            var relatedExercises = exercises.Where(e =>
                sessionDto.ExerciseRecords.Select(er => Utils.NormalizeString(er.ExerciseName)).Contains(e.Name)).ToList();

            var relatedTrainingTypes = relatedExercises
                .SelectMany(e => e.TrainingTypes)
                .Distinct()
                .ToList();

            var sessionCreatedAt = Utils.ParseDate(sessionDto.CreatedAt) ?? DateTime.UtcNow;
            var newSession = new TrainingSession
            {
                DurationInSeconds = Utils.DurationSecondsFromMinutes(sessionDto.DurationInMinutes),
                TotalCaloriesBurned = sessionDto.TotalCaloriesBurned,
                Notes = sessionDto.Notes,
                Mood = sessionDto.Mood,
                CreatedAt = sessionCreatedAt,
                TrainingSessionExerciseRecords = sessionDto.ExerciseRecords
                    .Select(er => new TrainingSessionExerciseRecord
                    {
                        ExerciseRecord = new ExerciseRecord
                        {
                            Repetitions = er.Repetitions,
                            TimerInSeconds = er.TimerInSeconds, 
                            DistanceInMeters = er.DistanceInMeters,
                            WeightUsedKg = er.WeightUsedKg,
                            Notes = er.Notes,
                            UserExercise = new UserExercise()
                            {
                                Exercise = relatedExercises.First(e => Utils.NormalizeString(e.Name) == Utils.NormalizeString(er.ExerciseName)) 
                            } ,
                            CreatedAt = sessionCreatedAt,
                            RateOfPerceivedExertion = er.RateOfPerceivedExertion,
                            Incline = er.Incline,
                            Speed = er.Speed,
                            KcalBurned = er.KcalBurned,
                            HeartRateAvg = er.HeartRateAvg,
                            RestInSeconds = er.RestInSeconds
                            
                        },
                        LastWeightUsedKg = er.WeightUsedKg,
                        CreatedAt = sessionCreatedAt
                    })
                    .ToList()
            };

            sessions.Add(newSession);
        }

        await _context.TrainingSessions.AddRangeAsync(sessions, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync(cancellationToken);
        return Result<bool>.Failure("Error creating bulk sessions: " + ex.Message, ex);
    }
}

    /// <summary>
    ///  Gets the related Exercises from the database based on the provided list of names.
    /// </summary>
    /// <param name="normalizedExerciseNames">List of exercise names to get from the database</param>
    /// <param name="cancellationToken">cancelation token</param>
    /// <returns>a List of Exercise </returns>
    /// <exception cref="Exception">if one or more related exercises not found in the database, it will throw an exception</exception>
    // TODO: Proper exception handling.
    public async Task<List<Exercise>> GetRelatedExercises(List<string> normalizedExerciseNames, CancellationToken cancellationToken)
    {
        List<Exercise> relatedExercises = await _context.Exercises
            .Include(x => x.TrainingTypes)
            .Where(x => normalizedExerciseNames.Contains(x.Name))
            .ToListAsync(cancellationToken);

        if (normalizedExerciseNames.Count != relatedExercises.Count)
            throw new Exception("one or more exercises could not be found");
        return relatedExercises;
    }



    // think a bit more about this
    public async Task<Result<bool>> UpdateSessionAsync(int sessionId, TrainingSessionWriteDto updateDto, CancellationToken cancellationToken)
    {
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = _context.Database.BeginTransaction();
        try
        {

            if (updateDto.ExerciseRecords is not null)
                await FullSessionUpdateAsync(sessionId, updateDto, cancellationToken);
            else
                await PartialUpdateAsync(sessionId, updateDto, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<bool>.Failure($"Error updating session: {ex.Message}", ex);
        }
    }

    private async Task PartialUpdateAsync(int sessionId, TrainingSessionWriteDto updateDto, CancellationToken cancellationToken)
    {


    }

    private async Task FullSessionUpdateAsync(int sessionId, TrainingSessionWriteDto updateDto, CancellationToken cancellationToken)
    {
        

    }




    // this simple for now.
    public async Task<Result<bool>> DeleteSessionAsync(int sessionId, CancellationToken cancellationToken)
    {
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            TrainingSession? session = await _context.TrainingSessions
                .Include(x => x.TrainingSessionExerciseRecords)
                    .ThenInclude(e => e.ExerciseRecord)
                .FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken)

                ;
            if (session is null) throw new ArgumentException($"session with the id of {sessionId}, could not be found");

            foreach (TrainingSessionExerciseRecord x in session.TrainingSessionExerciseRecords)
            {
                _context.Remove(x.ExerciseRecord!);
            }
            _context.TrainingSessions.Remove(session);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<bool>.Failure($"Error deleting Session of ID: {sessionId}, error: {ex.Message}", ex);
        }
    }
}
