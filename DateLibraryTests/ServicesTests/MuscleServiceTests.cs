using AutoMapper;
using DataLibrary.Context;
using DataLibrary.Core;
using DataLibrary.Dtos;
using DataLibrary.Models;
using DataLibrary.Services;
using DateLibraryTests.helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TestSupport.EfHelpers;

namespace DateLibraryTests.ServicesTests;

public class MuscleServiceTests : BaseTestClass
{
    private MuscleService service;
    private Mock<ILogger<MuscleService>> logger;

    public MuscleServiceTests() : base()
    {
        logger = new Mock<ILogger<MuscleService>>();
        service = new MuscleService(_context, _mapper, logger.Object);
        DatabaseHelpers.SeedLanguages(_context);
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
    public async Task GetAll_default_language_returns_Success()
    {
        Create3MusclesAndLocalizedMuscles();

        var result = await service.GetAllAsync(new CancellationToken());

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.Count());
    }

    [Fact]
    public async Task GetAll_Arabic_language_returns_Success()
    {
        Create3MusclesAndLocalizedMuscles();

        var result = await service.GetAllAsync(new CancellationToken(), "ar");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.Count());
        Assert.Equal("العضلة الدالية", result.Value[0].MuscleName);
    }

    [Fact]
    public async Task GetAll_Japanese_language_returns_Success()
    {
        Create3MusclesAndLocalizedMuscles();

        var result = await service.GetAllAsync(new CancellationToken(), "jp");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.Count());
        Assert.Equal("ヒラメ筋", result.Value[0].MuscleName);
    }


    /// <summary>
    /// creates 3 muscles and assigns 3 localized muscles for them, in EN, AR and JP.
    /// </summary>
    private void Create3MusclesAndLocalizedMuscles()
    {
        List<Muscle> newMuscles = [new Muscle(), new Muscle(), new Muscle()];
        _context.AddRange(newMuscles);
        _context.SaveChanges();
        _context.LocalizedMuscles.AddRange(
            new List<LocalizedMuscle>
            {
                new LocalizedMuscle
                {
                    Name = "deltoid anterior head",
                    MuscleGroup = "shoulders",
                    Function = "flexes and medially rotates the arm.",
                    WikiPageUrl = "https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part",
                    LanguageId = 1,
                    MuscleId = newMuscles[0].Id
                },
                new LocalizedMuscle
                {
                    Name = "brachialis",
                    MuscleGroup = "biceps",
                    Function = "mucle in the upper arm the flexes the elbow.",
                    WikiPageUrl = "https://en.wikipedia.org/wiki/Brachialis_muscle",
                    LanguageId = 1,
                    MuscleId = newMuscles[1].Id
                },
                new LocalizedMuscle
                {
                    Name = "soleus",
                    MuscleGroup = "calves",
                    Function = "plantar flexes the ankle.",
                    WikiPageUrl = "https://en.wikipedia.org/wiki/Soleus_muscle",
                    LanguageId = 1,
                    MuscleId = newMuscles[2].Id
                },
                new LocalizedMuscle
                {
                    Name = "عضلة نعلية",
                    MuscleGroup = "بطات",
                    Function = "ثني الكاحل",
                    WikiPageUrl = "https://ar.wikipedia.org/wiki/عضلة_نعلية",
                    LanguageId = 2,
                    MuscleId = newMuscles[2].Id
                },
                new LocalizedMuscle
                {
                    Name = "عضلة عضدية",
                    MuscleGroup = "عضد",
                    Function = "ت\u064cعتبر العضلة العضدية من أقوى العضلات التي تقوم بثني مفصل المرفق.",
                    WikiPageUrl = "https://ar.wikipedia.org/wiki/عضلة_عضدية",
                    LanguageId = 2,
                    MuscleId = newMuscles[1].Id
                },
                new LocalizedMuscle
                {
                    Name = "العضلة الدالية",
                    MuscleGroup = "كتف",
                    Function =
                        "تقوم الالياف الامامية للعضلة الدالية بت\u064eب\u0652عيد الكتف إذا كان في وضعية التدوير.",
                    WikiPageUrl = "https://ar.wikipedia.org/wiki/عضلة_مثلثة",
                    LanguageId = 2,
                    MuscleId = newMuscles[0].Id
                },
                new LocalizedMuscle
                {
                    Name = "三角筋",
                    MuscleGroup = "肩",
                    Function = "鎖骨部が上腕を屈曲・内転・内旋させる。",
                    WikiPageUrl = "https://jp.wikipedia.org/wiki/三角筋",
                    LanguageId = 3,
                    MuscleId = newMuscles[0].Id
                },
                new LocalizedMuscle
                {
                    Name = "上腕筋",
                    MuscleGroup = "上腕二頭筋",
                    Function = "上腕筋",
                    WikiPageUrl = "https://jp.wikipedia.org/wiki/上腕筋",
                    LanguageId = 3,
                    MuscleId = newMuscles[1].Id
                },
                new LocalizedMuscle
                {
                    Name = "ヒラメ筋",
                    MuscleGroup = "ふくらはぎ",
                    Function = "足首を曲げます。",
                    WikiPageUrl = "https://jp.wikipedia.org/wiki/ヒラメ筋",
                    LanguageId = 3,
                    MuscleId = newMuscles[2].Id
                },
            }
        );
        _context.SaveChanges();
    }

    [Theory]
    [InlineData("?:?")]
    [InlineData("deltoid anterior head")]
    public async Task GetByNameAsync_empty_returns_failure(string muscleName)
    {
        var result = await service.GetByNameAsync(muscleName, new CancellationToken());
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.ErrorMessage);
        Assert.Equal("Muscle not found.", result.ErrorMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetByNameAsync_invalid_input_returns_failure(string? muscleName)
    {
        var result = await service.GetByNameAsync(muscleName!, new CancellationToken());
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.ErrorMessage);
        Assert.Equal("Muscle name cannot be empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetByNameAsync_returns_Success()
    {
        Create3MusclesAndLocalizedMuscles();

        var result = await service.GetByNameAsync("deltoid anterior head", new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("deltoid anterior head", result.Value.MuscleName);
        Assert.Equal("shoulders", result.Value.MuscleGroup);
        Assert.Equal("flexes and medially rotates the arm.", result.Value.Function);
        Assert.Equal("english", result.Value.LanguageName);
        Assert.Equal("https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part", result.Value.WikiPageUrl);
    }


    [Theory]
    [InlineData("?:?")]
    [InlineData("shoulder")]
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
        var result = await service.GetAllByGroupAsync(muscleGroupName!, new CancellationToken());
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.ErrorMessage);
        Assert.Equal("Muscle group name cannot be empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetAllByGroupAsync_returns_Success()
    {
        Create3MusclesAndLocalizedMuscles();
        var result = await service.GetAllByGroupAsync("shoulders", new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);
        Assert.Equal("deltoid anterior head", result.Value[0].MuscleName);
        Assert.Equal("shoulders", result.Value[0].MuscleGroup);
        Assert.Equal("flexes and medially rotates the arm.", result.Value[0].Function);
        Assert.Equal("english", result.Value[0].LanguageName);
        Assert.Equal("https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part", result.Value[0].WikiPageUrl);
    }

    public static IEnumerable<object[]> MusclesToCreateHappy => new List<object[]>
    {
        new object[]
        {
            new MuscleWriteDto
            {
                Name = "external oblique", MuscleGroup = "core",
                Function = "flexes the trunk, rotates the trunk, and compresses the abdomen.",
                WikiPageUrl = "https://en.wikipedia.org/wiki/External_oblique_muscle",
                LanguageCode = "en"
            }
        },
        new object[]
        {
            new MuscleWriteDto
            {
                Name = "deltoid anterior head", MuscleGroup = "shoulders",
                Function = "flexes and medially rotates the arm.",
                WikiPageUrl = "https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part",
                LanguageCode = "en"
            }
        },
        new object[]
        {
            new MuscleWriteDto
            {
                Name = "brachialis", MuscleGroup = "biceps", Function = "mucle in the upper arm the flexes the elbow.",
                WikiPageUrl = "https://en.wikipedia.org/wiki/Brachialis_muscle",
                LanguageCode = "en"
            }
        },
        new object[]
        {
            new MuscleWriteDto
            {
                Name = "soleus", MuscleGroup = "calves", Function = "plantar flexes the ankle.",
                WikiPageUrl = "https://en.wikipedia.org/wiki/Soleus_muscle",
                LanguageCode = "en"
            }
        },
    };

    public static IEnumerable<object[]> MusclesToCreateUnHappy => new List<object[]>
    {
        new object[]
        {
            new MuscleWriteDto
            {
                Name = "", Function = "core",
                MuscleGroup = "flexes the trunk, rotates the trunk, and compresses the abdomen.",
                WikiPageUrl = "https://en.wikipedia.org/wiki/External_oblique_muscle", LanguageCode = "en"
            }
        },
        new object[]
        {
            new MuscleWriteDto
            {
                Name = "deltoid anterior head", MuscleGroup = "", Function = "flexes and medially rotates the arm.",
                WikiPageUrl = "https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part", LanguageCode = "en"
            }
        },
        new object[]
        {
            new MuscleWriteDto
            {
                Name = "brachialis", MuscleGroup = "biceps", Function = "",
                WikiPageUrl = "https://en.wikipedia.org/wiki/Brachialis_muscle", LanguageCode = "en"
            }
        },
        new object[]
        {
            new MuscleWriteDto
            {
                Name = "soleus", MuscleGroup = "calves", Function = "plantar flexes the ankle.",
                WikiPageUrl = "", LanguageCode = ""
            }
        },
    };

    [Theory]
    [MemberData(nameof(MusclesToCreateHappy))]
    public async Task CreateMuscleAsync_Correct_Input_Should_return_Success(MuscleWriteDto newMuscle)
    {
        var result = await service.CreateMuscleAsync(newMuscle, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.True(result.Value >= 1);

        var newRecord = _context.Muscles
            .Include(x => x.LocalizedMuscles)
            .ThenInclude(localizedMuscle => localizedMuscle.Language)
            .FirstOrDefault(x => x.Id == result.Value);
        Assert.NotNull(newRecord);
        Assert.Equal(newRecord.LocalizedMuscles.First().Name, newMuscle.Name);
        Assert.Equal(newRecord.LocalizedMuscles.First().Function, newMuscle.Function);
        Assert.Equal(newRecord.LocalizedMuscles.First().MuscleGroup, newMuscle.MuscleGroup);
        Assert.Equal(newRecord.LocalizedMuscles.First().WikiPageUrl, newMuscle.WikiPageUrl);
        Assert.Equal(newRecord.LocalizedMuscles.First().Language.Code, newMuscle.LanguageCode);
        Assert.Equal(1, _context.LocalizedMuscles.Count());
    }

    [Theory]
    [MemberData(nameof(MusclesToCreateUnHappy))]
    public async Task CreateMuscleAsync_InCorrect_Input_Should_return_Failure(MuscleWriteDto newMuscle)
    {
        var result = await service.CreateMuscleAsync(newMuscle, new CancellationToken());
        Assert.False(result.IsSuccess);
        Assert.Equal(0, result.Value);
        Assert.NotEmpty(result.ErrorMessage!);

        var newRecord = _context.Muscles.FirstOrDefault(x => x.Id == result.Value);
        Assert.Null(newRecord);
        Assert.Empty(_context.Muscles);
    }


    [Fact]
    public async Task CreateMuscleBulkAsync_Correct_input_returns_Success()
    {
        List<MuscleWriteDto> newMuscles = new List<MuscleWriteDto>
        {
            new MuscleWriteDto
            {
                Name = "External Oblique ", Function = "core",
                MuscleGroup = "flexes the trunk, rotates the trunk, and compresses the abdomen.",
                WikiPageUrl = "https://en.wikipedia.org/wiki/External_oblique_muscle", LanguageCode = "en"
            },
            new MuscleWriteDto
            {
                Name = "deltoid anterior head", MuscleGroup = "shoulders",
                Function = "flexes and medially rotates the arm.",
                WikiPageUrl = "https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part", LanguageCode = "en"
            },
            new MuscleWriteDto
            {
                Name = "brachialis", MuscleGroup = "biceps", Function = "mucle in the upper arm the flexes the elbow.",
                WikiPageUrl = "https://en.wikipedia.org/wiki/Brachialis_muscle", LanguageCode = "en"
            },
            new MuscleWriteDto
            {
                Name = "soleus", MuscleGroup = "calves", Function = "plantar flexes the ankle.",
                WikiPageUrl = "https://en.wikipedia.org/wiki/Soleus_muscle", LanguageCode = "en"
            },
        };
        var result = await service.CreateBulkAsync(newMuscles, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.Equal(4, _context.Muscles.Count());
        var localMusclesInDb = _context.LocalizedMuscles.ToList();
        Assert.Equal(newMuscles[0].Name, localMusclesInDb.First().Name);
        Assert.Equal(newMuscles[1].Name, localMusclesInDb[1].Name);
        Assert.Equal(newMuscles[2].Name, localMusclesInDb[2].Name);
        Assert.Equal(newMuscles[3].Name, localMusclesInDb[3].Name);
        Assert.Equal(newMuscles[0].MuscleGroup, localMusclesInDb[0].MuscleGroup);
        Assert.Equal(newMuscles[1].MuscleGroup, localMusclesInDb[1].MuscleGroup);
        Assert.Equal(newMuscles[2].MuscleGroup, localMusclesInDb[2].MuscleGroup);
        Assert.Equal(newMuscles[3].MuscleGroup, localMusclesInDb[3].MuscleGroup);
    }

    [Fact]
    public async Task CreateMuscleBulkAsync_InCorrect_input_returns_Failure()
    {
        List<MuscleWriteDto> newMuscles = new List<MuscleWriteDto>
        {
            new MuscleWriteDto
            {
                Name = "l", Function = "core",
                MuscleGroup = "flexes the trunk, rotates the trunk, and compresses the abdomen.",
                WikiPageUrl = "https://en.wikipedia.org/wiki/External_oblique_muscle", LanguageCode = "na"
            },
            new MuscleWriteDto
            {
                Name = "deltoid anterior head", MuscleGroup = "n", Function = "flexes and medially rotates the arm.",
                WikiPageUrl = "https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part", LanguageCode = "na"
            },
            new MuscleWriteDto
            {
                Name = "brachialis", MuscleGroup = "biceps", Function = "n",
                WikiPageUrl = "https://en.wikipedia.org/wiki/Brachialis_muscle", LanguageCode = "na"
            },
            new MuscleWriteDto
            {
                Name = "soleus", MuscleGroup = "calves", Function = "plantar flexes the ankle.", WikiPageUrl = "",
                LanguageCode = "na"
            },
            new MuscleWriteDto { Name = "K", MuscleGroup = "K", Function = "K", WikiPageUrl = "null" },
            new MuscleWriteDto { Name = "C", MuscleGroup = "C", Function = "null", WikiPageUrl = "C" },
            new MuscleWriteDto { Name = "X", MuscleGroup = "null", Function = "X", WikiPageUrl = "X" },
            new MuscleWriteDto
                { Name = "T", MuscleGroup = "ms", Function = "ms", WikiPageUrl = "ms", LanguageCode = "na" },
        };
        var result = await service.CreateBulkAsync(newMuscles, new CancellationToken());
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.ErrorMessage!);
        Assert.Empty(_context.Muscles);
    }

    [Fact]
    public async Task CreateMuscleBulkAsync_Correct_Duplicate_input_returns_Failure()
    {
        List<MuscleWriteDto> newMuscles = new List<MuscleWriteDto>
        {
            new MuscleWriteDto
            {
                Name = "external oblique", Function = "core",
                MuscleGroup = "flexes the trunk, rotates the trunk, and compresses the abdomen.",
                WikiPageUrl = "https://en.wikipedia.org/wiki/External_oblique_muscle",
                LanguageCode = "en"
            },
            new MuscleWriteDto
            {
                Name = "deltoid anterior head", MuscleGroup = "shoulders",
                Function = "flexes and medially rotates the arm.",
                WikiPageUrl = "https://en.wikipedia.org/wiki/Deltoid_muscle#Anterior_part",
                LanguageCode = "en"
            },
            new MuscleWriteDto
            {
                Name = "brachialis", MuscleGroup = "biceps", Function = "mucle in the upper arm the flexes the elbow.",
                WikiPageUrl = "https://en.wikipedia.org/wiki/Brachialis_muscle",
                LanguageCode = "en"
            },
            new MuscleWriteDto
            {
                Name = "soleus", MuscleGroup = "calves", Function = "plantar flexes the ankle.",
                WikiPageUrl = "https://en.wikipedia.org/wiki/Soleus_muscle",
                LanguageCode = "en"
            },
            new MuscleWriteDto
            {
                Name = "external oblique", Function = "core",
                MuscleGroup = "flexes the trunk, rotates the trunk, and compresses the abdomen.",
                WikiPageUrl = "https://en.wikipedia.org/wiki/External_oblique_muscle",
                LanguageCode = "en"
            }
        };

        var result = await service.CreateBulkAsync(newMuscles, new CancellationToken());
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.ErrorMessage!);
        Assert.Empty(_context.Muscles);
        Assert.Empty(_context.LocalizedMuscles);
    }

    [Fact]
    public async Task UpdateAsync_CorrectInput_FullUpdate_Returns_Success()
    {
        var newMuscle = new Muscle();
        _context.Add(newMuscle);
        _context.SaveChanges();
        var newLocalizedMuscle = new LocalizedMuscle
        {
            Name = "external oblique",
            Function = "core",
            MuscleGroup = "flexes the trunk, rotates the trunk, and compresses the abdomen.",
            WikiPageUrl = "https://en.wikipedia.org/wiki/External_oblique_muscle",
            LanguageId = 1,
            Muscle = newMuscle
        };
        _context.LocalizedMuscles.Add(newLocalizedMuscle);
        _context.SaveChanges();
        _context.ChangeTracker.Clear();

        var updateMuscle = new MuscleUpdateDto
        {
            Name = "gracilis",
            Function = "thigh",
            MuscleGroup =
                "the muscle adducts, medially rotates (with hip flexion), laterally rotates, and flexes the hip as above, and also aids in flexion of the knee.",
            WikiPageUrl = "https://en.wikipedia.org/wiki/Gracilis_muscle",
            LanguageCode = "en"
        };

        var result = await service.UpdateAsync(
            newMuscle.Id
            , updateMuscle
            , new CancellationToken());

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        var updatedMuscle = _context.Muscles
            .Include(x => x.LocalizedMuscles)
            .FirstOrDefault(x => x.Id == newMuscle.Id);
        Assert.NotNull(updatedMuscle);
        Assert.Equal(updatedMuscle.LocalizedMuscles.First().Name, updateMuscle.Name);
        Assert.Equal(updatedMuscle.LocalizedMuscles.First().MuscleGroup, updateMuscle.MuscleGroup);
        Assert.Equal(updatedMuscle.LocalizedMuscles.First().Function, updateMuscle.Function);
        Assert.Equal(updatedMuscle.LocalizedMuscles.First().WikiPageUrl, updateMuscle.WikiPageUrl);
    }

    [Fact]
    public async Task DeleteAsync_CorrectInput_returns_Success()
    {
        Create3MusclesAndLocalizedMuscles();

        var result = await service.DeleteAsync(1, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.Equal(2, _context.Muscles.Count());
        Assert.Equal(6, _context.LocalizedMuscles.Count());
    }

    [Fact]
    public async Task DeleteAsync_NoMuscle_returns_Failure()
    {
        var result = await service.DeleteAsync(1, new CancellationToken());
        Assert.False(result.IsSuccess);
        Assert.False(result.Value);
        Assert.NotEmpty(result.ErrorMessage!);
        Assert.Equal("Muscle not found.", result.ErrorMessage!);
    }
}