using DataLibrary.Dtos;
using DataLibrary.Models;
using DataLibrary.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace DateLibraryTests.ServicesTests;

public class LanguageServiceTests : BaseTestClass
{
    private LanguageService _service;
    private Mock<ILogger<LanguageService>> _logger;

    public LanguageServiceTests() : base()
    {
        _logger = new Mock<ILogger<LanguageService>>();
        _service = new LanguageService(_context, _logger.Object, _mapper);
    }
    
    [Fact]
    public async Task CreateBulkAsync_ValidData_ReturnsSuccess()
    {
        // Arrange
        var newLanguagesDtos = new List<LanguageWriteDto>
        {
            new LanguageWriteDto { Code = "en", Name = "English" },
            new LanguageWriteDto { Code = "fr", Name = "French" }
        };

        // Act
        var result = await _service.CreateBulkAsync(newLanguagesDtos, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }
    [Fact]
    public async Task CreateBulkAsync_InvalidData_ReturnsFailure()
    {
        // Arrange
        var newLanguagesDtos = new List<LanguageWriteDto>
        {
            new LanguageWriteDto { Code = "", Name = "English" },
            new LanguageWriteDto { Code = "fr", Name = "" }
        };

        // Act
        var result = await _service.CreateBulkAsync(newLanguagesDtos, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
    }
    

    [Fact]
    public async Task GetAllAsync_ReturnsAllLanguages()
    {
        // Arrange
        var languages = new List<Language>
        {
            new Language { Id = 1, Code = "en", Name = "English" },
            new Language { Id = 2, Code = "fr", Name = "French" }
        };

        await _context.Languages.AddRangeAsync(languages);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.Contains(result.Value, l => l.Code == "en" && l.Name == "English");
        Assert.Contains(result.Value, l => l.Code == "fr" && l.Name == "French");
    }
    [Fact]
    public async Task DeleteAsync_ValidId_ReturnsSuccess()
    {
        // Arrange
        var language = new Language { Id = 1, Code = "en", Name = "English" };
        await _context.Languages.AddAsync(language);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAsync(language.Id, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(await _context.Languages.FindAsync(language.Id));
    }

    [Fact]
    public async Task DeleteAsync_InvalidId_ReturnsFailure()
    {
        // Act
        var result = await _service.DeleteAsync(999, CancellationToken.None); // Non-existing ID

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Language not found.", result.ErrorMessage);
    }
    
        [Fact]
    public async Task CreateBulkAsync_ValidData_LogsInformation()
    {
        // Arrange
        var newLanguagesDtos = new List<LanguageWriteDto>
        {
            new LanguageWriteDto { Code = "en", Name = "English" },
            new LanguageWriteDto { Code = "fr", Name = "French" }
        };

        // Act
        var result = await _service.CreateBulkAsync(newLanguagesDtos, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _logger.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString() == "Successfully created new languages."),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }

    [Fact]
    public async Task CreateBulkAsync_InvalidData_LogsWarning()
    {
        // Arrange
        var newLanguagesDtos = new List<LanguageWriteDto>
        {
            new LanguageWriteDto { Code = "", Name = "English" },
            new LanguageWriteDto { Code = "fr", Name = "" }
        };

        // Act
        var result = await _service.CreateBulkAsync(newLanguagesDtos, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        _logger.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Warning),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString() == "Invalid language DTOs provided."),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ValidId_LogsInformation()
    {
        // Arrange
        var language = new Language { Id = 1, Code = "en", Name = "English" };
        await _context.Languages.AddAsync(language);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAsync(language.Id, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _logger.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Successfully deleted language with ID {language.Id}."),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_InvalidId_LogsWarning()
    {
        // Act
        var result = await _service.DeleteAsync(999, CancellationToken.None); // Non-existing ID

        // Assert
        Assert.False(result.IsSuccess);
        _logger.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Warning),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString() == "Language with ID 999 not found."),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
    }
    
    [Fact]
    public async Task UpdateAsync_ValidData_ReturnsSuccess()
    {
        // Arrange
        var language = new Language { Id = 1, Code = "en", Name = "English" };
        await _context.Languages.AddAsync(language);
        await _context.SaveChangesAsync();

        var updatedLanguageDto = new LanguageReadDto { Id = 1, Code = "en-US", Name = "American English" };

        // Act
        var result = await _service.UpdateAsync(updatedLanguageDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var updatedLanguage = await _context.Languages.FindAsync(1);
        Assert.Equal("en-us", updatedLanguage.Code);
        Assert.Equal("american english", updatedLanguage.Name);
    }
    
    [Fact]
    public async Task UpdateAsync_InvalidId_ReturnsFailure()
    {
        // Arrange
        var updatedLanguageDto = new LanguageReadDto { Id = 999, Code = "fr-CA", Name = "Canadian French" };

        // Act
        var result = await _service.UpdateAsync(updatedLanguageDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Language not found.", result.ErrorMessage);
    }
}