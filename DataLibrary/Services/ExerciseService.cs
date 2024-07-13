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

internal class ExerciseService : IExerciseService
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
            throw new Exception($"exercise : {exerciseName} does not exists.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ERROR]: something went wrong in {nameof(GetByNameAsync)}\n{ex.Message}\n{ex}");
            return Result<ExerciseReadDto>.Failure(ex.Message, ex);
        }
    }


    /// <summary>
    /// gets the exercises grouped by their muscle group in a paged list 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<Dictionary<string, List<ExerciseReadDto>>>> GetExercisesGroupedByTrainingTypeAsync(
        ExerciseQueryOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.Exercises
                .Include(e => e.TrainingTypes)
                .AsNoTracking();

            // Extract exercises and flatten the training types
            var exerciseTrainingTypes = await query
                .SelectMany(e => e.TrainingTypes.Select(t => new { Exercise = e, TrainingType = t.Name }))
                .ToListAsync(cancellationToken);

            // Group by TrainingType and map to DTO
            Dictionary<string, List<ExerciseReadDto>> groupedByType = exerciseTrainingTypes
                .GroupBy(x => x.TrainingType)
                .ToDictionary(
                    group => group.Key,
                    group => _mapper.Map<List<ExerciseReadDto>>(
                        group.Select(x => x.Exercise).ToList()
                    )
                );

            // Apply pagination on the groups
            Dictionary<string, List<ExerciseReadDto>> pagedGroups = groupedByType
                .OrderBy(g => g.Key) // Optionally order by the group key if needed
                .Skip((options.PageNumber - 1) * options.PageSize)
                .Take(options.PageSize)
                .ToDictionary(g => g.Key, g => g.Value);

            return Result<Dictionary<string, List<ExerciseReadDto>>>.Success(pagedGroups);
        }
        catch (Exception ex)
        {
            return Result<Dictionary<string, List<ExerciseReadDto>>>.Failure(
                "Failed to group exercises by training type: " + ex.Message, ex);
        }
    }


    // just a list of exercise names
    public async Task<Result<Dictionary<string, List<ExerciseReadDto>>>> GetByGroupAsync(
        ExerciseQueryOptions options,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            // Directly project and fetch only necessary data
            var exerciseMuscles = await _context.Exercises
                .Include(e => e.ExerciseMuscles) // Make sure to include ExerciseMuscles for access to MuscleGroup
                .ThenInclude(em => em.Muscle)
                .AsNoTracking()
                .SelectMany(
                    e => e.ExerciseMuscles,
                    (exercise, exerciseMuscle) => new
                    {
                        Exercise = exercise,
                        MuscleGroup = exerciseMuscle.Muscle.MuscleGroup
                    })
                .ToListAsync(cancellationToken);

            // Group by MuscleGroup
            Dictionary<string, List<ExerciseReadDto>> groupedByMuscleGroup = exerciseMuscles
                .GroupBy(x => x.MuscleGroup)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => _mapper.Map<ExerciseReadDto>(x.Exercise)).ToList()
                );

            // Apply Pagination to the groups dictionary
            Dictionary<string, List<ExerciseReadDto>> pagedGroups = groupedByMuscleGroup
                .OrderBy(g => g.Key) // Optional: Order by group key if needed
                .Skip((options.PageNumber - 1) * options.PageSize)
                .Take(options.PageSize)
                .ToDictionary(g => g.Key, g => g.Value);

            return Result<Dictionary<string, List<ExerciseReadDto>>>.Success(pagedGroups);
        }
        catch (Exception ex)
        {
            return Result<Dictionary<string, List<ExerciseReadDto>>>.Failure(
                "Failed to group exercises by Muscle Group: " + ex.Message, ex);
        }
    }


    // this is performance nightmare, make sure the db is indexed properly.
    // i already did add indexes on the tables, but double check after testing.
    public async Task<Result<PaginatedList<ExerciseReadDto>>> GetAsync(
        ExerciseQueryOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            IQueryable<ExerciseReadDto> query = _context.Exercises
                .AsNoTracking()
                .ProjectTo<ExerciseReadDto>(_mapper.ConfigurationProvider);

            int totalItemsCount = await query.CountAsync(cancellationToken); // Total number of items for the query

            query = ApplyFiltering(options, query);
            query = ApplySorting(options, query);

            int pageSize = options.PageSize;
            int currentPage = options.PageNumber;
            List<ExerciseReadDto> items = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            int totalPages = (int)Math.Ceiling(totalItemsCount / (double)pageSize);

            PaginatedList<ExerciseReadDto> result = new PaginatedList<ExerciseReadDto>
            {
                Items = items,
                Metadata = new PaginationMetadata
                {
                    TotalCount = totalItemsCount,
                    TotalPages = totalPages,
                    CurrentPage = currentPage,
                    PageSize = pageSize
                }
            };

            return Result<PaginatedList<ExerciseReadDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PaginatedList<ExerciseReadDto>>.Failure("Failed to retrieve exercises: " + ex.Message, ex);
        }
    }


    public async Task<Result<bool>> CreateAsync(ExerciseWriteDto newExerciseDto, CancellationToken cancellationToken)
    {
        using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Normalize and prepare lists for querying
            List<string> muscleNames = newExerciseDto.ExerciseMuscles.Select(em => Utils.NormalizeString(em.MuscleName))
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
                Description = Utils.NormalizeString(newExerciseDto.Description!),
                HowTo = Utils.NormalizeString(newExerciseDto.HowTo!),
                Difficulty = newExerciseDto.Difficulty.GetValueOrDefault(),
                ExerciseHowTos = newExerciseDto.HowTos.Select(howTo => new ExerciseHowTo
                {
                    Name = Utils.NormalizeString(howTo.Name),
                    Url = howTo.Url
                }).ToList(),
                ExerciseMuscles = newExerciseDto.ExerciseMuscles.Select(em => new ExerciseMuscle
                {
                    Muscle = muscles[Utils.NormalizeString(em.MuscleName)],
                    IsPrimary = em.IsPrimary
                }).ToList(),
                TrainingTypes = trainingTypes
            };

            await _context.Exercises.AddAsync(newExercise, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<bool>.Failure("Failed to create the exercise due to an error: " + ex.Message, ex);
        }
    }

    // Create Bulk
    public async Task<Result<bool>> CreateBulkAsync(List<ExerciseWriteDto> newExerciseDtos,
        CancellationToken cancellationToken)
    {
        using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
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

            List<Exercise> exercises = new List<Exercise>();

            foreach (ExerciseWriteDto dto in newExerciseDtos)
            {
                Exercise newExercise = new Exercise
                {
                    Name = Utils.NormalizeString(dto.Name),
                    Description = Utils.NormalizeString(dto.Description!),
                    HowTo = Utils.NormalizeString(dto.HowTo!),
                    Difficulty = dto.Difficulty.GetValueOrDefault(),
                    ExerciseHowTos = dto.HowTos.Count > 0
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
            exercise.Description = Utils.NormalizeString(exerciseDto.Description!);
            exercise.HowTo = Utils.NormalizeString(exerciseDto.HowTo!);
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

    // DELTE and UPDATE BULK LATOR 

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