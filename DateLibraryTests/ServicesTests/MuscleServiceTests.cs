using AutoMapper;

using DataLibrary.Context;
using DataLibrary.Core;
using DataLibrary.Dtos;
using DataLibrary.Models;
using DataLibrary.Services;

using Microsoft.Extensions.Logging;

using Moq;

using TestSupport.EfHelpers;

namespace DateLibraryTests.ServicesTests;
public class MuscleServiceTests
{
    DbContextOptionsDisposable<SqliteContext> options;
    Profiles myProfile;
    MapperConfiguration? configuration;
    Mapper mapper;
    private Mock<ILogger<MuscleSercive>> logger;
    SqliteContext context;
    MuscleSercive service;
    public MuscleServiceTests()
    {
        options = SqliteInMemory.CreateOptions<SqliteContext>();
        context = new SqliteContext(options);
        context.Database.EnsureCreated();
        myProfile = new Profiles();
        configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));
        mapper = new Mapper(configuration);
        logger = new Mock<ILogger<MuscleSercive>>();
        service = new MuscleSercive(context, mapper, logger.Object);
    }


    [Fact]
    public async Task GetAll_no_muscles_returns_Success()
    {
        var result = await service.GetAllAsync(new CancellationToken());

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetAll_returns_Success()
    {

        context.Muscles.AddRange(
        new List<Muscle> {
                new Muscle { Name = "deltoid anterior head", MuscleGroup = "shoulders", Function = "flexes and medially rotates the arm.", WikiPageUrl = "https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part"    },
                new Muscle { Name = "brachialis", MuscleGroup = "biceps", Function = "mucle in the upper arm the flexes the elbow.", WikiPageUrl = "https://en.wikipedia.org/wiki/Brachialis_muscle" },
                new Muscle { Name = "soleus", MuscleGroup = "calves", Function = "plantar flexes the ankle.", WikiPageUrl = "https://en.wikipedia.org/wiki/Soleus_muscle" },
                }
            );
        context.SaveChanges();

        var result = await service.GetAllAsync(new CancellationToken());

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.Count());
    }

    [Theory]
    [InlineData("?:?")]
    [InlineData("deltoid anterior head")]

    public async Task GetByNameAsync_empty_returns_failure(string muscleName)
    {
        // no need to normalize the muscleName, cause there's nothing to compare against
        var result = await service.GetByNameAsync(muscleName, new CancellationToken());
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.IsType<Exception>(result.Exception);
        Assert.NotNull(result.ErrorMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]

    public async Task GetByNameAsync_invalid_input_returns_failure(string? muscleName)
    {
        // no need to normalize the muscleName, cause there's nothing to compare against
        var result = await service.GetByNameAsync(muscleName, new CancellationToken());
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.IsType<ArgumentNullException>(result.Exception);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task GetByNameAsync_returns_Success()
    {
        context.Muscles.AddRange(
        new List<Muscle> {
            new Muscle { Name = "deltoid anterior head", MuscleGroup = "shoulders", Function = "flexes and medially rotates the arm.", WikiPageUrl = "https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part"    }
        });
        context.SaveChanges();
        var result = await service.GetByNameAsync("deltoid anterior head", new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("deltoid anterior head", result.Value.MuscleName);
        Assert.Equal("shoulders", result.Value.MuscleGroup);
        Assert.Equal("flexes and medially rotates the arm.", result.Value.Function);
        Assert.Equal("https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part", result.Value.WikiPageUrl);
    }


    [Theory]
    [InlineData("?:?")]
    [InlineData("shoulders")]

    public async Task GetAllByGroupAsync_empty_returns_Success(string muscleGroupName)
    {
        // no need to normalize the muscleName, cause there's nothing to compare against
        var result = await service.GetAllByGroupAsync(muscleGroupName, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]

    public async Task GetAllByGroupAsync_invalid_input_returns_failure(string? muscleGroupName)
    {
        // no need to normalize the muscleName, cause there's nothing to compare against
        var result = await service.GetAllByGroupAsync(muscleGroupName, new CancellationToken());
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.IsType<ArgumentNullException>(result.Exception);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task GetAllByGroupAsync_returns_Success()
    {
        context.Muscles.AddRange(
        new List<Muscle> {
            new Muscle { Name = "deltoid anterior head", MuscleGroup = "shoulders", Function = "flexes and medially rotates the arm.", WikiPageUrl = "https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part"    }
        });
        context.SaveChanges();
        var result = await service.GetAllByGroupAsync("shoulders", new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);
        Assert.Equal("deltoid anterior head", result.Value[0].MuscleName);
        Assert.Equal("shoulders", result.Value[0].MuscleGroup);
        Assert.Equal("flexes and medially rotates the arm.", result.Value[0].Function);
        Assert.Equal("https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part", result.Value[0].WikiPageUrl);
    }

    public static IEnumerable<object[]> MusclesToCreateHappy => new List<object[]>
    {
       new object[] { new MuscleWriteDto { Name = "external oblique", Function = "core", MuscleGroup = "flexes the trunk, rotates the trunk, and compresses the abdomen.", WikiPageUrl = "https://en.wikipedia.org/wiki/External_oblique_muscle" } },
        new object[] {new MuscleWriteDto { Name = "deltoid anterior head", MuscleGroup = "shoulders", Function = "flexes and medially rotates the arm.", WikiPageUrl = "https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part" } },
              new object[] { new MuscleWriteDto { Name = "brachialis", MuscleGroup = "biceps", Function = "mucle in the upper arm the flexes the elbow.", WikiPageUrl = "https://en.wikipedia.org/wiki/Brachialis_muscle" } },
       new object[] { new MuscleWriteDto { Name = "soleus", MuscleGroup = "calves", Function = "plantar flexes the ankle.", WikiPageUrl = "https://en.wikipedia.org/wiki/Soleus_muscle" } },
    };

    public static IEnumerable<object[]> MusclesToCreateUnHappy => new List<object[]>
    {
       new object[] { new MuscleWriteDto { Name = "", Function = "core", MuscleGroup = "flexes the trunk, rotates the trunk, and compresses the abdomen.", WikiPageUrl = "https://en.wikipedia.org/wiki/External_oblique_muscle" } },
        new object[] {new MuscleWriteDto { Name = "deltoid anterior head", MuscleGroup = "", Function = "flexes and medially rotates the arm.", WikiPageUrl = "https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part" } },
              new object[] { new MuscleWriteDto { Name = "brachialis", MuscleGroup = "biceps", Function = "", WikiPageUrl = "https://en.wikipedia.org/wiki/Brachialis_muscle" } },
       new object[] { new MuscleWriteDto { Name = "soleus", MuscleGroup = "calves", Function = "plantar flexes the ankle.", WikiPageUrl = "" } },
    };

    [Theory]
    [MemberData(nameof(MusclesToCreateHappy))]
    public async Task CreateMuscleAsync_Correct_Input_Should_return_Success(MuscleWriteDto newMuscle)
    {
        var result = await service.CreateMuscleAsync(newMuscle, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var newRecord = context.Muscles.FirstOrDefault(x => x.Id == result.Value);
        Assert.NotNull(newRecord);
        Assert.Equal(newRecord.Name, newMuscle.Name);
        Assert.Equal(newRecord.Function, newMuscle.Function);
        Assert.Equal(newRecord.MuscleGroup, newMuscle.MuscleGroup);
        Assert.Equal(newRecord.WikiPageUrl, newMuscle.WikiPageUrl);
        Assert.Equal(1, context.Muscles.Count());
    }

    [Theory]
    [MemberData(nameof(MusclesToCreateUnHappy))]
    public async Task CreateMuscleAsync_InCorrect_Input_Should_return_Failure(MuscleWriteDto newMuscle)
    {
        var result = await service.CreateMuscleAsync(newMuscle, new CancellationToken());
        Assert.False(result.IsSuccess);
        Assert.Equal(0, result.Value);
        Assert.NotEmpty(result.ErrorMessage);

        var newRecord = context.Muscles.FirstOrDefault(x => x.Id == result.Value);
        Assert.Null(newRecord);
        Assert.Empty(context.Muscles);
    }

    // tried to do it with the above collection, but it kept failing
    //TODO: learn why!
    [Fact]
    public async Task CreateMuscleBulkAsync_Correct_input_returns_Success()
    {
        List<MuscleWriteDto> newMuscles = new List<MuscleWriteDto>
        {
            new MuscleWriteDto { Name = "External Oblique ", Function = "core", MuscleGroup = "flexes the trunk, rotates the trunk, and compresses the abdomen.", WikiPageUrl = "https://en.wikipedia.org/wiki/External_oblique_muscle" } ,
            new MuscleWriteDto { Name = "deltoid anterior head", MuscleGroup = "shoulders", Function = "flexes and medially rotates the arm.", WikiPageUrl = "https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part" } ,
            new MuscleWriteDto { Name = "brachialis", MuscleGroup = "biceps", Function = "mucle in the upper arm the flexes the elbow.", WikiPageUrl = "https://en.wikipedia.org/wiki/Brachialis_muscle" } ,
            new MuscleWriteDto { Name = "soleus", MuscleGroup = "calves", Function = "plantar flexes the ankle.", WikiPageUrl = "https://en.wikipedia.org/wiki/Soleus_muscle" } ,
        };
        var result = await service.CreateBulkAsync(newMuscles, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.NotEmpty(context.Muscles);
        Assert.NotEqual(newMuscles[0].Name, context.Muscles.First().Name);
        Assert.Equal("external oblique", context.Muscles.First().Name);

    }

    [Fact]
    public async Task CreateMuscleBulkAsync_InCorrect_input_returns_Failure()
    {
        List<MuscleWriteDto> newMuscles = new List<MuscleWriteDto>
        {
            new MuscleWriteDto { Name = "", Function = "core", MuscleGroup = "flexes the trunk, rotates the trunk, and compresses the abdomen.", WikiPageUrl = "https://en.wikipedia.org/wiki/External_oblique_muscle" } ,
            new MuscleWriteDto { Name = "deltoid anterior head", MuscleGroup = "", Function = "flexes and medially rotates the arm.", WikiPageUrl = "https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part" } ,
            new MuscleWriteDto { Name = "brachialis", MuscleGroup = "biceps", Function = "", WikiPageUrl = "https://en.wikipedia.org/wiki/Brachialis_muscle" } ,
            new MuscleWriteDto { Name = "soleus", MuscleGroup = "calves", Function = "plantar flexes the ankle.", WikiPageUrl = "" } ,
            new MuscleWriteDto { Name = "K", MuscleGroup = "K", Function = "K", WikiPageUrl = null } ,
            new MuscleWriteDto { Name = "C", MuscleGroup = "C", Function = null, WikiPageUrl = "C" } ,
            new MuscleWriteDto { Name = "X", MuscleGroup = null, Function = "X", WikiPageUrl = "X"} ,
            new MuscleWriteDto { Name = null, MuscleGroup = "ms", Function = "ms", WikiPageUrl = "ms" } ,

        };
        var result = await service.CreateBulkAsync(newMuscles, new CancellationToken());
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.ErrorMessage);
        Assert.Empty(context.Muscles);

    }

    [Fact]
    public async Task CreateMuscleBulkAsync_Correct_Duplicate_input_returns_Failure()
    {
        List<MuscleWriteDto> newMuscles = new List<MuscleWriteDto>
        {
            new MuscleWriteDto { Name = "external oblique", Function = "core", MuscleGroup = "flexes the trunk, rotates the trunk, and compresses the abdomen.", WikiPageUrl = "https://en.wikipedia.org/wiki/External_oblique_muscle" } ,
            new MuscleWriteDto { Name = "deltoid anterior head", MuscleGroup = "shoulders", Function = "flexes and medially rotates the arm.", WikiPageUrl = "https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part" } ,
            new MuscleWriteDto { Name = "brachialis", MuscleGroup = "biceps", Function = "mucle in the upper arm the flexes the elbow.", WikiPageUrl = "https://en.wikipedia.org/wiki/Brachialis_muscle" } ,
            new MuscleWriteDto { Name = "soleus", MuscleGroup = "calves", Function = "plantar flexes the ankle.", WikiPageUrl = "https://en.wikipedia.org/wiki/Soleus_muscle" } ,
            new MuscleWriteDto { Name = "external oblique", Function = "core", MuscleGroup = "flexes the trunk, rotates the trunk, and compresses the abdomen.", WikiPageUrl = "https://en.wikipedia.org/wiki/External_oblique_muscle" }
        };

        var result = await service.CreateBulkAsync(newMuscles, new CancellationToken());
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.ErrorMessage);
        Assert.Empty(context.Muscles);
    }

    [Fact]
    public async Task UpdateAsync_CorrectInput_FullUpdate_Returns_Success()
    {
        var newMuscle = new Muscle
        {
            Name = "external oblique",
            Function = "core",
            MuscleGroup = "flexes the trunk, rotates the trunk, and compresses the abdomen.",
            WikiPageUrl = "https://en.wikipedia.org/wiki/External_oblique_muscle"
        };
        context.Muscles.Add(newMuscle);
        context.SaveChanges();
        context.ChangeTracker.Clear();

        var updateMuscle = new MuscleUpdateDto
        {
            Name = "gracilis",
            Function = "thigh",
            MuscleGroup = "the muscle adducts, medially rotates (with hip flexion), laterally rotates, and flexes the hip as above, and also aids in flexion of the knee.",
            WikiPageUrl = "https://en.wikipedia.org/wiki/Gracilis_muscle"
        };
        var result = await service.UpdateAsync(
            newMuscle.Id
            , updateMuscle
            , new CancellationToken());

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        var updatedMuscle = context.Muscles.FirstOrDefault(x => x.Id == newMuscle.Id);
        Assert.NotNull(updatedMuscle);
        Assert.Equal(updateMuscle.Name, updateMuscle.Name);
        Assert.Equal(updateMuscle.MuscleGroup, updateMuscle.MuscleGroup);
        Assert.Equal(updateMuscle.Function, updateMuscle.Function);
        Assert.Equal(updateMuscle.WikiPageUrl, updateMuscle.WikiPageUrl);
    }


    public static ICollection<object[]> updateAsyncCollection => new List<object[]>
    {
        new object[] {
            new Tuple<Muscle, MuscleUpdateDto>(
                new Muscle
                {
                    Name = "external oblique",
                    Function = "core",
                    MuscleGroup = "flexes the trunk, rotates the trunk, and compresses the abdomen.",
                    WikiPageUrl = "https://en.wikipedia.org/wiki/External_oblique_muscle"
                },new MuscleUpdateDto { Name = "gracilis" }
            ),
        },
        new object[] {
            new Tuple<Muscle, MuscleUpdateDto>(
                new Muscle
                {
                    Name = "external oblique",
                    Function = "core",
                    MuscleGroup = "flexes the trunk, rotates the trunk, and compresses the abdomen.",
                    WikiPageUrl = "https://en.wikipedia.org/wiki/External_oblique_muscle"
                },new MuscleUpdateDto { Function = "thigh" }
            ),
        },
        new object[] {
            new Tuple<Muscle, MuscleUpdateDto>(
                new Muscle
                {
                    Name = "external oblique",
                    Function = "core",
                    MuscleGroup = "flexes the trunk, rotates the trunk, and compresses the abdomen.",
                    WikiPageUrl = "https://en.wikipedia.org/wiki/External_oblique_muscle"
                },new MuscleUpdateDto { MuscleGroup = "the muscle adducts, medially rotates (with hip flexion), laterally rotates, and flexes the hip as above, and also aids in flexion of the knee." }
            ),
        },
        new object[]{
            new Tuple<Muscle, MuscleUpdateDto>(
                new Muscle
                {
                    Name = "external oblique",
                    Function = "core",
                    MuscleGroup = "flexes the trunk, rotates the trunk, and compresses the abdomen.",
                    WikiPageUrl = "https://en.wikipedia.org/wiki/External_oblique_muscle"
                },new MuscleUpdateDto
                {
                    WikiPageUrl = "https://en.wikipedia.org/wiki/Gracilis_muscle"
                }
            )
        }
    };

    [Theory]
    [MemberData(nameof(updateAsyncCollection))]
    public async Task UpdateAsync_CorrectInput_PartialUpdate_Returns_Success(Tuple<Muscle, MuscleUpdateDto> muscleTuple)
    {
        var newMuscle = muscleTuple.Item1;
        context.Muscles.Add(newMuscle);
        context.SaveChanges();
        context.ChangeTracker.Clear();

        var updateMuscle = muscleTuple.Item2;

        var result = await service.UpdateAsync(
            newMuscle.Id
            , updateMuscle
            , new CancellationToken());

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        var updatedMuscle = context.Muscles.FirstOrDefault(x => x.Id == newMuscle.Id);
        Assert.NotNull(updatedMuscle);

        // if null means ther was no update
        Assert.Equal(updateMuscle.Name ?? newMuscle.Name, updatedMuscle.Name);
        Assert.Equal(updateMuscle.MuscleGroup ?? newMuscle.MuscleGroup, updatedMuscle.MuscleGroup);
        Assert.Equal(updateMuscle.Function ?? newMuscle.Function, updatedMuscle.Function);
        Assert.Equal(updateMuscle.WikiPageUrl ?? newMuscle.WikiPageUrl, updatedMuscle.WikiPageUrl);

    }

    [Fact]
    public async Task DeleteAsync_CorrectInput_returns_Success()
    {
        var muscle = new Muscle
        {
            Name = "gracilis",
            Function = "thigh",
            MuscleGroup = "the muscle adducts, medially rotates (with hip flexion), laterally rotates, and flexes the hip as above, and also aids in flexion of the knee.",
            WikiPageUrl = "https://en.wikipedia.org/wiki/Gracilis_muscle"
        };

        context.Muscles.Add(muscle);
        context.SaveChanges();
        context.ChangeTracker.Clear();

        var result = await service.DeleteAsync(muscle.Id, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(context.Muscles);
    }

    [Fact]
    public async Task DeleteAsync_NoMuscle_returns_Failure()
    {
        var result = await service.DeleteAsync(1, new CancellationToken());
        Assert.False(result.IsSuccess);
        Assert.False(result.Value);
        Assert.IsType<Exception>(result.Exception);
        Assert.NotEmpty(result.ErrorMessage);
    }

}
