using AutoMapper;
using AutoMapper.QueryableExtensions;

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
        string? startDate
        , string? endDate
        , CancellationToken cancellationToken)
    {
        try
        {

            IQueryable<TrainingSessionReadDto> query = _context.TrainingSessions
                .AsNoTracking()
                .ProjectTo<TrainingSessionReadDto>(_mapper.ConfigurationProvider);

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
                query = query
                .Where(x => x.CreatedAt >= Utils.ParseDate(startDate) && x.CreatedAt <= Utils.ParseDate(endDate));

            List<TrainingSessionReadDto> sessions = await query
                .ToListAsync(cancellationToken);
            return Result<List<TrainingSessionReadDto>>.Success(sessions);
        }
        catch (Exception ex)
        {
            return Result<List<TrainingSessionReadDto>>.Failure(ex.Message, ex);
        }
    }

    public async Task<Result<bool>> CreateSessionAsync(TrainingSessionWriteDto newSession, CancellationToken cancellationToken)
    {
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            List<string> normalizedExerciseNames = newSession.ExerciseRecords
                .Select(x => Utils.NormalizeString(x.ExcerciseName))
                .Distinct()
                .ToList();

            Dictionary<string, Models.Exercise> relatedExercises = await _context.Exercises
                .Where(x => normalizedExerciseNames.Contains(x.Name))
                .ToDictionaryAsync(e => e.Name, e => e, cancellationToken);

            if (normalizedExerciseNames.Count != relatedExercises.Count)
                throw new Exception("one or more exercises could not be found");

            List<string> normailzedTrainingTypeNames = newSession.TrainingTypes
                .Select(x => Utils.NormalizeString(x))
                .Distinct()
                .ToList();
            Dictionary<string, TrainingType> relatedTypes = await _context.TrainingTypes
                .Where(x => normailzedTrainingTypeNames.Contains(x.Name))
                .ToDictionaryAsync(x => x.Name, x => x, cancellationToken);

            if (normailzedTrainingTypeNames.Count != relatedTypes.Count)
                throw new Exception("one or more training types could not be found");

            TrainingSession newTrainingSession = new TrainingSession()
            {
                DurationInSeconds = newSession.DurationInSeconds,
                Calories = newSession.Calories,
                Notes = newSession.Notes,
                Mood = newSession.Mood,
                CreatedAt = Utils.ParseDate(newSession.CreatedAt) ?? DateTime.UtcNow,
                TrainingTypes = relatedTypes.Values.ToList(),
                TrainingSessionExerciseRecords = newSession.ExerciseRecords.Select(x => new TrainingSessionExerciseRecord
                {
                    ExerciseRecord = new ExerciseRecord
                    {
                        Repetitions = x.Repetitions,
                        TimerInSeconds = x.TimerInSeconds,
                        DistanceInMeters = x.DistanceInMeters,
                        WeightUsedKg = x.WeightUsedKg,
                        Notes = x.Notes,
                        Exercise = relatedExercises[x.ExcerciseName]
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

            // Update exercise records
            foreach (ExerciseRecordWriteDto exerciseDto in updateDto.ExerciseRecords)
            {
                TrainingSessionExerciseRecord? exerciseRecord = trainingSession.TrainingSessionExerciseRecords
                    .FirstOrDefault(er => er.ExerciseRecord.Exercise.Name == exerciseDto.ExcerciseName);

                if (exerciseRecord != null)
                {
                    exerciseRecord.ExerciseRecord.Repetitions = exerciseDto.Repetitions;
                    exerciseRecord.ExerciseRecord.TimerInSeconds = exerciseDto.TimerInSeconds;
                    exerciseRecord.ExerciseRecord.DistanceInMeters = exerciseDto.DistanceInMeters;
                    exerciseRecord.ExerciseRecord.WeightUsedKg = exerciseDto.WeightUsedKg;
                    exerciseRecord.ExerciseRecord.Notes = exerciseDto.Notes;
                }
            }

            // Update training types
            // Assumes TrainingTypes are managed by names; may require adjustment based on actual relationship handling
            List<string> currentTypeNames = trainingSession.TrainingTypes.Select(tt => tt.Name).ToList();
            List<string> updatedTypeNames = updateDto.TrainingTypes;

            IQueryable<TrainingType> typesToAdd = _context.TrainingTypes.Where(tt => updatedTypeNames.Contains(tt.Name) && !currentTypeNames.Contains(tt.Name));
            List<TrainingType> typesToRemove = trainingSession.TrainingTypes.Where(tt => !updatedTypeNames.Contains(tt.Name)).ToList();

            foreach (TrainingType? type in typesToAdd)
            {
                trainingSession.TrainingTypes.Add(type);
            }

            foreach (TrainingType? type in typesToRemove)
            {
                trainingSession.TrainingTypes.Remove(type);
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
