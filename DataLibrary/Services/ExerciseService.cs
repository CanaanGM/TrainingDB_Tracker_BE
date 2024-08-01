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

public interface IExerciseService
{
    Task<Result<int>> CreateAsync(ExerciseWriteDto newExerciseDto, CancellationToken cancellationToken);
    Task<Result<bool>> CreateBulkAsync(List<ExerciseWriteDto> newExerciseDtos, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteExerciseAsync(int exerciseId, CancellationToken cancellationToken);
    Task<Result<ExerciseReadDto>> GetByNameAsync(string exerciseName, CancellationToken cancellationToken);
    Task<Result<bool>> UpdateAsync(int exerciseId, ExerciseWriteDto exerciseDto, CancellationToken cancellationToken);

    Task<Result<List<ExerciseSearchResultDto>>> SearchExercisesAsync(string searchTerm,
        CancellationToken cancellationToken);

    Task<Result<PaginatedList<ExerciseReadDto>>> GetAllAsync(
        ExerciseQueryOptions options,
        CancellationToken cancellationToken);

    Task<Result<bool>> DeleteBulkAsync(List<string> exerciseNames, CancellationToken cancellationToken);
}

public class ExerciseService
{
    private readonly SqliteContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ExerciseService> _logger;

    public ExerciseService(SqliteContext context, IMapper mapper, ILogger<ExerciseService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<bool>> CreateBulkAsync(List<ExerciseWriteDto> newExerciseDtos,
        CancellationToken cancellationToken)
    {
        if (newExerciseDtos == null || !newExerciseDtos.Any())
        {
            _logger.LogError("Attempted to create bulk exercises with no exercise data provided.");
            return Result<bool>.Failure("No exercises to create.");
        }

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            foreach (var newExerciseDto in newExerciseDtos)
            {
                var validationResult = CheckValues(newExerciseDto);
                if (!validationResult.IsSuccess)
                {
                    _logger.LogError(
                        $"Validation failed for exercise: {newExerciseDto.Name}. Error: {validationResult.ErrorMessage}");
                    return Result<bool>.Failure(validationResult.ErrorMessage!);
                }

                var language = await _context.Languages.FirstOrDefaultAsync(x => x.Code == newExerciseDto.LanguageCode,
                    cancellationToken);
                if (language == null)
                {
                    _logger.LogError($"Language code {newExerciseDto.LanguageCode} not found.");
                    return Result<bool>.Failure(
                        $"Language with code: {newExerciseDto.LanguageCode} could not be found.");
                }

                var muscleCreationResult =
                    await CreateMuscleDictionary(newExerciseDto.ExerciseMuscles, cancellationToken);
                if (!muscleCreationResult.IsSuccess)
                {
                    return Result<bool>.Failure(muscleCreationResult.ErrorMessage!);
                }

                var trainingTypeResult = await CreateTrainingTypeList(newExerciseDto.TrainingTypes, cancellationToken);
                if (!trainingTypeResult.IsSuccess)
                {
                    return Result<bool>.Failure(trainingTypeResult.ErrorMessage!);
                }

                var equipmentListResult =
                    await CreateEquipmentList(newExerciseDto.EquipmentNeeded ?? new List<string>(), cancellationToken);
                if (!equipmentListResult.IsSuccess)
                {
                    return Result<bool>.Failure(equipmentListResult.ErrorMessage!);
                }

                Exercise newExercise = new Exercise
                {
                    Difficulty = newExerciseDto.Difficulty,
                    ExerciseMuscles = newExerciseDto.ExerciseMuscles.Select(em => new ExerciseMuscle
                    {
                        Muscle = muscleCreationResult.Value[em.MuscleName].Muscle,
                        IsPrimary = em.IsPrimary ?? false
                    }).ToList(),
                    TrainingTypes = trainingTypeResult.Value,
                    Equipment = equipmentListResult.Value,
                    ExerciseHowTos = newExerciseDto.HowTos?.Select(ht => new ExerciseHowTo
                    {
                        Name = ht.Name,
                        Url = ht.Url
                    }).ToList() ?? new List<ExerciseHowTo>()
                };

                _context.Exercises.Add(newExercise);

                var newLocalizedExercise = new LocalizedExercise
                {
                    Name = newExerciseDto.Name,
                    Description = newExerciseDto.Description,
                    HowTo = newExerciseDto.HowTo,
                    Language = language,
                    Exercise = newExercise
                };

                _context.LocalizedExercises.Add(newLocalizedExercise);
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while creating multiple exercises: {ex.Message}");
            return Result<bool>.Failure("Failed to create exercises due to an unexpected error.");
        }
    }


    public async Task<Result<int>> CreateAsync(ExerciseWriteDto newExerciseDto, CancellationToken cancellationToken)
    {
        var validationResult = CheckValues(newExerciseDto);
        if (!validationResult.IsSuccess)
            return Result<int>.Failure(validationResult.ErrorMessage!);
        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var language =
                await _context.Languages
                    .FirstOrDefaultAsync(x => x.Code == newExerciseDto.LanguageCode,
                        cancellationToken);
            if (language is null)
            {
                _logger.LogError($"the language with code: {newExerciseDto.LanguageCode} could not be found.");
                return Result<int>.Failure(
                    $"the language with code: {newExerciseDto.LanguageCode} could not be found.");
            }

            var muscleCreationRes = await CreateMuscleDictionary(newExerciseDto.ExerciseMuscles, cancellationToken);
            if (!muscleCreationRes.IsSuccess)
                return Result<int>.Failure(muscleCreationRes.ErrorMessage!);


            Dictionary<string, LocalizedMuscle> muscles = muscleCreationRes.Value!;

            var trainingTypesCreationResult =
                await CreateTrainingTypeList(newExerciseDto.TrainingTypes, cancellationToken);
            if (!trainingTypesCreationResult.IsSuccess)
                return Result<int>.Failure(trainingTypesCreationResult.ErrorMessage!);

            List<TrainingType> trainingTypes = trainingTypesCreationResult.Value!;

            List<Equipment> neededEquipment = [];
            if (newExerciseDto.EquipmentNeeded is not null)
            {
                var neededEquipmentCreationList =
                    await CreateEquipmentList(newExerciseDto.EquipmentNeeded.Distinct().ToList(), cancellationToken);
                neededEquipment = neededEquipmentCreationList.Value!;
                if (!neededEquipmentCreationList.IsSuccess)
                    return Result<int>.Failure(trainingTypesCreationResult.ErrorMessage!);
            }

            Exercise newExercise = new Exercise()
            {
                Difficulty = newExerciseDto.Difficulty,
                ExerciseMuscles = newExerciseDto.ExerciseMuscles.Select(em => new ExerciseMuscle
                {
                    Muscle = muscles[em.MuscleName].Muscle,
                    IsPrimary = em.IsPrimary
                }).ToList(),
                TrainingTypes = trainingTypes,
                ExerciseHowTos = newExerciseDto.HowTos is not null
                    ? newExerciseDto.HowTos.Select(howTo => new ExerciseHowTo
                    {
                        Name = howTo.Name,
                        Url = howTo.Url
                    }).ToList()
                    : [],
                Equipment = neededEquipment
            };
            _context.Exercises.Add(newExercise);

            var newLocalizedExercise = new LocalizedExercise()
            {
                Name = newExerciseDto.Name,
                Description = newExerciseDto.Description,
                HowTo = newExerciseDto.HowTo,
                Language = language,
                Exercise = newExercise
            };

            await _context.LocalizedExercises.AddAsync(newLocalizedExercise, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<int>.Success(newExercise.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong creating {newExerciseDto.Name} in {nameof(CreateAsync)}.");
            return Result<int>.Failure("Failed to create the exercise due to an error: " + ex.Message, ex);
        }
    }

   public async Task<Result<int>> UpdateAsync(int exerciseId, ExerciseWriteDto updatedExerciseDto, CancellationToken cancellationToken)
{
    // Validate the incoming DTO
    var validationResult = CheckValues(updatedExerciseDto);
    if (!validationResult.IsSuccess)
    {
        return Result<int>.Failure(validationResult.ErrorMessage!);
    }

    try
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        // Find the existing exercise
        var existingExercise = await _context.Exercises
            .Include(e => e.ExerciseMuscles)
            .Include(e => e.TrainingTypes)
            .Include(e => e.ExerciseHowTos)
            .Include(e => e.Equipment)
            .FirstOrDefaultAsync(e => e.Id == exerciseId, cancellationToken);

        if (existingExercise == null)
        {
            _logger.LogError($"Exercise with ID {exerciseId} not found.");
            return Result<int>.Failure($"Exercise with ID {exerciseId} not found.");
        }

        // Update language
        var language = await _context.Languages
            .FirstOrDefaultAsync(x => x.Code == updatedExerciseDto.LanguageCode, cancellationToken);

        if (language == null)
        {
            _logger.LogError($"Language code {updatedExerciseDto.LanguageCode} not found.");
            return Result<int>.Failure($"Language with code {updatedExerciseDto.LanguageCode} not found.");
        }

        // Update muscles
        var muscleCreationRes = await CreateMuscleDictionary(updatedExerciseDto.ExerciseMuscles, cancellationToken);
        if (!muscleCreationRes.IsSuccess)
        {
            return Result<int>.Failure(muscleCreationRes.ErrorMessage!);
        }
        var muscles = muscleCreationRes.Value!;

        // Update training types
        var trainingTypesCreationResult = await CreateTrainingTypeList(updatedExerciseDto.TrainingTypes, cancellationToken);
        if (!trainingTypesCreationResult.IsSuccess)
        {
            return Result<int>.Failure(trainingTypesCreationResult.ErrorMessage!);
        }
        var trainingTypes = trainingTypesCreationResult.Value!;

        // Update equipment
        List<Equipment> neededEquipment = new();
        if (updatedExerciseDto.EquipmentNeeded != null)
        {
            var neededEquipmentCreationList = await CreateEquipmentList(updatedExerciseDto.EquipmentNeeded.Distinct().ToList(), cancellationToken);
            neededEquipment = neededEquipmentCreationList.Value!;
            if (!neededEquipmentCreationList.IsSuccess)
            {
                return Result<int>.Failure(neededEquipmentCreationList.ErrorMessage!);
            }
        }

        // Update the existing exercise
        existingExercise.Difficulty = updatedExerciseDto.Difficulty;
        existingExercise.ExerciseMuscles = updatedExerciseDto.ExerciseMuscles.Select(em => new ExerciseMuscle
        {
            Muscle = muscles[em.MuscleName].Muscle,
            IsPrimary = em.IsPrimary
        }).ToList();
        existingExercise.TrainingTypes = trainingTypes;
        existingExercise.Equipment = neededEquipment;
        existingExercise.ExerciseHowTos = updatedExerciseDto.HowTos != null 
            ? updatedExerciseDto.HowTos.Select(howTo => new ExerciseHowTo
            {
                Name = howTo.Name,
                Url = howTo.Url
            }).ToList()
            : new List<ExerciseHowTo>();

        // Update the localized exercise
        var existingLocalizedExercise = await _context.LocalizedExercises
            .FirstOrDefaultAsync(le => le.ExerciseId == exerciseId && le.LanguageId == language.Id, cancellationToken);
        if (existingLocalizedExercise == null)
        {
            _logger.LogError($"Localized exercise not found for ID {exerciseId} and language code {updatedExerciseDto.LanguageCode}.");
            return Result<int>.Failure($"Localized exercise not found for ID {exerciseId} and language code {updatedExerciseDto.LanguageCode}.");
        }

        existingLocalizedExercise.Name = updatedExerciseDto.Name;
        existingLocalizedExercise.Description = updatedExerciseDto.Description;
        existingLocalizedExercise.HowTo = updatedExerciseDto.HowTo;

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        
        return Result<int>.Success(existingExercise.Id);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Failed to update exercise {exerciseId} due to an exception.");
        return Result<int>.Failure("Failed to update exercise due to an error: " + ex.Message, ex);
    }
}

public async Task<Result<bool>> DeleteAsync(int exerciseId, CancellationToken cancellationToken)
{
    try
    {
        var exercise = await _context.Exercises
            .Include(e => e.LocalizedExercises)
            .Include(e => e.ExerciseMuscles)
            .Include(e => e.TrainingTypes)
            .Include(e => e.Equipment)
            .FirstOrDefaultAsync(e => e.Id == exerciseId, cancellationToken);

        if (exercise == null)
        {
            _logger.LogWarning("Exercise with ID {ExerciseId} not found.", exerciseId);
            return Result<bool>.Failure("Exercise not found.");
        }

        _context.Exercises.Remove(exercise);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Exercise with ID {ExerciseId} deleted successfully.", exerciseId);
        return Result<bool>.Success(true);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error deleting exercise with ID {ExerciseId}.", exerciseId);
        return Result<bool>.Failure($"Error deleting exercise: {ex.Message}");
    }
}

   
   
    private async Task<Result<Dictionary<string, LocalizedMuscle>>> CreateMuscleDictionary(
        List<ExerciseMuscleWriteDto> exerciseMuscles, CancellationToken cancellationToken)
    {
        var dtoMuscleNames = exerciseMuscles
            .Select(em => em.MuscleName)
            .Distinct()
            .ToList();

        Dictionary<string, LocalizedMuscle> muscles = await _context.LocalizedMuscles
            .Include(x => x.Muscle)
            .Include(x => x.Language)
            .Where(m => dtoMuscleNames.Contains(m.Name))
            .ToDictionaryAsync(m => m.Name, m => m, cancellationToken);

        if (muscles.Count == exerciseMuscles.Count)
            return Result<Dictionary<string, LocalizedMuscle>>.Success(muscles);

        var nonExistingMuscles = muscles.Values
            .Where(x => !dtoMuscleNames.Contains(x.Name));
        var errorMessage = string.Join(", ", nonExistingMuscles);
        _logger
            .LogError(
                $"Could not create exercise due to these muscles not being present in the database: {errorMessage}");
        return Result<Dictionary<string, LocalizedMuscle>>.Failure($"{errorMessage},muscles could not be found.");
    }

    private async Task<Result<List<TrainingType>>> CreateTrainingTypeList(List<string> dtoTrainingTypeNames,
        CancellationToken cancellationToken)
    {
        List<string> trainingTypeNames = dtoTrainingTypeNames
            .Select(x => x)
            .Distinct()
            .ToList();

        List<TrainingType> trainingTypes = await _context.TrainingTypes
            .Where(tt => trainingTypeNames.Contains(tt.Name))
            .ToListAsync(cancellationToken);

        if (trainingTypes.Count == trainingTypeNames.Count)
            return Result<List<TrainingType>>.Success(trainingTypes);

        var nonExistingTypes = trainingTypes.Where(x => trainingTypeNames.Contains(x.Name));
        var errorMessage = string.Join(", ", nonExistingTypes);
        _logger.LogError($"could not create exercise, due to these training types not existsing: {errorMessage}");

        return Result<List<TrainingType>>.Failure($"the following training types are not created: {errorMessage}");
    }

    private async Task<Result<List<Equipment>>> CreateEquipmentList(List<string> equipmentNames,
        CancellationToken cancellationToken)
    {
        var neededLocalizedEquipment = await _context.LocalizedEquipments
            .Include(x => x.Equipment)
            .Where(x => equipmentNames.Contains(x.Name))
            .ToListAsync(cancellationToken);
        if (equipmentNames.Count == neededLocalizedEquipment.Count)
        {
            var neededEquipment = neededLocalizedEquipment.Select(x => x.Equipment).ToList();
            return Result<List<Equipment>>.Success(neededEquipment);
        }

        var nonExistingEquipment = neededLocalizedEquipment
            .Where(x => equipmentNames.Contains(x.Name))
            .Select(x => x.Name)
            .ToList();
        var errorMessage = string.Join(", ", nonExistingEquipment);
        _logger.LogError($"tried to create exercise but the following equipment was not found: {errorMessage}");

        return Result<List<Equipment>>.Failure($"these equipment are not in the database: {errorMessage}.");
    }

    // TODO: make this static and checks the properties of the type passed into it.
    private Result CheckValues(ExerciseWriteDto newExerciseDto)
    {
        List<string> errorList = [];

        if (string.IsNullOrEmpty(newExerciseDto.Name))
            errorList.Add("name should have a value");
        if (string.IsNullOrEmpty(newExerciseDto.Description))
            errorList.Add("description should have a value");
        if (string.IsNullOrEmpty(newExerciseDto.HowTo))
            errorList.Add("how to should have a value");
        if (newExerciseDto.Difficulty <= 0 || newExerciseDto.Difficulty > 10)
            errorList.Add("difficulty should be between 1 and 5");
        if (newExerciseDto.ExerciseMuscles.Count == 0)
            errorList.Add("an exercise should have a related muscles");
        if (newExerciseDto.TrainingTypes.Count == 0)
            errorList.Add("an exercise should have a related training types");

        newExerciseDto.TrainingTypes.ForEach(x =>
        {
            if (string.IsNullOrEmpty(x))
                errorList.Add("[TRAINING TYPE]: should have a name");
        });

        newExerciseDto.ExerciseMuscles.ForEach(x =>
        {
            if (string.IsNullOrEmpty(x.MuscleName))
                errorList.Add("[MUSCLE NAME]: should not be empty!");
        });

        newExerciseDto.HowTos?.ForEach(x =>
        {
            if (string.IsNullOrEmpty(x.Name))
                errorList.Add("[HOW TO]: name should not be empty!");

            if (string.IsNullOrEmpty(x.Url))
                errorList.Add("[HOW TO]: url should not be empty!");
        });

        newExerciseDto.EquipmentNeeded?.ForEach(x =>
        {
            if (string.IsNullOrEmpty(x))
                errorList.Add("[NEEDED EQUIPMENTS]: name should not be empty!");
        });
        return errorList.Count != 0
            ? Result.Failure(error: string.Join("\n", errorList))
            : Result.Success();
    }


    private static IQueryable<ExerciseReadDto> ApplyFiltering(ExerciseQueryOptions options,
        IQueryable<ExerciseReadDto> query)
    {
        if (!string.IsNullOrEmpty(options.TrainingTypeName))
            query = query.Where(e => e.TrainingTypes.Any(tt => tt.Name == options.TrainingTypeName));

        if (!string.IsNullOrEmpty(options.MuscleName))
            query = query.Where(e => e.ExerciseMuscles.Any(em => em.Name == options.MuscleName));

        if (!string.IsNullOrEmpty(options.MuscleGroupName))
            query = query.Where(e => e.ExerciseMuscles.Any(em => em.MuscleGroup == options.MuscleGroupName));

        if (options.MinimumDifficulty.HasValue)
            query = query.Where(e => e.Difficulty >= options.MinimumDifficulty.Value);

        if (options.MaximumDifficulty.HasValue)
            query = query.Where(e => e.Difficulty <= options.MaximumDifficulty.Value);

        return query;
    }


    private static IQueryable<ExerciseReadDto> ApplySorting(ExerciseQueryOptions options,
        IQueryable<ExerciseReadDto> query)
    {
        query = options.SortBy switch
        {
            SortBy.NAME => options.Ascending ? query.OrderBy(e => e.Name) : query.OrderByDescending(e => e.Name),
            SortBy.DIFFICULTY => options.Ascending
                ? query.OrderBy(e => e.Difficulty)
                : query.OrderByDescending(e => e.Difficulty),
            SortBy.MUSCLE_GROUP => options.Ascending
                ? query.OrderBy(e => e.ExerciseMuscles.First().MuscleGroup)
                : query.OrderByDescending(e => e.ExerciseMuscles.First().MuscleGroup),
            SortBy.TRAINING_TYPE => options.Ascending
                ? query.OrderBy(e => e.TrainingTypes.First().Name)
                : query.OrderByDescending(e => e.TrainingTypes.First().Name),
            _ => query.OrderBy(e => e.Id)
        };
        return query;
    }
}