
using DataLibrary.Dtos;
using DataLibrary.Services;
using DateLibraryTests.helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DateLibraryTests.ServicesTests;

public class ExerciseServiceTests : BaseTestClass
{
    private Mock<ILogger<ExerciseService>> _logger;
    private ExerciseService _service;

    public ExerciseServiceTests() 
    {
        _logger = new Mock<ILogger<ExerciseService>>();
        _service = new ExerciseService(_context, _mapper, _logger.Object);
        DatabaseHelpers.SeedLanguages(_context);
        DatabaseHelpers.SeedEnglishMuscles(_context);
    }
    [Fact]
    public async Task CreateAsync_AllValidInputs_ReturnsSuccess()
    {
        // Arrange
        var dto = new ExerciseWriteDto
        {
            Name = "Complete Exercise",
            LanguageCode = "en",
            Difficulty = 2,
            Description = "A complete test",
            HowTo = "Do it well",
            ExerciseMuscles = new List<ExerciseMuscleWriteDto>
            {
                new ExerciseMuscleWriteDto { MuscleName = "adductor brevis", IsPrimary = true }
            },
            TrainingTypes = new List<string> { "Strength" },
            HowTos = new List<ExerciseHowToWriteDto>
            {
                new ExerciseHowToWriteDto { Name = "Step 1", Url = "http://example.com/howto1" }
            }
        };

        // Act
        var result = await _service.CreateAsync(dto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var createdExercise = await _context.Exercises
            .Include(x => x.LocalizedExercises)
            
            .FirstOrDefaultAsync(x => x.Id == result.Value);
        Assert.NotNull(createdExercise);
        Assert.Equal("complete exercise", createdExercise.LocalizedExercises.First().Name);
        Assert.Equal(1, createdExercise.Difficulty);
    }
    [Fact]
    public async Task CreateAsync_MissingLanguage_ReturnsFailure()
    {
        // Arrange
        var dto = new ExerciseWriteDto { LanguageCode = "invalid-code" };

        // Act
        var result = await _service.CreateAsync(dto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("language with code", result.ErrorMessage);
    }


}