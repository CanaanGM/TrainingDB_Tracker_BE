using DataLibrary.Context;
using DataLibrary.Core;
using DataLibrary.Dtos;
using DataLibrary.Helpers;
using DataLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataLibrary.Services;

public class PlanService
{
    private readonly SqliteContext _context;
    private readonly ILogger<PlanService> _logger;

    public PlanService(SqliteContext context, ILogger<PlanService> logger)
    {
        _context = context;
        _logger = logger;
    }


    public async Task<Result<int>> CreateAsync(TrainingPlanWriteDto newPlanDto, CancellationToken cancellationToken)
    {
        if (!ValidateTrainingPlan(newPlanDto, out var validationError))
            return Result<int>.Failure(validationError);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var relatedExercises = await GetRelatedExercises(newPlanDto, cancellationToken);
            var trainingTypes = await GetRelatedTrainingTypes(newPlanDto, cancellationToken);
            var relatedEquipment = await GetRelatedEquipment(newPlanDto, cancellationToken);

            var validationErrors = ValidateEntities(relatedExercises, trainingTypes, relatedEquipment, newPlanDto);
            if (validationErrors.Any())
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<int>.Failure(string.Join("; ", validationErrors));
            }

            var trainingPlan = MapToTrainingPlan(newPlanDto, relatedExercises, trainingTypes, relatedEquipment);

            await _context.TrainingPlans.AddAsync(trainingPlan, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result<int>.Success(trainingPlan.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, $"[ERROR]: failed to create a session in {nameof(CreateAsync)}");
            return Result<int>.Failure($"Failed to create training plan: {ex.Message}", ex);
        }
    }

public async Task<Result<bool>> UpdateAsync(int planId, TrainingPlanWriteDto updateDto, CancellationToken cancellationToken)
{
    await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    try
    {
        var existingPlan = await _context.TrainingPlans
            .Include(tp => tp.Weeks)
                .ThenInclude(tw => tw.Days)
                    .ThenInclude(td => td.Blocks)
                        .ThenInclude(b => b.Exercises)
            .Include(tp => tp.Equipment)
            .Include(tp => tp.TrainingTypes)
            .FirstOrDefaultAsync(tp => tp.Id == planId, cancellationToken);

        if (existingPlan == null)
        {
            return Result<bool>.Failure("Training plan not found.");
        }

        UpdateTrainingPlan(existingPlan, updateDto);
        UpdateRelatedEntities(existingPlan, updateDto);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync(cancellationToken);
        _logger.LogError(ex, $"[ERROR]: failed to update the training plan in {nameof(UpdateAsync)}");
        return Result<bool>.Failure($"Failed to update training plan: {ex.Message}", ex);
    }
}

private void UpdateTrainingPlan(TrainingPlan existingPlan, TrainingPlanWriteDto updateDto)
{
    if (!string.IsNullOrEmpty(updateDto.Name))
    {
        existingPlan.Name = Utils.NormalizeString(updateDto.Name);
    }

    if (!string.IsNullOrEmpty(updateDto.Description))
    {
        existingPlan.Description = updateDto.Description;
    }

    if (!string.IsNullOrEmpty(updateDto.Notes))
    {
        existingPlan.Notes = updateDto.Notes;
    }
}

private void UpdateRelatedEntities(TrainingPlan existingPlan, TrainingPlanWriteDto updateDto)
{
    UpdateEquipment(existingPlan, updateDto);
    UpdateTrainingTypes(existingPlan, updateDto);
    UpdateWeeks(existingPlan, updateDto);
}

private void UpdateEquipment(TrainingPlan existingPlan, TrainingPlanWriteDto updateDto)
{
    if (updateDto.Equipemnt != null && updateDto.Equipemnt.Any())
    {
        var normalizedEquipmentNames = Utils.NormalizeStringList(updateDto.Equipemnt);
        var existingEquipment = _context.Equipment
            .Where(e => normalizedEquipmentNames.Contains(e.Name))
            .ToList();

        existingPlan.Equipment.Clear();
        foreach (var equipment in existingEquipment)
        {
            existingPlan.Equipment.Add(equipment);
        }
    }
}

