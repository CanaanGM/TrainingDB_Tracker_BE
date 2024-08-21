using AutoMapper;
using AutoMapper.QueryableExtensions;
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
    private readonly IMapper _mapper;
    private readonly ILogger<PlanService> _logger;

    public PlanService(SqliteContext context, IMapper mapper, ILogger<PlanService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<int>> CreateAsync(TrainingPlanWriteDto newPlanDto, CancellationToken cancellationToken)
    {
        if (!ValidateTrainingPlan(newPlanDto, out var validationError))
            return Result<int>.Failure(validationError);
        
        try
        {

            var relatedExercises = await GetRelatedExercises(newPlanDto, cancellationToken);
            
            var validationErrors = ValidateEntities(relatedExercises, newPlanDto);
            if (validationErrors.Any())
                return Result<int>.Failure(string.Join("; ", validationErrors));
            
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            
            var trainingPlan = MapToTrainingPlan(newPlanDto, relatedExercises);

            await _context.TrainingPlans.AddAsync(trainingPlan, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result<int>.Success(trainingPlan.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[ERROR]: failed to create a session in {nameof(CreateAsync)}");
            return Result<int>.Failure($"Failed to create training plan: {ex.Message}", ex);
        }
    }
   
    public async Task<Result> CreateBulkAsync(List<TrainingPlanWriteDto> newPlanDtos, CancellationToken cancellationToken)
    {
        var allMissingExercises = new List<string>();

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            foreach (var newPlanDto in newPlanDtos)
            {
                if (!ValidateTrainingPlan(newPlanDto, out var validationError))
                    return Result.Failure(validationError);

                var relatedExercises = await GetRelatedExercises(newPlanDto, cancellationToken);
                var missingExercises = ValidateEntities(relatedExercises, newPlanDto);

                if (missingExercises.Any())
                    allMissingExercises.AddRange(missingExercises);
                else
                {
                    var trainingPlan = MapToTrainingPlan(newPlanDto, relatedExercises);
                    await _context.TrainingPlans.AddAsync(trainingPlan, cancellationToken);
                }
            }

            if (allMissingExercises.Any())
            {
                var allMissingExerciseNames = allMissingExercises;
                return Result.Failure($"The following exercises do not exist:\n{string.Join("\n", allMissingExerciseNames)}");
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success("All training plans created successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[ERROR]: failed to create bulk training plans in {nameof(CreateBulkAsync)}");
            return Result.Failure($"Failed to create bulk training plans: {ex.Message}", ex);
        }
    }

    
    public async Task<Result<TrainingPlanReadDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            // var trainingPlan = await _context.TrainingPlans
            //     .Include(tp => tp.TrainingWeeks)
            //     .ThenInclude(tw => tw.TrainingDays)
            //     .ThenInclude(td => td.Blocks)
            //     .ThenInclude(b => b.BlockExercises)
            //     .ThenInclude(be => be.Exercise)
            //     .ThenInclude(x => x.TrainingTypes)
            //         .Include(trainingPlan => trainingPlan.TrainingWeeks)
            //     .ThenInclude(trainingWeek => trainingWeek.TrainingDays)
            //     .ThenInclude(trainingDay => trainingDay.Blocks)
            //     .ThenInclude(block => block.BlockExercises)
            //     .ThenInclude(blockExercise => blockExercise.Exercise)
            //     .ThenInclude(exercise => exercise.Equipment)
            //     .FirstOrDefaultAsync(tp => tp.Id == id, cancellationToken);
            //
            
            var trainingPlan = await _context.TrainingPlans
                .ProjectTo<TrainingPlanReadDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(tp => tp.Id == id, cancellationToken);

            
            
            if (trainingPlan == null)
                return Result<TrainingPlanReadDto>.Failure("Training plan not found.");

            var blockExercises = trainingPlan.TrainingWeeks
                .SelectMany(x => x.TrainingDays)
                .SelectMany(x => x.Blocks)
                .SelectMany(r => r.BlockExercises)
                .ToList();
            
            trainingPlan.TrainingTypes = blockExercises
                .Select(x => x.Exercise)
                .SelectMany(x => x.TrainingTypes)
                .Select(f => f.Name)
                .Distinct()
                .ToList();
            
            // trainingPlan.RequiredEquipment = blockExercises
            //     .Select(x => x.Exercise)
            //     .SelectMany(r => r.Equipment)
            //     .Select(x => x.Name)
            //     .Distinct()
            //     .ToList();


            return Result<TrainingPlanReadDto>.Success(trainingPlan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[ERROR]: failed to retrieve training plan in {nameof(GetByIdAsync)}");
            return Result<TrainingPlanReadDto>.Failure($"Failed to retrieve training plan: {ex.Message}", ex);
        }
    }


    public async Task<Result<bool>> UpdateAsync(int planId, TrainingPlanWriteDto updateDto,
        CancellationToken cancellationToken)
    {
        if (!ValidateTrainingPlan(updateDto, out var validationError))
            return Result<bool>.Failure(validationError);


        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            
            var trainingPlan = await _context.TrainingPlans
                .Include(tp => tp.TrainingWeeks)
                .ThenInclude(tw => tw.TrainingDays)
                .ThenInclude(td => td.Blocks)
                .ThenInclude(b => b.BlockExercises)
                .ThenInclude(be => be.Exercise)
                .FirstOrDefaultAsync(tp => tp.Id == planId, cancellationToken);

            if (trainingPlan == null)
                return Result<bool>.Failure("Training plan not found.");

            var dtoExercises = updateDto.TrainingWeeks
                .SelectMany(w => w.TrainingDays)
                .SelectMany(d => d.Blocks)
                .SelectMany(b => b.BlockExercises)
                .Select(e => e.ExerciseName)
                .ToList();

            if (!dtoExercises.Any())
                return Result<bool>.Failure("The training plan must contain at least one exercise.");

            var relatedExercises = await _context.Exercises
                .Include(e => e.TrainingTypes)
                .Where(e => dtoExercises.Contains(e.Name))
                .ToDictionaryAsync(e => e.Name, e => e, cancellationToken);

            var missingExercises = dtoExercises.Except(relatedExercises.Keys).ToList();
            if (missingExercises.Any())
                return Result<bool>.Failure(
                    $"The following exercises do not exist: {string.Join(", ", missingExercises)}");


            trainingPlan.Name = Utils.NormalizeString(updateDto.Name);
            trainingPlan.Description = updateDto.Description;
            trainingPlan.Notes = updateDto.Notes;

            _context.TrainingWeeks.RemoveRange(trainingPlan.TrainingWeeks);

            trainingPlan.TrainingWeeks = updateDto.TrainingWeeks.Select(weekDto => new TrainingWeek
            {
                Name = weekDto.Name,
                OrderNumber = weekDto.OrderNumber,
                TrainingDays = weekDto.TrainingDays.Select(dayDto => new TrainingDay
                {
                    Name = dayDto.Name,
                    Notes = dayDto.Notes,
                    OrderNumber = dayDto.OrderNumber,
                    Blocks = dayDto.Blocks.Select(blockDto => new Block
                    {
                        Name = blockDto.Name,
                        Sets = blockDto.Sets,
                        RestInSeconds = blockDto.RestInSeconds,
                        Instructions = blockDto.Instructions,
                        OrderNumber = blockDto.OrderNumber,
                        BlockExercises = blockDto.BlockExercises.Select(exerciseDto => new BlockExercise
                        {
                            Exercise = relatedExercises[exerciseDto.ExerciseName],
                            OrderNumber = exerciseDto.OrderNumber,
                            Repetitions = exerciseDto.Repetitions,
                            TimerInSeconds = exerciseDto.TimerInSeconds,
                            DistanceInMeters = exerciseDto.DistanceInMeters
                        }).ToList()
                    }).ToList()
                }).ToList()
            }).ToList();

            _context.TrainingPlans.Update(trainingPlan);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ERROR]: Failed to update training plan in {nameof(UpdateAsync)}. Exception: {ex}");
            return Result<bool>.Failure($"Failed to update training plan: {ex.Message}", ex);
        }
    }

    public async Task<Result<bool>> DeleteAsync(int planId, CancellationToken cancellationToken)
    {
        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            var trainingPlan = await _context.TrainingPlans
                .Include(tp => tp.TrainingWeeks)
                .ThenInclude(tw => tw.TrainingDays)
                .ThenInclude(td => td.Blocks)
                .ThenInclude(b => b.BlockExercises)
                .FirstOrDefaultAsync(tp => tp.Id == planId, cancellationToken);

            if (trainingPlan == null)
                return Result<bool>.Failure("Training plan not found.");


            // TODO: this could be better
            // Remove associated entities
            foreach (var week in trainingPlan.TrainingWeeks)
            {
                foreach (var day in week.TrainingDays)
                {
                    foreach (var block in day.Blocks)
                    {
                        _context.BlockExercises.RemoveRange(block.BlockExercises);
                    }

                    _context.Blocks.RemoveRange(day.Blocks);
                }

                _context.TrainingDays.RemoveRange(week.TrainingDays);
            }

            _context.TrainingWeeks.RemoveRange(trainingPlan.TrainingWeeks);

            // Remove the training plan itself
            _context.TrainingPlans.Remove(trainingPlan);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[ERROR]: failed to delete training plan in {nameof(DeleteAsync)}");
            return Result<bool>.Failure($"Failed to delete training plan: {ex.Message}", ex);
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
        if (newPlanDto.TrainingWeeks.Count == 0 || !newPlanDto.TrainingWeeks
                .Any(week => week.TrainingDays.Any(day => day.Blocks.Any(block => block.BlockExercises.Any()))))
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
        var normalizedExerciseNames = newPlanDto.TrainingWeeks
            .SelectMany(x => x.TrainingDays)
            .SelectMany(q => q.Blocks)
            .SelectMany(t => t.BlockExercises)
            .Select(y => Utils.NormalizeString(y.ExerciseName))
            .ToList();

        return await _context.Exercises
            .Include(x => x.TrainingTypes)
            .Where(x => normalizedExerciseNames.Contains(x.Name))
            .ToDictionaryAsync(x => x.Name, x => x, cancellationToken);
    }
    
    private List<string> ValidateEntities(
        Dictionary<string, Exercise> relatedExercises,
        TrainingPlanWriteDto newPlanDto)
    {
        var errors = new List<string>();

        var missingExercises = newPlanDto.TrainingWeeks
            .SelectMany(x => x.TrainingDays)
            .SelectMany(q => q.Blocks)
            .SelectMany(t => t.BlockExercises)
            .Select(y => Utils.NormalizeString(y.ExerciseName))
            .Except(relatedExercises.Keys)
            .ToList();
        if (missingExercises.Any())
        {
            errors.Add($"{string.Join("\n", missingExercises)}");
        }
        return errors;
    }
    
    
    private TrainingPlan MapToTrainingPlan(TrainingPlanWriteDto newPlanDto,Dictionary<string, Exercise> relatedExercises)
    {
        return new TrainingPlan
        {
            Name = Utils.NormalizeString(newPlanDto.Name),
            Description = newPlanDto.Description,
            Notes = newPlanDto.Notes,
            TrainingWeeks = newPlanDto.TrainingWeeks.Select(weekDto => new TrainingWeek
            {
                Name = Utils.NormalizeString(weekDto.Name),
                OrderNumber = weekDto.OrderNumber,
                TrainingDays = weekDto.TrainingDays.Select(dayDto => new TrainingDay
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
                        BlockExercises = blockDto.BlockExercises.Select(exerciseDto => new BlockExercise
                        {
                            Exercise = relatedExercises[Utils.NormalizeString(exerciseDto.ExerciseName)],
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