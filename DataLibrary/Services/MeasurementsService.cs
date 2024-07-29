using AutoMapper;
using DataLibrary.Context;
using DataLibrary.Core;
using DataLibrary.Dtos;
using DataLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataLibrary.Services;

public class MeasurementsService
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

    public async Task<Result<List<MeasurementsReadDto>>> GetAll(int userId, CancellationToken cancellationToken)
    {
        try
        {
            var measurements = await _context.Measurements
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
            return Result<List<MeasurementsReadDto>>.Success(_mapper.Map<List<MeasurementsReadDto>>(measurements));
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ERROR]: Couldn't get measurements at this time, {ex}");
            return Result<List<MeasurementsReadDto>>.Failure($"Couldn't get measurements {ex.Message}", ex);
        }
    }

    public async Task<Result<int>> CreateAsync(int userId, MeasurementsWriteDto newMeasurementDto,
        CancellationToken cancellationToken)
    {
        var userResult = await ValidateUser(userId, cancellationToken);
        if (!userResult.IsSuccess)
            return Result<int>.Failure(userResult.ErrorMessage!);
        var currentUser = userResult.Value;

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            var newMeasurements = _mapper.Map<Measurement>(newMeasurementDto);

            if (newMeasurementDto.Minerals is not null
                && newMeasurementDto.Protein is not null
                && newMeasurementDto.TotalBodyWater is not null
                && newMeasurementDto.BodyFatMass is not null)
                newMeasurements.BodyWeight = newMeasurementDto.Minerals.Value + newMeasurementDto.Protein.Value +
                                             newMeasurementDto.TotalBodyWater.Value +
                                             newMeasurementDto.BodyFatMass.Value;
            else
                newMeasurements.BodyWeight = newMeasurementDto.BodyWeight;

            newMeasurements.UserId = currentUser.Id;

            await _context.Measurements.AddAsync(newMeasurements, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result<int>.Success(newMeasurements.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ERROR]: Couldn't get measurements at this time, {ex}");
            return Result<int>.Failure($"Couldn't create measurements {ex.Message}", ex);
        }
    }

    public async Task<Result<bool>> UpdateAsync(int userId, int measurementId, MeasurementsWriteDto newMeasurementDto,
        CancellationToken cancellationToken)
    {
        var measurementsToUpdate = await _context.Measurements
            .FirstOrDefaultAsync(x => x.Id == measurementId && x.UserId == userId, cancellationToken);
        if (measurementsToUpdate is null)
            return Result<bool>.Failure("Measurements record was not found");
        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            _mapper.Map(newMeasurementDto, measurementsToUpdate);
            if (newMeasurementDto.Minerals is not null
                && newMeasurementDto.Protein is not null
                && newMeasurementDto.TotalBodyWater is not null
                && newMeasurementDto.BodyFatMass is not null)
                measurementsToUpdate.BodyWeight = newMeasurementDto.Minerals.Value + newMeasurementDto.Protein.Value +
                                                  newMeasurementDto.TotalBodyWater.Value +
                                                  newMeasurementDto.BodyFatMass.Value;
            
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ERROR]: Couldn't update measurements at this time, {ex}");
            return Result<bool>.Failure($"failed to update measurements cause: {ex.Message}", ex);
        }
    }

    public async Task<Result<bool>> DeleteAsync(int userId, int measurementId, CancellationToken cancellationToken)
    {
        var userResult = await ValidateUser(userId, cancellationToken);
        if (!userResult.IsSuccess)
            return Result<bool>.Failure(userResult.ErrorMessage!);
        var currentUser = userResult.Value;
        
        var measurementToDelete = await _context.Measurements
            .SingleOrDefaultAsync(x => x.Id == measurementId && x.UserId == userId, cancellationToken);

        if (measurementToDelete is null)
            return Result<bool>.Failure($"Measurement with the id: {measurementId}, does not exists.");

        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            currentUser!.Measurements.Remove(currentUser.Measurements.First(x => x.Id == measurementToDelete.Id));
            _context.Users.Update(currentUser);
            _context.Remove(measurementToDelete);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[ERROR]: Couldn't delete measurements at this time, {ex}");

            return Result<bool>.Failure($"failed to delete measurements, cause: {ex.Message}", ex);
        }
    }
    
    private async Task<Result<User>> ValidateUser(int userId, CancellationToken cancellationToken) {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null) Result<User>.Failure("Invalid user");
        return Result<User>.Success(user!);
    }

}