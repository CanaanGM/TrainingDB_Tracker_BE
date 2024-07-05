using AutoMapper;

using DataLibrary.Context;
using DataLibrary.Core;
using DataLibrary.Dtos;
using DataLibrary.Helpers;
using DataLibrary.Models;

using Microsoft.EntityFrameworkCore;

namespace DataLibrary.Services;
internal class TrainingSessionService : ITrainingSessionService
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
        // get the exercise, 
        // create the record,
        // populate the last used weight inside the join table
        // get the types from each exercise
        // create the session
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            List<string> normalizedExerciseNames = newSession.ExerciseRecords
                .Select(x => Utils.NormalizeString(x.ExerciseName))
                .Distinct()
                .ToList();

            List<Exercise> relatedExercises = await _context.Exercises
                .Include(x => x.TrainingTypes)
                .Where(x => normalizedExerciseNames.Contains(x.Name))
                .ToListAsync(cancellationToken);

            if (normalizedExerciseNames.Count != relatedExercises.Count)
                throw new Exception("one or more exercises could not be found");

            List<TrainingType> relatedTrainingTypes = relatedExercises
                .SelectMany(x =>
                {
                    return x.TrainingTypes;
                })
                .Distinct()
                .ToList();

            TrainingSession newTrainingSession = new TrainingSession()
            {
                DurationInSeconds = newSession.DurationInSeconds,
                Calories = newSession.Calories,
                Notes = newSession.Notes,
                Mood = newSession.Mood,
                CreatedAt = Utils.ParseDate(newSession.CreatedAt) ?? DateTime.UtcNow,
                TrainingTypes = relatedTrainingTypes.ToList(),
                TrainingSessionExerciseRecords = newSession.ExerciseRecords.Select(x => new TrainingSessionExerciseRecord
                {
                    ExerciseRecord = new ExerciseRecord
                    {
                        Repetitions = x.Repetitions,
                        TimerInSeconds = x.TimerInSeconds,
                        DistanceInMeters = x.DistanceInMeters,
                        WeightUsedKg = x.WeightUsedKg,
                        Notes = x.Notes,
                        Exercise = relatedExercises.FirstOrDefault(e => Utils.NormalizeString(e.Name) == Utils.NormalizeString(x.ExerciseName))
                    },
                    LastWeightUsedKg = x.WeightUsedKg,
                    CreatedAt = DateTime.UtcNow
                }).ToList(),
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

    // think a bit more about this
    public async Task<Result<bool>> UpdateSessionAsync(int sessionId, TrainingSessionWriteDto updateDto, CancellationToken cancellationToken)
    {
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = _context.Database.BeginTransaction();
        try
        {
            TrainingSession? trainingSession = await _context.TrainingSessions
                .Include(ts => ts.TrainingSessionExerciseRecords)
                .Include(ts => ts.TrainingTypes)
                .FirstOrDefaultAsync(ts => ts.Id == sessionId, cancellationToken);

            if (trainingSession == null)
                return Result<bool>.Failure("Training session not found.");

            // Update properties
            trainingSession.DurationInSeconds = updateDto.DurationInSeconds;
            trainingSession.Calories = updateDto.Calories;
            trainingSession.Notes = updateDto.Notes;
            trainingSession.Mood = updateDto.Mood;

            List<TrainingType> newTypes = new();

            // Update exercise records
            foreach (ExerciseRecordWriteDto exerciseDto in updateDto.ExerciseRecords)
            {
                TrainingSessionExerciseRecord? exerciseRecord = trainingSession.TrainingSessionExerciseRecords
                    .FirstOrDefault(er => er.ExerciseRecord.Exercise.Name == exerciseDto.ExerciseName);

                if (exerciseRecord != null)
                {
                    exerciseRecord.ExerciseRecord.Repetitions = exerciseDto.Repetitions;
                    exerciseRecord.ExerciseRecord.TimerInSeconds = exerciseDto.TimerInSeconds;
                    exerciseRecord.ExerciseRecord.DistanceInMeters = exerciseDto.DistanceInMeters;
                    exerciseRecord.ExerciseRecord.WeightUsedKg = exerciseDto.WeightUsedKg;
                    exerciseRecord.ExerciseRecord.Notes = exerciseDto.Notes;
                    newTypes = exerciseRecord.ExerciseRecord.Exercise.TrainingTypes.ToList();

                }
            }

            foreach (TrainingType? type in trainingSession.TrainingTypes)
            {
                trainingSession.TrainingTypes.Remove(type);
            }

            foreach (TrainingType? type in newTypes)
            {
                trainingSession.TrainingTypes.Add(type);
            }

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
            _context.TrainingSessions.Remove(new TrainingSession { Id = sessionId });
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
