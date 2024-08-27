using AutoMapper;
using DataLibrary.Context;
using DataLibrary.Core;
using DataLibrary.Services;
using DateLibraryTests.helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SharedLibrary.Core;
using SharedLibrary.Dtos;
using SharedLibrary.Helpers;
using TestSupport.EfHelpers;

namespace DateLibraryTests.ServicesTests;

public class ExerciseServiceTests
{
    private DbContextOptions<SqliteContext> options;
    private Profiles myProfile;
    private MapperConfiguration configuration;
    private Mapper mapper;
    private Mock<ILogger<ExerciseService>> logger;
    private SqliteContext context;
    private ExerciseService service;

    public ExerciseServiceTests()
    {
        options = SqliteInMemory.CreateOptions<SqliteContext>();
        context = new SqliteContext(options);
        context.Database.EnsureCreated();

        myProfile = new Profiles();
        configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));
        mapper = new Mapper(configuration);
        logger = new Mock<ILogger<ExerciseService>>();
        service = new ExerciseService(context, mapper, logger.Object);
    }

    [Fact]
    public async Task GetByNameAsync_empty_should_return_failure()
    {
        var exerciseName = "dragon flag";
        var result = await service.GetByNameAsync(exerciseName, new CancellationToken());
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
        Assert.Equal($"exercise : {exerciseName} does not exists.", result.ErrorMessage);
    }

    [Theory]
    [InlineData("dragon flag")]
    public async Task GetByNameAsync_ExerciseExists_should_return_success_one(string exerciseName)
    {
        DatabaseHelpers.SeedTypesExercisesAndMuscles(context);
        var result = await service.GetByNameAsync(exerciseName, new CancellationToken());

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.IsType<ExerciseReadDto>(result.Value);

        Assert.Equal("deltoid anterior head", result.Value.ExerciseMuscles[0].Name);
        Assert.Equal("shoulders", result.Value.ExerciseMuscles[0].MuscleGroup);
        Assert.Equal("flexes and medially rotates the arm.", result.Value.ExerciseMuscles[0].Function);
        Assert.Equal("https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part",
            result.Value.ExerciseMuscles[0].WikiPageUrl);

        Assert.NotEmpty(result.Value.TrainingTypes);
        Assert.Equal(3, result.Value.TrainingTypes.Count);
    }

    [Theory]
    [InlineData("dragon flag")]
    [InlineData("rope jumping")]
    [InlineData("barbell curl")]
    public async Task GetByNameAsync_ExerciseExists_should_return_success(string exerciseName)
    {
        DatabaseHelpers.SeedTypesExercisesAndMuscles(context);
        var result = await service.GetByNameAsync(exerciseName, new CancellationToken());

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.IsType<ExerciseReadDto>(result.Value);
        Assert.NotEmpty(result.Value.ExerciseMuscles);
        Assert.NotEmpty(result.Value.TrainingTypes);
    }

    public static ICollection<object[]> NewExerciseWriteDtos() => new List<object[]>()
    {
        new object[]
        {
            new ExerciseWriteDto()
            {
                Name = "Deadlift",
                TrainingTypes = new List<string>() { "strength", "bodyBuilding" },
                ExerciseMuscles = new List<ExerciseMuscleWriteDto>()
                {
                    new ExerciseMuscleWriteDto()
                    {
                        MuscleName = "deltoid anterior head",
                        IsPrimary = true
                    },
                    new ExerciseMuscleWriteDto()
                    {
                        MuscleName = "",
                        IsPrimary = false
                    }
                },
                Description = "you lift a big ass weight off of the gorund",
                Difficulty = 5,
                HowTo = "grap weight, clinch thy booty, LIFT LIKE YOU'VE NEVER LIFTED BEFORE!"
            }
        },
        new object[]
        {
            new ExerciseWriteDto()
            {
                Name = "",
                TrainingTypes = new List<string>() { "strength", "bodyBuilding" },
                ExerciseMuscles = new List<ExerciseMuscleWriteDto>()
                {
                    new ExerciseMuscleWriteDto()
                    {
                        MuscleName = "deltoid anterior head",
                        IsPrimary = true
                    }
                },
                Description = "you lift a big ass weight off of the gorund",
                Difficulty = 5,
                HowTo = "grap weight, clinch thy booty, LIFT LIKE YOU'VE NEVER LIFTED BEFORE!"
            }
        },
        new object[]
        {
            new ExerciseWriteDto()
            {
                Name = "Deadlift",
                TrainingTypes = new List<string>() { },
                ExerciseMuscles = new List<ExerciseMuscleWriteDto>()
                {
                    new ExerciseMuscleWriteDto()
                    {
                        MuscleName = "deltoid anterior head",
                        IsPrimary = true
                    }
                },
                Description = "you lift a big ass weight off of the gorund",
                Difficulty = 5,
                HowTo = "grap weight, clinch thy booty, LIFT LIKE YOU'VE NEVER LIFTED BEFORE!"
            }
        },
        new object[]
        {
            new ExerciseWriteDto()
            {
                Name = "Deadlift",
                TrainingTypes = new List<string>() { "strength", "bodyBuilding" },
                ExerciseMuscles = new List<ExerciseMuscleWriteDto>()
                {
                },
                Description = "you lift a big ass weight off of the gorund",
                Difficulty = 5,
                HowTo = "grap weight, clinch thy booty, LIFT LIKE YOU'VE NEVER LIFTED BEFORE!"
            }
        },
        new object[]
        {
            new ExerciseWriteDto()
            {
                Name = "Deadlift",
                TrainingTypes = new List<string>() { "", "bodyBuilding" },
                ExerciseMuscles = new List<ExerciseMuscleWriteDto>()
                {
                    new ExerciseMuscleWriteDto()
                    {
                        MuscleName = "deltoid anterior head",
                        IsPrimary = true
                    }
                },
                Description = "you lift a big ass weight off of the gorund",
                Difficulty = 5,
                HowTo = "grap weight, clinch thy booty, LIFT LIKE YOU'VE NEVER LIFTED BEFORE!"
            }
        },
        new object[]
        {
            new ExerciseWriteDto()
            {
                Name = "Deadlift",
                TrainingTypes = new List<string>() { "strength", "bodyBuilding" },
                ExerciseMuscles = new List<ExerciseMuscleWriteDto>()
                {
                    new ExerciseMuscleWriteDto()
                    {
                        MuscleName = "deltoid anterior head",
                        IsPrimary = true
                    },
                },
                Description = "",
                Difficulty = 5,
                HowTo = "grap weight, clinch thy booty, LIFT LIKE YOU'VE NEVER LIFTED BEFORE!"
            }
        },
        new object[]
        {
            new ExerciseWriteDto()
            {
                Name = "Deadlift",
                TrainingTypes = new List<string>() { "strength", "bodyBuilding" },
                ExerciseMuscles = new List<ExerciseMuscleWriteDto>()
                {
                    new ExerciseMuscleWriteDto()
                    {
                        MuscleName = "deltoid anterior head",
                        IsPrimary = true
                    },
                },
                Description = "down by the river",
                Difficulty = 51,
                HowTo = "grap weight, clinch thy booty, LIFT LIKE YOU'VE NEVER LIFTED BEFORE!"
            }
        },
        new object[]
        {
            new ExerciseWriteDto()
            {
                Name = "Deadlift",
                TrainingTypes = new List<string>() { "strength", "bodyBuilding" },
                ExerciseMuscles = new List<ExerciseMuscleWriteDto>()
                {
                    new ExerciseMuscleWriteDto()
                    {
                        MuscleName = "deltoid anterior head",
                        IsPrimary = true
                    },
                },
                Description = "inky embers",
                Difficulty = -2,
                HowTo = "grap weight, clinch thy booty, LIFT LIKE YOU'VE NEVER LIFTED BEFORE!"
            }
        },
        new object[]
        {
            new ExerciseWriteDto()
            {
                Name = "Deadlift",
                TrainingTypes = new List<string>() { "strength", "bodyBuilding" },
                ExerciseMuscles = new List<ExerciseMuscleWriteDto>()
                {
                    new ExerciseMuscleWriteDto()
                    {
                        MuscleName = "deltoid anterior head",
                        IsPrimary = true
                    },
                },
                Description = "inky embers",
                Difficulty = 2,
                HowTo = ""
            }
        },
        new object[]
        {
            new ExerciseWriteDto()
            {
                Name = "Deadlift",
                TrainingTypes = new List<string>() { "strength", "bodyBuilding" },
                ExerciseMuscles = new List<ExerciseMuscleWriteDto>()
                {
                    new ExerciseMuscleWriteDto()
                    {
                        MuscleName = "deltoid anterior head",
                        IsPrimary = true
                    },
                },
                Description = "inky embers",
                Difficulty = 2,
                HowTo = "nyahahaha",
                HowTos = new List<ExerciseHowToWriteDto>()
                {
                    new ExerciseHowToWriteDto
                    {
                        Name = "",
                        Url = "null"
                    }
                }
            }
        },
        new object[]
        {
            new ExerciseWriteDto()
            {
                Name = "Deadlift",
                TrainingTypes = new List<string>() { "strength", "bodyBuilding" },
                ExerciseMuscles = new List<ExerciseMuscleWriteDto>()
                {
                    new ExerciseMuscleWriteDto()
                    {
                        MuscleName = "deltoid anterior head",
                        IsPrimary = true
                    },
                },
                Description = "inky embers",
                Difficulty = 2,
                HowTo = "nyahahaha",
                HowTos = new List<ExerciseHowToWriteDto>()
                {
                    new ExerciseHowToWriteDto
                    {
                        Name = "nyayayaya",
                        Url = ""
                    }
                }
            }
        },
    };

    [Theory]
    [MemberData(nameof(NewExerciseWriteDtos))]
    public async Task CreateAsync_incorrectInput_should_return_failure(ExerciseWriteDto nE)
    {
        var newExercise = nE;

        var result = await service.CreateAsync(newExercise, new CancellationToken());
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Exception);
        Assert.NotNull(result.ErrorMessage);
        Assert.IsType<ArgumentException>(result.Exception);
    }

    public static ICollection<object[]> NewExercisesBadTypeAndMuscles() => new List<object[]>
    {
        new object[]
        {
            new Tuple<string, ExerciseWriteDto>("One or more specified training types could not be found.",
                new ExerciseWriteDto()
                {
                    Name = "Deadlift",
                    TrainingTypes = new List<string>() { "strength", "bodyBuilding", "UNKOWN" },
                    ExerciseMuscles = new List<ExerciseMuscleWriteDto>()
                    {
                        new ExerciseMuscleWriteDto()
                        {
                            MuscleName = "deltoid anterior head",
                            IsPrimary = true
                        }
                    },
                    Description = "you lift a big ass weight off of the gorund",
                    Difficulty = 5,
                    HowTo = "grap weight, clinch thy booty, LIFT LIKE YOU'VE NEVER LIFTED BEFORE!"
                }
            ),
        },
        new object[]
        {
            new Tuple<string, ExerciseWriteDto>("One or more specified muscles could not be found.",
                new ExerciseWriteDto()
                {
                    Name = "Deadlift",
                    TrainingTypes = new List<string>() { "strength", "bodyBuilding" },
                    ExerciseMuscles = new List<ExerciseMuscleWriteDto>()
                    {
                        new ExerciseMuscleWriteDto()
                        {
                            MuscleName = "UNKNOWN",
                            IsPrimary = true
                        }
                    },
                    Description = "you lift a big ass weight off of the gorund",
                    Difficulty = 5,
                    HowTo = "grap weight, clinch thy booty, LIFT LIKE YOU'VE NEVER LIFTED BEFORE!"
                }
            ),
        }
    };

    [Theory]
    [MemberData(nameof(NewExercisesBadTypeAndMuscles))]
    public async Task CreateAsync_correctInput_missing_trainingType_should_return_failure(
        Tuple<string, ExerciseWriteDto> nE)
    {
        DatabaseHelpers.SeedTypesExercisesAndMuscles(context);
        var newExercise = nE.Item2;

        var result = await service.CreateAsync(newExercise, new CancellationToken());
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Exception);
        Assert.NotNull(result.ErrorMessage);
        Assert.Equal(nE.Item1, result.Exception.Message);
        Assert.IsType<Exception>(result.Exception);
    }


    public static ICollection<object[]> NewExercises() => new List<object[]>
    {
        new object[]
        {
            new ExerciseWriteDto()
            {
                Name = "Deadlift",
                TrainingTypes = new List<string>() { "strength", "bodyBuilding" },
                ExerciseMuscles = new List<ExerciseMuscleWriteDto>()
                {
                    new ExerciseMuscleWriteDto()
                    {
                        MuscleName = "deltoid anterior head",
                        IsPrimary = true
                    }
                },
                Description = "you lift a big ass weight off of the gorund",
                Difficulty = 5,
                HowTo = "grap weight, clinch thy booty, LIFT LIKE YOU'VE NEVER LIFTED BEFORE!"
            }
        },
        new object[]
        {
            new ExerciseWriteDto()
            {
                Name = "Sumo Deadlift",
                TrainingTypes = new List<string>() { "strength", "bodyBuilding" },
                ExerciseMuscles = new List<ExerciseMuscleWriteDto>()
                {
                    new ExerciseMuscleWriteDto()
                    {
                        MuscleName = "deltoid anterior head",
                        IsPrimary = true
                    }
                },
                Description = "you lift a big ass weight off of the gorund",
                Difficulty = 5,
                HowTo = "grap weight, clinch thy booty, LIFT LIKE YOU'VE NEVER LIFTED BEFORE!",
                HowTos = new List<ExerciseHowToWriteDto>()
                {
                    new ExerciseHowToWriteDto()
                    {
                        Name = "canaan",
                        Url = "there and back again"
                    }
                }
            }
        }
    };

    [Fact]
    public async Task CreateAsync_Duplicate_should_return_failure()
    {
        DatabaseHelpers.SeedTypesExercisesAndMuscles(context);
        var result = await service.CreateAsync(new ExerciseWriteDto
        {
            Name = "dragon flag",
            TrainingTypes = new List<string>() { "strength", "bodyBuilding" },
            ExerciseMuscles = new List<ExerciseMuscleWriteDto>()
            {
                new ExerciseMuscleWriteDto()
                {
                    MuscleName = "deltoid anterior head",
                    IsPrimary = true
                }
            },
            Description = "you lift a big ass weight off of the gorund",
            Difficulty = 5,
            HowTo = "grap weight, clinch thy booty, LIFT LIKE YOU'VE NEVER LIFTED BEFORE!"
        }, new CancellationToken());

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Exception);
        Assert.NotNull(result.ErrorMessage);
    }

    [Theory]
    [MemberData(nameof(NewExercises))]
    public async Task CreateAsync_Should_return_success(ExerciseWriteDto newExercise)
    {
        DatabaseHelpers.SeedTypesExercisesAndMuscles(context);

        var result = await service.CreateAsync(newExercise, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.True(result.Value >= 1);

        var justCreatedExercise = context.Exercises
            .Include(x => x.TrainingTypes)
            .Include(x => x.ExerciseHowTos)
            .Include(x => x.ExerciseMuscles)
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefault(x => x.Id == result.Value);

        Assert.NotNull(justCreatedExercise);
        Assert.Equal(newExercise.TrainingTypes.Count, justCreatedExercise.TrainingTypes.Count);
        Assert.Equal(newExercise.ExerciseMuscles.Count, justCreatedExercise.ExerciseMuscles.Count);
        Assert.Equal(Utils.NormalizeString(newExercise.Name), justCreatedExercise.Name);
        Assert.Equal(newExercise.Description, justCreatedExercise.Description);
        Assert.Equal(newExercise.Difficulty, justCreatedExercise.Difficulty);
        Assert.Equal(newExercise.HowTo, justCreatedExercise.HowTo);

        if (newExercise.HowTos is not null) // TODO: Lists is never null, check count
        {
            Assert.Equal(newExercise.HowTos.Count, justCreatedExercise.ExerciseHowTos.Count);
            for (int i = 0; i < newExercise.HowTos.Count(); i++)
            {
                var dbExercise = justCreatedExercise.ExerciseHowTos.ToList()[i];
                var dto = newExercise.HowTos[i];
                Assert.Equal(Utils.NormalizeString(dto.Name), dbExercise.Name);
                Assert.Equal(Utils.NormalizeString(dto.Url), dbExercise.Url);
            }
        }
    }

    public static ICollection<object[]> NewExerciseList() => new List<object[]>
    {
        new object[]
        {
            new List<ExerciseWriteDto>
            {
                new ExerciseWriteDto()
                {
                    Name = "Deadlift",
                    TrainingTypes = new List<string>() { "strength", "bodyBuilding" },
                    ExerciseMuscles = new List<ExerciseMuscleWriteDto>()
                    {
                        new ExerciseMuscleWriteDto()
                        {
                            MuscleName = "deltoid anterior head",
                            IsPrimary = true
                        }
                    },
                    Description = "you lift a big ass weight off of the gorund",
                    Difficulty = 5,
                    HowTo = "grap weight, clinch thy booty, LIFT LIKE YOU'VE NEVER LIFTED BEFORE!"
                },
                new ExerciseWriteDto()
                {
                    Name = "Sumo Deadlift",
                    TrainingTypes = new List<string>() { "strength", "bodyBuilding" },
                    ExerciseMuscles = new List<ExerciseMuscleWriteDto>()
                    {
                        new ExerciseMuscleWriteDto()
                        {
                            MuscleName = "deltoid anterior head",
                            IsPrimary = true
                        }
                    },
                    Description = "you lift a big ass weight off of the gorund",
                    Difficulty = 5,
                    HowTo = "grap weight, clinch thy booty, LIFT LIKE YOU'VE NEVER LIFTED BEFORE!",
                    HowTos = new List<ExerciseHowToWriteDto>()
                    {
                        new ExerciseHowToWriteDto()
                        {
                            Name = "canaan",
                            Url = "there and back again"
                        }
                    }
                }
            }
        }
    };


    [Theory]
    [MemberData(nameof(NewExerciseList))]
    public async Task CreateAsync_Bulk_Should_return_success(List<ExerciseWriteDto> newExercise)
    {
        DatabaseHelpers.SeedTypesExercisesAndMuscles(context);

        var result = await service.CreateBulkAsync(newExercise, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        var justCreatedExerciseCount = context.Exercises.Count();

        Assert.Equal(5, justCreatedExerciseCount);
    }
    [Fact]
    public async Task CreateBulkAsync_MissingTrainingTypes_ShouldReturnFailure()
    {
        DatabaseHelpers.SeedTypesExercisesAndMuscles(context);
        // Arrange
        var newExercises = new List<ExerciseWriteDto>
        {
            new ExerciseWriteDto
            {
                Name = "Deadlift",
                TrainingTypes = new List<string> { "strength", "bodybuilding", "missingType" },
                ExerciseMuscles = new List<ExerciseMuscleWriteDto>
                {
                    new ExerciseMuscleWriteDto { MuscleName = "deltoid anterior head", IsPrimary = true }
                },
                Description = "Lift heavy weights from the ground",
                Difficulty = 5,
                HowTo = "Keep your back straight, lift with your legs"
            }
        };

        // Act
        var result = await service.CreateBulkAsync(newExercises, new CancellationToken());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("The following training types could not be found: missingtype", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_ExerciseNotFound_ShouldReturnFailure()
    {
        var exerciseId = 999;
        var exerciseDto = new ExerciseWriteDto
        {
            Name = "Updated Exercise",
            Description = "Updated Description",
            HowTo = "Updated HowTo",
            Difficulty = 5,
            TrainingTypes = new List<string> { "strength" },
            ExerciseMuscles = new List<ExerciseMuscleWriteDto>
            {
                new ExerciseMuscleWriteDto { MuscleName = "deltoid anterior head", IsPrimary = true }
            }
        };

        var result = await service.UpdateAsync(exerciseId, exerciseDto, new CancellationToken());

        Assert.False(result.IsSuccess);
        Assert.Equal("Exercise not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_SuccessfulUpdate_ShouldReturnSuccess()
    {
        DatabaseHelpers.SeedTypesExercisesAndMuscles(context);
        var existingExercise = context.Exercises.First();
        var exerciseId = existingExercise.Id;

        var exerciseDto = new ExerciseWriteDto
        {
            Name = "Updated Exercise",
            Description = "Updated Description",
            HowTo = "Updated HowTo",
            Difficulty = 5,
            TrainingTypes = new List<string> { "strength" },
            ExerciseMuscles = new List<ExerciseMuscleWriteDto>
            {
                new ExerciseMuscleWriteDto { MuscleName = "deltoid anterior head", IsPrimary = true }
            },
            HowTos = new List<ExerciseHowToWriteDto>
            {
                new ExerciseHowToWriteDto { Name = "HowTo1", Url = "http://example.com/howto1" }
            }
        };

        var result = await service.UpdateAsync(exerciseId, exerciseDto, new CancellationToken());

        Assert.True(result.IsSuccess);

        var updatedExercise = await context.Exercises
            .Include(e => e.ExerciseHowTos)
            .Include(e => e.ExerciseMuscles)
            .Include(e => e.TrainingTypes)
            .FirstOrDefaultAsync(e => e.Id == exerciseId);

        Assert.NotNull(updatedExercise);
        Assert.Equal("updated exercise", updatedExercise.Name);
        Assert.Equal("Updated Description", updatedExercise.Description);
        Assert.Equal("Updated HowTo", updatedExercise.HowTo);
        Assert.Equal(5, updatedExercise.Difficulty);
        Assert.Single(updatedExercise.TrainingTypes);
        Assert.Single(updatedExercise.ExerciseMuscles);
        Assert.Single(updatedExercise.ExerciseHowTos);
    }

    // i got burned out, gpt's help starts here
    [Fact]
    public async Task UpdateAsync_TransactionRollbackOnError_ShouldReturnFailure()
    {
        DatabaseHelpers.SeedTypesExercisesAndMuscles(context);
        var existingExercise = context.Exercises.First();
        var exerciseId = existingExercise.Id;

        var exerciseDto = new ExerciseWriteDto
        {
            Name = "Updated Exercise",
            Description = "Updated Description",
            HowTo = "Updated HowTo",
            Difficulty = 5,
            TrainingTypes = new List<string> { "strength" },
            ExerciseMuscles = new List<ExerciseMuscleWriteDto>
            {
                new ExerciseMuscleWriteDto { MuscleName = "nonexistent muscle", IsPrimary = true }
            }
        };

        var result = await service.UpdateAsync(exerciseId, exerciseDto, new CancellationToken());

        Assert.False(result.IsSuccess);
        Assert.Contains("Failed to update exercise", result.ErrorMessage);

        var unchangedExercise = await context.Exercises
            .Include(e => e.ExerciseHowTos)
            .Include(e => e.ExerciseMuscles)
            .Include(e => e.TrainingTypes)
            .FirstOrDefaultAsync(e => e.Id == exerciseId);

        // Ensure the exercise is unchanged
        Assert.NotNull(unchangedExercise);
        Assert.Equal(existingExercise.Name, unchangedExercise.Name);
        Assert.Equal(existingExercise.Description, unchangedExercise.Description);
        Assert.Equal(existingExercise.HowTo, unchangedExercise.HowTo);
        Assert.Equal(existingExercise.Difficulty, unchangedExercise.Difficulty);
    }

    [Fact]
    public async Task DeleteExerciseAsync_SuccessfulDeletion_ShouldReturnSuccess()
    {
        DatabaseHelpers.SeedTypesExercisesAndMuscles(context);
        var existingExercise = context.Exercises.First();
        var exerciseId = existingExercise.Id;

        var result = await service.DeleteExerciseAsync(exerciseId, new CancellationToken());

        Assert.True(result.IsSuccess);

        var deletedExercise = await context.Exercises
            .Include(e => e.ExerciseHowTos)
            .Include(e => e.ExerciseMuscles)
            .Include(e => e.TrainingTypes)
            .FirstOrDefaultAsync(e => e.Id == exerciseId);

        Assert.Null(deletedExercise);
    }

    [Fact]
    public async Task DeleteExerciseAsync_ExerciseNotFound_ShouldReturnFailure()
    {
        var exerciseId = 999; // Non-existing ID

        var result = await service.DeleteExerciseAsync(exerciseId, new CancellationToken());

        Assert.False(result.IsSuccess);
        Assert.Equal("Exercise not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ShouldReturnPaginatedList()
    {
        // Arrange
        DatabaseHelpers.SeedExtendedTypesExercisesAndMuscles(context);

        var queryOptions = new ExerciseQueryOptions
        {
            PageNumber = 1,
            PageSize = 5
        };

        // Act
        var result = await service.GetAllAsync(queryOptions, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(5, result.Value.Items.Count);
        Assert.Equal(5, result.Value.Metadata.PageSize);
        Assert.Equal(1, result.Value.Metadata.CurrentPage);
        Assert.Equal(2, result.Value.Metadata.TotalPages); // Assuming there are 10 exercises seeded
    }

    [Fact]
    public async Task GetAllAsync_FilterByMuscleName_ShouldReturnFilteredExercises()
    {
        // Arrange
        var queryOptions = new ExerciseQueryOptions
        {
            MuscleName = "deltoid anterior head"
        };

        // Act
        var result = await service.GetAllAsync(queryOptions, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.All(result.Value.Items,
            exercise => { Assert.Contains(exercise.ExerciseMuscles, em => em.Name == "deltoid anterior head"); });
    }

    [Fact]
    public async Task GetAllAsync_FilterByMuscleGroup_ShouldReturnFilteredExercises()
    {
        // Arrange
        var queryOptions = new ExerciseQueryOptions
        {
            MuscleGroupName = "shoulders"
        };

        // Act
        var result = await service.GetAllAsync(queryOptions, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.All(result.Value.Items,
            exercise => { Assert.Contains(exercise.ExerciseMuscles, em => em.MuscleGroup == "shoulders"); });
    }

    [Fact]
    public async Task GetAllAsync_FilterByTrainingType_ShouldReturnFilteredExercises()
    {
        // Arrange
        var queryOptions = new ExerciseQueryOptions
        {
            TrainingTypeName = "bodybuilding"
        };

        // Act
        var result = await service.GetAllAsync(queryOptions, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.All(result.Value.Items,
            exercise => { Assert.Contains(exercise.TrainingTypes, tt => tt.Name == "bodybuilding"); });
    }

    [Fact]
    public async Task GetAllAsync_FilterByDifficulty_ShouldReturnFilteredExercises()
    {
        // Arrange
        var queryOptions = new ExerciseQueryOptions
        {
            MinimumDifficulty = 2,
            MaximumDifficulty = 4
        };

        // Act
        var result = await service.GetAllAsync(queryOptions, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.All(result.Value.Items, exercise => { Assert.InRange(exercise.Difficulty.Value, 2, 4); });
    }

    [Fact]
    public async Task GetAllAsync_AllFilters_ShouldReturnFilteredExercises()
    {
        // Arrange
        DatabaseHelpers.SeedExtendedTypesExercisesAndMuscles(context);

        var queryOptions = new ExerciseQueryOptions
        {
            MuscleGroupName = "shoulders",
            TrainingTypeName = "bodybuilding",
            MinimumDifficulty = 2,
            MaximumDifficulty = 4,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await service.GetAllAsync(queryOptions, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.All(result.Value.Items, exercise =>
        {
            Assert.Contains(exercise.TrainingTypes, tt => tt.Name == "bodybuilding");
            Assert.Contains(exercise.ExerciseMuscles, em => em.MuscleGroup == "shoulders");
            Assert.InRange(exercise.Difficulty.Value, 2, 4);
        });
        Assert.Equal(1, result.Value.Metadata.CurrentPage);
        Assert.Equal(10, result.Value.Metadata.PageSize);
    }

    [Fact]
    public async Task GetAllAsync_SortByName_ShouldReturnSortedExercises()
    {
        // Arrange
        DatabaseHelpers.SeedExtendedTypesExercisesAndMuscles(context);

        var queryOptions = new ExerciseQueryOptions
        {
            SortBy = SortBy.NAME,
            Ascending = true
        };

        // Act
        var result = await service.GetAllAsync(queryOptions, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var exercises = result.Value.Items;
        Assert.True(exercises.SequenceEqual(exercises.OrderBy(e => e.Name)));
    }

    [Fact]
    public async Task GetAllAsync_SortByDifficulty_ShouldReturnSortedExercises()
    {
        // Arrange
        DatabaseHelpers.SeedExtendedTypesExercisesAndMuscles(context);

        var queryOptions = new ExerciseQueryOptions
        {
            SortBy = SortBy.DIFFICULTY,
            Ascending = true
        };

        // Act
        var result = await service.GetAllAsync(queryOptions, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var exercises = result.Value.Items;
        Assert.True(exercises.SequenceEqual(exercises.OrderBy(e => e.Difficulty)));
    }

    [Fact]
    public async Task GetAllAsync_SortByMuscleGroup_ShouldReturnSortedExercises()
    {
        // Arrange
        DatabaseHelpers.SeedExtendedTypesExercisesAndMuscles(context);

        var queryOptions = new ExerciseQueryOptions
        {
            SortBy = SortBy.MUSCLE_GROUP,
            Ascending = true
        };

        // Act
        var result = await service.GetAllAsync(queryOptions, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var exercises = result.Value.Items;
        Assert.True(exercises.SequenceEqual(exercises.OrderBy(e => e.ExerciseMuscles.First().MuscleGroup)));
    }

    [Fact]
    public async Task GetAllAsync_SortByTrainingType_ShouldReturnSortedExercises()
    {
        // Arrange
        DatabaseHelpers.SeedExtendedTypesExercisesAndMuscles(context);

        var queryOptions = new ExerciseQueryOptions
        {
            SortBy = SortBy.TRAINING_TYPE,
            Ascending = true
        };

        // Act
        var result = await service.GetAllAsync(queryOptions, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var exercises = result.Value.Items;
        Assert.True(exercises.SequenceEqual(exercises.OrderBy(e => e.TrainingTypes.First().Name)));
    }

    [Fact]
    public async Task GetAllAsync_ShouldLoadRelatedData()
    {
        // Arrange
        DatabaseHelpers.SeedExtendedTypesExercisesAndMuscles(context);

        var queryOptions = new ExerciseQueryOptions
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await service.GetAllAsync(queryOptions, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.All(result.Value.Items, exercise =>
        {
            Assert.NotEmpty(exercise.ExerciseMuscles);
            Assert.NotEmpty(exercise.TrainingTypes);
            Assert.NotEmpty(exercise.HowTos);
        });
    }
    [Fact]
    public async Task GetAllAsync_NullOrEmptyFilters_ShouldReturnAllExercises()
    {
        // Arrange
        DatabaseHelpers.SeedExtendedTypesExercisesAndMuscles(context);

        var queryOptions = new ExerciseQueryOptions
        {
            MuscleGroupName = null,
            TrainingTypeName = null,
            MinimumDifficulty = null,
            MaximumDifficulty = null,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await service.GetAllAsync(queryOptions, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(10, result.Value.Items.Count); // Assuming 10 exercises were seeded
    }
    [Fact]
    public async Task GetAllAsync_SortByWithEmptyCollections_ShouldHandleGracefully()
    {
        // Arrange
        DatabaseHelpers.SeedExtendedTypesExercisesAndMuscles(context);

        var queryOptions = new ExerciseQueryOptions
        {
            SortBy = SortBy.NAME,
            Ascending = true,
            PageNumber = 1,
            PageSize = 10
        };

        // Remove all exercises to create an empty collection scenario
        context.Exercises.RemoveRange(context.Exercises);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllAsync(queryOptions, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.Items);
    }
    [Fact]
    public async Task GetAllAsync_FiltersNoMatchingResults_ShouldReturnEmptyList()
    {
        // Arrange
        DatabaseHelpers.SeedExtendedTypesExercisesAndMuscles(context);

        var queryOptions = new ExerciseQueryOptions
        {
            MuscleGroupName = "non-existent muscle group",
            TrainingTypeName = "non-existent training type",
            MinimumDifficulty = 10
        };

        // Act
        var result = await service.GetAllAsync(queryOptions, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.Items);
    }
    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(-1, 10)]
    [InlineData(1, -10)]
    public async Task GetAllAsync_InvalidPagination_ShouldHandleGracefully(int pageNumber, int pageSize)
    {
        // Arrange
        var queryOptions = new ExerciseQueryOptions
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        // Act
        var result = await service.GetAllAsync(queryOptions, new CancellationToken());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
        Assert.Equal("Page number and page size must be greater than zero.", result.ErrorMessage);
    }
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllExercises_WhenNoFilters()
    {
        DatabaseHelpers.SeedExtendedTypesExercisesAndMuscles(context);
        // Arrange
        var queryOptions = new ExerciseQueryOptions
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await service.GetAllAsync(queryOptions, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value.Items);
        Assert.All(result.Value.Items, exercise =>
        {
            Assert.NotEmpty(exercise.ExerciseMuscles);
            Assert.NotEmpty(exercise.TrainingTypes);
            // No assertion for HowTos as they might be optional
        });
    }


    [Fact]
    public async Task GetAllAsync_WithFilters_ShouldReturnFilteredResults()
    {
        // Arrange
        var queryOptions = new ExerciseQueryOptions
        {
            MuscleGroupName = "core",
            TrainingTypeName = "bodybuilding",
            MinimumDifficulty = 4,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await service.GetAllAsync(queryOptions, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.All(result.Value.Items, exercise =>
        {
            Assert.Contains(exercise.ExerciseMuscles, em => em.MuscleGroup == "core");
            Assert.Contains(exercise.TrainingTypes, tt => tt.Name == "bodybuilding");
            Assert.True(exercise.Difficulty >= 4);
        });
    }
    
    [Theory]
    [InlineData("dragon flag")]
    [InlineData("rope jumping")]
    [InlineData("barbell curl")]
    public async Task SearchExercisesAsync_ShouldReturnMatchingExercises(string searchTerm)
    {
        // Arrange
        DatabaseHelpers.SeedExtendedTypesExercisesAndMuscles(context);

        // Act
        var result = await service.SearchExercisesAsync(searchTerm, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);
        Assert.All(result.Value, exercise => 
            Assert.Contains(searchTerm.Split(' '), term => exercise.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
        );
    }

    [Fact]
    public async Task SearchExercisesAsync_EmptySearchTerm_ShouldReturnFailure()
    {
        // Arrange
        string searchTerm = "";

        // Act
        var result = await service.SearchExercisesAsync(searchTerm, new CancellationToken());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Search term cannot be empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task SearchExercisesAsync_NoMatchingExercises_ShouldReturnFailure()
    {
        // Arrange
        DatabaseHelpers.SeedExtendedTypesExercisesAndMuscles(context);
        string searchTerm = "nonexistent exercise";

        // Act
        var result = await service.SearchExercisesAsync(searchTerm, new CancellationToken());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("No exercises found matching the search term.", result.ErrorMessage);
    }
    
}