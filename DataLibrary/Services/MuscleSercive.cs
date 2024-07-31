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

public interface IMuscleService
{
    Task<Result<List<MuscleReadDto>>> GetAllAsync(CancellationToken cancellationToken, string languageCode = "en");
    Task<Result<MuscleReadDto>> GetByNameAsync(string muscleName, CancellationToken cancellationToken);
    Task<Result<bool>> CreateBulkAsync(ICollection<MuscleWriteDto> newMuscles, CancellationToken cancellationToken);
    Task<Result<int>> CreateMuscleAsync(MuscleWriteDto newMuscle, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteAsync(int muscleId, CancellationToken cancellationToken);
    Task<Result<List<MuscleReadDto>>> GetAllByGroupAsync(string muscleGroupName, CancellationToken cancellationToken);
    Task<Result<bool>> UpdateAsync(int muscleId, MuscleUpdateDto updatedMuscle, CancellationToken cancellationToken);

    Task<Result<List<MuscleReadDto>>> SearchMuscleAsync(string searchTerm,
        CancellationToken cancellationToken);
}

public class MuscleService : IMuscleService
{
    private readonly SqliteContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<MuscleService> _logger;

    public MuscleService(SqliteContext context, IMapper mapper, ILogger<MuscleService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<MuscleReadDto>>> GetAllAsync(CancellationToken cancellationToken,
        string languageCode = "en")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                _logger.LogWarning("Language code was empty or whitespace.");
                return Result<List<MuscleReadDto>>.Failure("Language code cannot be empty.");
            }

            var normalizedLanguageCode = Utils.NormalizeString(languageCode);

            _logger.LogInformation("Fetching all muscles with normalized language code: {LanguageCode}",
                normalizedLanguageCode);

            var muscles = await _context.LocalizedMuscles
                .Where(lm => lm.Language.Code.ToLower() == normalizedLanguageCode.ToLower())
                .ProjectTo<MuscleReadDto>(_mapper.ConfigurationProvider)
                .OrderBy(x => x.MuscleName)
                .ToListAsync(cancellationToken);

            if (!muscles.Any())
            {
                _logger.LogWarning("No muscles found for language code: {LanguageCode}", normalizedLanguageCode);
                return Result<List<MuscleReadDto>>.Failure("No muscles found.");
            }

