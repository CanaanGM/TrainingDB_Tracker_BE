using AutoMapper;
using DataLibrary.Context;
using DataLibrary.Core;
using DataLibrary.Models;
using DataLibrary.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SharedLibrary.Core;
using SharedLibrary.Dtos;
using TestSupport.EfHelpers;
using TrainingTests.APIControllersUnitTests;
using TrainingTests.helpers;

namespace TrainingTests.ServicesTests;

public class MeasurementsServiceTests : BaseIntegrationTestClass
{
    private Mock<ILogger<MeasurementsService>> logger;
    private MeasurementsService service;


    public MeasurementsServiceTests()
    {
        logger = new Mock<ILogger<MeasurementsService>>();
        service = new MeasurementsService(_context, _mapper, logger.Object);

        ProductionDatabaseHelpers.SeedDummyUsers(_context);
    }

    [Fact]
    public async Task GetALlMeasurements_Empty_Returns_empty_list()
    {
        var result = await service.GetAll(1,new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetALlMeasurement_Empty_Returns_empty()
    {
	    var result = await service.GetByIdAsync(1,1,CancellationToken.None);
	    Assert.True(result.IsSuccess);
	    Assert.Null(result.Value);
    }
    
    [Fact]
    public async Task GetALlMeasurement_Not_Empty_Returns_One()
    {
	    ProductionDatabaseHelpers.SeedMeasurements(_context);
	    var result = await service.GetByIdAsync(1,1,CancellationToken.None);
	    Assert.True(result.IsSuccess);
	    Assert.
		    NotNull(result.Value);
	    var measurement = result.Value;
	    Assert.Equal(90.5, measurement.Hip );
	    Assert.Equal(100 , measurement.Chest);
	    Assert.Equal(85.2 , measurement.WaistUnderBelly);
	    Assert.Equal(88.1 , measurement.WaistOnBelly);
	    Assert.Equal(56.3 , measurement.LeftThigh);
	    Assert.Equal(56.4 , measurement.RightThigh);
	    Assert.Equal(38.5 , measurement.LeftCalf);
	    Assert.Equal(38.6 , measurement.RightCalf);
	    Assert.Equal(34.1 , measurement.LeftUpperArm);
	    Assert.Equal(28.3 , measurement.LeftForearm);
	    Assert.Equal(34.2 , measurement.RightUpperArm);
	    Assert.Equal(28.4 , measurement.RightForearm);
	    Assert.Equal(37.0 , measurement.Neck);
	    Assert.Equal(4.5 , measurement.Minerals);
	    Assert.Equal(7.2 , measurement.Protein);
	    Assert.Equal(40.0 , measurement.TotalBodyWater);
	    Assert.Equal( 15.0 , measurement.BodyFatMass);
	    Assert.Equal(70.0 , measurement.BodyWeight);
	    Assert.Equal( 21.4 , measurement.BodyFatPercentage);
	    Assert.Equal(32.0 , measurement.SkeletalMuscleMass);
	    Assert.Equal( 85.0 , measurement.InBodyScore);
	    Assert.Equal( 24.3 , measurement.BodyMassIndex);
	    Assert.Equal(1600 , measurement.BasalMetabolicRate);
	    Assert.Equal(9 , measurement.VisceralFatLevel);

    }
    
    [Fact]
    public async Task GetAllNotEmpty_returns_OrderedList()
    {
        ProductionDatabaseHelpers.SeedMeasurements(_context);
        
        var result = await service.GetAll(1, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);
        Assert.Single( result.Value);
        Assert.Equal(100.0, result.Value[0].Chest); // it should be ordered
    }

    [Fact]
    public async Task CreateAsync_Should_return_sucess()
    {
        MeasurementsWriteDto measurementsWriteDto = MeasurementsDtoFactory.CreateOneNoBodyWeight();

        var result = await service.CreateAsync(1, measurementsWriteDto, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.True(result.Value >= 1);

        // it's the only one there
        var newMeasuremnt = _context.Measurements.FirstOrDefault();
        Assert.NotNull(newMeasuremnt);
        Assert.Equal(measurementsWriteDto.Chest, newMeasuremnt.Chest);
        Assert.Equal(measurementsWriteDto.Hip, newMeasuremnt.Hip);
        Assert.Equal(measurementsWriteDto.Neck, newMeasuremnt.Neck);
        Assert.Equal(measurementsWriteDto.LeftThigh, newMeasuremnt.LeftThigh);
        Assert.Equal(measurementsWriteDto.LeftCalf, newMeasuremnt.LeftCalf);
        Assert.Equal(measurementsWriteDto.RightCalf, newMeasuremnt.RightCalf);
        Assert.Equal(measurementsWriteDto.RightForearm, newMeasuremnt.RightForearm);
        Assert.Equal(measurementsWriteDto.LeftForearm, newMeasuremnt.LeftForearm);
        Assert.Equal(measurementsWriteDto.LeftUpperArm, newMeasuremnt.LeftUpperArm);
        Assert.Equal(measurementsWriteDto.RightUpperArm, newMeasuremnt.RightUpperArm);
        Assert.Equal(measurementsWriteDto.WaistOnBelly, newMeasuremnt.WaistOnBelly);
        Assert.Equal(measurementsWriteDto.WaistUnderBelly, newMeasuremnt.WaistUnderBelly);
        Assert.NotNull(newMeasuremnt.CreatedAt); // TODO: Do not use Assert.NotNull() on value type 'DateTime'.
    }

    [Fact]
    public async Task CreateAsync_NoBodyWeight_Components_Should_Create_propper_weight_return_sucess()
    {
        MeasurementsWriteDto measurementsWriteDto = MeasurementsDtoFactory.CreateOneWithBodyWeight();

        var result = await service.CreateAsync(1, measurementsWriteDto, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value); // TODO: Do not use Assert.NotNull() on value type 'int'
        Assert.True(result.Value >= 1);

        // it's the only one there
        var newMeasuremnt = _context.Measurements.FirstOrDefault();
        Assert.NotNull(newMeasuremnt);
        Assert.Equal(measurementsWriteDto.Chest, newMeasuremnt.Chest);
        Assert.Equal(measurementsWriteDto.Hip, newMeasuremnt.Hip);
        Assert.Equal(measurementsWriteDto.Neck, newMeasuremnt.Neck);
        Assert.Equal(measurementsWriteDto.LeftThigh, newMeasuremnt.LeftThigh);
        Assert.Equal(measurementsWriteDto.LeftCalf, newMeasuremnt.LeftCalf);
        Assert.Equal(measurementsWriteDto.RightCalf, newMeasuremnt.RightCalf);
        Assert.Equal(measurementsWriteDto.RightForearm, newMeasuremnt.RightForearm);
        Assert.Equal(measurementsWriteDto.LeftForearm, newMeasuremnt.LeftForearm);
        Assert.Equal(measurementsWriteDto.LeftUpperArm, newMeasuremnt.LeftUpperArm);
        Assert.Equal(measurementsWriteDto.RightUpperArm, newMeasuremnt.RightUpperArm);
        Assert.Equal(measurementsWriteDto.WaistOnBelly, newMeasuremnt.WaistOnBelly);
        Assert.Equal(measurementsWriteDto.WaistUnderBelly, newMeasuremnt.WaistUnderBelly);
        Assert.Equal(measurementsWriteDto.BodyWeight, newMeasuremnt.BodyWeight);
        Assert.NotNull(newMeasuremnt.CreatedAt);
    }

    [Fact]
    public async Task CreateAsync_BodyWeight_Components_Should_Create_propper_weight_return_sucess()
    {
        MeasurementsWriteDto measurementsWriteDto = MeasurementsDtoFactory.CreateOneWithBMI();

        var result = await service.CreateAsync(1, measurementsWriteDto, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value); // TODO: Do not use Assert.NotNull() on value type 'int'
        Assert.True(result.Value >= 1);

        // it's the only one there
        var newMeasuremnt = _context.Measurements.FirstOrDefault();
        Assert.NotNull(newMeasuremnt);
        Assert.Equal(measurementsWriteDto.Chest, newMeasuremnt.Chest);
        Assert.Equal(measurementsWriteDto.Hip, newMeasuremnt.Hip);
        Assert.Equal(measurementsWriteDto.Neck, newMeasuremnt.Neck);
        Assert.Equal(measurementsWriteDto.LeftThigh, newMeasuremnt.LeftThigh);
        Assert.Equal(measurementsWriteDto.LeftCalf, newMeasuremnt.LeftCalf);
        Assert.Equal(measurementsWriteDto.RightCalf, newMeasuremnt.RightCalf);
        Assert.Equal(measurementsWriteDto.RightForearm, newMeasuremnt.RightForearm);
        Assert.Equal(measurementsWriteDto.LeftForearm, newMeasuremnt.LeftForearm);
        Assert.Equal(measurementsWriteDto.LeftUpperArm, newMeasuremnt.LeftUpperArm);
        Assert.Equal(measurementsWriteDto.RightUpperArm, newMeasuremnt.RightUpperArm);
        Assert.Equal(measurementsWriteDto.WaistOnBelly, newMeasuremnt.WaistOnBelly);
        Assert.Equal(measurementsWriteDto.WaistUnderBelly, newMeasuremnt.WaistUnderBelly);
        Assert.Equal(
            (measurementsWriteDto.BodyFatMass + measurementsWriteDto.Protein + measurementsWriteDto.Minerals +
             measurementsWriteDto.TotalBodyWater)
            , newMeasuremnt.BodyWeight);
        Assert.NotNull(newMeasuremnt
            .CreatedAt); // TODO: Do not use Assert.NotNull() on value type 'DateTime'. Remove this assert.
    }

    [Fact]
    public async Task
        CreateAsync_BothBodyWeight_AndComponents_BodyWeightComponentsShouldTakePrecendance_Should_Create_propper_weight_return_sucess()
    {
        MeasurementsWriteDto measurementsWriteDto = MeasurementsDtoFactory.CreateOneWithBMIAndWeight();

        var result = await service.CreateAsync(1, measurementsWriteDto, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.True(result.Value >= 1);

        // it's the only one there
        var newMeasuremnt = _context.Measurements.FirstOrDefault();
        Assert.NotNull(newMeasuremnt);
        Assert.Equal(measurementsWriteDto.Chest, newMeasuremnt.Chest);
        Assert.Equal(measurementsWriteDto.Hip, newMeasuremnt.Hip);
        Assert.Equal(measurementsWriteDto.Neck, newMeasuremnt.Neck);
        Assert.Equal(measurementsWriteDto.LeftThigh, newMeasuremnt.LeftThigh);
        Assert.Equal(measurementsWriteDto.LeftCalf, newMeasuremnt.LeftCalf);
        Assert.Equal(measurementsWriteDto.RightCalf, newMeasuremnt.RightCalf);
        Assert.Equal(measurementsWriteDto.RightForearm, newMeasuremnt.RightForearm);
        Assert.Equal(measurementsWriteDto.LeftForearm, newMeasuremnt.LeftForearm);
        Assert.Equal(measurementsWriteDto.LeftUpperArm, newMeasuremnt.LeftUpperArm);
        Assert.Equal(measurementsWriteDto.RightUpperArm, newMeasuremnt.RightUpperArm);
        Assert.Equal(measurementsWriteDto.WaistOnBelly, newMeasuremnt.WaistOnBelly);
        Assert.Equal(measurementsWriteDto.WaistUnderBelly, newMeasuremnt.WaistUnderBelly);
        Assert.Equal(
            (measurementsWriteDto.BodyFatMass + measurementsWriteDto.Protein + measurementsWriteDto.Minerals +
             measurementsWriteDto.TotalBodyWater)
            , newMeasuremnt.BodyWeight);
        Assert.NotNull(newMeasuremnt.CreatedAt);
    }

    [Fact]
    public async Task Update_Full_Should_Return_Succeess()
    {
        ProductionDatabaseHelpers.SeedMeasurements(_context);

        MeasurementsWriteDto updateMeasurementsWriteDto = MeasurementsDtoFactory.CreateOneWithBMI();

        var result = await service.UpdateAsync(1, 1, updateMeasurementsWriteDto,
            new CancellationToken());

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        var updatedEntry = _context.Measurements.FirstOrDefault();
        Assert.NotNull(updatedEntry);
        Assert.Equal(updateMeasurementsWriteDto.Chest, updatedEntry.Chest);
        Assert.Equal(updateMeasurementsWriteDto.Hip, updatedEntry.Hip);
        Assert.Equal(updateMeasurementsWriteDto.Neck, updatedEntry.Neck);
        Assert.Equal(updateMeasurementsWriteDto.LeftThigh, updatedEntry.LeftThigh);
        Assert.Equal(updateMeasurementsWriteDto.LeftCalf, updatedEntry.LeftCalf);
        Assert.Equal(updateMeasurementsWriteDto.RightCalf, updatedEntry.RightCalf);
        Assert.Equal(updateMeasurementsWriteDto.RightForearm, updatedEntry.RightForearm);
        Assert.Equal(updateMeasurementsWriteDto.LeftForearm, updatedEntry.LeftForearm);
        Assert.Equal(updateMeasurementsWriteDto.LeftUpperArm, updatedEntry.LeftUpperArm);
        Assert.Equal(updateMeasurementsWriteDto.RightUpperArm, updatedEntry.RightUpperArm);
        Assert.Equal(updateMeasurementsWriteDto.WaistOnBelly, updatedEntry.WaistOnBelly);
        Assert.Equal(updateMeasurementsWriteDto.Protein, updatedEntry.Protein);
        Assert.Equal(updateMeasurementsWriteDto.Minerals, updatedEntry.Minerals);
        Assert.Equal(updateMeasurementsWriteDto.BodyFatMass, updatedEntry.BodyFatMass);
        Assert.Equal(updateMeasurementsWriteDto.TotalBodyWater, updatedEntry.TotalBodyWater);
        Assert.Equal(
            (updateMeasurementsWriteDto.BodyFatMass
             + updateMeasurementsWriteDto.Protein
             + updateMeasurementsWriteDto.Minerals
             + updateMeasurementsWriteDto.TotalBodyWater
            )
            , updatedEntry.BodyWeight);

        Assert.NotNull(updatedEntry.CreatedAt);
    }
 [Fact]
    public async Task Update_Full_WrongId_Should_Return_Failure()
    {
        ProductionDatabaseHelpers.SeedMeasurements(_context);

        MeasurementsWriteDto updateMeasurementsWriteDto = MeasurementsDtoFactory.CreateOneWithBMI();

        var result = await service.UpdateAsync(1, 2, updateMeasurementsWriteDto,
            new CancellationToken());

        Assert.False(result.IsSuccess);
        Assert.Equal("Measurements was not found", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteAsync_Should_delete_successfully()
    {
        ProductionDatabaseHelpers.SeedMeasurements(_context);

        var result = await service.DeleteAsync(1, 1, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        Assert.Equal(3, _context.Measurements.Count());

        var user =  _context.Users.Include(x => x.Measurements).First(x => x.Id == 1);
        Assert.Empty(user.Measurements);
    }
}