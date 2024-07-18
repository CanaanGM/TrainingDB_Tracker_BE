using AutoMapper;

using DataLibrary.Context;
using DataLibrary.Core;
using DataLibrary.Dtos;
using DataLibrary.Helpers;
using DataLibrary.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataLibrary.Services;
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

    public async Task<Result<int>> UpdateAsync(TrainingTypeWriteDto newTrainingType, CancellationToken cancellationToken)
    {
        try
        {
            var oldType = await _context.TrainingTypes
                    .SingleOrDefaultAsync(x => x.Name == newTrainingType.Name, cancellationToken);
            if (oldType is not null)
                return Result<int>.Success(oldType.Id);

            TrainingType newType = _mapper.Map<TrainingType>(newTrainingType);
            newType.Name = Utils.NormalizeString(newType.Name);

            await _context.TrainingTypes.AddAsync(newType, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(newType.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ERROR]: an exception was thrown while calling {nameof(UpdateAsync)}/n{ex} ");

            return Result<int>.Failure(ex.Message, ex);
        }
    }

    public async Task<Result<bool>> CreateBulkAsync(ICollection<TrainingTypeWriteDto> newTypes, CancellationToken cancellationToken)
    {
        using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await _context
            .Database
            .BeginTransactionAsync(cancellationToken);


        try
        {
            List<TrainingType> normalizedTrainingTypes = newTypes.Select(nt =>
            {
                TrainingType newType = _mapper.Map<TrainingType>(nt);
                newType.Name = Utils.NormalizeString(newType.Name);
                return newType;
            }).ToList();

            await _context.TrainingTypes.AddRangeAsync(normalizedTrainingTypes, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ERROR]: an exception was thrown while calling {nameof(CreateBulkAsync)}/n{ex} ");
            await transaction.RollbackAsync(cancellationToken);
            return Result<bool>.Failure(ex.Message, ex);
        }
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> Update(int typeId, TrainingTypeWriteDto updatedType, CancellationToken cancellationToken)
    {
        try
        {
            TrainingType? oldType = await _context.TrainingTypes.SingleOrDefaultAsync(ot => ot.Id == typeId, cancellationToken);
            if (oldType is null)
                throw new Exception("TODO: Change to appropriate exception type.. .err.. i mean\nTHE TRAINING TYPE IS NOT THERE!!");
            //TODO: remove the KEY in the database, the NAME is a contraint, cause i done goofed
            _mapper.Map(updatedType, oldType);
            _context.Entry(oldType).State = EntityState.Modified;
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ERROR]: an exception was thrown while calling {nameof(Update)} with an id of {typeId}./n{ex} ");
            return Result<bool>.Failure(ex.Message, ex);
        }
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> DeleteAsync(int typeId, CancellationToken cancellationToken)
    {
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _context.TrainingTypes.Remove(new TrainingType { Id = typeId });
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ERROR]: an exception was thrown while calling {nameof(DeleteAsync)} with an id of {typeId}./n{ex} ");
            await transaction.RollbackAsync(cancellationToken);
            return Result<bool>.Failure($" failed to remove TrainingType of the id: {ex.Message}.\n", ex);

        }
    }

}
