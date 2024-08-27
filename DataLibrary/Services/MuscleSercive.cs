using AutoMapper;
using AutoMapper.QueryableExtensions;
using DataLibrary.Context;
using DataLibrary.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedLibrary.Core;
using SharedLibrary.Dtos;
using SharedLibrary.Helpers;

namespace DataLibrary.Services;
public interface IMuscleService
{
    Task<Result<bool>> CreateBulkAsync(ICollection<MuscleWriteDto> newMuscles, CancellationToken cancellationToken);
    Task<Result<int>> CreateMuscleAsync(MuscleWriteDto newMuscle, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteAsync(int muscleId, CancellationToken cancellationToken);
    Task<Result<List<MuscleReadDto>>> GetAllAsync(CancellationToken cancellationToken);
    Task<Result<List<MuscleReadDto>>> GetAllByGroupAsync(string muscleGroupName, CancellationToken cancellationToken);
    Task<Result<MuscleReadDto>> GetByNameAsync(string muscleName, CancellationToken cancellationToken);
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

    public async Task<Result<List<MuscleReadDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        try
        {
            return Result<List<MuscleReadDto>>.Success(
                _mapper.Map<List<MuscleReadDto>>(
                    await _context.Muscles
                        .AsNoTracking()
                        .ToListAsync(cancellationToken)
                    )
                );

        }
        catch (Exception ex)
        {
            _logger.LogError($"[ERROR]: {ex.Message} {nameof(GetAllAsync)}.\n", ex);
            return Result<List<MuscleReadDto>>.Failure($"something went wrong getting the muscles: {ex.Message}", ex);
        }
    }

    public async Task<Result<MuscleReadDto>> GetByNameAsync(string muscleName, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(muscleName))
                throw new ArgumentNullException("muscleName cannot be null");

            var muscle = await _context.Muscles
                        .AsNoTracking()
                        .SingleOrDefaultAsync(muscle => muscle.Name == Utils.NormalizeString(muscleName), cancellationToken);

            if (muscle is null)
                throw new Exception("Muscle was not found");


            return Result<MuscleReadDto>.Success(_mapper.Map<MuscleReadDto>(muscle));
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ERROR]: {ex.Message} {nameof(GetByNameAsync)}.\n", ex);
            return Result<MuscleReadDto>.Failure($"something went wrong getting the muscle: {ex.Message}", ex);
        }

    }

    public async Task<Result<List<MuscleReadDto>>> GetAllByGroupAsync(string muscleGroupName, CancellationToken cancellationToken)
    {
        try
        {
    
            if (string.IsNullOrEmpty(muscleGroupName))
                throw new ArgumentNullException("muscleGroupName cannot be null");
            var groupedMuscles = await _context.Muscles
                        .AsNoTracking()
                        .Where(muscle => muscle.MuscleGroup == Utils.NormalizeString(muscleGroupName))
                        .ToListAsync(cancellationToken);


            return Result<List<MuscleReadDto>>.Success(_mapper.Map<List<MuscleReadDto>>(groupedMuscles));
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ERROR]: {ex.Message} {nameof(GetAllByGroupAsync)}.\n", ex);
            return Result<List<MuscleReadDto>>.Failure("something went wrong getting the muscle: { ex.Message}", ex);
        }

    }

    public async Task<Result<int>> CreateMuscleAsync(MuscleWriteDto newMuscle, CancellationToken cancellationToken)
    {
        var validation = Validation.ValidateDtoStrings(newMuscle);
        if (!validation.IsSuccess)
            return Result<int>.Failure(validation.ErrorMessage);
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            Muscle newMusce = _mapper.Map<Muscle>(newMuscle);
            newMusce.Name = Utils.NormalizeString(newMusce.Name);
            newMusce.MuscleGroup = Utils.NormalizeString(newMusce.MuscleGroup);
            newMusce.Function = Utils.NormalizeString(newMusce.Function);
            // no need to check the above, cause normalize would throw a fit in case of null or empty.
            if (string.IsNullOrEmpty(newMuscle.WikiPageUrl)) throw new ArgumentException("WikiPage is important yo!");

            await _context.Muscles.AddAsync(newMusce, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<int>.Success(newMusce.Id);

        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError($"[ERROR]: {ex.Message} {nameof(CreateMuscleAsync)}.\n", ex);
            return Result<int>.Failure(ex.Message, ex);
        }


    }

    public async Task<Result<bool>> CreateBulkAsync(ICollection<MuscleWriteDto> newMuscles, CancellationToken cancellationToken)
    {
        using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            List<Muscle> normalizedMuscles = newMuscles.Select(
                newMuscleDto =>
                {
                    Muscle newMuscle = _mapper.Map<Muscle>(newMuscleDto);
                    newMuscle.Name = Utils.NormalizeString(newMuscle.Name!);
                    newMuscle.MuscleGroup = Utils.NormalizeString(newMuscle.MuscleGroup!);
                    newMuscle.Function = Utils.NormalizeString(newMuscle.Function!);
                    return newMuscle;
                }
                ).ToList();


            await _context.Muscles.AddRangeAsync(normalizedMuscles, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError($"[ERROR]: {ex.Message} {nameof(CreateBulkAsync)}.\n", ex);
            return Result<bool>.Failure(ex.Message, ex);
        }
        return Result<bool>.Success(true);
    }


    public async Task<Result<bool>> UpdateAsync(int muscleId, MuscleUpdateDto updatedMuscle, CancellationToken cancellationToken)
    {
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            Muscle? oldMuscle = await _context.Muscles.SingleOrDefaultAsync(x => x.Id == muscleId, cancellationToken);
            if (oldMuscle is null)
                throw new Exception("Muscle not Found, double check the name!");

            _mapper.Map(updatedMuscle, oldMuscle);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError($"[ERROR]: {ex.Message} {nameof(UpdateAsync)}.\n", ex);
            return Result<bool>.Failure(ex.Message, ex);
        }
    }

    public async Task<Result<bool>> DeleteAsync(int muscleId, CancellationToken cancellationToken)
    {
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            Muscle? muscleToTear = await _context.Muscles.SingleOrDefaultAsync(m => m.Id == muscleId, cancellationToken);
            if (muscleToTear is null)
                throw new Exception("Muscle was resilient to tearing . . . CASUE IT'S NOT THERE!!");

            _context.Muscles.Remove(muscleToTear);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError($"[ERROR]: {ex.Message} {nameof(DeleteAsync)}.\n", ex);
            return Result<bool>.Failure(ex.Message, ex);
        }
        return Result<bool>.Success(true);
    }

    public async Task<Result<List<MuscleReadDto>>> SearchMuscleAsync(string searchTerm,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return Result<List<MuscleReadDto>>.Failure("Search term cannot be empty.");
            }
            searchTerm = Utils.NormalizeString(searchTerm);

            var muscles = await _context.Muscles
                .AsNoTracking()
                .Where(e => EF.Functions.Like(e.Name,$"%{searchTerm}%" ))
                .ProjectTo<MuscleReadDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            if (muscles is null || muscles.Count() <= 0)
            {
                return Result<List<MuscleReadDto>>.Failure($"no muscles were found similar to {searchTerm}.");
            }

            return Result<List<MuscleReadDto>>.Success(muscles);

        }
        catch (Exception ex)
        {
            return Result<List<MuscleReadDto>>.Failure("Something went Wrong", ex);
        }
    }


}
