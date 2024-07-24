using AutoMapper;
using DataLibrary.Context;
using DataLibrary.Core;
using DataLibrary.Dtos;
using DataLibrary.Helpers;
using DataLibrary.Models;
using DataLibrary.Services;
using DateLibraryTests.helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TestSupport.EfHelpers;

namespace DateLibraryTests.ServicesTests;

public class PlanServiceTests
{
    DbContextOptionsDisposable<SqliteContext> options;
    SqliteContext context;
    Profiles myProfile;
    MapperConfiguration? configuration;
    Mapper mapper;
    private PlanService service;
    private Mock<ILogger<PlanService>> logger;


    public PlanServiceTests()
    {
        options = SqliteInMemory.CreateOptions<SqliteContext>();
        context = new SqliteContext(options);
        context.Database.EnsureCreated();
        myProfile = new Profiles();
        configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));
        mapper = new Mapper(configuration);
        logger = new Mock<ILogger<PlanService>>();
        service = new PlanService(context, mapper, logger.Object);
    }


    [Fact]
    public async Task CreateAsyncWithEquipment_ShouldCreateTrainingPlanSuccessfully()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(context);

        var newPlanDto = _correctPlanWithEquipment;

        // Act
        var result = await service.CreateAsync(newPlanDto, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value > 0);

        var createdPlan = await GetCreatedPlan(result.Value);
        AssertPlanCreatedSuccessfully(newPlanDto, createdPlan);

        Assert.Equal(2, context.Database.SqlQueryRaw<int>("select count(*) from training_plan_equipment").ToList()[0]);
    }

    [Fact]
    public async Task CreateAsyncWithNoEquipment_ShouldCreateTrainingPlanSuccessfully()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(context);

        var newPlanDto = _correctPlanWithNoEquipment;

        // Act
        var result = await service.CreateAsync(newPlanDto, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value > 0);

        var createdPlan = await GetCreatedPlan(result.Value);
        AssertPlanCreatedSuccessfully(newPlanDto, createdPlan);

        Assert.Equal(0, context.Database.SqlQueryRaw<int>("select count(*) from training_plan_equipment").ToList()[0]);
    }

    [Fact]
    public async Task CreateAsync_MissingExercises_ShouldReturnErrorWithAllMissingExercises()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(context);
        var newPlanDto = _missingExerciesPlan;
        // Act
        var result = await service.CreateAsync(newPlanDto, new CancellationToken());
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("The following exercises do not exist: nargacuga, tigrex", result.ErrorMessage);
        Assert.Empty(context.TrainingPlans);
    }

    [Fact]
    public async Task CreateAsync_MissingTrainingTypes_ShouldReturnErrorWithAllMissingExercises()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(context);
        var newPlanDto = _missingTrainingTypesPlan;
        // Act
        var result = await service.CreateAsync(newPlanDto, new CancellationToken());
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("The following training types do not exist: white fatalis, safijiva", result.ErrorMessage);
        Assert.Empty(context.TrainingPlans);
    }

    [Fact]
    public async Task CreateAsync_EmptyExercises_ShouldReturnError()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(context);

        var newPlanDto = _noExercisePlan;

        // Act
        var result = await service.CreateAsync(newPlanDto, new CancellationToken());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("The training plan must have at least one week with one day and one exercise.",
            result.ErrorMessage);
        Assert.Empty(context.TrainingPlans);
    }

    [Fact]
    public async Task CreateAsync_InvalidEquipment_ShouldReturnError()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(context);

        var newPlanDto = _invalidEquipmentPlan;

        // Act
        var result = await service.CreateAsync(newPlanDto, new CancellationToken());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("The following equipment do not exist: noh", result.ErrorMessage);
        Assert.Empty(context.TrainingPlans);
    }

    [Fact]
    public async Task CreateAsync_EmptyPlan_ShouldReturnError()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(context);

        var newPlanDto = new TrainingPlanWriteDto
        {
            Name = "Empty Plan",
            Description = "An empty training plan",
            Notes = "Some notes",
            Equipemnt = new List<string>(),
            TrainingTypes = new List<string>(),
            Weeks = new List<TrainingWeekWriteDto>()
        };

        // Act
        var result = await service.CreateAsync(newPlanDto, new CancellationToken());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("The training plan must have at least one week with one day and one exercise.",
            result.ErrorMessage);
        Assert.Empty(context.TrainingPlans);
    }

    private void AssertPlanCreatedSuccessfully(TrainingPlanWriteDto newPlanDto, TrainingPlan createdPlan)
    {
        Assert.NotNull(createdPlan);
        Assert.Equal(Utils.NormalizeString(newPlanDto.Name), createdPlan.Name);
        Assert.Equal(newPlanDto.Description, createdPlan.Description);
        Assert.Equal(newPlanDto.Notes, createdPlan.Notes);
        Assert.NotEmpty(createdPlan.Weeks);
        Assert.All(createdPlan.Weeks, week => Assert.NotEmpty(week.Days));
        Assert.All(createdPlan.Weeks.SelectMany(week => week.Days), day => Assert.NotEmpty(day.Blocks));
        Assert.All(createdPlan.Weeks.SelectMany(week => week.Days).SelectMany(day => day.Blocks),
            block => Assert.NotEmpty(block.Exercises));
    }

    private async Task<TrainingPlan> GetCreatedPlan(int planId)
    {
        return await context.TrainingPlans
            .Include(tp => tp.Weeks)
            .ThenInclude(tw => tw.Days)
            .ThenInclude(td => td.Blocks)
            .ThenInclude(b => b.Exercises)
            .ThenInclude(be => be.Exercise)
            .Include(tp => tp.Equipment)
            .Include(tp => tp.TrainingTypes)
            .FirstOrDefaultAsync(tp => tp.Id == planId);
    }


    [Fact]
    public async Task UpdateAsync_MissingExercises_ShouldReturnFailure()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(context);

        var initialPlan = _correctPlanWithEquipment;
        var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
        Assert.True(initialResult.IsSuccess);

        var updateDto = new TrainingPlanWriteDto
        {
            Name = "Updated Training Plan",
            Description = "Updated description",
            Notes = "Updated notes",
            Equipemnt = new List<string> { "Steel Dumbbells", "Straight Barbell" },
            TrainingTypes = new List<string> { "Strength", "Cardio", "Flexibility" },
            Weeks = new List<TrainingWeekWriteDto>
            {
                new TrainingWeekWriteDto
                {
                    Name = "Updated Week 1",
                    OrderNumber = 1,
                    Days = new List<TrainingDaysWriteDto>
                    {
                        new TrainingDaysWriteDto
                        {
                            Name = "Updated Day 1",
                            OrderNumber = 1,
                            Blocks = new List<BlockWriteDto>
                            {
                                new BlockWriteDto
                                {
                                    Name = "Updated Block 1",
                                    Sets = 4,
                                    RestInSeconds = 90,
                                    Instructions = "Updated instructions",
                                    OrderNumber = 1,
                                    Exercises = new List<BlockExerciseWriteDto>
                                    {
                                        new BlockExerciseWriteDto
                                        {
                                            ExerciseName = "NonExistentExercise",
                                            Notes = "Updated notes",
                                            OrderNumber = 1,
                                            Repetitions = 12
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var updateResult = await service.UpdateAsync(initialResult.Value, updateDto, new CancellationToken());

        // Assert
        Assert.False(updateResult.IsSuccess);
        Assert.Equal("The following exercises do not exist: nonexistentexercise", updateResult.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_FullUpdate_ShouldUpdateTrainingPlanSuccessfully()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(context);

        var initialPlan = _correctPlanWithEquipment;
        var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
        Assert.True(initialResult.IsSuccess);

        var updateDto = new TrainingPlanWriteDto
        {
            Name = "Updated Training Plan",
            Description = "Updated description",
            Notes = "Updated notes",
            Equipemnt = new List<string> { "Steel Dumbbells", "Straight Barbell" },
            TrainingTypes = new List<string> { "Strength", "Cardio", "Flexibility" },
            Weeks = new List<TrainingWeekWriteDto>
            {
                new TrainingWeekWriteDto
                {
                    Name = "Updated Week 1",
                    OrderNumber = 1,
                    Days = new List<TrainingDaysWriteDto>
                    {
                        new TrainingDaysWriteDto
                        {
                            Name = "Updated Day 1",
                            OrderNumber = 1,
                            Blocks = new List<BlockWriteDto>
                            {
                                new BlockWriteDto
                                {
                                    Name = "Updated Block 1",
                                    Sets = 4,
                                    RestInSeconds = 90,
                                    Instructions = "Updated instructions",
                                    OrderNumber = 1,
                                    Exercises = new List<BlockExerciseWriteDto>
                                    {
                                        new BlockExerciseWriteDto
                                        {
                                            ExerciseName = "dumbbell bench press - flat",
                                            Notes = "Updated notes",
                                            OrderNumber = 1,
                                            Repetitions = 12
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var updateResult = await service.UpdateAsync(initialResult.Value, updateDto, new CancellationToken());

        // Assert
        Assert.True(updateResult.IsSuccess);

        var updatedPlan = await context.TrainingPlans
            .Include(tp => tp.Weeks)
            .ThenInclude(tw => tw.Days)
            .ThenInclude(td => td.Blocks)
            .ThenInclude(b => b.Exercises)
            .ThenInclude(be => be.Exercise)
            .Include(tp => tp.Equipment)
            .Include(tp => tp.TrainingTypes)
            .FirstOrDefaultAsync(tp => tp.Id == initialResult.Value);

        Assert.NotNull(updatedPlan);
        Assert.Equal(Utils.NormalizeString(updateDto.Name), updatedPlan.Name);
        Assert.Equal(updateDto.Description, updatedPlan.Description);
        Assert.Equal(updateDto.Notes, updatedPlan.Notes);
        Assert.NotEmpty(updatedPlan.Weeks);
        Assert.NotEmpty(updatedPlan.Equipment);
        Assert.NotEmpty(updatedPlan.TrainingTypes);
        Assert.Equal("updated block 1", updatedPlan.Weeks.First().Days.First().Blocks.First().Name);
    }

    [Fact]
    public async Task UpdateAsync_PartialUpdate_ShouldUpdateTrainingPlanSuccessfully()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(context);

        var initialPlan = _correctPlanWithEquipment;
        var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
        Assert.True(initialResult.IsSuccess);

        var partialUpdateDto = new TrainingPlanWriteDto
        {
            Name = "Partially Updated Training Plan",
            Description = "Updated description",
            Notes = "Updated notes",
            Equipemnt = new List<string> { "Steel Dumbbells" },
            TrainingTypes = new List<string> { "Strength", "Cardio" },
            Weeks = new List<TrainingWeekWriteDto>
            {
                new TrainingWeekWriteDto
                {
                    Name = "Updated Week 1",
                    OrderNumber = 1,
                    Days = new List<TrainingDaysWriteDto>
                    {
                        new TrainingDaysWriteDto
                        {
                            Name = "Updated Day 1",
                            OrderNumber = 1,
                            Blocks = new List<BlockWriteDto>
                            {
                                new BlockWriteDto
                                {
                                    Name = "Updated Block 1",
                                    Sets = 3,
                                    RestInSeconds = 90,
                                    Instructions = "Updated instructions",
                                    OrderNumber = 1,
                                    Exercises = new List<BlockExerciseWriteDto>
                                    {
                                        new BlockExerciseWriteDto
                                        {
                                            ExerciseName = "dumbbell bench press - flat",
                                            Notes = "Updated notes",
                                            OrderNumber = 1,
                                            Repetitions = 12
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var updateResult = await service.UpdateAsync(initialResult.Value, partialUpdateDto, new CancellationToken());

        // Assert
        Assert.True(updateResult.IsSuccess);

        var updatedPlan = await context.TrainingPlans
            .Include(tp => tp.Weeks)
            .ThenInclude(tw => tw.Days)
            .ThenInclude(td => td.Blocks)
            .ThenInclude(b => b.Exercises)
            .ThenInclude(be => be.Exercise)
            .Include(tp => tp.Equipment)
            .Include(tp => tp.TrainingTypes)
            .FirstOrDefaultAsync(tp => tp.Id == initialResult.Value);

        Assert.NotNull(updatedPlan);
        Assert.Equal(Utils.NormalizeString(partialUpdateDto.Name), updatedPlan.Name);
        Assert.Equal(partialUpdateDto.Description, updatedPlan.Description);
        Assert.Equal(partialUpdateDto.Notes, updatedPlan.Notes);
        Assert.NotEmpty(updatedPlan.Weeks);
        Assert.NotEmpty(updatedPlan.Equipment);
        Assert.NotEmpty(updatedPlan.TrainingTypes);
        Assert.Equal("updated block 1", updatedPlan.Weeks.First().Days.First().Blocks.First().Name);
    }

    
    [Fact]
    public async Task UpdateAsync_NoChanges_ShouldNotChangeTrainingPlan()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(context);

        var initialPlan = _correctPlanWithEquipment;
        var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
        Assert.True(initialResult.IsSuccess);

        // Act
        var updateResult = await service.UpdateAsync(initialResult.Value, initialPlan, new CancellationToken());

        // Assert
        Assert.True(updateResult.IsSuccess);

        var updatedPlan = await context.TrainingPlans
            .Include(tp => tp.Weeks)
            .ThenInclude(tw => tw.Days)
            .ThenInclude(td => td.Blocks)
            .ThenInclude(b => b.Exercises)
            .ThenInclude(be => be.Exercise)
            .Include(tp => tp.Equipment)
            .Include(tp => tp.TrainingTypes)
            .FirstOrDefaultAsync(tp => tp.Id == initialResult.Value);

        Assert.NotNull(updatedPlan);
        Assert.Equal(Utils.NormalizeString(initialPlan.Name), updatedPlan.Name);
        Assert.Equal(initialPlan.Description, updatedPlan.Description);
        Assert.Equal(initialPlan.Notes, updatedPlan.Notes);
        Assert.NotEmpty(updatedPlan.Weeks);
        Assert.NotEmpty(updatedPlan.Equipment);
        Assert.NotEmpty(updatedPlan.TrainingTypes);
        Assert.Equal("block 1", updatedPlan.Weeks.First().Days.First().Blocks.First().Name);
    }

    [Fact]
    public async Task UpdateAsync_OnlyEquipment_ShouldUpdateEquipmentSuccessfully()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(context);

        var initialPlan = _correctPlanWithEquipment;
        var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
        Assert.True(initialResult.IsSuccess);

        var updateDto = new TrainingPlanWriteDto
        {
            Name = initialPlan.Name,
            Description = initialPlan.Description,
            Notes = initialPlan.Notes,
            Equipemnt = new List<string> { "Steel Dumbbells", "Plastic Dumbbells" },
            TrainingTypes = initialPlan.TrainingTypes,
            Weeks = initialPlan.Weeks
        };

        // Act
        var updateResult = await service.UpdateAsync(initialResult.Value, updateDto, new CancellationToken());

        // Assert
        Assert.True(updateResult.IsSuccess);

        var updatedPlan = await context.TrainingPlans
            .Include(tp => tp.Equipment)
            .FirstOrDefaultAsync(tp => tp.Id == initialResult.Value);

        Assert.NotNull(updatedPlan);
        Assert.Equal(2, updatedPlan.Equipment.Count);
        Assert.Contains(updatedPlan.Equipment, e => e.Name == "plastic dumbbells");
    }

    [Fact]
    public async Task UpdateAsync_OnlyTrainingTypes_ShouldUpdateTrainingTypesSuccessfully()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(context);

        var initialPlan = _correctPlanWithEquipment;
        var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
        Assert.True(initialResult.IsSuccess);

        var updateDto = new TrainingPlanWriteDto
        {
            Name = initialPlan.Name,
            Description = initialPlan.Description,
            Notes = initialPlan.Notes,
            Equipemnt = initialPlan.Equipemnt,
            TrainingTypes = new List<string> { "Strength", "Flexibility" },
            Weeks = initialPlan.Weeks
        };

        // Act
        var updateResult = await service.UpdateAsync(initialResult.Value, updateDto, new CancellationToken());

        // Assert
        Assert.True(updateResult.IsSuccess);

        var updatedPlan = await context.TrainingPlans
            .Include(tp => tp.TrainingTypes)
            .FirstOrDefaultAsync(tp => tp.Id == initialResult.Value);

        Assert.NotNull(updatedPlan);
        Assert.Equal(2, updatedPlan.TrainingTypes.Count);
        Assert.Contains(updatedPlan.TrainingTypes, tt => tt.Name == "flexibility");
    }

    [Fact]
public async Task UpdateAsync_InvalidExerciseName_ShouldReturnError()
{
    // Arrange
    ProductionDatabaseHelpers.SeedProductionData(context);

    var initialPlan = _correctPlanWithEquipment;
    var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
    Assert.True(initialResult.IsSuccess);

    var updateDto = new TrainingPlanWriteDto
    {
        Name = initialPlan.Name,
        Description = initialPlan.Description,
        Notes = initialPlan.Notes,
        Equipemnt = initialPlan.Equipemnt,
        TrainingTypes = initialPlan.TrainingTypes,
        Weeks = new List<TrainingWeekWriteDto>
        {
            new TrainingWeekWriteDto
            {
                Name = "Week 1",
                OrderNumber = 1,
                Days = new List<TrainingDaysWriteDto>
                {
                    new TrainingDaysWriteDto
                    {
                        Name = "Day 1",
                        OrderNumber = 1,
                        Blocks = new List<BlockWriteDto>
                        {
                            new BlockWriteDto
                            {
                                Name = "Block 1",
                                Sets = 3,
                                RestInSeconds = 60,
                                Instructions = "Some instructions",
                                OrderNumber = 1,
                                Exercises = new List<BlockExerciseWriteDto>
                                {
                                    new BlockExerciseWriteDto
                                    {
                                        ExerciseName = "Invalid Exercise Name",
                                        Notes = "Some notes",
                                        OrderNumber = 1,
                                        Repetitions = 10
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    };

    // Act
    var updateResult = await service.UpdateAsync(initialResult.Value, updateDto, new CancellationToken());

    // Assert
    Assert.False(updateResult.IsSuccess);
    Assert.Equal("The following exercises do not exist: invalid exercise name", updateResult.ErrorMessage);
}

    [Fact]
    public async Task UpdateAsync_InvalidTrainingTypeName_ShouldReturnError()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(context);

        var initialPlan = _correctPlanWithEquipment;
        var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
        Assert.True(initialResult.IsSuccess);

        var updateDto = new TrainingPlanWriteDto
        {
            Name = initialPlan.Name,
            Description = initialPlan.Description,
            Notes = initialPlan.Notes,
            Equipemnt = initialPlan.Equipemnt,
            TrainingTypes = new List<string> { "Invalid Training Type" },
            Weeks = initialPlan.Weeks
        };

        // Act
        var updateResult = await service.UpdateAsync(initialResult.Value, updateDto, new CancellationToken());

        // Assert
        Assert.False(updateResult.IsSuccess);
        Assert.Equal("The following training types do not exist: invalid training type", updateResult.ErrorMessage);
    }
    
    [Fact]
    public async Task UpdateAsync_EmptyTrainingPlan_ShouldReturnError()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(context);

        var initialPlan = _correctPlanWithEquipment;
        var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
        Assert.True(initialResult.IsSuccess);

        var updateDto = new TrainingPlanWriteDto
        {
            Name = "Updated Plan",
            Description = "Updated Description",
            Notes = "Updated Notes",
            Equipemnt = new List<string>(),
            TrainingTypes = new List<string>(),
            Weeks = new List<TrainingWeekWriteDto>()
        };

        // Act
        var updateResult = await service.UpdateAsync(initialResult.Value, updateDto, new CancellationToken());

        // Assert
        Assert.False(updateResult.IsSuccess);
        Assert.Equal("The training plan must have at least one week with one day and one exercise.", updateResult.ErrorMessage);
    }
    
    
    [Fact]
    public async Task UpdateAsync_RemoveAllEquipment_ShouldUpdateSuccessfully()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(context);

        var initialPlan = _correctPlanWithEquipment;
        var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
        Assert.True(initialResult.IsSuccess);

        var updateDto = new TrainingPlanWriteDto
        {
            Name = initialPlan.Name,
            Description = initialPlan.Description,
            Notes = initialPlan.Notes,
            Equipemnt = new List<string>(),
            TrainingTypes = initialPlan.TrainingTypes,
            Weeks = initialPlan.Weeks
        };

        // Act
        var updateResult = await service.UpdateAsync(initialResult.Value, updateDto, new CancellationToken());

        // Assert
        Assert.True(updateResult.IsSuccess);

        var updatedPlan = await context.TrainingPlans
            .Include(tp => tp.Equipment)
            .FirstOrDefaultAsync(tp => tp.Id == initialResult.Value);

        Assert.NotNull(updatedPlan);
        Assert.Empty(updatedPlan.Equipment);
    }

    [Fact]
    public async Task UpdateAsync_RemoveAllWeeks_ShouldReturnError()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(context);

        var initialPlan = _correctPlanWithEquipment;
        var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
        Assert.True(initialResult.IsSuccess);

        var updateDto = new TrainingPlanWriteDto
        {
            Name = initialPlan.Name,
            Description = initialPlan.Description,
            Notes = initialPlan.Notes,
            Equipemnt = initialPlan.Equipemnt,
            TrainingTypes = initialPlan.TrainingTypes,
            Weeks = new List<TrainingWeekWriteDto>()
        };

        // Act
        var updateResult = await service.UpdateAsync(initialResult.Value, updateDto, new CancellationToken());

        // Assert
        Assert.False(updateResult.IsSuccess);
        Assert.Equal("The training plan must have at least one week with one day and one exercise.", updateResult.ErrorMessage);
    }


    [Fact]
    public async Task UpdateAsync_PartiallyRemoveBlocks_ShouldUpdateSuccessfully()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(context);

        var initialPlan = _correctPlanWithEquipment;
        var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
        Assert.True(initialResult.IsSuccess);

        var updateDto = new TrainingPlanWriteDto
        {
            Name = initialPlan.Name,
            Description = initialPlan.Description,
            Notes = initialPlan.Notes,
            Equipemnt = initialPlan.Equipemnt,
            TrainingTypes = initialPlan.TrainingTypes,
            Weeks = initialPlan.Weeks.Select(w => new TrainingWeekWriteDto
            {
                Name = w.Name,
                OrderNumber = w.OrderNumber,
                Days = w.Days.Select(d => new TrainingDaysWriteDto
                {
                    Name = d.Name,
                    OrderNumber = d.OrderNumber,
                    Blocks = d.Blocks.Take(1).ToList() // Take only the first block
                }).ToList()
            }).ToList()
        };

        // Act
        var updateResult = await service.UpdateAsync(initialResult.Value, updateDto, new CancellationToken());

        // Assert
        Assert.True(updateResult.IsSuccess);

        var updatedPlan = await context.TrainingPlans
            .Include(tp => tp.Weeks)
            .ThenInclude(tw => tw.Days)
            .ThenInclude(td => td.Blocks)
            .FirstOrDefaultAsync(tp => tp.Id == initialResult.Value);

        Assert.NotNull(updatedPlan);
        Assert.All(updatedPlan.Weeks, week =>
        {
            Assert.All(week.Days, day =>
            {
                Assert.Single(day.Blocks);
            });
        });
    }

    
    [Fact]
    public async Task UpdateAsync_UpdatePlanNameOnly_ShouldUpdateSuccessfully()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(context);

        var initialPlan = _correctPlanWithEquipment;
        var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
        Assert.True(initialResult.IsSuccess);

        var updateDto = new TrainingPlanWriteDto
        {
            Name = "Updated Plan Name",
            Description = initialPlan.Description,
            Notes = initialPlan.Notes,
            Equipemnt = initialPlan.Equipemnt,
            TrainingTypes = initialPlan.TrainingTypes,
            Weeks = initialPlan.Weeks
        };

        // Act
        var updateResult = await service.UpdateAsync(initialResult.Value, updateDto, new CancellationToken());

        // Assert
        Assert.True(updateResult.IsSuccess);

        var updatedPlan = await context.TrainingPlans
            .Include(tp => tp.Weeks)
            .ThenInclude(tw => tw.Days)
            .ThenInclude(td => td.Blocks)
            .FirstOrDefaultAsync(tp => tp.Id == initialResult.Value);

        Assert.NotNull(updatedPlan);
        Assert.Equal("updated plan name", updatedPlan.Name);
        Assert.Equal(initialPlan.Description, updatedPlan.Description);
        Assert.Equal(initialPlan.Notes, updatedPlan.Notes);
    }

    [Fact]
    public async Task UpdateAsync_UpdatePlanDescriptionOnly_ShouldUpdateSuccessfully()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(context);

        var initialPlan = _correctPlanWithEquipment;
        var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
        Assert.True(initialResult.IsSuccess);

        var updateDto = new TrainingPlanWriteDto
        {
            Name = initialPlan.Name,
            Description = "Updated Description",
            Notes = initialPlan.Notes,
            Equipemnt = initialPlan.Equipemnt,
            TrainingTypes = initialPlan.TrainingTypes,
            Weeks = initialPlan.Weeks
        };

        // Act
        var updateResult = await service.UpdateAsync(initialResult.Value, updateDto, new CancellationToken());

        // Assert
        Assert.True(updateResult.IsSuccess);

        var updatedPlan = await context.TrainingPlans
            .Include(tp => tp.Weeks)
            .ThenInclude(tw => tw.Days)
            .ThenInclude(td => td.Blocks)
            .FirstOrDefaultAsync(tp => tp.Id == initialResult.Value);

        Assert.NotNull(updatedPlan);
        Assert.Equal(Utils.NormalizeString( initialPlan.Name), updatedPlan.Name);
        Assert.Equal("Updated Description", updatedPlan.Description);
        Assert.Equal(initialPlan.Notes, updatedPlan.Notes);
    }
    [Fact]
    public async Task UpdateAsync_UpdatePlanNotesOnly_ShouldUpdateSuccessfully()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(context);

        var initialPlan = _correctPlanWithEquipment;
        var initialResult = await service.CreateAsync(initialPlan, new CancellationToken());
        Assert.True(initialResult.IsSuccess);

        var updateDto = new TrainingPlanWriteDto
        {
            Name = initialPlan.Name,
            Description = initialPlan.Description,
            Notes = "Updated Notes",
            Equipemnt = initialPlan.Equipemnt,
            TrainingTypes = initialPlan.TrainingTypes,
            Weeks = initialPlan.Weeks
        };

        // Act
        var updateResult = await service.UpdateAsync(initialResult.Value, updateDto, new CancellationToken());

        // Assert
        Assert.True(updateResult.IsSuccess);

        var updatedPlan = await context.TrainingPlans
            .Include(tp => tp.Weeks)
            .ThenInclude(tw => tw.Days)
            .ThenInclude(td => td.Blocks)
            .FirstOrDefaultAsync(tp => tp.Id == initialResult.Value);

        Assert.NotNull(updatedPlan);
        Assert.Equal(Utils.NormalizeString( initialPlan.Name), updatedPlan.Name);
        Assert.Equal(initialPlan.Description, updatedPlan.Description);
        Assert.Equal("Updated Notes", updatedPlan.Notes);
    }

    
     [Fact]
    public async Task DeleteAsync_ValidPlanId_ShouldDeleteSuccessfully()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(context);

        var newPlanDto = _correctPlanWithEquipment;
        var createResult = await service.CreateAsync(newPlanDto, new CancellationToken());
        Assert.True(createResult.IsSuccess);

        // Act
        var deleteResult = await service.DeleteAsync(createResult.Value, new CancellationToken());

        // Assert
        Assert.True(deleteResult.IsSuccess);
        var deletedPlan = await context.TrainingPlans
            .FirstOrDefaultAsync(tp => tp.Id == createResult.Value);
        Assert.Null(deletedPlan);
    }

    [Fact]
    public async Task DeleteAsync_InvalidPlanId_ShouldReturnFailure()
    {
        // Act
        var deleteResult = await service.DeleteAsync(999, new CancellationToken());

        // Assert
        Assert.False(deleteResult.IsSuccess);
        Assert.Equal("Training plan not found.", deleteResult.ErrorMessage);
    }
    
    [Fact]
    public async Task DeleteAsync_PlanWithAssociatedEntities_ShouldDeleteAllRelatedEntities()
    {
        // Arrange
        ProductionDatabaseHelpers.SeedProductionData(context);

        var newPlanDto = _correctPlanWithEquipment;
        var createResult = await service.CreateAsync(newPlanDto, new CancellationToken());
        Assert.True(createResult.IsSuccess);

        // Act
        var deleteResult = await service.DeleteAsync(createResult.Value, new CancellationToken());

        // Assert
        Assert.True(deleteResult.IsSuccess);

        var deletedPlan = await context.TrainingPlans
            .Include(tp => tp.Weeks)
            .ThenInclude(tw => tw.Days)
            .ThenInclude(td => td.Blocks)
            .ThenInclude(b => b.Exercises)
            .FirstOrDefaultAsync(tp => tp.Id == createResult.Value);
        Assert.Null(deletedPlan);

        var weeks = await context.TrainingWeeks
            .Where(tw => tw.TrainingPlanId == createResult.Value)
            .ToListAsync();
        Assert.Empty(weeks);

        var days = await context.TrainingDays
            .Where(td => td.TrainingWeek.TrainingPlanId == createResult.Value)
            .ToListAsync();
        Assert.Empty(days);

        var blocks = await context.Blocks
            .Where(b => b.TrainingDay.TrainingWeek.TrainingPlanId == createResult.Value)
            .ToListAsync();
        Assert.Empty(blocks);

        var blockExercises = await context.BlockExercises
            .Where(be => be.Block.TrainingDay.TrainingWeek.TrainingPlanId == createResult.Value)
            .ToListAsync();
        Assert.Empty(blockExercises);
    }
    
 [Fact]
public async Task DeleteAsync_ShouldDeleteAllRelatedEntities()
{
    // Arrange
    ProductionDatabaseHelpers.SeedProductionData(context);

    var newPlanDto = _correctPlanWithEquipment;
    var createResult = await service.CreateAsync(newPlanDto, new CancellationToken());
    Assert.True(createResult.IsSuccess);

    // Act
    var deleteResult = await service.DeleteAsync(createResult.Value, new CancellationToken());

    // Assert
    Assert.True(deleteResult.IsSuccess);

    // Check if the plan is deleted
    var deletedPlan = await context.TrainingPlans
        .FirstOrDefaultAsync(tp => tp.Id == createResult.Value);
    Assert.Null(deletedPlan);

    // Check if all related weeks are deleted
    var deletedWeeks = await context.TrainingWeeks
        .Where(tw => tw.TrainingPlanId == createResult.Value)
        .ToListAsync();
    Assert.Empty(deletedWeeks);

    // Check if all related days are deleted
    var deletedDays = await context.TrainingDays
        .Where(td => td.TrainingWeek.TrainingPlanId == createResult.Value)
        .ToListAsync();
    Assert.Empty(deletedDays);

    // Check if all related blocks are deleted
    var deletedBlocks = await context.Blocks
        .Where(b => b.TrainingDay.TrainingWeek.TrainingPlanId == createResult.Value)
        .ToListAsync();
    Assert.Empty(deletedBlocks);

    // Check if all related block exercises are deleted
    var deletedBlockExercises = await context.BlockExercises
        .Where(be => be.Block.TrainingDay.TrainingWeek.TrainingPlanId == createResult.Value)
        .ToListAsync();
    Assert.Empty(deletedBlockExercises);

    // Check if all related equipment links are deleted via raw SQL query
    var equipmentLinksCount =  context.Database
        .SqlQuery<int>($"SELECT COUNT(*) FROM training_plan_equipment WHERE training_plan_id = {createResult.Value}")
        .ToList()[0];
    Assert.Equal(0, equipmentLinksCount);

    // Check if all related training type links are deleted via raw SQL query
    var trainingTypeLinksCount =  context.Database
        .SqlQuery<int>($"SELECT COUNT(*) FROM training_plan_type WHERE training_plan_id = {createResult.Value}" )
        .ToList()[0];
    Assert.Equal(0, trainingTypeLinksCount);
}


[Fact]
public async Task GetByIdAsync_ShouldReturnTrainingPlan_WhenTrainingPlanExists()
{
    ProductionDatabaseHelpers.SeedProductionData(context);
    // Arrange
    var newPlanDto = _correctPlanWithEquipment;
    var createResult = await service.CreateAsync(newPlanDto, new CancellationToken());

    // Act
    var result = await service.GetByIdAsync(createResult.Value, new CancellationToken());

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(createResult.Value, result.Value.Id);
    Assert.NotEmpty(result.Value.Weeks);
    Assert.NotEmpty(result.Value.Equipemnt);
    Assert.NotEmpty(result.Value.TrainingTypes);
}

[Fact]
public async Task GetByIdAsync_ShouldReturnFailure_WhenTrainingPlanDoesNotExist()
{
    // Arrange
    var nonExistentPlanId = 999;

    // Act
    var result = await service.GetByIdAsync(nonExistentPlanId, new CancellationToken());

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal("Training plan not found.", result.ErrorMessage);
}

[Fact]
public async Task GetByIdAsync_ShouldReturnFailure_WhenExceptionOccurs()
{
    // Arrange
    var newPlanDto = _correctPlanWithEquipment;
    var createResult = await service.CreateAsync(newPlanDto, new CancellationToken());

    context.Dispose(); // Force an exception by disposing the context

    // Act
    var result = await service.GetByIdAsync(createResult.Value, new CancellationToken());

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains("Failed to retrieve training plan", result.ErrorMessage);
}

    
    private static readonly TrainingPlanWriteDto _correctPlanWithEquipment = new TrainingPlanWriteDto
    {
        Name = "New Training Plan",
        Description = "A new training plan for testing",
        Notes = "Some notes",
        Equipemnt = new List<string> { "steel Dumbbells", "Straight Barbell" },
        TrainingTypes = new List<string> { "Strength", "Cardio" },
        Weeks = new List<TrainingWeekWriteDto>
        {
            new TrainingWeekWriteDto
            {
                Name = "Week 1",
                OrderNumber = 1,
                Days = new List<TrainingDaysWriteDto>
                {
                    new TrainingDaysWriteDto
                    {
                        Name = "Day 1",
                        OrderNumber = 1,
                        Blocks = new List<BlockWriteDto>
                        {
                            new BlockWriteDto
                            {
                                Name = "Block 1",
                                Sets = 3,
                                RestInSeconds = 60,
                                Instructions = "Some instructions",
                                OrderNumber = 1,
                                Exercises = new List<BlockExerciseWriteDto>
                                {
                                    new BlockExerciseWriteDto
                                    {
                                        ExerciseName = "dumbbell bench press - flat",
                                        Notes = "Some notes",
                                        OrderNumber = 1,
                                        Repetitions = 10
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    };

    private static readonly TrainingPlanWriteDto _correctPlanWithNoEquipment = new TrainingPlanWriteDto
    {
        Name = "New Training Plan",
        Description = "A new training plan for testing",
        Notes = "Some notes",
        Equipemnt = new List<string> { },
        TrainingTypes = new List<string> { "Strength", "Cardio" },
        Weeks = new List<TrainingWeekWriteDto>
        {
            new TrainingWeekWriteDto
            {
                Name = "Week 1",
                OrderNumber = 1,
                Days = new List<TrainingDaysWriteDto>
                {
                    new TrainingDaysWriteDto
                    {
                        Name = "Day 1",
                        OrderNumber = 1,
                        Blocks = new List<BlockWriteDto>
                        {
                            new BlockWriteDto
                            {
                                Name = "Block 1",
                                Sets = 3,
                                RestInSeconds = 60,
                                Instructions = "Some instructions",
                                OrderNumber = 1,
                                Exercises = new List<BlockExerciseWriteDto>
                                {
                                    new BlockExerciseWriteDto
                                    {
                                        ExerciseName = "dumbbell bench press - flat",
                                        Notes = "Some notes",
                                        OrderNumber = 1,
                                        Repetitions = 10
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    };

    private static readonly TrainingPlanWriteDto _noExercisePlan = new TrainingPlanWriteDto
    {
        Name = "Plan With Empty Exercises",
        Description = "A training plan with empty exercises",
        Notes = "Some notes",
        Equipemnt = new List<string> { "steel Dumbbells", "Straight Barbell" },
        TrainingTypes = new List<string> { "Strength", "Cardio" },
        Weeks = new List<TrainingWeekWriteDto>
        {
            new TrainingWeekWriteDto
            {
                Name = "Week 1",
                OrderNumber = 1,
                Days = new List<TrainingDaysWriteDto>
                {
                    new TrainingDaysWriteDto
                    {
                        Name = "Day 1",
                        OrderNumber = 1,
                        Blocks = new List<BlockWriteDto>
                        {
                            new BlockWriteDto
                            {
                                Name = "Block 1",
                                Sets = 3,
                                RestInSeconds = 60,
                                Instructions = "Some instructions",
                                OrderNumber = 1,
                                Exercises = new List<BlockExerciseWriteDto>()
                            }
                        }
                    }
                }
            }
        }
    };


    private static readonly TrainingPlanWriteDto _missingTrainingTypesPlan = new TrainingPlanWriteDto
    {
        Name = "New Training Plan",
        Description = "A new training plan for testing",
        Notes = "Some notes",
        Equipemnt = new List<string> { "steel Dumbbells", "Straight Barbell" },
        TrainingTypes = new List<string> { "white fatalis", "safijiva", "bodyBuilding", "strength" },
        Weeks = new List<TrainingWeekWriteDto>
        {
            new TrainingWeekWriteDto
            {
                Name = "Week 1",
                OrderNumber = 1,
                Days = new List<TrainingDaysWriteDto>
                {
                    new TrainingDaysWriteDto
                    {
                        Name = "Day 1",
                        OrderNumber = 1,
                        Blocks = new List<BlockWriteDto>
                        {
                            new BlockWriteDto
                            {
                                Name = "Block 1",
                                Sets = 3,
                                RestInSeconds = 60,
                                Instructions = "Some instructions",
                                OrderNumber = 1,
                                Exercises = new List<BlockExerciseWriteDto>
                                {
                                    new BlockExerciseWriteDto
                                    {
                                        ExerciseName = "dumbbell bench press - flat",
                                        Notes = "Some notes",
                                        OrderNumber = 1,
                                        Repetitions = 10
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    };

    private static readonly TrainingPlanWriteDto _missingExerciesPlan = new TrainingPlanWriteDto
    {
        Name = "New Training Plan",
        Description = "A new training plan for testing",
        Notes = "Some notes",
        Equipemnt = new List<string> { "steel Dumbbells", "Straight Barbell" },
        TrainingTypes = new List<string> { "Strength", "Cardio" },
        Weeks = new List<TrainingWeekWriteDto>
        {
            new TrainingWeekWriteDto
            {
                Name = "Week 1",
                OrderNumber = 1,
                Days = new List<TrainingDaysWriteDto>
                {
                    new TrainingDaysWriteDto
                    {
                        Name = "Day 1",
                        OrderNumber = 1,
                        Blocks = new List<BlockWriteDto>
                        {
                            new BlockWriteDto
                            {
                                Name = "Block 1",
                                Sets = 3,
                                RestInSeconds = 60,
                                Instructions = "Some instructions",
                                OrderNumber = 1,
                                Exercises = new List<BlockExerciseWriteDto>
                                {
                                    new BlockExerciseWriteDto
                                    {
                                        ExerciseName = "nargacuga",
                                        Notes = "Some notes",
                                        OrderNumber = 1,
                                        Repetitions = 10
                                    },
                                    new BlockExerciseWriteDto
                                    {
                                        ExerciseName = "Tigrex",
                                        Notes = "Some notes",
                                        OrderNumber = 1,
                                        Repetitions = 10
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    };

    private static readonly TrainingPlanWriteDto _invalidEquipmentPlan = new TrainingPlanWriteDto
    {
        Name = "New Training Plan",
        Description = "A new training plan for testing",
        Notes = "Some notes",
        Equipemnt = new List<string> { "noh", "Straight Barbell" },
        TrainingTypes = new List<string> { "Strength", "Cardio" },
        Weeks = new List<TrainingWeekWriteDto>
        {
            new TrainingWeekWriteDto
            {
                Name = "Week 1",
                OrderNumber = 1,
                Days = new List<TrainingDaysWriteDto>
                {
                    new TrainingDaysWriteDto
                    {
                        Name = "Day 1",
                        OrderNumber = 1,
                        Blocks = new List<BlockWriteDto>
                        {
                            new BlockWriteDto
                            {
                                Name = "Block 1",
                                Sets = 3,
                                RestInSeconds = 60,
                                Instructions = "Some instructions",
                                OrderNumber = 1,
                                Exercises = new List<BlockExerciseWriteDto>
                                {
                                    new BlockExerciseWriteDto
                                    {
                                        ExerciseName = "dumbbell bench press - flat",
                                        Notes = "Some notes",
                                        OrderNumber = 1,
                                        Repetitions = 10
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    };
}