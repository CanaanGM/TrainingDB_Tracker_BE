using DataLibrary.Dtos;
using DataLibrary.Helpers;

namespace DateLibraryTests;

public class ValidationTests
{
    [Fact]
    public void ValidateDtoStrings_NullStringProperty_ReturnsError()
    {
        // Arrange
        var dto = new ExerciseHowToWriteDto { Name = null, Url = "https://example.com" };

        // Act
        var result = Validation.ValidateDtoStrings(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Name cannot be null", result.ErrorMessage);
    }

    [Fact]
    public void ValidateDtoStrings_EmptyStringProperty_ReturnsError()
    {
        // Arrange
        var dto = new ExerciseHowToWriteDto { Name = "", Url = "https://example.com" };

        // Act
        var result = Validation.ValidateDtoStrings(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Name cannot be empty", result.ErrorMessage);
    }

    [Fact]
    public void ValidateDtoStrings_ValidStringProperty_ReturnsSuccess()
    {
        // Arrange
        var dto = new ExerciseHowToWriteDto { Name = "Exercise Name", Url = "https://example.com" };

        // Act
        var result = Validation.ValidateDtoStrings(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("ExerciseHowToWriteDto validation succeeded", result.SuccessMessage);
    }

    [Fact]
    public void ValidateDtoStrings_MultipleErrorProperties_ReturnsMultipleErrors()
    {
        // Arrange
        var dto = new ExerciseHowToWriteDto { Name = "", Url = null };

        // Act
        var result = Validation.ValidateDtoStrings(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Name cannot be empty", result.ErrorMessage);
        Assert.Contains("Url cannot be null", result.ErrorMessage);
    }
}