private void UpdateTrainingTypes(TrainingPlan existingPlan, TrainingPlanWriteDto updateDto)
{
    if (updateDto.TrainingTypes != null && updateDto.TrainingTypes.Any())
    {
        var normalizedTrainingTypes = Utils.NormalizeStringList(updateDto.TrainingTypes);
        var existingTrainingTypes = _context.TrainingTypes
            .Where(tt => normalizedTrainingTypes.Contains(tt.Name))
            .ToList();

        existingPlan.TrainingTypes.Clear();
        foreach (var trainingType in existingTrainingTypes)
        {
            existingPlan.TrainingTypes.Add(trainingType);
        }
    }
}

private void UpdateWeeks(TrainingPlan existingPlan, TrainingPlanWriteDto updateDto)
{
    if (updateDto.Weeks != null && updateDto.Weeks.Any())
    {
        existingPlan.Weeks.Clear();
        foreach (var weekDto in updateDto.Weeks)
        {
            var week = new TrainingWeek
            {
                Name = Utils.NormalizeString(weekDto.Name),
                OrderNumber = weekDto.OrderNumber,
                Days = weekDto.Days.Select(dayDto => new TrainingDay
                {
                    Name = Utils.NormalizeString(dayDto.Name),
                    Notes = dayDto.Notes,
                    OrderNumber = dayDto.OrderNumber,
                    Blocks = dayDto.Blocks.Select(blockDto => new Block
                    {
                        Name = Utils.NormalizeString(blockDto.Name),
                        Sets = blockDto.Sets,
                        RestInSeconds = blockDto.RestInSeconds,
                        Instructions = blockDto.Instructions,
                        OrderNumber = blockDto.OrderNumber,
                        Exercises = blockDto.Exercises.Select(exerciseDto => new BlockExercise
                        {
                            ExerciseId = _context.Exercises.FirstOrDefault(e => e.Name == Utils.NormalizeString(exerciseDto.ExerciseName))?.Id ?? 0,
                            Notes = exerciseDto.Notes,
                            OrderNumber = exerciseDto.OrderNumber,
                            Repetitions = exerciseDto.Repetitions,
                            TimerInSeconds = exerciseDto.TimerInSeconds,
                            DistanceInMeters = exerciseDto.DistanceInMeters
                        }).ToList()
                    }).ToList()
                }).ToList()
            };
            existingPlan.Weeks.Add(week);
        }
    }
}


    /// <summary>
    /// Validated if there's any exercises in a plan, as it should have at least one
    /// </summary>
    /// <param name="newPlanDto">the creation dto</param>
    /// <param name="validationError">if there were no exercises found</param>
    /// <returns></returns>
    private bool ValidateTrainingPlan(TrainingPlanWriteDto newPlanDto, out string validationError)
    {
        validationError = string.Empty;
        if (newPlanDto.Weeks.Count == 0 || !newPlanDto.Weeks
                .Any(week => week.Days.Any(day => day.Blocks.Any(block => block.Exercises.Any()))))
        {
            validationError = "The training plan must have at least one week with one day and one exercise.";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets and returns the related exercises for a plan dto
    /// </summary>
    /// <param name="newPlanDto">your plan</param>
    /// <param name="cancellationToken">the cancelation token</param>
    /// <returns>a dictionary<string, Excercise/> for the plan dto passed</returns>
    private async Task<Dictionary<string, Exercise>> GetRelatedExercises(TrainingPlanWriteDto newPlanDto,
        CancellationToken cancellationToken)
    {
        var normalizedExerciseNames = newPlanDto.Weeks
            .SelectMany(x => x.Days)
            .SelectMany(q => q.Blocks)
            .SelectMany(t => t.Exercises)
            .Select(y => Utils.NormalizeString(y.ExerciseName))
            .ToList();

        return await _context.Exercises
            .Include(x => x.TrainingTypes)
            .Where(x => normalizedExerciseNames.Contains(x.Name))
            .ToDictionaryAsync(x => x.Name, x => x, cancellationToken);
    }


    private async Task<List<TrainingType>> GetRelatedTrainingTypes(TrainingPlanWriteDto newPlanDto,
        CancellationToken cancellationToken)
    {
        var trainingTypes = Utils.NormalizeStringList(newPlanDto.TrainingTypes);
        return await _context.TrainingTypes
            .Where(x => trainingTypes.Contains(x.Name))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// gets the related equipment for the plan from the database
    /// </summary>
    /// <param name="newPlanDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>a List[Equipment] that are in the newPlanDto equipment list.</returns>
    private async Task<List<Equipment>> GetRelatedEquipment(TrainingPlanWriteDto newPlanDto,
        CancellationToken cancellationToken)
    {
        if (newPlanDto.Equipemnt.Count == 0) return new List<Equipment>();

        var normalizedEquipmentNames = Utils.NormalizeStringList(newPlanDto.Equipemnt);
        return await _context.Equipment
            .Where(x => normalizedEquipmentNames.Contains(x.Name))
            .ToListAsync(cancellationToken);
    }

    private List<string> ValidateEntities(
        Dictionary<string, Exercise> relatedExercises,
        List<TrainingType> trainingTypes,
        List<Equipment> relatedEquipment,
        TrainingPlanWriteDto newPlanDto)
    {
        var errors = new List<string>();

        var missingExercises = newPlanDto.Weeks
            .SelectMany(x => x.Days)
            .SelectMany(q => q.Blocks)
            .SelectMany(t => t.Exercises)
            .Select(y => Utils.NormalizeString(y.ExerciseName))
            .Except(relatedExercises.Keys)
            .ToList();
        if (missingExercises.Any())
        {
            errors.Add($"The following exercises do not exist: {string.Join(", ", missingExercises)}");
        }

        var missingTrainingTypes = Utils.NormalizeStringList(newPlanDto.TrainingTypes)
            .Except(trainingTypes.Select(tt => tt.Name))
            .ToList();
        if (missingTrainingTypes.Any())
        {
            errors.Add($"The following training types do not exist: {string.Join(", ", missingTrainingTypes)}");
        }

        var missingEquipment = Utils.NormalizeStringList(newPlanDto.Equipemnt)
            .Except(relatedEquipment.Select(eq => eq.Name))
            .ToList();
        if (missingEquipment.Any())
        {
            errors.Add($"The following equipment do not exist: {string.Join(", ", missingEquipment)}");
        }

        return errors;
    }

    private TrainingPlan MapToTrainingPlan(
        TrainingPlanWriteDto newPlanDto,
        Dictionary<string, Exercise> relatedExercises,
        List<TrainingType> trainingTypes,
        List<Equipment> relatedEquipment)
    {
        return new TrainingPlan
        {
            Name = Utils.NormalizeString(newPlanDto.Name),
            Description = newPlanDto.Description,
            Notes = newPlanDto.Notes,
            TrainingTypes = trainingTypes,
            Equipment = relatedEquipment,
            Weeks = newPlanDto.Weeks.Select(weekDto => new TrainingWeek
            {
                Name = Utils.NormalizeString(weekDto.Name),
                OrderNumber = weekDto.OrderNumber,
                Days = weekDto.Days.Select(dayDto => new TrainingDay
                {
                    Name = Utils.NormalizeString(dayDto.Name),
                    Notes = dayDto.Notes,
                    OrderNumber = dayDto.OrderNumber,
                    Blocks = dayDto.Blocks.Select(blockDto => new Block
                    {
                        Name = Utils.NormalizeString(blockDto.Name),
                        Sets = blockDto.Sets,
                        RestInSeconds = blockDto.RestInSeconds,
                        Instructions = blockDto.Instructions,
                        OrderNumber = blockDto.OrderNumber,
                        Exercises = blockDto.Exercises.Select(exerciseDto => new BlockExercise
                        {
                            Exercise = relatedExercises[Utils.NormalizeString(exerciseDto.ExerciseName)],
                            Notes = exerciseDto.Notes,
                            OrderNumber = exerciseDto.OrderNumber,
                            Repetitions = exerciseDto.Repetitions,
                            TimerInSeconds = exerciseDto.TimerInSeconds,
                            DistanceInMeters = exerciseDto.DistanceInMeters
                        }).ToList()
                    }).ToList()
                }).ToList()
            }).ToList()
        };
    }
}