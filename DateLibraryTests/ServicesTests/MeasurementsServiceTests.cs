using DataLibrary.Dtos;
using DataLibrary.Models;
using DataLibrary.Services;
using DateLibraryTests.helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace DateLibraryTests.ServicesTests;

public class MeasurementsServiceTests : BaseTestClass
{
    private Mock<ILogger<MeasurementsService>> logger;
    private MeasurementsService service;
    
    public MeasurementsServiceTests() : base()
    {
        logger = new Mock<ILogger<MeasurementsService>>();
        service = new MeasurementsService(_context, _mapper, logger.Object);
    }

    [Fact]
    public async Task GetALlMeasurements_Empty_Returns_empty_list()
    {
        DatabaseHelpers.SeedDummyUsers(_context);
        var result = await service.GetAll(1, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetAllNotEmpty_returns_OrderedList_for_user()
    {
        DatabaseHelpers.SeedDummyUsers(_context);
        var currentDate = DateTime.UtcNow;
        _context.Measurements.AddRange(new List<Measurement>()
        {
            new Measurement()
            {
                Chest = 10,
                Hip = 10,
                Neck = 10,
                LeftThigh = 10, 
                LeftCalf = 10,
                RightCalf = 10,
                RightForearm = 10,
                RightThigh = 10,
                LeftForearm = 10,
                LeftUpperArm = 10,
                RightUpperArm = 10,
                WaistOnBelly = 10,
                WaistUnderBelly = 10,
                UserId = 1,
                CreatedAt = currentDate.AddDays(2)
            },
            new Measurement()
            {
                Chest = 666,
                Hip = 666,
                Neck = 666,
                LeftThigh = 666, 
                LeftCalf = 666,
                RightCalf = 666,
                RightForearm = 666,
                RightThigh = 666,
                LeftForearm = 666,
                LeftUpperArm = 666,
                RightUpperArm = 666,
                WaistOnBelly = 666,
                WaistUnderBelly = 666,
                UserId = 1,
                CreatedAt = currentDate
            },
            new Measurement()
            {
                Chest = 5,
                Hip = 5,
                Neck = 5,
                LeftThigh = 5, 
                LeftCalf = 5,
                RightCalf = 5,
                RightForearm = 5,
                RightThigh = 5,
                LeftForearm = 5,
                LeftUpperArm = 5,
                RightUpperArm = 5,
                WaistOnBelly = 5,
                WaistUnderBelly = 5,
                UserId = 3,
                CreatedAt = currentDate.AddDays(1)
            }
            
        });
        _context.SaveChanges();
        
        var result = await service.GetAll(1,new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(666, result.Value[0].Chest); // it should be ordered
    }

    [Fact]
    public async Task CreateAsync_Should_return_sucess()
    {
        DatabaseHelpers.SeedDummyUsers(_context);
        MeasurementsWriteDto measurementsWriteDto = new MeasurementsWriteDto()
        {
            Chest = 10,
            Hip = 10,
            Neck = 10,
            LeftThigh = 10,
            LeftCalf = 10,
            RightCalf = 10,
            RightForearm = 10,
            RightThigh = 10,
            LeftForearm = 10,
            LeftUpperArm = 10,
            RightUpperArm = 10,
            WaistOnBelly = 10,
            WaistUnderBelly = 10
        };

        var result = await service.CreateAsync(1, measurementsWriteDto, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.True(result.Value >= 1);

        // it's the only one there
        var newMeasuremnt = _context.Measurements.FirstOrDefault();
        var user = _context.Users.FirstOrDefault(x => x.Id == newMeasuremnt.UserId);
        Assert.NotNull(user);
        Assert.Equal("Canaan", user.Username);
        Assert.NotEmpty(user.Measurements);
        Assert.NotNull(newMeasuremnt);
        Assert.Equal(measurementsWriteDto.Chest,newMeasuremnt.Chest );
        Assert.Equal(measurementsWriteDto.Hip,newMeasuremnt.Hip );
        Assert.Equal(measurementsWriteDto.Neck,newMeasuremnt.Neck );
        Assert.Equal(measurementsWriteDto.LeftThigh,newMeasuremnt.LeftThigh );
        Assert.Equal(measurementsWriteDto.LeftCalf,newMeasuremnt.LeftCalf );
        Assert.Equal(measurementsWriteDto.RightCalf,newMeasuremnt.RightCalf);
        Assert.Equal(measurementsWriteDto.RightForearm,newMeasuremnt.RightForearm );
        Assert.Equal(measurementsWriteDto.LeftForearm,newMeasuremnt.LeftForearm );
        Assert.Equal(measurementsWriteDto.LeftUpperArm,newMeasuremnt.LeftUpperArm );
        Assert.Equal(measurementsWriteDto.RightUpperArm,newMeasuremnt.RightUpperArm );
        Assert.Equal(measurementsWriteDto.WaistOnBelly,newMeasuremnt.WaistOnBelly);
        Assert.Equal(measurementsWriteDto.WaistUnderBelly,newMeasuremnt.WaistUnderBelly);
        
        Assert.NotNull(newMeasuremnt.CreatedAt); // TODO: Do not use Assert.NotNull() on value type 'DateTime'.
    }
    [Fact]
    public async Task CreateAsync_NoBodyWeight_Components_Should_Create_propper_weight_return_sucess()
    {
        DatabaseHelpers.SeedDummyUsers(_context);
        MeasurementsWriteDto measurementsWriteDto = new MeasurementsWriteDto()
        {
            Chest = 10,
            Hip = 10,
            Neck = 10,
            LeftThigh = 10,
            LeftCalf = 10,
            RightCalf = 10,
            RightForearm = 10,
            RightThigh = 10,
            LeftForearm = 10,
            LeftUpperArm = 10,
            RightUpperArm = 10,
            WaistOnBelly = 10,
            WaistUnderBelly = 10,
            BodyWeight = 65.5
        };

        var result = await service.CreateAsync(2,measurementsWriteDto, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value); // TODO: Do not use Assert.NotNull() on value type 'int'
        Assert.True(result.Value >= 1);

        // it's the only one there
        var newMeasuremnt = _context.Measurements.FirstOrDefault();
        var user = _context.Users.FirstOrDefault(x => x.Id == newMeasuremnt.UserId);
        Assert.NotNull(user);
        Assert.Equal("Dante", user.Username);
        Assert.NotEmpty(user.Measurements);
        Assert.NotNull(newMeasuremnt);
        Assert.Equal(measurementsWriteDto.Chest,newMeasuremnt.Chest );
        Assert.Equal(measurementsWriteDto.Hip,newMeasuremnt.Hip );
        Assert.Equal(measurementsWriteDto.Neck,newMeasuremnt.Neck );
        Assert.Equal(measurementsWriteDto.LeftThigh,newMeasuremnt.LeftThigh );
        Assert.Equal(measurementsWriteDto.LeftCalf,newMeasuremnt.LeftCalf );
        Assert.Equal(measurementsWriteDto.RightCalf,newMeasuremnt.RightCalf);
        Assert.Equal(measurementsWriteDto.RightForearm,newMeasuremnt.RightForearm );
        Assert.Equal(measurementsWriteDto.LeftForearm,newMeasuremnt.LeftForearm );
        Assert.Equal(measurementsWriteDto.LeftUpperArm,newMeasuremnt.LeftUpperArm );
        Assert.Equal(measurementsWriteDto.RightUpperArm,newMeasuremnt.RightUpperArm );
        Assert.Equal(measurementsWriteDto.WaistOnBelly,newMeasuremnt.WaistOnBelly);
        Assert.Equal(measurementsWriteDto.WaistUnderBelly,newMeasuremnt.WaistUnderBelly);
        Assert.Equal(measurementsWriteDto.BodyWeight,newMeasuremnt.BodyWeight);
        Assert.NotNull(newMeasuremnt.CreatedAt);
        
    }
    [Fact]
    public async Task CreateAsync_BodyWeight_Components_Should_Create_propper_weight_return_sucess()
    {
        DatabaseHelpers.SeedDummyUsers(_context);
        MeasurementsWriteDto measurementsWriteDto = new MeasurementsWriteDto()
        {
            Chest = 10,
            Hip = 10,
            Neck = 10,
            LeftThigh = 10,
            LeftCalf = 10,
            RightCalf = 10,
            RightForearm = 10,
            RightThigh = 10,
            LeftForearm = 10,
            LeftUpperArm = 10,
            RightUpperArm = 10,
            WaistOnBelly = 10,
            WaistUnderBelly = 10,
            Protein = 12,
            Minerals = 12,
            BodyFatMass = 41,
            TotalBodyWater = 54
        };

        var result = await service.CreateAsync(3,measurementsWriteDto, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value); // TODO: Do not use Assert.NotNull() on value type 'int'
        Assert.True(result.Value >= 1);

        // it's the only one there
        var newMeasuremnt = _context.Measurements.FirstOrDefault();
        var user = _context.Users.FirstOrDefault(x => x.Id == newMeasuremnt.UserId);
        Assert.NotNull(user);
        Assert.Equal("Alphrad", user.Username);
        Assert.NotEmpty(user.Measurements);
        Assert.NotNull(newMeasuremnt);
        Assert.Equal(measurementsWriteDto.Chest,newMeasuremnt.Chest );
        Assert.Equal(measurementsWriteDto.Hip,newMeasuremnt.Hip );
        Assert.Equal(measurementsWriteDto.Neck,newMeasuremnt.Neck );
        Assert.Equal(measurementsWriteDto.LeftThigh,newMeasuremnt.LeftThigh );
        Assert.Equal(measurementsWriteDto.LeftCalf,newMeasuremnt.LeftCalf );
        Assert.Equal(measurementsWriteDto.RightCalf,newMeasuremnt.RightCalf);
        Assert.Equal(measurementsWriteDto.RightForearm,newMeasuremnt.RightForearm );
        Assert.Equal(measurementsWriteDto.LeftForearm,newMeasuremnt.LeftForearm );
        Assert.Equal(measurementsWriteDto.LeftUpperArm,newMeasuremnt.LeftUpperArm );
        Assert.Equal(measurementsWriteDto.RightUpperArm,newMeasuremnt.RightUpperArm );
        Assert.Equal(measurementsWriteDto.WaistOnBelly,newMeasuremnt.WaistOnBelly);
        Assert.Equal(measurementsWriteDto.WaistUnderBelly,newMeasuremnt.WaistUnderBelly);
        Assert.Equal(
            (measurementsWriteDto.BodyFatMass + measurementsWriteDto.Protein + measurementsWriteDto.Minerals + measurementsWriteDto.TotalBodyWater)
            ,newMeasuremnt.BodyWeight);
        Assert.NotNull(newMeasuremnt.CreatedAt);
    }
    [Fact]
    public async Task CreateAsync_BothBodyWeight_AndComponents_BodyWeightComponentsShouldTakePrecendance_Should_Create_propper_weight_return_sucess()
    {
        DatabaseHelpers.SeedDummyUsers(_context);
        MeasurementsWriteDto measurementsWriteDto = new MeasurementsWriteDto()
        {
            Chest = 10,
            Hip = 10,
            Neck = 10,
            LeftThigh = 10,
            LeftCalf = 10,
            RightCalf = 10,
            RightForearm = 10,
            RightThigh = 10,
            LeftForearm = 10,
            LeftUpperArm = 10,
            RightUpperArm = 10,
            WaistOnBelly = 10,
            WaistUnderBelly = 10,
            Protein = 12,
            Minerals = 12,
            BodyFatMass = 41,
            TotalBodyWater = 54,
            BodyWeight = 99
        };

        var result = await service.CreateAsync(4, measurementsWriteDto, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.True(result.Value >= 1);

        // it's the only one there
        var newMeasuremnt = _context.Measurements.FirstOrDefault();
        var user = _context.Users.FirstOrDefault(x => x.Id == newMeasuremnt.UserId);
        Assert.NotNull(user);
        Assert.Equal("Nero", user.Username);
        Assert.NotEmpty(user.Measurements);
        Assert.NotNull(newMeasuremnt);
        Assert.Equal(measurementsWriteDto.Chest,newMeasuremnt.Chest );
        Assert.Equal(measurementsWriteDto.Hip,newMeasuremnt.Hip );
        Assert.Equal(measurementsWriteDto.Neck,newMeasuremnt.Neck );
        Assert.Equal(measurementsWriteDto.LeftThigh,newMeasuremnt.LeftThigh );
        Assert.Equal(measurementsWriteDto.LeftCalf,newMeasuremnt.LeftCalf );
        Assert.Equal(measurementsWriteDto.RightCalf,newMeasuremnt.RightCalf);
        Assert.Equal(measurementsWriteDto.RightForearm,newMeasuremnt.RightForearm );
        Assert.Equal(measurementsWriteDto.LeftForearm,newMeasuremnt.LeftForearm );
        Assert.Equal(measurementsWriteDto.LeftUpperArm,newMeasuremnt.LeftUpperArm );
        Assert.Equal(measurementsWriteDto.RightUpperArm,newMeasuremnt.RightUpperArm );
        Assert.Equal(measurementsWriteDto.WaistOnBelly,newMeasuremnt.WaistOnBelly);
        Assert.Equal(measurementsWriteDto.WaistUnderBelly,newMeasuremnt.WaistUnderBelly);
        Assert.Equal(
            (measurementsWriteDto.BodyFatMass + measurementsWriteDto.Protein + measurementsWriteDto.Minerals + measurementsWriteDto.TotalBodyWater)
            ,newMeasuremnt.BodyWeight);
        Assert.NotNull(newMeasuremnt.CreatedAt);
        
    }

    [Fact]
    public async Task Update_Full_Should_Return_Suceess()
    {
        DatabaseHelpers.SeedDummyUsers(_context);
        var newMeasurements = new Measurement()
        {
            Chest = 10,
            Hip = 10,
            Neck = 10,
            LeftThigh = 10, 
            LeftCalf = 10,
            RightCalf = 10,
            RightForearm = 10,
            RightThigh = 10,
            LeftForearm = 10,
            LeftUpperArm = 10,
            RightUpperArm = 10,
            WaistOnBelly = 10,
            WaistUnderBelly = 10,
            UserId = 1
        };
        _context.Measurements.Add(newMeasurements);
        _context.SaveChanges();
        
        MeasurementsWriteDto updateMeasurementsWriteDto = new MeasurementsWriteDto()
        {
            Chest = 105,
            Hip = 60,
            Neck = 17,
            LeftThigh = 105,
            LeftCalf = 10,
            RightCalf = 103,
            RightForearm = 1220,
            RightThigh = 310,
            LeftForearm = 140,
            LeftUpperArm = 120,
            RightUpperArm = 130,
            WaistOnBelly = 160,
            WaistUnderBelly = 17,
            Protein = 812,
            Minerals = 912,
            BodyFatMass = 041,
            TotalBodyWater = 654
        };

        var result = await service.UpdateAsync(1,newMeasurements.Id, updateMeasurementsWriteDto,
            new CancellationToken());
        
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        var updatedEntry = _context.Measurements.FirstOrDefault();
        var user = _context.Users.FirstOrDefault(x => x.Id == updatedEntry.UserId);
        Assert.NotNull(user);
        Assert.Equal("Canaan", user.Username);
        Assert.NotNull(updatedEntry);
        Assert.Equal(updateMeasurementsWriteDto.Chest,updatedEntry.Chest );
        Assert.Equal(updateMeasurementsWriteDto.Hip,updatedEntry.Hip );
        Assert.Equal(updateMeasurementsWriteDto.Neck,updatedEntry.Neck );
        Assert.Equal(updateMeasurementsWriteDto.LeftThigh,updatedEntry.LeftThigh );
        Assert.Equal(updateMeasurementsWriteDto.LeftCalf,updatedEntry.LeftCalf );
        Assert.Equal(updateMeasurementsWriteDto.RightCalf,updatedEntry.RightCalf);
        Assert.Equal(updateMeasurementsWriteDto.RightForearm,updatedEntry.RightForearm );
        Assert.Equal(updateMeasurementsWriteDto.LeftForearm,updatedEntry.LeftForearm );
        Assert.Equal(updateMeasurementsWriteDto.LeftUpperArm,updatedEntry.LeftUpperArm );
        Assert.Equal(updateMeasurementsWriteDto.RightUpperArm,updatedEntry.RightUpperArm );
        Assert.Equal(updateMeasurementsWriteDto.WaistOnBelly,updatedEntry.WaistOnBelly);
        Assert.Equal(updateMeasurementsWriteDto.Protein,updatedEntry.Protein);
        Assert.Equal(updateMeasurementsWriteDto.Minerals,updatedEntry.Minerals);
        Assert.Equal(updateMeasurementsWriteDto.BodyFatMass,updatedEntry.BodyFatMass);
        Assert.Equal(updateMeasurementsWriteDto.TotalBodyWater,updatedEntry.TotalBodyWater);
        Assert.Equal(
            (updateMeasurementsWriteDto.BodyFatMass
             + updateMeasurementsWriteDto.Protein
             + updateMeasurementsWriteDto.Minerals
             + updateMeasurementsWriteDto.TotalBodyWater
             )
            ,updatedEntry.BodyWeight);
        
        Assert.NotNull(updatedEntry.CreatedAt);

        // the record was updated successfully 
        Assert.Equal( updateMeasurementsWriteDto.Chest, user.Measurements.First().Chest);
    }

    [Fact]
    public async Task DeleteAsync_Should_delete_successfuly()
    {
        DatabaseHelpers.SeedDummyUsers(_context);
        var newMeasurements = new Measurement()
        {
            Chest = 10,
            Hip = 10,
            Neck = 10,
            LeftThigh = 10, 
            LeftCalf = 10,
            RightCalf = 10,
            RightForearm = 10,
            RightThigh = 10,
            LeftForearm = 10,
            LeftUpperArm = 10,
            RightUpperArm = 10,
            WaistOnBelly = 10,
            WaistUnderBelly = 10,
            UserId = 1
        };
        _context.Measurements.Add(newMeasurements);
        _context.SaveChanges();

        var result = await service.DeleteAsync(1, newMeasurements.Id, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        
        Assert.Empty(_context.Measurements);
        
    }
    
}