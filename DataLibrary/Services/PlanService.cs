// using AutoMapper;
// using DataLibrary.Context;
// using DataLibrary.Core;
// using DataLibrary.Dtos;
// using DataLibrary.Helpers;
// using DataLibrary.Models;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
//
// namespace DataLibrary.Services;
//
// public interface IPlanService
// {
//     Task<Result<TrainingPlanReadDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
//     Task<Result<int>> CreateAsync(TrainingPlanWriteDto newPlanDto, CancellationToken cancellationToken);
//
//     Task<Result<bool>> UpdateAsync(int planId, TrainingPlanWriteDto updateDto,
//         CancellationToken cancellationToken);
//
//     Task<Result<bool>> DeleteAsync(int planId, CancellationToken cancellationToken);
// }
//
// public class PlanService : IPlanService
// {
//     private readonly SqliteContext _context;
//     private readonly IMapper _mapper;
//     private readonly ILogger<PlanService> _logger;
//
//     public PlanService(SqliteContext context, IMapper mapper, ILogger<PlanService> logger)
//     {
//         _context = context;
//         _mapper = mapper;
//         _logger = logger;
//     }
//
//     public async Task<Result<TrainingPlanReadDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
//     {
//         try
//         {
//             var trainingPlan = await _context.TrainingPlans
//                 .Include(tp => tp.Weeks)
//                 .ThenInclude(tw => tw.Days)
//                 .ThenInclude(td => td.Blocks)
//                 .ThenInclude(b => b.Exercises)
//                 .ThenInclude(be => be.Exercise)
//                 .Include(tp => tp.Equipment)
//                 .Include(tp => tp.TrainingTypes)
//                 .FirstOrDefaultAsync(tp => tp.Id == id, cancellationToken);
//
//             if (trainingPlan == null)
//                 return Result<TrainingPlanReadDto>.Failure("Training plan not found.");
//
//             var trainingPlanDto = _mapper.Map<TrainingPlanReadDto>(trainingPlan);
//             return Result<TrainingPlanReadDto>.Success(trainingPlanDto);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, $"[ERROR]: failed to retrieve training plan in {nameof(GetByIdAsync)}");
//             return Result<TrainingPlanReadDto>.Failure($"Failed to retrieve training plan: {ex.Message}", ex);
//         }
//     }
//
//
//     public async Task<Result<int>> CreateAsync(TrainingPlanWriteDto newPlanDto, CancellationToken cancellationToken)
//     {
//         if (!ValidateTrainingPlan(newPlanDto, out var validationError))
//             return Result<int>.Failure(validationError);
//
//         await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
//
//         try
//         {
//             var relatedExercises = await GetRelatedExercises(newPlanDto, cancellationToken);
//             var trainingTypes = await GetRelatedTrainingTypes(newPlanDto, cancellationToken);
//             var relatedEquipment = await GetRelatedEquipment(newPlanDto, cancellationToken);
//
//             var validationErrors = ValidateEntities(relatedExercises, trainingTypes, relatedEquipment, newPlanDto);
//             if (validationErrors.Any())
//             {
//                 await transaction.RollbackAsync(cancellationToken);
//                 return Result<int>.Failure(string.Join("; ", validationErrors));
//             }
//
//             // var trainingPlan = _mapper.Map<TrainingPlan>(newPlanDto);
//             var trainingPlan = MapToTrainingPlan(newPlanDto, relatedExercises, trainingTypes, relatedEquipment);
//
//             await _context.TrainingPlans.AddAsync(trainingPlan, cancellationToken);
//             await _context.SaveChangesAsync(cancellationToken);
//             await transaction.CommitAsync(cancellationToken);
//
//             return Result<int>.Success(trainingPlan.Id);
//         }
//         catch (Exception ex)
//         {
//             await transaction.RollbackAsync(cancellationToken);
//             _logger.LogError(ex, $"[ERROR]: failed to create a session in {nameof(CreateAsync)}");
//             return Result<int>.Failure($"Failed to create training plan: {ex.Message}", ex);
//         }
//     }
//
//
//     public async Task<Result<bool>> UpdateAsync(int planId, TrainingPlanWriteDto updateDto,
//         CancellationToken cancellationToken)
//     {
//         if (!ValidateTrainingPlan(updateDto, out var validationError))
//             return Result<bool>.Failure(validationError);
//
//         await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
//
//         try
//         {
//             var trainingPlan = await _context.TrainingPlans
//                 .Include(tp => tp.Weeks)
//                 .ThenInclude(tw => tw.Days)
//                 .ThenInclude(td => td.Blocks)
//                 .ThenInclude(b => b.Exercises)
//                 .ThenInclude(be => be.Exercise)
//                 .Include(tp => tp.Equipment)
//                 .Include(tp => tp.TrainingTypes)
//                 .FirstOrDefaultAsync(tp => tp.Id == planId, cancellationToken);
//
//             if (trainingPlan == null)
//                 return Result<bool>.Failure("Training plan not found.");
//
//             var normalizedExerciseNames = updateDto.Weeks
//                 .SelectMany(w => w.Days)
//                 .SelectMany(d => d.Blocks)
//                 .SelectMany(b => b.Exercises)
//                 .Select(e => Utils.NormalizeString(e.ExerciseName))
//                 .ToList();
//
//             if (!normalizedExerciseNames.Any())
//                 return Result<bool>.Failure("The training plan must contain at least one exercise.");
//
//             var relatedExercises = await _context.Exercises
//                 .Include(e => e.TrainingTypes)
//                 .Where(e => normalizedExerciseNames.Contains(e.Name))
//                 .ToDictionaryAsync(e => e.Name, e => e, cancellationToken);
//
//             var missingExercises = normalizedExerciseNames.Except(relatedExercises.Keys).ToList();
//             if (missingExercises.Any())
//                 return Result<bool>.Failure(
//                     $"The following exercises do not exist: {string.Join(", ", missingExercises)}");
//
//             var trainingTypes = Utils.NormalizeStringList(updateDto.TrainingTypes);
//             var dbRelatedTypes = await _context.TrainingTypes
//                 .Where(tt => trainingTypes.Contains(tt.Name))
//                 .ToListAsync(cancellationToken);
//
//             var missingTypes = trainingTypes.Except(dbRelatedTypes.Select(tt => tt.Name)).ToList();
//             if (missingTypes.Any())
//                 return Result<bool>.Failure(
//                     $"The following training types do not exist: {string.Join(", ", missingTypes)}");
//
//             var relatedEquipment = new List<Equipment>();
//             if (updateDto.Equipemnt.Any())
//             {
//                 var normalizedEquipmentNames = Utils.NormalizeStringList(updateDto.Equipemnt);
//                 relatedEquipment = await _context.Equipment
//                     .Where(eq => normalizedEquipmentNames.Contains(eq.Name))
//                     .ToListAsync(cancellationToken);
//
//                 var missingEquipment = normalizedEquipmentNames.Except(relatedEquipment.Select(eq => eq.Name)).ToList();
//                 if (missingEquipment.Any())
//                     return Result<bool>.Failure(
//                         $"The following equipment do not exist: {string.Join(", ", missingEquipment)}");
//             }
//
//             // Update the existing training plan
//             trainingPlan.Name = Utils.NormalizeString(updateDto.Name);
//             trainingPlan.Description = updateDto.Description;
//             trainingPlan.Notes = updateDto.Notes;
//             trainingPlan.TrainingTypes = dbRelatedTypes;
//             trainingPlan.Equipment = relatedEquipment;
//
//             // Remove existing weeks, days, and blocks
//             _context.TrainingWeeks.RemoveRange(trainingPlan.Weeks);
//
//             // Add new weeks, days, and blocks
//             trainingPlan.Weeks = updateDto.Weeks.Select(weekDto => new TrainingWeek
//             {
//                 Name = Utils.NormalizeString(weekDto.Name),
//                 OrderNumber = weekDto.OrderNumber,
//                 Days = weekDto.Days.Select(dayDto => new TrainingDay
//                 {
//                     Name = Utils.NormalizeString(dayDto.Name),
//                     Notes = dayDto.Notes,
//                     OrderNumber = dayDto.OrderNumber,
//                     Blocks = dayDto.Blocks.Select(blockDto => new Block
//                     {
//                         Name = Utils.NormalizeString(blockDto.Name),
//                         Sets = blockDto.Sets,
//                         RestInSeconds = blockDto.RestInSeconds,
//                         Instructions = blockDto.Instructions,
//                         OrderNumber = blockDto.OrderNumber,
//                         Exercises = blockDto.Exercises.Select(exerciseDto => new BlockExercise
//                         {
//                             Exercise = relatedExercises[Utils.NormalizeString(exerciseDto.ExerciseName)],
//                             Notes = exerciseDto.Notes,
//                             OrderNumber = exerciseDto.OrderNumber,
//                             Repetitions = exerciseDto.Repetitions,
//                             TimerInSeconds = exerciseDto.TimerInSeconds,
//                             DistanceInMeters = exerciseDto.DistanceInMeters
//                         }).ToList()
//                     }).ToList()
//                 }).ToList()
//             }).ToList();
//
//             _context.TrainingPlans.Update(trainingPlan);
//             await _context.SaveChangesAsync(cancellationToken);
//             await transaction.CommitAsync(cancellationToken);
//
//             return Result<bool>.Success(true);
//         }
//         catch (Exception ex)
//         {
//             await transaction.RollbackAsync(cancellationToken);
//             _logger.LogError($"[ERROR]: Failed to update training plan in {nameof(UpdateAsync)}. Exception: {ex}");
//             return Result<bool>.Failure($"Failed to update training plan: {ex.Message}", ex);
//         }
//     }
//
//     public async Task<Result<bool>> DeleteAsync(int planId, CancellationToken cancellationToken)
//     {
//         await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
//
//         try
//         {
//             var trainingPlan = await _context.TrainingPlans
//                 .Include(tp => tp.Weeks)
//                 .ThenInclude(tw => tw.Days)
//                 .ThenInclude(td => td.Blocks)
//                 .ThenInclude(b => b.Exercises)
//                 .Include(tp => tp.Equipment)
//                 .Include(tp => tp.TrainingTypes)
//                 .FirstOrDefaultAsync(tp => tp.Id == planId, cancellationToken);
//
//             if (trainingPlan == null)
//                 return Result<bool>.Failure("Training plan not found.");
//
//             // Remove associated entities
//             foreach (var week in trainingPlan.Weeks)
//             {
//                 foreach (var day in week.Days)
//                 {
//                     foreach (var block in day.Blocks)
//                     {
//                         _context.BlockExercises.RemoveRange(block.Exercises);
//                     }
//
//                     _context.Blocks.RemoveRange(day.Blocks);
//                 }
//
//                 _context.TrainingDays.RemoveRange(week.Days);
//             }
//
//             _context.TrainingWeeks.RemoveRange(trainingPlan.Weeks);
//
//             // Remove the training plan itself
//             _context.TrainingPlans.Remove(trainingPlan);
//
//             await _context.SaveChangesAsync(cancellationToken);
//             await transaction.CommitAsync(cancellationToken);
//
//             return Result<bool>.Success(true);
//         }
//         catch (Exception ex)
//         {
//             await transaction.RollbackAsync(cancellationToken);
//             _logger.LogError(ex, $"[ERROR]: failed to delete training plan in {nameof(DeleteAsync)}");
//             return Result<bool>.Failure($"Failed to delete training plan: {ex.Message}", ex);
//         }
//     }
//
//
//     /// <summary>
//     /// Validated if there's any exercises in a plan, as it should have at least one
//     /// </summary>
//     /// <param name="newPlanDto">the creation dto</param>
//     /// <param name="validationError">if there were no exercises found</param>
//     /// <returns></returns>
//     private bool ValidateTrainingPlan(TrainingPlanWriteDto newPlanDto, out string validationError)
//     {
//         validationError = string.Empty;
//         if (newPlanDto.Weeks.Count == 0 || !newPlanDto.Weeks
//                 .Any(week => week.Days.Any(day => day.Blocks.Any(block => block.Exercises.Any()))))
//         {
//             validationError = "The training plan must have at least one week with one day and one exercise.";
//             return false;
//         }
//
//         return true;
//     }
//
//     /// <summary>
//     /// Gets and returns the related exercises for a plan dto
//     /// </summary>
//     /// <param name="newPlanDto">your plan</param>
//     /// <param name="cancellationToken">the cancelation token</param>
//     /// <returns>a dictionary<string, Excercise/> for the plan dto passed</returns>
//     private async Task<Dictionary<string, Exercise>> GetRelatedExercises(TrainingPlanWriteDto newPlanDto,
//         CancellationToken cancellationToken)
//     {
//         var normalizedExerciseNames = newPlanDto.Weeks
//             .SelectMany(x => x.Days)
//             .SelectMany(q => q.Blocks)
//             .SelectMany(t => t.Exercises)
//             .Select(y => Utils.NormalizeString(y.ExerciseName))
//             .ToList();
//
//         return await _context.Exercises
//             .Include(x => x.TrainingTypes)
//             .Where(x => normalizedExerciseNames.Contains(x.Name))
//             .ToDictionaryAsync(x => x.Name, x => x, cancellationToken);
//     }
//
//
//     private async Task<List<TrainingType>> GetRelatedTrainingTypes(TrainingPlanWriteDto newPlanDto,
//         CancellationToken cancellationToken)
//     {
//         var trainingTypes = Utils.NormalizeStringList(newPlanDto.TrainingTypes);
//         return await _context.TrainingTypes
//             .Where(x => trainingTypes.Contains(x.Name))
//             .ToListAsync(cancellationToken);
//     }
//
//     /// <summary>
//     /// gets the related equipment for the plan from the database
//     /// </summary>
//     /// <param name="newPlanDto"></param>
//     /// <param name="cancellationToken"></param>
//     /// <returns>a List[Equipment] that are in the newPlanDto equipment list.</returns>
//     private async Task<List<Equipment>> GetRelatedEquipment(TrainingPlanWriteDto newPlanDto,
//         CancellationToken cancellationToken)
//     {
//         if (newPlanDto.Equipemnt.Count == 0) return new List<Equipment>();
//
//         var normalizedEquipmentNames = Utils.NormalizeStringList(newPlanDto.Equipemnt);
//         return await _context.Equipment
//             .Where(x => normalizedEquipmentNames.Contains(x.Name))
//             .ToListAsync(cancellationToken);
//     }
//
//     private List<string> ValidateEntities(
//         Dictionary<string, Exercise> relatedExercises,
//         List<TrainingType> trainingTypes,
//         List<Equipment> relatedEquipment,
//         TrainingPlanWriteDto newPlanDto)
//     {
//         var errors = new List<string>();
//
//         var missingExercises = newPlanDto.Weeks
//             .SelectMany(x => x.Days)
//             .SelectMany(q => q.Blocks)
//             .SelectMany(t => t.Exercises)
//             .Select(y => Utils.NormalizeString(y.ExerciseName))
//             .Except(relatedExercises.Keys)
//             .ToList();
//         if (missingExercises.Any())
//         {
//             errors.Add($"The following exercises do not exist: {string.Join(", ", missingExercises)}");
//         }
//
//         var missingTrainingTypes = Utils.NormalizeStringList(newPlanDto.TrainingTypes)
//             .Except(trainingTypes.Select(tt => tt.Name))
//             .ToList();
//         if (missingTrainingTypes.Any())
//         {
//             errors.Add($"The following training types do not exist: {string.Join(", ", missingTrainingTypes)}");
//         }
//
//         var missingEquipment = Utils.NormalizeStringList(newPlanDto.Equipemnt)
//             .Except(relatedEquipment.Select(eq => eq.Name))
//             .ToList();
//         if (missingEquipment.Any())
//         {
//             errors.Add($"The following equipment do not exist: {string.Join(", ", missingEquipment)}");
//         }
//
//         return errors;
//     }
//
//     private TrainingPlan MapToTrainingPlan(
//         TrainingPlanWriteDto newPlanDto,
//         Dictionary<string, Exercise> relatedExercises,
//         List<TrainingType> trainingTypes,
//         List<Equipment> relatedEquipment)
//     {
//         return new TrainingPlan
//         {
//             Name = Utils.NormalizeString(newPlanDto.Name),
//             Description = newPlanDto.Description,
//             Notes = newPlanDto.Notes,
//             TrainingTypes = trainingTypes,
//             Equipment = relatedEquipment,
//             Weeks = newPlanDto.Weeks.Select(weekDto => new TrainingWeek
//             {
//                 Name = Utils.NormalizeString(weekDto.Name),
//                 OrderNumber = weekDto.OrderNumber,
//                 Days = weekDto.Days.Select(dayDto => new TrainingDay
//                 {
//                     Name = Utils.NormalizeString(dayDto.Name),
//                     Notes = dayDto.Notes,
//                     OrderNumber = dayDto.OrderNumber,
//                     Blocks = dayDto.Blocks.Select(blockDto => new Block
//                     {
//                         Name = Utils.NormalizeString(blockDto.Name),
//                         Sets = blockDto.Sets,
//                         RestInSeconds = blockDto.RestInSeconds,
//                         Instructions = blockDto.Instructions,
//                         OrderNumber = blockDto.OrderNumber,
//                         Exercises = blockDto.Exercises.Select(exerciseDto => new BlockExercise
//                         {
//                             Exercise = relatedExercises[Utils.NormalizeString(exerciseDto.ExerciseName)],
//                             Notes = exerciseDto.Notes,
//                             OrderNumber = exerciseDto.OrderNumber,
//                             Repetitions = exerciseDto.Repetitions,
//                             TimerInSeconds = exerciseDto.TimerInSeconds,
//                             DistanceInMeters = exerciseDto.DistanceInMeters
//                         }).ToList()
//                     }).ToList()
//                 }).ToList()
//             }).ToList()
//         };
//     }
// }