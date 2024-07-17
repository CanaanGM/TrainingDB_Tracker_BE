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
public class TrainingSessionServiceTests
{
    DbContextOptionsDisposable<SqliteContext> options;
    Profiles myProfile;
    MapperConfiguration? configuration;
    Mapper mapper;
    private Mock<ILogger<TrainingSessionService>> logger;
    private TrainingSessionService service;
    SqliteContext context;


    public TrainingSessionServiceTests()
    {
        options = SqliteInMemory.CreateOptions<SqliteContext>();
        context = new SqliteContext(options);
        context.Database.EnsureCreated();
        myProfile = new Profiles();
        configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));
        mapper = new Mapper(configuration);
        logger = new Mock<ILogger<TrainingSessionService>>();
        service = new TrainingSessionService(context, mapper, logger.Object);


    }

    ~TrainingSessionServiceTests()
    {
        System.Console.WriteLine("Death!");
    }

    [Fact]
    public async Task GetTrainingSessionsAsync_NoDateRange_Empty_Should_Return_Success()
    {
        var result = await service.GetTrainingSessionsAsync(null, null, new CancellationToken());

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetTrainingSessionsAsync_WithDateRange_Empty_Should_Return_Success()
    {
        var result = await service.GetTrainingSessionsAsync("5-13-2024", "6-1-2024", new CancellationToken());

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetTrainingSessionsAsync_NoDateRange_Should_ReturnAll_Success()
    {
        DatabaseHelpers.SeedTypesExercisesAndMuscles(context);
        var trainingSessions = new List<TrainingSession>
            {
                new TrainingSession
                {
                    TotalCaloriesBurned = 666,
                    CreatedAt = DateTime.UtcNow,
                    DurationInSeconds  = 7200,
                    Mood = 9,
                    Notes = "Test record for testing"
                }
            };


        context.TrainingSessions.AddRange(trainingSessions);
        context.SaveChanges();
        trainingSessions.ForEach(x => DatabaseHelpers.SeedWorkOutRecords(x.Id, context));

        var result = await service.GetTrainingSessionsAsync(
            null, null, new CancellationToken()
            );

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);
        Assert.Equal(4, result.Value[0].TrainingTypes.Count());
        Assert.Equal(3, result.Value[0].ExerciseRecords.Count());
    }



    [Theory]
    [InlineData("5-1-2024", "5-30-2024")]
    [InlineData("5-1-2024 00:00:00", "6-1-2024 00:00:00")] // 1 month of records
    [InlineData("5-1-2024 00:00:00", "6-1-2024")]
    [InlineData("5-1-2024", "6-1-2024 16:44:21")]
    [InlineData("7-1-2024", "7-30-2024")]
    [InlineData("7-1-2024 00:00:00", "7-30-2024 16:44:21")]
    [InlineData("7-1-2024", "7-30-2024 16:44:21")]
    [InlineData("7-1-2024 00:00:00", "7-30-2024")]


    public async Task GetTrainingSessionsAsync_DateRange_Should_ReturnAll_WithinDate_Success(string firstDate, string secondDate)
    {
        var date = new DateTime(2024, 5, 1);
        DatabaseHelpers.SeedTypesExercisesAndMuscles(context);

        var trainingSessions = new List<TrainingSession>
            {
                new TrainingSession
                {
                    TotalCaloriesBurned = 666,
                    CreatedAt = date, // may
                    DurationInSeconds  = 7200,
                    Mood = 9,
                    Notes = "Test record for testing 1"
                },
                new TrainingSession
                {
                    TotalCaloriesBurned = 666,
                    CreatedAt = date.AddDays(5), // may
                    DurationInSeconds  = 7200,
                    Mood = 9,
                    Notes = "Test record for testing 2"
                },
                new TrainingSession
                {
                    TotalCaloriesBurned = 666,
                    CreatedAt = date.AddMonths(2), // july
                    DurationInSeconds  = 7200,
                    Mood = 9,
                    Notes = "Test record for testing 3"
                },
                new TrainingSession
                {
                    TotalCaloriesBurned = 666,
                    CreatedAt = date.AddMonths(2).AddDays(2), // july
                    DurationInSeconds  = 7200,
                    Mood = 9,
                    Notes = "Test record for testing 4"
                }
            };

        context.TrainingSessions.AddRange(trainingSessions);
        context.SaveChanges();

        trainingSessions.ForEach(x => DatabaseHelpers.SeedWorkOutRecords(x.Id, context));

        var result = await service.GetTrainingSessionsAsync(firstDate, secondDate, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count());
        Assert.Equal(4, result.Value[0].TrainingTypes.Count());
        Assert.Equal(3, result.Value[0].ExerciseRecords.Count());

    }

    [Theory]
    [InlineData("5-13-2024")]
    [InlineData("5-13-2024 17:34:45")]
    [InlineData("2-12-2024 11:18:05")]
    public async Task CreateSessionAsync_creating_return_success(string creationDate)
    {
        DatabaseHelpers.SeedTypesExercisesAndMuscles(context);

        var newSessionDto = new TrainingSessionWriteDto
        {
            TotalCaloriesBurned = 666,
            CreatedAt = creationDate,
            DurationInMinutes = 35,
            Mood = 9,
            Notes = "Test record for testing",
            ExerciseRecords = new List<ExerciseRecordWriteDto>
            {
                new ExerciseRecordWriteDto
                {
                    ExerciseName = "Dragon Flag",
                    Repetitions = 20
                },
                new ExerciseRecordWriteDto
                {
                    ExerciseName = "Rope Jumping",
                    TimerInSeconds = 1800
                },
                new ExerciseRecordWriteDto
                {
                    ExerciseName = "barbell curl",
                    WeightUsedKg = 20,
                    Repetitions = 20
                }
            }
        };

        var result = await service.CreateSessionAsync(newSessionDto, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.True(result.Value >= 1);

        var newSession = context.TrainingSessions
            .Include(x => x.TrainingSessionExerciseRecords)
            .ThenInclude(e => e.ExerciseRecord)
            .Include(t => t.TrainingTypes)
            .FirstOrDefault(w => w.Id == result.Value);

        Assert.NotNull(newSession);
        Assert.NotEmpty(newSession.TrainingSessionExerciseRecords);
        Assert.NotEmpty(newSession.TrainingTypes);

        Assert.True(newSession.TrainingTypes.Count() == 4);
        Assert.True(newSession.TrainingSessionExerciseRecords.Count() == 3);

        foreach (var sessionRecord in newSession.TrainingSessionExerciseRecords)
        {
            var exerciseRecord = sessionRecord!.ExerciseRecord!;

            if (exerciseRecord.WeightUsedKg is not null) Assert.Equal(20, exerciseRecord.WeightUsedKg);
            if (exerciseRecord.TimerInSeconds is not null) Assert.Equal(1800, exerciseRecord.TimerInSeconds);
            if (exerciseRecord.Repetitions is not null) Assert.Equal(20, exerciseRecord.Repetitions);

            Assert.Equal(DateTime.Parse(creationDate), sessionRecord.CreatedAt);
            Assert.Equal(DateTime.Parse(creationDate), exerciseRecord.CreatedAt);

        }
        Assert.Equal(35 * 60, newSession.DurationInSeconds); // duration conversion
        Assert.Equal(DateTime.Parse(creationDate), newSession.CreatedAt);

    }


    [Fact]
    public async Task UpdateSessionAsync_FullSessionUpdateWithExerciseRecords_Returns_Success()
    {
        DatabaseHelpers.SeedTypesExercisesAndMuscles(context);
        var trainingSessionToBUpdated =
                new TrainingSession
                {
                    TotalCaloriesBurned = 666,
                    CreatedAt = DateTime.UtcNow,
                    DurationInSeconds = 7200,
                    Mood = 9,
                    Notes = "Test record for testing"
                };

        context.TrainingSessions.Add(trainingSessionToBUpdated);
        context.SaveChanges();
        DatabaseHelpers.SeedWorkOutRecords(trainingSessionToBUpdated.Id, context);
        context.ChangeTracker.Clear();

        var updateDto = new TrainingSessionWriteDto
        {
            TotalCaloriesBurned = 777,
            CreatedAt = "6-2-2024",
            DurationInMinutes = 120,
            Mood = 5,
            Notes = "Full on update",
            ExerciseRecords = new List<ExerciseRecordWriteDto>
            {
                new ExerciseRecordWriteDto
                {
                    ExerciseName = "dragon flag",
                    Repetitions = 30
                }
            }
        };

        var result = await service.UpdateSessionAsync(trainingSessionToBUpdated.Id, updateDto, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        var updatedSession = context.TrainingSessions
            .Include(ts => ts.TrainingSessionExerciseRecords)
                    .ThenInclude(e => e.ExerciseRecord)
                        .ThenInclude(w => w.Exercise)
            .Include(ts => ts.TrainingTypes)
            .FirstOrDefault(x => x.Id == trainingSessionToBUpdated.Id);

        Assert.NotNull(updatedSession);
        Assert.NotEmpty(updatedSession.TrainingSessionExerciseRecords);
        Assert.Equal(3, updatedSession.TrainingTypes.Count);
        Assert.Equal(DateTime.Parse(updateDto.CreatedAt), updatedSession.CreatedAt);
        Assert.Equal(updateDto.Notes, updatedSession.Notes);
        Assert.Equal(updateDto.TotalCaloriesBurned, updatedSession.TotalCaloriesBurned);
        Assert.Equal(updateDto.DurationInMinutes, updatedSession.DurationInSeconds / 60);
        Assert.Equal(updateDto.DurationInMinutes * 60, updatedSession.DurationInSeconds);
        Assert.Equal(updateDto.Mood, updatedSession.Mood);

        Assert.Equal("dragon flag", updatedSession.TrainingSessionExerciseRecords.First().ExerciseRecord.Exercise.Name);
        Assert.Equal(30, updatedSession.TrainingSessionExerciseRecords.First().ExerciseRecord.Repetitions);
    }

    /// <summary>
    /// Partial update should leave related <em> exercise records</em> and <em> training type </em> <strong>intact</strong>.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task UpdateSessionAsync_FullSessionUpdateWithNoExerciseRecordsUpdate_Returns_Success()
    {
        DatabaseHelpers.SeedTypesExercisesAndMuscles(context);
        var trainingSessionToBUpdated =
                new TrainingSession
                {
                    TotalCaloriesBurned = 666,
                    CreatedAt = DateTime.UtcNow,
                    DurationInSeconds = 7200,
                    Mood = 9,
                    Notes = "Test record for testing"
                };

        context.TrainingSessions.Add(trainingSessionToBUpdated);
        context.SaveChanges();
        DatabaseHelpers.SeedWorkOutRecords(trainingSessionToBUpdated.Id, context);
        context.ChangeTracker.Clear();

        var updateDto = new TrainingSessionWriteDto
        {
            TotalCaloriesBurned = 777,
            CreatedAt = "6-2-2024",
            DurationInMinutes = 120,
            Mood = 5,
            Notes = "Full on update"
        };

        var result = await service.UpdateSessionAsync(trainingSessionToBUpdated.Id, updateDto, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        var updatedSession = context.TrainingSessions
            .Include(ts => ts.TrainingSessionExerciseRecords)
                    .ThenInclude(e => e.ExerciseRecord)
                        .ThenInclude(w => w.Exercise)
            .Include(ts => ts.TrainingTypes)
            .FirstOrDefault(x => x.Id == trainingSessionToBUpdated.Id);

        Assert.NotNull(updatedSession);
        Assert.NotEmpty(updatedSession.TrainingSessionExerciseRecords);
        Assert.Equal(4, updatedSession.TrainingTypes.Count);
        Assert.Equal(DateTime.Parse(updateDto.CreatedAt), updatedSession.CreatedAt);
        Assert.Equal(updateDto.Notes, updatedSession.Notes);
        Assert.Equal(updateDto.TotalCaloriesBurned, updatedSession.TotalCaloriesBurned);
        Assert.Equal(updateDto.DurationInMinutes, updatedSession.DurationInSeconds / 60);
        Assert.Equal(updateDto.DurationInMinutes * 60, updatedSession.DurationInSeconds);
        Assert.Equal(updateDto.Mood, updatedSession.Mood);

        string[] relatedExerciseNames = { "dragon flag", "rope jumping", "barbell curl" };

        foreach (var exerciseRecord in updatedSession.TrainingSessionExerciseRecords)
        {
            Assert.True(relatedExerciseNames.Contains(exerciseRecord.ExerciseRecord.Exercise.Name));
        };
    }


    public static ICollection<object[]> TrainingSessionsToTestTheUpdate()
    {
        var sessionDate = new DateTime(2024, 5, 1, 17, 50, 21);
        return new List<object[]>
        {
            new object[]
            {
                new Tuple<TrainingSession, TrainingSessionWriteDto>(
                    new TrainingSession {
                    TotalCaloriesBurned = 000,
                    Notes = "what the fuck am i seeing ?!",
                    DurationInSeconds =  1300,
                    Mood = 5,
                    CreatedAt = sessionDate,
                }, new TrainingSessionWriteDto{
                    TotalCaloriesBurned = 777
                } ),
            },
           new object[]
            {
                new Tuple<TrainingSession, TrainingSessionWriteDto>(
                    new TrainingSession {
                    TotalCaloriesBurned = 001,
                    Notes = "Food is calling ?",
                    DurationInSeconds =  1700,
                    Mood = 2,
                    CreatedAt = sessionDate,
                }, new TrainingSessionWriteDto{
                    Notes = "IT IS !!",
                   }),
            },
            new object[]
            {
                new Tuple<TrainingSession, TrainingSessionWriteDto>(
                    new TrainingSession {
                    TotalCaloriesBurned = 000,
                    Notes = "wait, i did not cook yet!",
                    DurationInSeconds =  2,
                    Mood = 1,
                    CreatedAt = sessionDate,
                }, new TrainingSessionWriteDto{
                    CreatedAt = sessionDate.AddDays(1).ToString()
                } ),
            },
            new object[]
            {
                new Tuple<TrainingSession, TrainingSessionWriteDto>(
                    new TrainingSession {
                    TotalCaloriesBurned = 000,
                    Notes = "what the fuck am i seeing ?!",
                    DurationInSeconds =  1300,
                    Mood = 5,
                    CreatedAt = sessionDate,
                }, new TrainingSessionWriteDto{
                    Mood = 6
                } ),
            },
            new object[]
            {
                new Tuple<TrainingSession, TrainingSessionWriteDto>(
                    new TrainingSession {
                    TotalCaloriesBurned = 000,
                    Notes = "what the fuck am i seeing ?!",
                    DurationInSeconds =  1300,
                    Mood = 5,
                    CreatedAt = sessionDate,
                }, new TrainingSessionWriteDto{
                    DurationInMinutes = 50
                } ),
            }
        };
    }


    [Theory]
    [MemberData(nameof(TrainingSessionsToTestTheUpdate))]
    public async Task UpdateSessionAsync_PartialSessionUpdateWithoutExerciseRecords_Returns_Success(Tuple<TrainingSession, TrainingSessionWriteDto> sessionData)
    {
        DatabaseHelpers.SeedTypesExercisesAndMuscles(context);
        var trainingSessionToBUpdated = sessionData.Item1;

        context.TrainingSessions.Add(trainingSessionToBUpdated);
        context.SaveChanges();
        DatabaseHelpers.SeedWorkOutRecords(trainingSessionToBUpdated.Id, context);
        context.ChangeTracker.Clear();

        var updateDto = sessionData.Item2;

        var result = await service.UpdateSessionAsync(trainingSessionToBUpdated.Id, updateDto, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        var updatedSession = context.TrainingSessions
            .Include(ts => ts.TrainingSessionExerciseRecords)
                    .ThenInclude(e => e.ExerciseRecord)
                        .ThenInclude(w => w.Exercise)
            .Include(ts => ts.TrainingTypes)
            .FirstOrDefault(x => x.Id == trainingSessionToBUpdated.Id);

        Assert.NotNull(updatedSession);
        Assert.Equal(3, updatedSession.TrainingSessionExerciseRecords.Count);
        Assert.Equal(4, updatedSession.TrainingTypes.Count); // 3 exercises, 4 training types


        if (updateDto.CreatedAt is not null) Assert.Equal(DateTime.Parse(updateDto.CreatedAt), updatedSession.CreatedAt);
        if (updateDto.Notes is not null) Assert.Equal(updateDto.Notes, updatedSession.Notes);
        if (updateDto.TotalCaloriesBurned is not null) Assert.Equal(updateDto.TotalCaloriesBurned, updatedSession.TotalCaloriesBurned);
        if (updateDto.DurationInMinutes is not null) Assert.Equal(updateDto.DurationInMinutes, updatedSession.DurationInSeconds / 60);
        if (updateDto.DurationInMinutes is not null) Assert.Equal(updateDto.DurationInMinutes * 60, updatedSession.DurationInSeconds);
        if (updateDto.Mood is not null) Assert.Equal(updateDto.Mood, updatedSession.Mood);

    }


    public static ICollection<object[]> TrainingSessionsWithRecordsToTestTheUpdate()
    {
        var sessionDate = new DateTime(2024, 5, 1, 17, 50, 21);
        return new List<object[]>
        {
            new object[]
            {
                new Tuple<TrainingSession, TrainingSessionWriteDto>(
                    new TrainingSession {
                    TotalCaloriesBurned = 000,
                    Notes = "what the fuck am i seeing ?!",
                    DurationInSeconds =  1300,
                    Mood = 5,
                    CreatedAt = sessionDate,
                }, new TrainingSessionWriteDto{
                    TotalCaloriesBurned = 777,
                    ExerciseRecords = new List<ExerciseRecordWriteDto>
                    {
                        new ExerciseRecordWriteDto
                        {
                            ExerciseName = "dragon flag",
                            Repetitions = 20
                        },
                        new ExerciseRecordWriteDto
                        {
                            ExerciseName = "dragon flag",
                            Repetitions = 20
                        },
                        new ExerciseRecordWriteDto
                        {
                            ExerciseName = "dragon flag",
                            Repetitions = 20
                        },
                        new ExerciseRecordWriteDto
                        {
                            ExerciseName = "rope jumping",
                            TimerInSeconds = 1700
                        }
                    }
                } ),
            },
           new object[]
            {
                new Tuple<TrainingSession, TrainingSessionWriteDto>(
                    new TrainingSession {
                    TotalCaloriesBurned = 001,
                    Notes = "Food is calling ?",
                    DurationInSeconds =  1700,
                    Mood = 2,
                    CreatedAt = sessionDate,
                }, new TrainingSessionWriteDto{
                    Notes = "IT IS !!",
                    ExerciseRecords = new List<ExerciseRecordWriteDto>
                    {
                        new ExerciseRecordWriteDto
                        {
                            ExerciseName = "dragon flag",
                            Repetitions = 20
                        },
                        new ExerciseRecordWriteDto
                        {
                            ExerciseName = "dragon flag",
                            Repetitions = 20
                        },
                        new ExerciseRecordWriteDto
                        {
                            ExerciseName = "dragon flag",
                            Repetitions = 20
                        },
                        new ExerciseRecordWriteDto
                        {
                            ExerciseName = "rope jumping",
                            TimerInSeconds = 1700,
                            DistanceInMeters = 12,
                            Repetitions = 1500,
                            WeightUsedKg = 1
                        }
                    }
                   }),
            },
            new object[]
            {
                new Tuple<TrainingSession, TrainingSessionWriteDto>(
                    new TrainingSession {
                    TotalCaloriesBurned = 000,
                    Notes = "wait, i did not cook yet!",
                    DurationInSeconds =  2,
                    Mood = 1,
                    CreatedAt = sessionDate,
                }, new TrainingSessionWriteDto{
                    CreatedAt = sessionDate.AddDays(1).ToString(),
                    ExerciseRecords = new List<ExerciseRecordWriteDto>
                    {
                        new ExerciseRecordWriteDto
                        {
                            ExerciseName = "dragon flag",
                            Repetitions = 20
                        },
                        new ExerciseRecordWriteDto
                        {
                            ExerciseName = "dragon flag",
                            Repetitions = 20
                        },
                        new ExerciseRecordWriteDto
                        {
                            ExerciseName = "dragon flag",
                            Repetitions = 20
                        },
                        new ExerciseRecordWriteDto
                        {
                            ExerciseName = "rope jumping",
                            TimerInSeconds = 1700,
                            DistanceInMeters = 12,
                            Repetitions = 1500,
                            WeightUsedKg = 1
                        },
                        new ExerciseRecordWriteDto
                        {
                            ExerciseName = "barbell curl",
                            Notes = "there was an attempt",
                            WeightUsedKg = 30,
                            Repetitions = 20
                        }
                    }
                } ),
            },
            new object[]
            {
                new Tuple<TrainingSession, TrainingSessionWriteDto>(
                    new TrainingSession {
                    TotalCaloriesBurned = 000,
                    Notes = "what the fuck am i seeing ?!",
                    DurationInSeconds =  1300,
                    Mood = 5,
                    CreatedAt = sessionDate,
                }, new TrainingSessionWriteDto{
                    Mood = 6,
                    ExerciseRecords = new List<ExerciseRecordWriteDto>
                    {
                        new ExerciseRecordWriteDto
                        {
                            ExerciseName = "dragon flag",
                            Repetitions = 20
                        },
                        new ExerciseRecordWriteDto
                        {
                            ExerciseName = "dragon flag",
                            Repetitions = 20
                        },
                        new ExerciseRecordWriteDto
                        {
                            ExerciseName = "dragon flag",
                            Repetitions = 20
                        },
                        new ExerciseRecordWriteDto
                        {
                            ExerciseName = "rope jumping",
                            TimerInSeconds = 1700,
                            DistanceInMeters = 12,
                            Repetitions = 1500,
                            WeightUsedKg = 1
                        },
                        new ExerciseRecordWriteDto
                        {
                            ExerciseName = "barbell curl",
                            Notes = "there was an attempt",
                            WeightUsedKg = 30,
                            Repetitions = 20
                        }
                    }
                } ),
            },
            new object[]
            {
                new Tuple<TrainingSession, TrainingSessionWriteDto>(
                    new TrainingSession {
                    TotalCaloriesBurned = 000,
                    Notes = "what the fuck am i seeing ?!",
                    DurationInSeconds =  1300,
                    Mood = 5,
                    CreatedAt = sessionDate,
                }, new TrainingSessionWriteDto{
                    DurationInMinutes = 50,
                    ExerciseRecords = new List<ExerciseRecordWriteDto>
                    {
                        new ExerciseRecordWriteDto
                        {
                            ExerciseName = "dragon flag",
                            Repetitions = 20
                        },
                        new ExerciseRecordWriteDto
                        {
                            ExerciseName = "dragon flag",
                            Repetitions = 20
                        },
                        new ExerciseRecordWriteDto
                        {
                            ExerciseName = "dragon flag",
                            Repetitions = 20
                        },
                        new ExerciseRecordWriteDto
                        {
                            ExerciseName = "rope jumping",
                            TimerInSeconds = 1700,
                            DistanceInMeters = 12,
                            Repetitions = 1500,
                            WeightUsedKg = 1
                        },
                        new ExerciseRecordWriteDto
                        {
                            ExerciseName = "barbell curl",
                            Notes = "there was an attempt",
                            WeightUsedKg = 30,
                            Repetitions = 20
                        }
                    }
                } ),
            }
        };
    }


    [Theory]
    [MemberData(nameof(TrainingSessionsWithRecordsToTestTheUpdate))]
    public async Task UpdateSessionAsync_PartialSessionWithExerciseRecordsUpdate_Returns_Success(Tuple<TrainingSession, TrainingSessionWriteDto> sessionData)
    {
        DatabaseHelpers.SeedTypesExercisesAndMuscles(context);
        var trainingSessionToBUpdated = sessionData.Item1;

        context.TrainingSessions.Add(trainingSessionToBUpdated);
        context.SaveChanges();
        DatabaseHelpers.SeedWorkOutRecords(trainingSessionToBUpdated.Id, context);
        context.ChangeTracker.Clear();

        var updateDto = sessionData.Item2;

        var result = await service.UpdateSessionAsync(trainingSessionToBUpdated.Id, updateDto, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        var updatedSession = context.TrainingSessions
            .Include(ts => ts.TrainingSessionExerciseRecords)
                    .ThenInclude(e => e.ExerciseRecord)
                        .ThenInclude(w => w.Exercise)
            .Include(ts => ts.TrainingTypes)
            .FirstOrDefault(x => x.Id == trainingSessionToBUpdated.Id);

        Assert.NotNull(updatedSession);

        if (updateDto.CreatedAt is not null) Assert.Equal(DateTime.Parse(updateDto.CreatedAt), updatedSession.CreatedAt);
        if (updateDto.Notes is not null) Assert.Equal(updateDto.Notes, updatedSession.Notes);
        if (updateDto.TotalCaloriesBurned is not null) Assert.Equal(updateDto.TotalCaloriesBurned, updatedSession.TotalCaloriesBurned);
        if (updateDto.DurationInMinutes is not null) Assert.Equal(updateDto.DurationInMinutes, updatedSession.DurationInSeconds / 60);
        if (updateDto.DurationInMinutes is not null) Assert.Equal(updateDto.DurationInMinutes * 60, updatedSession.DurationInSeconds);
        if (updateDto.Mood is not null) Assert.Equal(updateDto.Mood, updatedSession.Mood);

        // test related entities

        // get related exercise and from them the types
        var relatedExercises = await service.GetRelatedExercises(
            updatedSession.TrainingSessionExerciseRecords
                .Select(x => Utils.NormalizeString(x.ExerciseRecord.Exercise.Name))
                .Distinct()
                .ToList()
                , new CancellationToken()
                );

        List<TrainingType> relatedTypes = relatedExercises
                .SelectMany(x => x.TrainingTypes)
                .Distinct()
                .ToList();

        Assert.Equal(relatedTypes.Count, updatedSession.TrainingTypes.Count);

        for (var i = 0; i < updatedSession.TrainingSessionExerciseRecords.Count(); i++)
        {
            Assert.NotNull(updatedSession.TrainingSessionExerciseRecords.ToList()[i]);
            var trainingSessionExerciseRecord = updatedSession.TrainingSessionExerciseRecords.ToList()[i];
            var exerciseRecord = trainingSessionExerciseRecord.ExerciseRecord!;

            if (exerciseRecord.WeightUsedKg is not null)
                Assert.Equal(exerciseRecord.WeightUsedKg, trainingSessionExerciseRecord.LastWeightUsedKg);

            if (updateDto.ExerciseRecords[i].ExerciseName is not null)
                Assert.Equal(updateDto.ExerciseRecords[i].ExerciseName, exerciseRecord.Exercise.Name);

            if (updateDto.ExerciseRecords[i].Notes is not null)
                Assert.Equal(updateDto.ExerciseRecords[i].Notes, exerciseRecord.Notes);

            if (updateDto.ExerciseRecords[i].WeightUsedKg is not null)
                Assert.Equal(updateDto.ExerciseRecords[i].WeightUsedKg, exerciseRecord.WeightUsedKg);

            if (updateDto.ExerciseRecords[i].Repetitions is not null)
                Assert.Equal(updateDto.ExerciseRecords[i].Repetitions, exerciseRecord.Repetitions);

            if (updateDto.ExerciseRecords[i].DistanceInMeters is not null)
                Assert.Equal(updateDto.ExerciseRecords[i].DistanceInMeters, exerciseRecord.DistanceInMeters);

            if (updateDto.ExerciseRecords[i].TimerInSeconds is not null)
                Assert.Equal(updateDto.ExerciseRecords[i].TimerInSeconds, exerciseRecord.TimerInSeconds);

        }

    }

    [Fact]
    public async Task Delete_should_clean_all_related_records_and_returns_success()
    {
        DatabaseHelpers.SeedTypesExercisesAndMuscles(context);
        var trainingSessionToBUpdated =
                new TrainingSession
                {
                    TotalCaloriesBurned = 666,
                    CreatedAt = DateTime.UtcNow,
                    DurationInSeconds = 7200,
                    Mood = 9,
                    Notes = "Test record for testing"
                };

        context.TrainingSessions.Add(trainingSessionToBUpdated);
        context.SaveChanges();
        DatabaseHelpers.SeedWorkOutRecords(trainingSessionToBUpdated.Id, context);
        context.ChangeTracker.Clear();

        var result = await service.DeleteSessionAsync(trainingSessionToBUpdated.Id, new CancellationToken());
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        var recordInTheDataBase = context.TrainingSessions.FirstOrDefault(x => x.Id == trainingSessionToBUpdated.Id);

        Assert.Null(recordInTheDataBase);
        Assert.Empty(context.TrainingSessions);
        Assert.Empty(context.TrainingSessionExerciseRecords);
        Assert.Empty(context.ExerciseRecords);
        // exercise types relation table should also be empoty!

    }
        [Fact]
    public async Task CreateBulkSessionsAsync_ShouldInsertSessionsCorrectly()
    {
        // Arrange
        DatabaseHelpers.SeedExtendedTypesExercisesAndMuscles(context);

        var newSessions = new List<TrainingSessionWriteDto>
        {
            new TrainingSessionWriteDto
            {
                DurationInMinutes = 90,
                TotalCaloriesBurned = 300,
                Notes = "Morning session",
                Mood = 8,
                CreatedAt = "07-07-2024",
                ExerciseRecords = new List<ExerciseRecordWriteDto>
                {
                    new ExerciseRecordWriteDto
                    {
                        ExerciseName = "dragon flag",
                        Repetitions = 20,
                        WeightUsedKg = 0
                    },
                    new ExerciseRecordWriteDto
                    {
                        ExerciseName = "rope jumping",
                        TimerInSeconds = 60
                    }
                }
            },
            new TrainingSessionWriteDto
            {
                DurationInMinutes = 45,
                TotalCaloriesBurned = 200,
                Notes = "Evening session",
                Mood = 7,
                CreatedAt = "07-07-2024",
                ExerciseRecords = new List<ExerciseRecordWriteDto>
                {
                    new ExerciseRecordWriteDto
                    {
                        ExerciseName = "barbell curl",
                        Repetitions = 15,
                        WeightUsedKg = 30
                    }
                }
            }
        };

        // Act
        var result = await service.CreateBulkSessionsAsync(newSessions, new CancellationToken());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        var insertedSessions = context.TrainingSessions
            .Include(ts => ts.TrainingSessionExerciseRecords)
                .ThenInclude(ts => ts.ExerciseRecord)
            .Include(ts => ts.TrainingTypes)
            .ToList();

        Assert.Equal(2, insertedSessions.Count);
        Assert.All(insertedSessions, session =>
        {
            Assert.NotEmpty(session.TrainingSessionExerciseRecords);
            Assert.NotEmpty(session.TrainingTypes);
        });
    }

    [Fact]
    public async Task CreateBulkSessionsAsync_ShouldHandleEmptyList()
    {
        // Arrange
        var newSessions = new List<TrainingSessionWriteDto>();

        // Act
        var result = await service.CreateBulkSessionsAsync(newSessions, new CancellationToken());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Error creating bulk sessions: The input list is empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateBulkSessionsAsync_ShouldHandleInvalidExercises()
    {
        // Arrange
        DatabaseHelpers.SeedExtendedTypesExercisesAndMuscles(context);

        var newSessions = new List<TrainingSessionWriteDto>
        {
            new TrainingSessionWriteDto
            {
                DurationInMinutes = 90,
                TotalCaloriesBurned = 300,
                Notes = "Morning session",
                Mood = 8,
                CreatedAt = "07-07-2024",
                ExerciseRecords = new List<ExerciseRecordWriteDto>
                {
                    new ExerciseRecordWriteDto
                    {
                        ExerciseName = "invalid exercise",
                        Repetitions = 20,
                        WeightUsedKg = 0
                    }
                }
            }
        };

        // Act
        var result = await service.CreateBulkSessionsAsync(newSessions, new CancellationToken());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Error creating bulk sessions: one or more exercises could not be found", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateBulkSessionsAsync_ShouldHandleMixedValidInvalidExercises()
    {
        // Arrange
        DatabaseHelpers.SeedExtendedTypesExercisesAndMuscles(context);

        var newSessions = new List<TrainingSessionWriteDto>
        {
            new TrainingSessionWriteDto
            {
                DurationInMinutes = 90,
                TotalCaloriesBurned = 300,
                Notes = "Morning session",
                Mood = 8,
                CreatedAt = "07-07-2024",
                ExerciseRecords = new List<ExerciseRecordWriteDto>
                {
                    new ExerciseRecordWriteDto
                    {
                        ExerciseName = "dragon flag",
                        Repetitions = 20,
                        WeightUsedKg = 0
                    },
                    new ExerciseRecordWriteDto
                    {
                        ExerciseName = "invalid exercise",
                        Repetitions = 20,
                        WeightUsedKg = 0
                    }
                }
            },
            new TrainingSessionWriteDto
            {
                DurationInMinutes = 45,
                TotalCaloriesBurned = 200,
                Notes = "Evening session",
                Mood = 7,
                CreatedAt = "07-07-2024",
                ExerciseRecords = new List<ExerciseRecordWriteDto>
                {
                    new ExerciseRecordWriteDto
                    {
                        ExerciseName = "barbell curl",
                        Repetitions = 15,
                        WeightUsedKg = 30
                    }
                }
            }
        };

        // Act
        var result = await service.CreateBulkSessionsAsync(newSessions, new CancellationToken());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Error creating bulk sessions: one or more exercises could not be found", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateBulkSessionsAsync_ShouldHandleNoExerciseRecords()
    {
        // Arrange
        DatabaseHelpers.SeedExtendedTypesExercisesAndMuscles(context);

        var newSessions = new List<TrainingSessionWriteDto>
        {
            new TrainingSessionWriteDto
            {
                DurationInMinutes = 90,
                TotalCaloriesBurned = 300,
                Notes = "Morning session",
                Mood = 8,
                CreatedAt = "07-07-2024",
                ExerciseRecords = new List<ExerciseRecordWriteDto>()
            }
        };

        // Act
        var result = await service.CreateBulkSessionsAsync(newSessions, new CancellationToken());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Error creating bulk sessions: No exercise records found in one or more sessions.", result.ErrorMessage);
    }
}
