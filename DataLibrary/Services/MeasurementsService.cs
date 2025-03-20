using AutoMapper;
using DataLibrary.Context;
using DataLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedLibrary.Core;
using SharedLibrary.Dtos;

namespace DataLibrary.Services;

public interface IMeasurementsService
{
    Task<Result<List<MeasurementsReadDto>>> GetAll(int userId,CancellationToken cancellationToken);
    Task<Result<int>> CreateAsync(int userId,MeasurementsWriteDto newMeasurementDto, CancellationToken cancellationToken);

    Task<Result<bool>> UpdateAsync(int userId,int measurementId, MeasurementsWriteDto newMeasurementDto,
        CancellationToken cancellationToken);

    Task<Result<bool>> DeleteAsync(int userId,int measurementId, CancellationToken cancellationToken);
    Task<Result<MeasurementsReadDto>> GetByIdAsync(int userId, int id, CancellationToken cancellationToken);
}

public class MeasurementsService : IMeasurementsService
{
    private readonly SqliteContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<MeasurementsService> _logger;

    public MeasurementsService(SqliteContext context, IMapper mapper, ILogger<MeasurementsService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<MeasurementsReadDto>>> GetAll(int userId,CancellationToken cancellationToken)
    {
        try
        {
            var measurements = await _context.Measurements
                .AsNoTracking()
                .OrderBy(x => x.CreatedAt)
                .Where(x => x.UserId == userId)
                .ToListAsync(cancellationToken);
            return Result<List<MeasurementsReadDto>>.Success(_mapper.Map <List<MeasurementsReadDto>>(measurements));
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ERROR]: Couldn't get measuremnts at this time, {ex}");
            return Result<List<MeasurementsReadDto>>.Failure($"Couldn't get measurements {ex.Message}", ex);
        }
    }

    public async Task<Result<MeasurementsReadDto>> GetByIdAsync(int userId, int id, CancellationToken cancellationToken)
    {
	    var measurement = await _context.Measurements
		    .AsNoTracking()
		    .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
		    return Result<MeasurementsReadDto>.Success(_mapper.Map<MeasurementsReadDto>(measurement));
    }

    public async Task<Result<int>> CreateAsync(int userId,MeasurementsWriteDto newMeasurementDto, CancellationToken cancellationToken)
    {
        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            
            var newMeasurements = _mapper.Map<Measurement>(newMeasurementDto);
            if (newMeasurementDto.Minerals is not null
                && newMeasurementDto.Protein is not null
                && newMeasurementDto.TotalBodyWater is not null
                && newMeasurementDto.BodyFatMass is not null)
                newMeasurements.BodyWeight = newMeasurementDto.Minerals.Value + newMeasurementDto.Protein.Value +
                                            newMeasurementDto.TotalBodyWater.Value + newMeasurementDto.BodyFatMass.Value;

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user is null)
                return Result<int>.Failure("User does not exists");
            newMeasurements.User = user;

            await _context.Measurements.AddAsync(newMeasurements, cancellationToken);
            
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result<int>.Success(newMeasurements.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ERROR]: Couldn't get measuremnts at this time, {ex}");
            return Result<int>.Failure($"Couldn't get measurements {ex.Message}", ex);
        }
    }

    public async Task<Result<bool>> UpdateAsync(int userId,int measurementId, MeasurementsWriteDto newMeasurementDto,
        CancellationToken cancellationToken)
    {
        try
        {
            var userWithMeasurements = await _context.Users
                .Include(x => x.Measurements)
                .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (userWithMeasurements is null) 
                return Result<bool>.Failure("user does not exists");
            
            var measurementsToUpdate = userWithMeasurements.Measurements
                    .FirstOrDefault(x => x.Id == measurementId);
            
            if (measurementsToUpdate is null)
                return Result<bool>.Failure("Measurements was not found");
       
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            
            _mapper.Map(newMeasurementDto, measurementsToUpdate);
            if (newMeasurementDto.Minerals is not null
                && newMeasurementDto.Protein is not null
                && newMeasurementDto.TotalBodyWater is not null
                && newMeasurementDto.BodyFatMass is not null)
                measurementsToUpdate.BodyWeight = newMeasurementDto.Minerals.Value + newMeasurementDto.Protein.Value +
                                                  newMeasurementDto.TotalBodyWater.Value + newMeasurementDto.BodyFatMass.Value;

            
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<bool>.Success(true);

        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"failed to update measurements cause: {ex.Message}", ex);
        }
    }

    public async Task<Result<bool>> DeleteAsync(int userId,int measurementId, CancellationToken cancellationToken)
    {
        var measurementToDelete = await _context.Measurements
            .SingleOrDefaultAsync(x => x.Id == measurementId, cancellationToken);
        
        if (measurementToDelete is null)
            return Result<bool>.Failure($"Measurement with the id: {measurementId}, does not exists.");

        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _context.Remove(measurementToDelete);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<bool>.Failure($"failed to delete measurements, cause: {ex.Message}", ex);

        }
    }
}