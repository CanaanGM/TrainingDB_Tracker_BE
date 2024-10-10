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

public interface IEquipmentService
{
    Task<Result<int>> UpsertAsync(EquipmentWriteDto newEquipmentDto, CancellationToken cancellationToken);

    Task<Result<bool>> CreateBulkAsync(List<EquipmentWriteDto> newEquipmentsDto,
        CancellationToken cancellationToken);

    Task<Result<List<EquipmentReadDto>>> GetAsync(CancellationToken cancellationToken);
    Task<Result<EquipmentReadDto>> GetByNameAsync(string equipmentName, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteAsync(string equipmentName, CancellationToken cancellationToken);
}

public class EquipmentService : IEquipmentService
{
    private readonly SqliteContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<EquipmentService> _logger;

    public EquipmentService( SqliteContext context, IMapper mapper, ILogger<EquipmentService> logger )
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }
    public async Task<Result<int>> UpsertAsync(EquipmentWriteDto newEquipmentDto, CancellationToken cancellationToken)
    {
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var equipmentAlreadyExist =
                await _context.Equipment.SingleOrDefaultAsync(x => x.Name == Utils.NormalizeString( newEquipmentDto.Name), cancellationToken);
            //TODO: this needs  to be better.
            if (equipmentAlreadyExist is not null)
            {
                // no need for mapper
                equipmentAlreadyExist.Name = newEquipmentDto.NewName is not null 
                    ? Utils.NormalizeString(newEquipmentDto.NewName) 
                    : equipmentAlreadyExist.Name; 
                equipmentAlreadyExist.Description = newEquipmentDto.Description ?? equipmentAlreadyExist.Description; 
                equipmentAlreadyExist.WeightKg = newEquipmentDto.WeightKg ?? equipmentAlreadyExist.WeightKg; 
                
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return Result<int>.Success(equipmentAlreadyExist.Id);
            }
            
            var newEquipment = _mapper.Map<Equipment>(newEquipmentDto);
            newEquipment.Name = Utils.NormalizeString(newEquipmentDto.Name);

            
            await _context.Equipment.AddAsync(newEquipment, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<int>.Success(newEquipment.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError($"[ERROR]: an error occured creating an equipment in {nameof(UpsertAsync)}.");
            return Result<int>.Failure($"an error occured creating the equipment: {ex.Message}", ex);
        }
    }

    public async Task<Result<bool>> CreateBulkAsync(List<EquipmentWriteDto> newEquipmentsDto,
        CancellationToken cancellationToken)
    {
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var newEquipments = newEquipmentsDto.Select(
                newEquipmentDto => new Equipment()
                    {
                        Name = Utils.NormalizeString(newEquipmentDto.Name),
                        Description = newEquipmentDto.Description,
                        WeightKg = newEquipmentDto.WeightKg ?? 0
                    }                    
                ).ToList();
            await _context.Equipment.AddRangeAsync(newEquipments, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError($"[ERROR]: an error occured creating an equipment in {nameof(CreateBulkAsync)}.");
            return Result<bool>.Failure($"an error occured creating the equipments: {ex.Message}", ex);
        }
    }
    
    
    public async Task<Result<List<EquipmentReadDto>>> GetAsync(CancellationToken cancellationToken)
    {
        try
        {
            var equipments = await _context.Equipment
                .AsNoTracking()
                .ProjectTo<EquipmentReadDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
            return Result<List<EquipmentReadDto>>.Success(equipments);
        }
        catch (Exception e)
        {
            return Result<List<EquipmentReadDto>>.Failure($"An error occured getting the equipments: {e.Message}", e);
        }
    }

    public async Task<Result<EquipmentReadDto>> GetByNameAsync(string equipmentName, CancellationToken cancellationToken)
    {
        try
        {
            var equipment = await _context.Equipment
                .AsNoTracking()
                .ProjectTo<EquipmentReadDto>(_mapper.ConfigurationProvider)
                .Where(x => x.Name == Utils.NormalizeString( equipmentName))
                .SingleOrDefaultAsync(cancellationToken);
            
            return Result<EquipmentReadDto>.Success(equipment);
        }
        catch (Exception e)
        {
            return Result<EquipmentReadDto>.Failure($"An error occured getting the equipments: {e.Message}", e);
        }
    }

    public async Task<Result<bool>> DeleteAsync(string equipmentName, CancellationToken cancellationToken)
    {
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (string.IsNullOrEmpty(equipmentName?.Trim()))
                throw new ArgumentException("Name cannot be empty!");
            
            var equipmentToDelete =
                await _context.Equipment.SingleOrDefaultAsync(
                    x => x.Name == Utils.NormalizeString(equipmentName), cancellationToken
                    );

            if (equipmentToDelete is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<bool>.Failure($"Equipment with the name: {equipmentName}, does not exists");
            }

            _context.Remove(equipmentToDelete);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError($"[ERROR]: an error occured creating an equipment in {nameof(CreateBulkAsync)}.");
            return Result<bool>.Failure($"an error occured creating the equipments: {ex.Message}", ex);
        }
    } 
    
}