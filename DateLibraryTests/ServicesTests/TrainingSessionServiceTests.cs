using AutoMapper;

using DataLibrary.Context;
using DataLibrary.Dtos;
using DataLibrary.Models;
using DataLibrary.Services;

using Microsoft.EntityFrameworkCore;

using Moq;

using TestSupport.EfHelpers;

namespace DateLibraryTests.ServicesTests;
public class TrainingSessionServiceTests
{
    private Mock<IMapper> _mockMapper;

    public TrainingSessionServiceTests()
    {
        System.Console.WriteLine("Life!");
        _mockMapper = new Mock<IMapper>();
    }

    ~TrainingSessionServiceTests()
    {
        System.Console.WriteLine("Death!");
    }


    [Fact]
    public async Task EnsureTestsAreRanSuccess()
    {
        Assert.Equal(1, 4 - 3);
    }

    [Fact]
    public async Task CreateSessionAsync_ShouldReturnSuccess_WhenSessionIsCreated()
    {
        DbContextOptionsDisposable<SqliteContext> options = SqliteInMemory.CreateOptions<SqliteContext>();
        using SqliteContext context = new SqliteContext(options);
        context.Database.EnsureCreated();

        TrainingSessionService trainingSessionService = new TrainingSessionService(context, _mockMapper.Object);

        DateTime sessionCreationDate = new DateTime(2020, 1, 1);
        TrainingSessionWriteDto newSession = new TrainingSessionWriteDto
        {
            Calories = 666,
            DurationInMinutes = 120,
            Mood = 9,
            Notes = "a test session",
            CreatedAt = sessionCreationDate.ToString()
        };

        List<ExerciseRecordWriteDto> newSessionTrainingRecords = new List<ExerciseRecordWriteDto> {
        new ExerciseRecordWriteDto
        {
            ExerciseName = "Dragon Flag",
            Repetitions = 20
        },
        new ExerciseRecordWriteDto{
            ExerciseName = "Dragon Flag",
            Repetitions = 15
        },
        new ExerciseRecordWriteDto{
            ExerciseName = "Dragon Flag",
            Repetitions = 10,
            Notes = "failure"
        },
        new ExerciseRecordWriteDto {
            ExerciseName = "Rope jumping",
            TimerInSeconds = 2000,
            Notes = "Should i add in a calories here too ?"
        }
     };

        newSession.ExerciseRecords = newSessionTrainingRecords;

        DataLibrary.Core.Result<bool> result = await trainingSessionService.CreateSessionAsync(newSession, new CancellationToken());

        TrainingSession? theSession = context.TrainingSessions
            .Include(x => x.TrainingTypes)
            .FirstOrDefault();
        Assert.NotNull(theSession);
        Assert.True(result.IsSuccess);
        Assert.Equal(1, context.TrainingSessions.Count());
        Assert.Equal(4, context.TrainingSessionExerciseRecords.Count());


    }
}
