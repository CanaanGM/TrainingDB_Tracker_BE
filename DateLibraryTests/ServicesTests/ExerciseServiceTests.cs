using DataLibrary.Dtos;
using DataLibrary.Models;
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
        DatabaseHelpers.SeedEquipmentAndAssociateThemWithLanguages(_context);
        DatabaseHelpers.SeedTrainingTypes(_context);
    }

    private ExerciseWriteDto GenerateValidExerciseDto(string name = "Squat", string languageCode = "en")
    {
        return new ExerciseWriteDto
        {
            Name = name,
            LanguageCode = languageCode,
            Difficulty = 3,
            Description = "A standard exercise to strengthen lower body muscles.",
            HowTo = "Perform a standard squat ensuring proper form.",
            ExerciseMuscles = new List<ExerciseMuscleWriteDto>
            {
                new ExerciseMuscleWriteDto
                {
                    MuscleName = "gluteus maximus",
                    IsPrimary = true
                }
            },
            TrainingTypes = new List<string>
            {
                "strength"
            },
            HowTos = new List<ExerciseHowToWriteDto>
            {
                new ExerciseHowToWriteDto
                {
                    Name = "standard squat",
                    Url = "http://example.com/squat"
                }
            },
            EquipmentNeeded = new List<string>
            {
                "barbell"
            }
        };
    }


    [Fact]
    public async Task CreateAsync_Success()
    {
        // Arrange
        var dto = GenerateValidExerciseDto();

        // Act
        var result = await _service.CreateAsync(dto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(0, result.Value);
    }

    [Fact]
    public async Task CreateAsync_Failure_MissingLanguage()
    {
        // Arrange
        var dto = GenerateValidExerciseDto(languageCode: "es"); // Assuming 'es' is not seeded

        // Act
        var result = await _service.CreateAsync(dto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("could not be found", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateAsync_Failure_MissingMuscle()
    {
        // Arrange
        var dto = GenerateValidExerciseDto();
        dto.ExerciseMuscles.Add(new ExerciseMuscleWriteDto { MuscleName = "Nonexistent Muscle" });

        // Act
        var result = await _service.CreateAsync(dto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("muscles could not be found", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateAsync_Failure_MissingTrainingType()
    {
        // Arrange
        var dto = GenerateValidExerciseDto();
        dto.TrainingTypes.Add("Nonexistent Training Type");

        // Act
        var result = await _service.CreateAsync(dto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("training types are not created", result.ErrorMessage);
    }


    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public async Task CreateAsync_DifficultyBoundary_Valid(int difficulty)
    {
        // Arrange
        var dto = GenerateValidExerciseDto();
        dto.Difficulty = difficulty;

        // Act
        var result = await _service.CreateAsync(dto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CreateAsync_MultipleHowTos_Success()
    {
        // Arrange
        var dto = GenerateValidExerciseDto();
        dto.HowTos.Add(new ExerciseHowToWriteDto { Name = "Jump Squat", Url = "http://example.com/jumpsquat" });

        // Act
        var result = await _service.CreateAsync(dto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, dto.HowTos.Count);
    }

    [Fact]
    public async Task CreateAsync_ConcurrentCreation()
    {
        // Arrange
        var dto1 = GenerateValidExerciseDto();
        var dto2 = GenerateValidExerciseDto("Sprint");

        var task1 = _service.CreateAsync(dto1, CancellationToken.None);
        var task2 = _service.CreateAsync(dto2, CancellationToken.None);

        // Act
        var results = await Task.WhenAll(task1, task2);

        // Assert
        Assert.All(results, result => Assert.True(result.IsSuccess));
    }

    [Fact]
    public async Task CreateAsync_EmptyOptionalLists_Success()
    {
        // Arrange
        var dto = GenerateValidExerciseDto();
        dto.EquipmentNeeded = new List<string>();
        dto.HowTos = new List<ExerciseHowToWriteDto>();

        // Act
        var result = await _service.CreateAsync(dto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CreateBulkAsync_ConcurrentCreation_ShouldHandleCorrectly()
    {
        // Arrange
        var dto1 = GenerateValidExerciseDto("Bench Press");
        var dto2 = GenerateValidExerciseDto("Leg Press");

        // These DTOs should be different to simulate concurrent creation of different exercises.
        var task1 = _service.CreateBulkAsync(new List<ExerciseWriteDto> { dto1 }, CancellationToken.None);
        var task2 = _service.CreateBulkAsync(new List<ExerciseWriteDto> { dto2 }, CancellationToken.None);

        // Act
        var results = await Task.WhenAll(task1, task2);

        // Assert
        Assert.All(results, result => Assert.True(result.IsSuccess, "Both operations should be successful."));
    }

    [Fact]
    public async Task CreateBulkAsync_WithValidExercises_ShouldReturnSuccess()
    {
        // Arrange
        var exercises = GenerateValidExerciseList();

        // Act
        var result = await _service.CreateBulkAsync(exercises, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }


    [Fact]
    public async Task CreateBulkAsync_WithNullList_ShouldFail()
    {
        // Act
        var result = await _service.CreateBulkAsync(null, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("No exercises to create.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateBulkAsync_WithEmptyList_ShouldFail()
    {
        // Arrange
        var exercises = new List<ExerciseWriteDto>();

        // Act
        var result = await _service.CreateBulkAsync(exercises, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("No exercises to create.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateBulkAsync_WithMissingMuscles_ShouldFail()
    {
        // Arrange
        var exercises = new List<ExerciseWriteDto>
        {
            new ExerciseWriteDto
            {
                Name = "squat",
                LanguageCode = "en",
                Difficulty = 3,
                HowTo = "some string",
                Description = "some description",
                ExerciseMuscles = new List<ExerciseMuscleWriteDto>
                {
                    new ExerciseMuscleWriteDto { MuscleName = "NonExistentMuscle", IsPrimary = true }
                },
                TrainingTypes = new List<string> { "strength" },
                HowTos = new List<ExerciseHowToWriteDto>
                {
                    new ExerciseHowToWriteDto { Name = "Standard Squat", Url = "http://example.com/squat" }
                },
                EquipmentNeeded = new List<string> { "Barbell" }
            }
        };

        // Act
        var result = await _service.CreateBulkAsync(exercises, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("muscles could not be found", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateBulkAsync_WithAllValidExercises_ShouldReturnSuccess()
    {
        // Arrange
        var exercises = GenerateValidExerciseList();

        // Act
        var result = await _service.CreateBulkAsync(exercises, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task CreateBulkAsync_WithInvalidLanguageCodes_ShouldFail()
    {
        // Arrange
        var exercises = GenerateValidExerciseList();
        exercises.ForEach(e => e.LanguageCode = "xx"); // Invalid language code

        // Act
        var result = await _service.CreateBulkAsync(exercises, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Language with code", result.ErrorMessage);
    }

    private List<ExerciseWriteDto> GenerateValidExerciseList()
    {
        return new List<ExerciseWriteDto>
        {
            new ExerciseWriteDto
            {
                Name = "squat",
                LanguageCode = "en",
                Difficulty = 3,
                Description = "A compound exercise targeting the lower body",
                HowTo = "Stand with feet hip-width apart, then lower down as if sitting in a chair and rise back up.",
                ExerciseMuscles = new List<ExerciseMuscleWriteDto>
                {
                    new ExerciseMuscleWriteDto { MuscleName = "gluteus maximus", IsPrimary = true },
                    new ExerciseMuscleWriteDto { MuscleName = "rectus femoris", IsPrimary = false }
                },
                TrainingTypes = new List<string> { "strength" },
                HowTos = new List<ExerciseHowToWriteDto>
                {
                    new ExerciseHowToWriteDto { Name = "Standard Squat", Url = "http://example.com/squat" }
                },
                EquipmentNeeded = new List<string> { "Barbell" }
            },
            new ExerciseWriteDto
            {
                Name = "deadlift",
                LanguageCode = "en",
                Difficulty = 4,
                Description = "A strength exercise that targets multiple muscles",
                HowTo =
                    "Bend at the hip to grip the bar at shoulder width, pull your body up and back as you stand up with the bar.",
                ExerciseMuscles = new List<ExerciseMuscleWriteDto>
                {
                    new ExerciseMuscleWriteDto { MuscleName = "biceps femoris", IsPrimary = true },
                    new ExerciseMuscleWriteDto { MuscleName = "gluteus maximus", IsPrimary = false }
                },
                TrainingTypes = new List<string> { "strength" },
                HowTos = new List<ExerciseHowToWriteDto>
                {
                    new ExerciseHowToWriteDto { Name = "Conventional Deadlift", Url = "http://example.com/deadlift" }
                },
                EquipmentNeeded = new List<string> { "Barbell" }
            }
        };
    }

    [Fact]
    public async Task UpdateAsync_ValidExercise_ShouldReturnSuccess()
    {
        // Arrange
        var exerciseDto = GenerateValidExerciseDto();
        var createResult = await _service.CreateAsync(exerciseDto, CancellationToken.None);
        var updateDto = GenerateValidExerciseDto("Updated Squat");

        // Act
        var result = await _service.UpdateAsync(createResult.Value, updateDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(createResult.Value, result.Value);
    }

    [Fact]
    public async Task UpdateAsync_ExerciseNotFound_ShouldReturnFailure()
    {
        // Arrange
        var updateDto = GenerateValidExerciseDto("Updated Squat");

        // Act
        var result = await _service.UpdateAsync(-1, updateDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Exercise with ID -1 not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_LanguageNotFound_ShouldReturnFailure()
    {
        // Arrange
        var exerciseDto = GenerateValidExerciseDto();
        var createResult = await _service.CreateAsync(exerciseDto, CancellationToken.None);
        var updateDto = GenerateValidExerciseDto("Updated Squat");
        updateDto.LanguageCode = "xx"; // Invalid language code

        // Act
        var result = await _service.UpdateAsync(createResult.Value, updateDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateAsync_InvalidMuscle_ShouldReturnFailure()
    {
        // Arrange
        var exerciseDto = GenerateValidExerciseDto();
        var createResult = await _service.CreateAsync(exerciseDto, CancellationToken.None);
        var updateDto = GenerateValidExerciseDto("Updated Squat");
        updateDto.ExerciseMuscles = new List<ExerciseMuscleWriteDto>
            { new ExerciseMuscleWriteDto { MuscleName = "invalid muscle", IsPrimary = true } };

        // Act
        var result = await _service.UpdateAsync(createResult.Value, updateDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("muscles could not be found.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_InvalidTrainingType_ShouldReturnFailure()
    {
        // Arrange
        var exerciseDto = GenerateValidExerciseDto();
        var createResult = await _service.CreateAsync(exerciseDto, CancellationToken.None);
        var updateDto = GenerateValidExerciseDto("Updated Squat");
        updateDto.TrainingTypes = new List<string> { "invalid training type" };

        // Act
        var result = await _service.UpdateAsync(createResult.Value, updateDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("training types are not created", result.ErrorMessage);
    }
    
    [Fact]
    public async Task UpdateAsync_ValidExerciseWithNewHowTo_ShouldReturnSuccess()
    {

        var exerciseDto = GenerateValidExerciseDto();
        var createResult = await _service.CreateAsync(exerciseDto, CancellationToken.None);

        var updateDto = GenerateValidExerciseDto("Updated Squat");
        updateDto.HowTos = new List<ExerciseHowToWriteDto>
        {
            new ExerciseHowToWriteDto { Name = "New HowTo", Url = "http://example.com/new-howto" }
        };

        // Act
        var result = await _service.UpdateAsync(createResult.Value, updateDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(createResult.Value, result.Value);
    }

    [Fact]
    public async Task UpdateAsync_ValidExerciseWithUpdatedDifficulty_ShouldReturnSuccess()
    {

        var exerciseDto = GenerateValidExerciseDto();
        var createResult = await _service.CreateAsync(exerciseDto, CancellationToken.None);

        var updateDto = GenerateValidExerciseDto("Updated Squat");
        updateDto.Difficulty = 4;

        // Act
        var result = await _service.UpdateAsync(createResult.Value, updateDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(createResult.Value, result.Value);
    }

    [Fact]
    public async Task UpdateAsync_ValidExerciseWithAdditionalEquipment_ShouldReturnSuccess()
    {

        var exerciseDto = GenerateValidExerciseDto();
        var createResult = await _service.CreateAsync(exerciseDto, CancellationToken.None);

        var updateDto = GenerateValidExerciseDto("Updated Squat");
        updateDto.EquipmentNeeded = new List<string> { "barbell", "dumbbell" };

        // Act
        var result = await _service.UpdateAsync(createResult.Value, updateDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(createResult.Value, result.Value);
    }

    [Fact]
    public async Task UpdateAsync_ValidExerciseWithNewDescription_ShouldReturnSuccess()
    {

        var exerciseDto = GenerateValidExerciseDto();
        var createResult = await _service.CreateAsync(exerciseDto, CancellationToken.None);

        var updateDto = GenerateValidExerciseDto("Updated Squat");
        updateDto.Description = "Updated Description";

        // Act
        var result = await _service.UpdateAsync(createResult.Value, updateDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(createResult.Value, result.Value);
    }

    [Fact]
    public async Task UpdateAsync_ValidExerciseWithNewTrainingTypes_ShouldReturnSuccess()
    {
        var exerciseDto = GenerateValidExerciseDto();
        var createResult = await _service.CreateAsync(exerciseDto, CancellationToken.None);

        var updateDto = GenerateValidExerciseDto("Updated Squat");
        updateDto.TrainingTypes = new List<string> { "strength", "endurance" };

        // Act
        var result = await _service.UpdateAsync(createResult.Value, updateDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(createResult.Value, result.Value);
    }
    
     [Fact]
    public async Task DeleteAsync_ExistingExercise_ShouldReturnSuccess()
    {
        // Arrange
        var exerciseDto = GenerateValidExerciseDto();
        var createResult = await _service.CreateAsync(exerciseDto, CancellationToken.None);

        // Act
        var result = await _service.DeleteAsync(createResult.Value, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        var deletedExercise = await _context.Exercises.FindAsync(createResult.Value);
        Assert.Null(deletedExercise);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingExercise_ShouldReturnFailure()
    {
        // Act
        var result = await _service.DeleteAsync(9999, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Exercise not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteAsync_ExerciseWithDependencies_ShouldReturnSuccess()
    {
        // Arrange
        var exerciseDto = GenerateValidExerciseDto();
        var createResult = await _service.CreateAsync(exerciseDto, CancellationToken.None);

        // Act
        var result = await _service.DeleteAsync(createResult.Value, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        var deletedExercise = await _context.Exercises.FindAsync(createResult.Value);
        Assert.Null(deletedExercise);
    }

    [Fact]
    public async Task DeleteAsync_ConcurrentDeletion_ShouldReturnSuccess()
    {
        // Arrange
        var exerciseDto1 = GenerateValidExerciseDto("exercise1");
        var exerciseDto2 = GenerateValidExerciseDto("exercise2");
        var createResult1 = await _service.CreateAsync(exerciseDto1, CancellationToken.None);
        var createResult2 = await _service.CreateAsync(exerciseDto2, CancellationToken.None);

        // Act
        var task1 = _service.DeleteAsync(createResult1.Value, CancellationToken.None);
        var task2 = _service.DeleteAsync(createResult2.Value, CancellationToken.None);
        var results = await Task.WhenAll(task1, task2);

        // Assert
        Assert.All(results, result => Assert.True(result.IsSuccess));

        var deletedExercise1 = await _context.Exercises.FindAsync(createResult1.Value);
        var deletedExercise2 = await _context.Exercises.FindAsync(createResult2.Value);
        Assert.Null(deletedExercise1);
        Assert.Null(deletedExercise2);
    }

    [Fact]
    public async Task DeleteAsync_ExerciseWithRelatedEntities_ShouldReturnSuccess()
    {
        var exerciseDto = GenerateValidExerciseDto();
        var createResult = await _service.CreateAsync(exerciseDto, CancellationToken.None);

        // Act
        var result = await _service.DeleteAsync(createResult.Value, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        var deletedExercise = await _context.Exercises.FindAsync(createResult.Value);
        Assert.Null(deletedExercise);
    }
}