using AutoMapper;

using DataLibrary.Context;
using DataLibrary.Core;
using DataLibrary.Dtos;
using DataLibrary.Helpers;
using DataLibrary.Models;

using Microsoft.EntityFrameworkCore;

namespace DataLibrary.Services;
public class TrainingSessionService : ITrainingSessionService
{
    private readonly SqliteContext _context;
    private readonly IMapper _mapper;

    public TrainingSessionService(SqliteContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
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

            IQueryable<TrainingSession> query = _context.TrainingSessions
                .AsNoTracking()
                    .Include(x => x.TrainingSessionExerciseRecords)
                        .ThenInclude(w => w.ExerciseRecord)
                        .ThenInclude(w => w.Exercise)
                            .ThenInclude(e => e.ExerciseMuscles)
                            .ThenInclude(em => em.Muscle)
                    .Include(r => r.TrainingTypes);

            if (start.HasValue && end.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= start.Value && x.CreatedAt <= end.Value);
            }

            List<TrainingSession> sessions = await query.ToListAsync(cancellationToken);
            return Result<List<TrainingSessionReadDto>>.Success(_mapper.Map<List<TrainingSessionReadDto>>(sessions));
        }
        catch (Exception ex)
        {
            return Result<List<TrainingSessionReadDto>>.Failure($"Error retrieving training sessions: {ex.Message}", ex);
        }
    }


    public async Task<Result<bool>> CreateSessionAsync(TrainingSessionWriteDto newSession, CancellationToken cancellationToken)
    {
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            List<string> normalizedExerciseNames = Utils.NormalizeStringList(newSession.ExerciseRecords.Select(x => x.ExerciseName).ToList());
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
                Calories = newSession.Calories,
                Notes = newSession.Notes,
                Mood = newSession.Mood,
                CreatedAt = sessionCreatedAt,
                TrainingTypes = relatedTrainingTypes.ToList(),
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
                            Exercise = relatedExercises
                                .FirstOrDefault(e => Utils.NormalizeString(e.Name) == Utils.NormalizeString(x.ExerciseName)),
                            CreatedAt = sessionCreatedAt
                        },
                        LastWeightUsedKg = x.WeightUsedKg,
                        CreatedAt = sessionCreatedAt
                    })
                    .ToList(),
            };

            await _context.TrainingSessions.AddAsync(newTrainingSession, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<bool>.Failure($"Error Creating Session: {ex.Message}", ex);
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
    private async Task<List<Exercise>> GetRelatedExercises(List<string> normalizedExerciseNames, CancellationToken cancellationToken)
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
            TrainingSession? trainingSession = await _context.TrainingSessions
                .Include(ts => ts.TrainingSessionExerciseRecords)
                    .ThenInclude(e => e.ExerciseRecord)
                .Include(ts => ts.TrainingTypes)
                .FirstOrDefaultAsync(ts => ts.Id == sessionId, cancellationToken);

            if (trainingSession == null)
                return Result<bool>.Failure("Training session not found.");

            List<string> normalizedExerciseNames = Utils.NormalizeStringList(
                trainingSession.TrainingSessionExerciseRecords
                    .Select(s => s.ExerciseRecord!.Exercise!.Name).ToList()
                    );

            List<Exercise> relatedExercises = await GetRelatedExercises(normalizedExerciseNames, cancellationToken);

            List<TrainingType> relatedTypes = relatedExercises
                .SelectMany(x => x.TrainingTypes)
                .Distinct()
                .ToList();

            // clean old records
            foreach (TrainingSessionExerciseRecord x in trainingSession.TrainingSessionExerciseRecords)
                _context.Remove(x.ExerciseRecord);

            foreach (TrainingType? type in trainingSession.TrainingTypes)
            {
                trainingSession.TrainingTypes.Remove(type);
            }


            // create the date
            DateTime? updatedCreatedAt = Utils.ParseDate(updateDto.CreatedAt);


            _mapper.Map(updateDto, trainingSession);
            trainingSession.TrainingTypes = relatedTypes;
            trainingSession.TrainingSessionExerciseRecords = updateDto.ExerciseRecords
                .Select(x => new TrainingSessionExerciseRecord
                {
                    ExerciseRecord = new ExerciseRecord
                    {
                        CreatedAt = updatedCreatedAt,
                        DistanceInMeters = x.DistanceInMeters, // do it from utils
                        Exercise = relatedExercises
                            .FirstOrDefault(relatedExercise => Utils.NormalizeString(relatedExercise.Name) == Utils.NormalizeString(x.ExerciseName)),
                        Notes = x.Notes,
                        Repetitions = x.Repetitions,
                        TimerInSeconds = Utils.DurationSecondsFromMinutes(x.TimerInSeconds),
                        WeightUsedKg = x.WeightUsedKg
                    }
                })
                .ToList();


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
