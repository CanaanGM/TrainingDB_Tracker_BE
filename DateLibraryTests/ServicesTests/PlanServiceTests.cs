using DataLibrary.Context;
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
    private PlanService service;
    private Mock<ILogger<PlanService>> logger;

    public PlanServiceTests()
    {
        options = SqliteInMemory.CreateOptions<SqliteContext>();
        context = new SqliteContext(options);
        context.Database.EnsureCreated();
        logger = new Mock<ILogger<PlanService>>();
        service = new PlanService(context, logger.Object);
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
        Assert.Equal("The training plan must have at least one week with one day and one exercise.", result.ErrorMessage);
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
        Assert.Equal("The training plan must have at least one week with one day and one exercise.", result.ErrorMessage);
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