            _logger.LogInformation("Found {Count} muscles for language code: {LanguageCode}", muscles.Count,
                normalizedLanguageCode);
            return Result<List<MuscleReadDto>>.Success(muscles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching all muscles for language code: {LanguageCode}",
                languageCode);
            return Result<List<MuscleReadDto>>.Failure($"Error fetching all muscles: {ex.Message}");
        }
    }


    public async Task<Result<MuscleReadDto>> GetByNameAsync(string muscleName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(muscleName))
        {
            _logger.LogWarning("Muscle name was empty or whitespace.");
            return Result<MuscleReadDto>.Failure("Muscle name cannot be empty.");
        }

        try
        {
            _logger.LogInformation("Searching for muscle by name: {MuscleName}", muscleName);
            var normalizedMuscleName = Utils.NormalizeString(muscleName);

            var muscle = await _context.LocalizedMuscles
                .Where(lm => EF.Functions.Like(lm.Name.ToLower(), $"%{normalizedMuscleName}%"))
                .ProjectTo<MuscleReadDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (muscle == null)
            {
                _logger.LogWarning("No muscle found with name: {MuscleName}", muscleName);
                return Result<MuscleReadDto>.Failure("Muscle not found.");
            }

            _logger.LogInformation("Muscle found: {MuscleName}", muscle.MuscleName);
            return Result<MuscleReadDto>.Success(muscle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching for muscle by name: {MuscleName}", muscleName);
            return Result<MuscleReadDto>.Failure($"Error searching for muscle: {ex.Message}");
        }
    }


    public async Task<Result<List<MuscleReadDto>>> GetAllByGroupAsync(string muscleGroupName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(muscleGroupName))
        {
            _logger.LogWarning("Muscle group name was empty or whitespace.");
            return Result<List<MuscleReadDto>>.Failure("Muscle group name cannot be empty.");
        }

        try
        {
            _logger.LogInformation("Fetching muscles for muscle group: {MuscleGroupName}", muscleGroupName);
            var normalizedGroupName = Utils.NormalizeString(muscleGroupName);

            var muscles = await _context.LocalizedMuscles
                .Where(lmg => lmg.MuscleGroup == normalizedGroupName)
                .ProjectTo<MuscleReadDto>(_mapper.ConfigurationProvider)
                .Distinct()
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {Count} muscles associated with the muscle group {MuscleGroupName}.",
                muscles.Count, muscleGroupName);
            return Result<List<MuscleReadDto>>.Success(muscles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching muscles for muscle group {MuscleGroupName}.",
                muscleGroupName);
            return Result<List<MuscleReadDto>>.Failure($"Error fetching muscles for the muscle group: {ex.Message}");
        }
    }


    public async Task<Result<int>> CreateMuscleAsync(MuscleWriteDto newMuscle, CancellationToken cancellationToken)
    {
        var validation = ValidateNewMuscle(newMuscle);
        if (!validation.IsSuccess)
            return Result<int>.Failure(validation.ErrorMessage);

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            var language = await _context.Languages
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Code == newMuscle.LanguageCode, cancellationToken);
            if (language is null)
            {
                _logger.LogWarning("Language not found for code: {LanguageCode}", newMuscle.LanguageCode);
                return Result<int>.Failure("Language not found.");
            }

            var muscle = new Muscle();
            _context.Muscles.Add(muscle);

            var localizedMuscle = new LocalizedMuscle
            {
                LanguageId = language.Id,
                Name = newMuscle.Name,
                Function = newMuscle.Function,
                WikiPageUrl = newMuscle.WikiPageUrl,
                MuscleGroup = newMuscle.MuscleGroup,
                Muscle = muscle
            };

            muscle.LocalizedMuscles.Add(localizedMuscle);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result<int>.Success(muscle.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create muscle due to an exception.");
            return Result<int>.Failure("Failed to create muscle.");
        }
    }

    public Result ValidateNewMuscle(MuscleWriteDto newMuscle)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(newMuscle.Name))
            errors.Add("Name cannot be empty.");

        if (string.IsNullOrWhiteSpace(newMuscle.MuscleGroup))
            errors.Add("MuscleGroup cannot be empty.");

        if (string.IsNullOrWhiteSpace(newMuscle.LanguageCode))
            errors.Add("LanguageCode cannot be empty.");

        if (string.IsNullOrWhiteSpace(newMuscle.Function))
            errors.Add("Function cannot be empty.");

        return errors.Any()
            ? Result.Failure($"Could not create muscle: {string.Join("; ", errors)}\n")
            : Result.Success();
    }


    public async Task<Result<bool>> CreateBulkAsync(ICollection<MuscleWriteDto> newMuscles,
        CancellationToken cancellationToken)
    {
        if (newMuscles == null || newMuscles.Count == 0)
        {
            return Result<bool>.Failure("No muscles to create.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var languageCodes = newMuscles.Select(m => m.LanguageCode).Distinct().ToList();
            var languages = await _context.Languages
                .Where(l => languageCodes.Contains(l.Code))
                .ToListAsync(cancellationToken);

            if (languageCodes.Count != languages.Count)
                return Result<bool>.Failure("Incorrect Language");
            List<string> errors = [];
            foreach (var muscleDto in newMuscles)
            {
                var language = languages.FirstOrDefault(l => l.Code == muscleDto.LanguageCode);
                if (language == null)
                {
                    _logger.LogWarning("Language not found for code: {LanguageCode}", muscleDto.LanguageCode);
                    continue;
                }

                var res = ValidateNewMuscle(muscleDto);
                if (!res.IsSuccess)
                {
                    errors.Add(res.ErrorMessage);
                    _logger.LogWarning($"Could not create muscle {muscleDto}");
                    continue;
                }

                var muscle = new Muscle();
                var localizedMuscle = new LocalizedMuscle
                {
                    LanguageId = language.Id,
                    Name = muscleDto.Name,
                    Function = muscleDto.Function,
                    WikiPageUrl = muscleDto.WikiPageUrl,
                    MuscleGroup = muscleDto.MuscleGroup,
                    Muscle = muscle
                };

                muscle.LocalizedMuscles.Add(localizedMuscle);
                _context.Muscles.Add(muscle);
            }

            if (errors.Count > 0)
            {
                return Result<bool>.Failure(string.Join(", ", errors));
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to create muscles in bulk.");
            return Result<bool>.Failure("Failed to create muscles due to an exception.");
        }
    }

    public async Task<Result<bool>> UpdateAsync(int muscleId, MuscleUpdateDto updatedMuscle,
        CancellationToken cancellationToken)
    {
        if (updatedMuscle == null)
        {
            _logger.LogError("Update data cannot be null.");
            return Result<bool>.Failure("Update data cannot be null.");
        }

        var validation = ValidateNewMuscle(updatedMuscle);
        if (!validation.IsSuccess)
        {
            _logger.LogError($"error: {validation.ErrorMessage}");
            return Result<bool>.Failure(validation.ErrorMessage);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Attempting to update muscle with ID {MuscleId}", muscleId);

            var language = await _context.Languages
                .SingleOrDefaultAsync(l => l.Code == updatedMuscle.LanguageCode, cancellationToken);
            if (language == null)
            {
                _logger.LogWarning("Language with code {LanguageCode} not found.", updatedMuscle.LanguageCode);
                return Result<bool>.Failure("Language not found.");
            }

            var localizedMuscle = _context.LocalizedMuscles
                .Include(x => x.Muscle)
                .Include(r => r.Language)
                .FirstOrDefault(lm => lm.LanguageId == language.Id);


            if (localizedMuscle is null)
            {
                return Result<bool>.Failure($"muscle could not be found");
            }

            _mapper.Map(updatedMuscle, localizedMuscle);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Muscle with ID {MuscleId} updated successfully.", muscleId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating muscle with ID {MuscleId}.", muscleId);
            await transaction.RollbackAsync(cancellationToken);
            return Result<bool>.Failure($"Error updating muscle: {ex.Message}");
        }
    }


    public async Task<Result<bool>> DeleteAsync(int muscleId, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Attempting to delete muscle with ID {MuscleId}", muscleId);
            var muscle = await _context.Muscles.FindAsync(muscleId, cancellationToken);
            if (muscle == null)
            {
                _logger.LogWarning("Muscle with ID {MuscleId} not found.", muscleId);
                return Result<bool>.Failure("Muscle not found.");
            }

            _context.Muscles.Remove(muscle);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Muscle with ID {MuscleId} deleted successfully.", muscleId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting muscle with ID {MuscleId}.", muscleId);
            await transaction.RollbackAsync(cancellationToken);
            return Result<bool>.Failure($"Error deleting muscle: {ex.Message}");
        }
    }


    public async Task<Result<List<MuscleReadDto>>> SearchMuscleAsync(string searchTerm,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            _logger.LogWarning("Search term was empty or whitespace.");
            return Result<List<MuscleReadDto>>.Failure("Search term cannot be empty.");
        }

        try
        {
            _logger.LogInformation("Searching for muscles with term: {SearchTerm}", searchTerm);
            var normalizedSearchTerm = Utils.NormalizeString(searchTerm);

            var muscles = await _context.LocalizedMuscles
                .Where(lm => EF.Functions.Like(lm.Name.ToLower(), $"%{normalizedSearchTerm}%"))
                .ProjectTo<MuscleReadDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {Count} muscles matching the search term.", muscles.Count);
            return Result<List<MuscleReadDto>>.Success(muscles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching for muscles with term: {SearchTerm}", searchTerm);
            return Result<List<MuscleReadDto>>.Failure($"Error searching for muscles: {ex.Message}");
        }
    }
}