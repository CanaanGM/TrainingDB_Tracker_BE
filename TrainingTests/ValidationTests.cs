using SharedLibrary.Dtos;
using SharedLibrary.Helpers;

namespace TrainingTests;

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
    
      private class TestDto
    {
        public ICollection<string> StringsCollection { get; set; }
        public ICollection<int> IntegersCollection { get; set; }
        public ICollection<double> EmptyCollection { get; set; }
        public string SomeString { get; set; }
    }

    [Fact]
    public void ValidateDtoICollections_AllCollectionsValid_Success()
    {
        // Arrange
        var dto = new TestDto
        {
            StringsCollection = new List<string> { "value1", "value2" },
            IntegersCollection = new List<int> { 1, 2, 3 },
            EmptyCollection = new List<double> { 1.1 }, // Non-empty collection
            SomeString = "Valid string"
        };

        // Act
        var result = Validation.ValidateDtoICollections(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("TestDto validation succeeded", result.SuccessMessage);
    }

    [Fact]
    public void ValidateDtoICollections_EmptyCollections_Failure()
    {
        // Arrange
        var dto = new TestDto
        {
            StringsCollection = new List<string>(),
            IntegersCollection = new List<int>(),
            EmptyCollection = null, // Null collection
            SomeString = "Valid string"
        };

        // Act
        var result = Validation.ValidateDtoICollections(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("StringsCollection cannot be null or empty", result.ErrorMessage);
        Assert.Contains("IntegersCollection cannot be null or empty", result.ErrorMessage);
        Assert.Contains("EmptyCollection cannot be null or empty", result.ErrorMessage);
    }

    [Fact]
    public void ValidateDtoICollections_NullCollection_Failure()
    {
        // Arrange
        var dto = new TestDto
        {
            StringsCollection = null,
            IntegersCollection = new List<int> { 1, 2, 3 },
            EmptyCollection = new List<double>(), // Empty but valid collection
            SomeString = "Valid string"
        };

        // Act
        var result = Validation.ValidateDtoICollections(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("StringsCollection cannot be null or empty", result.ErrorMessage);
    }

    [Fact]
    public void ValidateDtoICollections_SingleEmptyCollection_Failure()
    {
        // Arrange
        var dto = new TestDto
        {
            StringsCollection = new List<string> { "value1", "value2" },
            IntegersCollection = new List<int>(),
            EmptyCollection = new List<double>(), // Empty but valid collection
            SomeString = "Valid string"
        };

        // Act
        var result = Validation.ValidateDtoICollections(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("IntegersCollection cannot be null or empty", result.ErrorMessage);
    }
}