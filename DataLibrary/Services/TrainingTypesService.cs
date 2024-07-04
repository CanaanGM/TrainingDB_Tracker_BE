using AutoMapper;

using DataLibrary.Context;
using DataLibrary.Core;
using DataLibrary.Dtos;
using DataLibrary.Helpers;
using DataLibrary.Models;

using Microsoft.EntityFrameworkCore;

namespace DataLibrary.Services;
internal class TrainingTypesService : ITrainingTypesService
{
    private readonly SqliteContext _context;
    private readonly IMapper _mapper;

    public TrainingTypesService(SqliteContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
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
            return Result<List<TrainingTypeReadDto>>.Failure(ex.Message, ex);
        }
    }

    public async Task<Result<bool>> CreateAsync(TrainingTypeWriteDto newTrainingType, CancellationToken cancellationToken)
    {
        try
        {
            if (
                await _context.TrainingTypes
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Name == newTrainingType.Name, cancellationToken)
                is not null
                )
                return Result<bool>.Success(false);//TODO: later change to meaningful codes or something

            TrainingType newType = _mapper.Map<TrainingType>(newTrainingType);
            newType.Name = Utils.NormalizeString(newType.Name);

            await _context.TrainingTypes.AddAsync(newType, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {

            return Result<bool>.Failure(ex.Message, ex);
        }
    }

    public async Task<Result<bool>> CreateBulkAsync(HashSet<TrainingTypeWriteDto> newTypes, CancellationToken cancellationToken)
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

            return Result<bool>.Failure(ex.Message, ex);
        }
        return Result<bool>.Success(true);
    }

}
