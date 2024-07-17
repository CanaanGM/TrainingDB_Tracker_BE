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

public class ExerciseService : IExerciseService
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

    //GET
    public async Task<Result<ExerciseReadDto>> GetByNameAsync(string exerciseName, CancellationToken cancellationToken)
    {
        try
        {
            ExerciseReadDto? exercise = await _context.Exercises
                .AsNoTracking()
                .ProjectTo<ExerciseReadDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(x => x.Name == exerciseName, cancellationToken);

            if (exercise is not null) return Result<ExerciseReadDto>.Success(value: exercise);

            _logger.LogError($"[ERROR]: exercise {exerciseName} was not found.\nin {nameof(GetByNameAsync)}");
            return Result<ExerciseReadDto>.Failure($"exercise : {exerciseName} does not exists.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ERROR]: something went wrong in {nameof(GetByNameAsync)}\n{ex.Message}\n{ex}");
            return Result<ExerciseReadDto>.Failure(ex.Message, ex);
        }
    }


    // get all
    // returns a list of :
        // exercise by muscle or muscle group, muscle group takes precedence
        // by trianing type
        // by difficulty
        // all 3 of them can be combined ; give me all exercises for the core where difficulty > 4 and type is body building
    
        public async Task<Result<PaginatedList<ExerciseReadDto>>> GetAllAsync(ExerciseQueryOptions options, CancellationToken cancellationToken)
        {
            try
            {
                if (options.PageNumber <= 0 || options.PageSize <= 0)
                {
                    return Result<PaginatedList<ExerciseReadDto>>.Failure("Page number and page size must be greater than zero.");
                }

                var query = _context.Exercises
                    .Include(e => e.TrainingTypes)
                    .Include(e => e.ExerciseMuscles)
                    .ThenInclude(em => em.Muscle)
                    .Include(e => e.ExerciseHowTos)
                    .ProjectTo<ExerciseReadDto>(_mapper.ConfigurationProvider)
                    .AsNoTracking();

                query = ApplyFiltering(options, query);
                query = ApplySorting(options, query);

                var totalItems = await query.CountAsync(cancellationToken);
                var totalPages = (int)Math.Ceiling(totalItems / (double)options.PageSize);

                var exercises = await query
                    .Skip((options.PageNumber - 1) * options.PageSize)
                    .Take(options.PageSize)
                    .ToListAsync(cancellationToken);

                var paginatedList = new PaginatedList<ExerciseReadDto>
                {
                    Items = exercises,
                    Metadata = new PaginationMetadata
                    {
                        TotalCount = totalItems,
                        TotalPages = totalPages,
                        CurrentPage = options.PageNumber,
                        PageSize = options.PageSize
                    }
                };

                return Result<PaginatedList<ExerciseReadDto>>.Success(paginatedList);
            }
            catch (Exception ex)
            {
                return Result<PaginatedList<ExerciseReadDto>>.Failure("Failed to retrieve exercises: " + ex.Message, ex);
            }
        }



        
    public async Task<Result<int>> CreateAsync(ExerciseWriteDto newExerciseDto, CancellationToken cancellationToken)
    {
        using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            CheckValues(newExerciseDto);
            // Normalize and prepare lists for querying
            List<string> muscleNames = newExerciseDto.ExerciseMuscles.Select(em =>
                    Utils.NormalizeString(em.MuscleName))
                .Distinct()
                .ToList();
            Dictionary<string, Muscle> muscles = await _context.Muscles
                .Where(m => muscleNames.Contains(m.Name))
                .ToDictionaryAsync(m => m.Name, m => m, cancellationToken); // Using a dictionary for quick lookup

            // Ensure all muscles are found
            if (muscles.Count != newExerciseDto.ExerciseMuscles.Count)
                throw new Exception("One or more specified muscles could not be found.");

            List<string> trainingTypeNames = newExerciseDto.TrainingTypes.Select(Utils.NormalizeString)
                .Distinct()
                .ToList();
            List<TrainingType> trainingTypes = await _context.TrainingTypes
                .Where(tt => trainingTypeNames.Contains(tt.Name))
                .ToListAsync(cancellationToken);

            // Ensure all training types are found
            if (trainingTypes.Count != trainingTypeNames.Count)
                throw new Exception("One or more specified training types could not be found.");

            Exercise newExercise = new Exercise
            {
                Name = Utils.NormalizeString(newExerciseDto.Name),
                Description = newExerciseDto.Description,
                HowTo = newExerciseDto.HowTo,
                Difficulty = newExerciseDto.Difficulty.GetValueOrDefault(),

                ExerciseMuscles = newExerciseDto.ExerciseMuscles.Select(em => new ExerciseMuscle
                {
                    Muscle = muscles[Utils.NormalizeString(em.MuscleName)],
                    IsPrimary = em.IsPrimary
                }).ToList(),
                TrainingTypes = trainingTypes
            };

            if(newExerciseDto.HowTos is not null)
                 newExercise.ExerciseHowTos = newExerciseDto.HowTos.Select(howTo => new ExerciseHowTo
                {
                    Name = Utils.NormalizeString(howTo.Name),
                    Url = howTo.Url
                }).ToList();

            await _context.Exercises.AddAsync(newExercise, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<int>.Success(newExercise.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<int>.Failure("Failed to create the exercise due to an error: " + ex.Message, ex);
        }
    }

    /// <summary>
    /// checks if the input for creating an exercise are not empty 
    /// </summary>
    /// <param name="newExerciseDto">the creation dto</param>
    /// <exception cref="ArgumentException"></exception>
    private void CheckValues(ExerciseWriteDto newExerciseDto)
    {
        if (string.IsNullOrEmpty(newExerciseDto.Name))
            throw new ArgumentException("name should have a value");
        if (string.IsNullOrEmpty(newExerciseDto.Description))
            throw new ArgumentException("description should have a value");
        if (string.IsNullOrEmpty(newExerciseDto.HowTo))
            throw new ArgumentException("how to should have a value");
        if (newExerciseDto.Difficulty <= 0 || newExerciseDto.Difficulty > 10)
            throw new ArgumentException("difficulty should be between 1 and 10");
        if (newExerciseDto.ExerciseMuscles.Count == 0)
            throw new ArgumentException("an exercise should have a related muscles");
        if (newExerciseDto.TrainingTypes.Count == 0)
            throw new ArgumentException("an exercise should have a related training types");
        newExerciseDto.TrainingTypes.ForEach(x =>
        {
            if (string.IsNullOrEmpty(x))
                throw new ArgumentException("a training type should have a name");
        });
        newExerciseDto.ExerciseMuscles.ForEach(x =>
        {
            if (string.IsNullOrEmpty(x.MuscleName))
                throw new ArgumentException("muscle name should not be empty!");
        });

        newExerciseDto.HowTos?.ForEach(x =>
        {
            if (string.IsNullOrEmpty(x.Name))
                throw new ArgumentException("how to name should not be empty!");

            if (string.IsNullOrEmpty(x.Url))
                throw new ArgumentException("how to url should not be empty!");
        });
    }

    // Create Bulk
public async Task<Result<bool>> CreateBulkAsync(List<ExerciseWriteDto> newExerciseDtos,
    CancellationToken cancellationToken)
{
    // maybe get the exercise names at the start of the thingy, check then go back to requester
    using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
        await _context.Database.BeginTransactionAsync(cancellationToken);
    try
    {
        // this is important, cause let's say you have 300 in the db, and u want to create 60 but 13 are duplicate,
        // you would want to know which are duplicated without goin to the db, dammit! 
        var exerciseNamesInTheDatabase = await _context.Exercises
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);
        
        var exerciseNamesFromDto = newExerciseDtos.Select(x => Utils.NormalizeString(x.Name)).ToList();
        var duplicates = exerciseNamesFromDto.Intersect(exerciseNamesInTheDatabase).ToList();

        if (duplicates.Any())
        {
            // maybe draw them for cool effect ? 
            return Result<bool>.Failure($"{string.Join(", ", duplicates)}");
        }


        List<string> muscleNames = newExerciseDtos
            .SelectMany(dto => dto.ExerciseMuscles.Select(m => Utils.NormalizeString(m.MuscleName)))
            .Distinct()
            .ToList();
        List<string> typeNames = newExerciseDtos
            .SelectMany(dto => dto.TrainingTypes)
            .Distinct()
            .Select(Utils.NormalizeString)
            .ToList();

        
        // Fetch all relevant data once
        Dictionary<string, Muscle> muscles = await _context.Muscles
            .Where(m => muscleNames.Contains(m.Name))
            .ToDictionaryAsync(m => m.Name, m => m, cancellationToken);
        Dictionary<string, TrainingType> trainingTypes = await _context.TrainingTypes
            .Where(tt => typeNames.Contains(tt.Name))
            .ToDictionaryAsync(tt => tt.Name, tt => tt, cancellationToken);

        // Identify missing training types
        var missingTrainingTypes = typeNames.Except(trainingTypes.Keys).ToList();
        if (missingTrainingTypes.Any())
        {
            var missingTypesMessage =
                $"The following training types could not be found: {string.Join(", ", missingTrainingTypes)}";
            _logger.LogError(missingTypesMessage);
            return Result<bool>.Failure(missingTypesMessage);
        }
        

        List<Exercise> exercises = new List<Exercise>();

        foreach (ExerciseWriteDto dto in newExerciseDtos)
        {
            Exercise newExercise = new Exercise
            {
                Name = Utils.NormalizeString(dto.Name),
                Description = dto.Description,
                HowTo = dto.HowTo,
                Difficulty = dto.Difficulty.GetValueOrDefault(),
                ExerciseHowTos = dto.HowTos is not null
                    ? dto.HowTos.Select(howTo => new ExerciseHowTo
                    {
                        Name = Utils.NormalizeString(howTo.Name),
                        Url = howTo.Url
                    }).ToList()
                    : [],
                ExerciseMuscles = dto.ExerciseMuscles.Select(em => new ExerciseMuscle
                {
                    Muscle = muscles[Utils.NormalizeString(em.MuscleName)],
                    IsPrimary = em.IsPrimary
                }).ToList(),
                TrainingTypes = dto.TrainingTypes.Select(tt => trainingTypes[Utils.NormalizeString(tt)]).ToList()
                
            };
            _logger.LogCritical($"Now Adding: {newExercise.Name}");

            exercises.Add(newExercise);
        }

        await _context.Exercises.AddRangeAsync(exercises, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
    catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
    {
        await transaction.RollbackAsync(cancellationToken);
        return Result<bool>.Failure("exercise already exists: " + ex.InnerException?.Message, ex);
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync(cancellationToken);
        return Result<bool>.Failure("Failed to create exercises: " + ex.Message, ex);
    }
}


    // Update
    public async Task<Result<bool>> UpdateAsync(int exerciseId, ExerciseWriteDto exerciseDto,
        CancellationToken cancellationToken)
    {
        using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            Exercise? exercise = await _context.Exercises
                .Include(e => e.ExerciseHowTos)
                .Include(e => e.ExerciseMuscles)
                .Include(e => e.TrainingTypes)
                .SingleOrDefaultAsync(e => e.Id == exerciseId, cancellationToken);

            if (exercise == null)
                return Result<bool>.Failure("Exercise not found.");

            exercise.Name = Utils.NormalizeString(exerciseDto.Name);
            exercise.Description =exerciseDto.Description;
            exercise.HowTo = exerciseDto.HowTo;
            exercise.Difficulty = exerciseDto.Difficulty.GetValueOrDefault();

            _context.ExerciseHowTos.RemoveRange(exercise.ExerciseHowTos);
            exercise.ExerciseHowTos = exerciseDto.HowTos.Select(howTo => new ExerciseHowTo
            {
                ExerciseId = exercise.Id,
                Name = Utils.NormalizeString(howTo.Name),
                Url = howTo.Url
            }).ToList();

            _context.ExerciseMuscles.RemoveRange(exercise.ExerciseMuscles);

            List<Muscle> muscles = await _context.Muscles
                .Where(m => exerciseDto.ExerciseMuscles.Select(em => Utils.NormalizeString(em.MuscleName))
                    .Contains(m.Name))
                .ToListAsync(cancellationToken);
            exercise.ExerciseMuscles = exerciseDto.ExerciseMuscles.Select(em => new ExerciseMuscle
            {
                Muscle = muscles.Single(m => m.Name == Utils.NormalizeString(em.MuscleName)),
                IsPrimary = em.IsPrimary
            }).ToList();

            exercise.TrainingTypes.Clear();
            List<TrainingType> trainingTypes = await _context.TrainingTypes
                .Where(tt => exerciseDto.TrainingTypes.Contains(tt.Name))
                .ToListAsync(cancellationToken);
            foreach (TrainingType? type in trainingTypes)
            {
                exercise.TrainingTypes.Add(type);
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<bool>.Failure("Failed to update exercise: " + ex.Message, ex);
        }
    }

    public async Task<Result<bool>> DeleteExerciseAsync(int exerciseId, CancellationToken cancellationToken)
    {
        using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            Exercise? exercise = await _context.Exercises
                .Include(e => e.ExerciseHowTos)
                .Include(e => e.ExerciseMuscles)
                .Include(e => e.TrainingTypes)
                .SingleOrDefaultAsync(e => e.Id == exerciseId, cancellationToken);

            if (exercise == null)
                return Result<bool>.Failure("Exercise not found.");

            _context.ExerciseHowTos.RemoveRange(exercise.ExerciseHowTos);
            _context.ExerciseMuscles.RemoveRange(exercise.ExerciseMuscles);

            _context.Exercises.Remove(exercise);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<bool>.Failure("Failed to delete exercise: " + ex.Message, ex);
        }
    }
    public async Task<Result<List<ExerciseSearchResultDto>>> SearchExercisesAsync(string searchTerm, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return Result<List<ExerciseSearchResultDto>>.Failure("Search term cannot be empty.");
            }

            searchTerm = Utils.NormalizeString(searchTerm);

            var exercises = await _context.Exercises
                .AsNoTracking()
                .Where(e => EF.Functions.Like(e.Name, $"%{searchTerm}%"))
                .ProjectTo<ExerciseSearchResultDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            if (exercises == null || exercises.Count == 0)
            {
                return Result<List<ExerciseSearchResultDto>>.Failure("No exercises found matching the search term.");
            }

            return Result<List<ExerciseSearchResultDto>>.Success(exercises);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ERROR]: something went wrong in {nameof(SearchExercisesAsync)}\n{ex.Message}\n{ex}");
            return Result<List<ExerciseSearchResultDto>>.Failure(ex.Message, ex);
        }
    }


    public async Task<Result<bool>> DeleteBulkAsync(List<string> exerciseNames, CancellationToken cancellationToken)
    {
            var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var exercisesToDelete = await _context.Exercises
                .Include(e => e.ExerciseHowTos)
                .Include(e => e.ExerciseMuscles)
                .Include(e => e.TrainingTypes)
                .Where(x => Utils.NormalizeStringList(exerciseNames).Contains(x.Name))
                .ToListAsync(cancellationToken);
            
            _context.RemoveRange(exercisesToDelete);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<bool>.Success(true);

        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<bool>.Failure(ex.Message, ex);
            
        }
    }

    private static IQueryable<ExerciseReadDto> ApplyFiltering(ExerciseQueryOptions options, IQueryable<ExerciseReadDto> query)
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
