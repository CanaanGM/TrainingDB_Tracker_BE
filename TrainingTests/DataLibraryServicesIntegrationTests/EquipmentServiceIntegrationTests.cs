using System.Collections;
using AutoMapper;
using DataLibrary.Context;
using DataLibrary.Models;
using DataLibrary.Services;
using Microsoft.Extensions.Logging;
using Moq;
using SharedLibrary.Dtos;
using SharedLibrary.Helpers;
using TestSupport.EfHelpers;

namespace TrainingTests.ServicesTests;

public class EquipmentServiceIntegrationTests : BaseIntegrationTestClass
{

    private readonly Mock<ILogger<EquipmentServiceIntegration>> logger;
    private readonly EquipmentServiceIntegration _serviceIntegration;

    public EquipmentServiceIntegrationTests()
    {
        logger = new Mock<ILogger<EquipmentServiceIntegration>>();
        _serviceIntegration = new EquipmentServiceIntegration(_context, _mapper, logger.Object);
    }

    [Fact]
    public async Task GetAll_Empty_success()
    {
        var result = await _serviceIntegration.GetAsync(new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetAllNotEmpty_Success()
    {
        var date =  DateTime.Today;
        List<Equipment> equipments = new()
        {
            new Equipment()
            {
                Name = "straight bar",
                WeightKg = 10,
                Description = "Home training bar",
                CreatedAt = date
            },
            new Equipment()
            {
                Name = "swiggly bar",
                WeightKg = 7,
                Description = "Home training swiggly bar",
                CreatedAt = date.AddDays(1)
            }
        };
        _context.Equipment.AddRange(equipments);
        _context.SaveChanges();

        var result = await _serviceIntegration.GetAsync(new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value);
        Assert.Equal(equipments[0].Name, result.Value[0].Name);
        Assert.Equal(equipments[0].WeightKg, result.Value[0].WeightKg);
        Assert.Equal(equipments[0].Description, result.Value[0].Description);
        Assert.Equal(equipments[0].CreatedAt, result.Value[0].CreatedAt);
        Assert.Equal(equipments[1].Name, result.Value[1].Name);
        Assert.Equal(equipments[1].WeightKg, result.Value[1].WeightKg);
        Assert.Equal(equipments[1].Description, result.Value[1].Description);
        Assert.Equal(equipments[1].CreatedAt, result.Value[1].CreatedAt);
    }

    [Fact]
    public async Task Upsert_CreationShouldReturnSucess()
    {
        var n = new EquipmentWriteDto()
        {
            Name = "straight bar",
            WeightKg = 10,
            Description = "Home training bar"
        };


        var result = await _serviceIntegration.UpsertAsync(n, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.True(result.Value >= 1);

        var newEquip = _context.Equipment.First();
        Assert.Equal(newEquip.Name, n.Name);
        Assert.Equal(newEquip.WeightKg, n.WeightKg);
        Assert.Equal(newEquip.Description, n.Description);
        Assert.Equal(DateTime.UtcNow.Day, newEquip.CreatedAt?.Day);
    }

   public static IEnumerable<object[]> UpsertRecords() => new List<object[]>()
    {
        new[]
        {
            new Tuple<Equipment, EquipmentWriteDto>(new Equipment()
            {
                Name = "straight bar",
                Description = "Home training bar",
                WeightKg = 10
            }, new EquipmentWriteDto()
            {
                Name = "straight bar",
                NewName = "swiggly bar",
                Description = "Home training bar, the non olympic version",
                WeightKg = 101
            }),
        },
        new[]
        {
            new Tuple<Equipment, EquipmentWriteDto>(new Equipment()
            {
                Name = "straight bar",
                Description = "Home training bar",
                WeightKg = 10
            }, new EquipmentWriteDto()
            {
                Name = "straight bar",
                Description = "Home training bar, the non olympic version"
            }),
        },
        new[]
        {
            new Tuple<Equipment, EquipmentWriteDto>(new Equipment()
            {
                Name = "straight bar",
                Description = "Home training bar",
                WeightKg = 10
            }, new EquipmentWriteDto()
            {
                Name = "Straight bar",
                WeightKg = 101
            }),
        },
        new[]
        {
            new Tuple<Equipment, EquipmentWriteDto>(new Equipment()
            {
                Name = "straight bar",
                Description = "Home training bar",
                WeightKg = 10
            }, new EquipmentWriteDto()
            {
                Name = "Straight Bar",
                WeightKg = 101
            }),
        },
    };


    [Theory]
    [MemberData(nameof(UpsertRecords))]
    public async Task Upsert_UpdateShouldReturnSucess(Tuple<Equipment, EquipmentWriteDto> data)
    {
        var eq = data.Item1;
        _context.Equipment.Add(eq);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        
        var n = data.Item2;

        var result = await _serviceIntegration.UpsertAsync(n, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.True(result.Value >= 1);

        
        var newEquip = _context.Equipment.First();
        if(n.NewName is not null)
            Assert.Equal(newEquip.Name, Utils.NormalizeString(n.NewName));
        if (n.Description is not null)
            Assert.Equal(newEquip.Description, n.Description);
        if (n.WeightKg is not null)
            Assert.Equal(newEquip.WeightKg, n.WeightKg);
        
        Assert.Equal(DateTime.UtcNow.Day, newEquip.CreatedAt?.Day); // the day should match, it's created today after all.
    }   public static IEnumerable<object[]> CreationBulkFaulty() => new List<object[]>()
    {
        new[]
        {
            new List<EquipmentWriteDto> ()
            {
                new EquipmentWriteDto()
                {
                    Name = "straight bar",
                    Description = "Home training bar",
                    WeightKg = 10
                },
                new EquipmentWriteDto()
                {
                    Name = "plastic dumbbell",
                    Description = "plastic dumbbell",
                    WeightKg = 0.75
                },new EquipmentWriteDto()
                {
                    Name = "Swiggly bar",
                    Description = "Home Swiggly training bar",
                    WeightKg = 7.5
                },new EquipmentWriteDto()
                {
                    Name = "iron dumbbell",
                    Description = "iron dumbbell. daaauh",
                    WeightKg = 1.5
                }
            }
        },
    };
    
    [Theory]
    [MemberData(nameof(CreationBulkFaulty))]
    public async Task CreateBulk_ShouldReturnSucess(List<EquipmentWriteDto> data)
    {
        var result = await _serviceIntegration.CreateBulkAsync(data, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        Assert.Equal(data.Count, _context.Equipment.Count());
        
    }
    
    [Fact]
    public async Task Delete_NoRecords_Failure()
    {
        var equipmentName = "canaan";
        var result = await _serviceIntegration.DeleteAsync(equipmentName, new CancellationToken());
        Assert.False(result.IsSuccess);
        Assert.Equal($"Equipment with the name: {equipmentName}, does not exists", result.ErrorMessage);
    }

    [Fact]
    public async Task Delete_ExistingRecords_Sucess()
    {
        var n = new Equipment()
        {
            Name = "canaan",
            Description = "who is making this",
            WeightKg = 82.5
        };
        _context.Equipment.Add(n);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        
        
        var equipmentName = "canaan";
        var result = await _serviceIntegration.DeleteAsync(equipmentName, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        
        Assert.Empty(_context.Equipment);
    }
    
    [Theory]
    [InlineData(" ")]
    [InlineData("\n\t\r")]
    [InlineData(null)]
    
    public async Task Delete_ExistingRecords_FaultyName_returns_failure(string name)
    {
        
        var result = await _serviceIntegration.DeleteAsync(name, new CancellationToken());
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Exception);
        Assert.Equal("Name cannot be empty!", result.Exception.Message );
        Assert.IsType<ArgumentException>(result.Exception);

    }
    
}