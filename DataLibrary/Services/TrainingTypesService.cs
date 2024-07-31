using AutoMapper;
using DataLibrary.Context;
using DataLibrary.Core;
using DataLibrary.Dtos;
using DataLibrary.Helpers;
using DataLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataLibrary.Services;

public interface ITrainingTypesService
{
    Task<Result<int>> UpsertAsync(TrainingTypeUpdateDto newTrainingType, CancellationToken cancellationToken);
    Task<Result<bool>> CreateBulkAsync(ICollection<TrainingTypeWriteDto> newTypes, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteAsync(int typeId, CancellationToken cancellationToken);
    Task<Result<List<TrainingTypeReadDto>>> GetAllAsync(CancellationToken cancellationToken);
}

public class TrainingTypesService : ITrainingTypesService
{
    private readonly SqliteContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<TrainingTypesService> _logger;

    public TrainingTypesService(SqliteContext context, IMapper mapper, ILogger<TrainingTypesService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<TrainingTypeReadDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        try
        {
            return Result<List<TrainingTypeReadDto>>.Success(
                _mapper.Map<List<TrainingTypeReadDto>>(
                    await _context.TrainingTypes
                        .AsNoTracking()
                        .ToListAsync(cancellationToken)
                ));
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ERROR]: an exception was thrown while calling {nameof(GetAllAsync)}/n{ex} ");
            return Result<List<TrainingTypeReadDto>>.Failure(ex.Message, ex);
        }
    }

    public async Task<Result<int>> UpsertAsync(TrainingTypeUpdateDto newTrainingType,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var oldType = await _context.TrainingTypes
                .Include(x => x.Language)
                .SingleOrDefaultAsync(x => x.Name == newTrainingType.Name, cancellationToken);

            var language = await _context.Languages
                .Where(x => x.Code == newTrainingType.LanguageCode)
                .FirstOrDefaultAsync(cancellationToken);
            if (language is null)
                return Result<int>.Failure("language is faulty");

            if (oldType is not null)
            {
                _logger.LogInformation($"{oldType.Name} was found, commencing update . . .");
                oldType.Name = newTrainingType.NewName ?? oldType.Name;
                oldType.Language = language;

                _context.TrainingTypes.Update(oldType);
            }
            else
            {
                _logger.LogInformation($"{newTrainingType.Name} was not found, commencing creation . . .");
                oldType = new TrainingType()
                {
                    Name = newTrainingType.Name,
                    Language = language,
                };
                await _context.TrainingTypes.AddAsync(oldType, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<int>.Success(oldType.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ERROR]: an exception was thrown while calling {nameof(UpsertAsync)}/n{ex} ");
            return Result<int>.Failure(ex.Message, ex);
        }
    }

    public async Task<Result<bool>> CreateBulkAsync(ICollection<TrainingTypeWriteDto> newTypes,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var transaction = await _context
                .Database
                .BeginTransactionAsync(cancellationToken);
            var languageNames = newTypes.Select(x => x.LanguageCode).ToList();

            var languageDict = await _context.Languages
                .Where(x => languageNames.Contains(x.Code))
                .ToDictionaryAsync(x => x.Code, c => c, cancellationToken);
            
            var normalizedTrainingTypes = newTypes.Select(nt =>
            {
                _logger.LogInformation($"creating: {nt.Name} . . . ");
                return new TrainingType()
                {
                    Name = nt.Name,
                    Language = languageDict[nt.LanguageCode]
                };
            }).ToList();

            await _context.TrainingTypes.AddRangeAsync(normalizedTrainingTypes, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ERROR]: an exception was thrown while calling {nameof(CreateBulkAsync)}/n{ex} ");
            return Result<bool>.Failure(ex.Message, ex);
        }

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> DeleteAsync(int typeId, CancellationToken cancellationToken)
    {
        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            var trainingTypeToRemove =
                await _context.TrainingTypes
                    .Include(x => x.Language)
                    .FirstOrDefaultAsync(x => x.Id == typeId, cancellationToken);
            if (trainingTypeToRemove is null)
            {
                _logger.LogError($"The type of the id: {typeId}, was not found");
                return Result<bool>.Failure("not found ~!");
            }

            _context.TrainingTypes.Remove(trainingTypeToRemove);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"[ERROR]: an exception was thrown while calling {nameof(DeleteAsync)} with an id of {typeId}./n{ex} ");
            return Result<bool>.Failure($" failed to remove TrainingType of the id: {ex.Message}.\n", ex);
        }
    }
}