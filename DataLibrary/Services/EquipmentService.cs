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

public interface IEquipmentService
{
    Task<Result<int>> UpsertAsync(EquipmentWriteDto newEquipmentDto, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteAsync(string equipmentName, CancellationToken cancellationToken);
    Task<Result<List<EquipmentReadDto>>> GetAllByLanguageAsync(string languageCode, CancellationToken cancellationToken);
    Task<Result<EquipmentReadDto>> GetByNameAsync(string name, CancellationToken cancellationToken);
}

public class EquipmentService : IEquipmentService
{
    private readonly SqliteContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<EquipmentService> _logger;

    public EquipmentService(SqliteContext context, IMapper mapper, ILogger<EquipmentService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<int>> UpsertAsync(EquipmentWriteDto newEquipmentDto, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var language = await _context.Languages
                .FirstOrDefaultAsync(l => l.Code == Utils.NormalizeString(newEquipmentDto.LanguageCode),
                    cancellationToken);
            if (language == null)
                return Result<int>.Failure("Language code is invalid.");


            var normalizedEquipmentName = Utils.NormalizeString(newEquipmentDto.Name);
            var existingLocalizedEquipment = await _context.LocalizedEquipments
                .Include(le => le.Equipment)
                .FirstOrDefaultAsync(le => le.Name == normalizedEquipmentName && le.LanguageId == language.LanguageId,
                    cancellationToken);

            Equipment equipment;
            if (existingLocalizedEquipment is not null)
            {
                equipment = existingLocalizedEquipment.Equipment;
                equipment.WeightKg = newEquipmentDto.WeightKg ?? equipment.WeightKg;

                existingLocalizedEquipment.Description =
                    newEquipmentDto.Description ?? existingLocalizedEquipment.Description;
                existingLocalizedEquipment.HowTo = newEquipmentDto.HowTo ?? existingLocalizedEquipment.HowTo;

                if (!string.IsNullOrEmpty(newEquipmentDto.NewName))
                    existingLocalizedEquipment.Name = Utils.NormalizeString(newEquipmentDto.NewName);
            }
            else
            {
                equipment = new Equipment
                {
                    WeightKg = newEquipmentDto.WeightKg
                };

                _context.Equipment.Add(equipment);
                await _context.SaveChangesAsync(cancellationToken);

                existingLocalizedEquipment = new LocalizedEquipment
                {
                    Name = normalizedEquipmentName,
                    Description = newEquipmentDto.Description,
                    HowTo = newEquipmentDto.HowTo,
                    LanguageId = language.LanguageId,
                    EquipmentId = equipment.Id
                };

                _context.LocalizedEquipments.Add(existingLocalizedEquipment);
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<int>.Success(equipment.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<int>.Failure("An error occurred creating/updating the equipment.", ex);
        }
    }

    public async Task<Result<bool>> DeleteAsync(string equipmentName, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Normalize the equipment name to ensure consistent lookup
            var normalizedEquipmentName = Utils.NormalizeString(equipmentName);

            // Find the localized equipment entries that match the equipment name
            var localizedEquipments = await _context.LocalizedEquipments
                .Include(le => le.Equipment)
                .Where(le => le.Name == normalizedEquipmentName)
                .ToListAsync(cancellationToken);

            if (!localizedEquipments.Any())
            {
                return Result<bool>.Failure("Equipment not found.");
            }

            // Remove all associated LocalizedEquipment entries
            _context.LocalizedEquipments.RemoveRange(localizedEquipments);

            // If all LocalizedEquipments are to be deleted, delete the Equipment as well
            foreach (var equipmentGroup in localizedEquipments.GroupBy(le => le.EquipmentId))
            {
                var allLocalized = await _context.LocalizedEquipments
                    .Where(le => le.EquipmentId == equipmentGroup.Key)
                    .ToListAsync(cancellationToken);

                // Check if all localizations are being deleted
                if (allLocalized.Count == equipmentGroup.Count())
                {
                    var equipment = await _context.Equipment.FindAsync(equipmentGroup.Key);
                    _context.Equipment.Remove(equipment);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<bool>.Failure("An error occurred while deleting the equipment: " + ex.Message, ex);
        }
    }
    
    public async Task<Result<List<EquipmentReadDto>>> GetAllByLanguageAsync(string languageCode, CancellationToken cancellationToken)
    {
        try
        {
            
            // Check if language code is provided and normalize if not null or empty
            var normalizedLanguageCode = !string.IsNullOrWhiteSpace(languageCode) ? Utils.NormalizeString(languageCode) : null;

            var query = _context.LocalizedEquipments
                .Include(le => le.Equipment)
                .AsQueryable();

            if (!string.IsNullOrEmpty(normalizedLanguageCode))
            {
                query = query.Where(le => le.Language.Code == normalizedLanguageCode);
            }

            var equipmentList = await query
                .ProjectTo<EquipmentReadDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);


            return Result<List<EquipmentReadDto>>.Success(equipmentList);
        }
        catch (Exception ex)
        {
            return Result<List<EquipmentReadDto>>.Failure("An error occurred while retrieving the equipment: " + ex.Message, ex);
        }
    }
    
    public async Task<Result<EquipmentReadDto>> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<EquipmentReadDto>.Failure("Equipment name must not be empty.");

        try
        {
            var normalizedEquipmentName = Utils.NormalizeString(name);

            // Assume each equipment name is unique across all languages
            var equipmentDto = await _context.LocalizedEquipments
                .Where(le => le.Name == normalizedEquipmentName)
                .ProjectTo<EquipmentReadDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);  // Retrieves the first or default entry matching the name

            return equipmentDto == null 
                ? Result<EquipmentReadDto>.Failure("No equipment found with the specified name.")
                : Result<EquipmentReadDto>.Success(equipmentDto);
        }
        catch (Exception ex)
        {
            // Log the exception details while providing a generic error message to the client
            _logger.LogError($"Failed to retrieve equipment by name: {ex.Message}", ex);
            return Result<EquipmentReadDto>.Failure("An error occurred while retrieving the equipment.");
        }
    }


}