
using DataLibrary.Dtos;
using DataLibrary.Models;
using DataLibrary.Services;
using DateLibraryTests.helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DateLibraryTests.ServicesTests;

public class EquipmentServiceTests : BaseTestClass
{
    private Mock<ILogger<EquipmentService>> _logger;
    private EquipmentService _service;
    public EquipmentServiceTests() : base()
    {
        _logger = new Mock<ILogger<EquipmentService>>();
        _service = new EquipmentService(_context, _mapper, _logger.Object);
        DatabaseHelpers.SeedLanguages(_context);
    }
    

    [Fact]
    public async Task UpsertAsync_ShouldUpdateEquipmentName_WhenNewNameProvided()
    {
        // Arrange
        var equipment = new Equipment { WeightKg = 15 };
        _context.Equipment.Add(equipment);
        _context.SaveChanges();  // Save here to ensure Equipment gets an ID

        _context.LocalizedEquipments.Add(new LocalizedEquipment
        {
            EquipmentId = equipment.Id,
            LanguageId = 1, // Ensure 'en' corresponds to ID 1 in the seed
            Name = "dumbbell",
            Description = "Old weight"
        });
        _context.SaveChanges();  // Initial setup completed, database is in expected state

        var updateDto = new EquipmentWriteDto
        {
            Name = "dumbbell",
            NewName = "adjustable dumbbell",
            LanguageCode = "en",
            Description = "A more versatile weight",
            HowTo = "Use it carefully"
        };

        // Act
        var result = await _service.UpsertAsync(updateDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var updated = _context.LocalizedEquipments.FirstOrDefault(le => le.Name == "adjustable dumbbell");
        Assert.NotNull(updated);
        Assert.Equal("A more versatile weight", updated.Description);
        Assert.Equal("Use it carefully", updated.HowTo);
    }

    [Fact]
    public async Task UpsertAsync_ShouldAddNewEquipment_WhenEquipmentDoesNotExist()
    {
        // Arrange
        var newDto = new EquipmentWriteDto
        {
            Name = "kettlebell",
            LanguageCode = "en",
            Description = "Compact and high-intensity weight",
            HowTo = "Swing it safely",
            WeightKg = 24
        };

        // Act
        var result = await _service.UpsertAsync(newDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var newEquipment = _context.Equipment.FirstOrDefault(e => e.WeightKg == 24);
        Assert.NotNull(newEquipment);
        var localized = _context.LocalizedEquipments.FirstOrDefault(le => le.EquipmentId == newEquipment.Id);
        Assert.NotNull(localized);
        Assert.Equal("kettlebell", localized.Name);
        Assert.Equal("Compact and high-intensity weight", localized.Description);
        Assert.Equal("Swing it safely", localized.HowTo);
    }

    [Fact]
    public async Task UpsertAsync_ShouldFail_WhenInvalidLanguageCodeProvided()
    {
        // Arrange
        var newDto = new EquipmentWriteDto
        {
            Name = "barbell",
            LanguageCode = "xx",  // Invalid language code
            Description = "Standard barbell.",
            HowTo = "Lift carefully",
            WeightKg = 20
        };

        // Act
        var result = await _service.UpsertAsync(newDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("Language code is invalid", result.ErrorMessage);
    }
    [Fact]
    public async Task UpsertAsync_ShouldUpdateEquipmentDescriptionAndHowTo_WhenNoNewNameProvided()
    {
        // Arrange
        var equipment = new Equipment { WeightKg = 15 };
        _context.Equipment.Add(equipment);
        _context.SaveChanges();  // Save here to ensure Equipment gets an ID

        _context.LocalizedEquipments.Add(new LocalizedEquipment
        {
            EquipmentId = equipment.Id,
            LanguageId = 1, // Assuming 'en' corresponds to ID 1 in the seed
            Name = "dumbbell",
            Description = "Old weight",
            HowTo = "Old usage instructions"
        });
        _context.SaveChanges();

        var updateDto = new EquipmentWriteDto
        {
            Name = "dumbbell",
            LanguageCode = "en",
            Description = "Updated weight for more versatile workouts",
            HowTo = "Updated usage instructions",
            WeightKg = 15
        };

        // Act
        var result = await _service.UpsertAsync(updateDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var updated = _context.LocalizedEquipments
            .Include(x => x.Language)
            .FirstOrDefault(le => le.Name == "dumbbell");
        Assert.NotNull(updated);
        Assert.Equal("Updated weight for more versatile workouts", updated.Description);
        Assert.Equal("Updated usage instructions", updated.HowTo);
        Assert.Equal(updateDto.LanguageCode, updated.Language.Code);
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldSuccessfullyDeleteEquipment_WhenAllLocalizationsAreDeleted()
    {
        // Arrange
        var equipment = new Equipment { WeightKg = 20 };
        _context.Equipment.Add(equipment);
        _context.SaveChanges();

        _context.LocalizedEquipments.Add(new LocalizedEquipment
        {
            EquipmentId = equipment.Id,
            LanguageId = 1, 
            Name = "barbell",
            Description = "Heavy bar"
        });
        _context.SaveChanges();

        // Act
        var result = await _service.DeleteAsync("barbell", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(_context.Equipment);  // Equipment should be deleted
        Assert.Empty(_context.LocalizedEquipments);  // Localized entries should be deleted
    }
    [Fact]
    public async Task DeleteAsync_ShouldFail_WhenEquipmentDoesNotExist()
    {
        // Act
        var result = await _service.DeleteAsync("nonexistent", CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Equipment not found.", result.ErrorMessage);
    }
    [Fact]
    public async Task DeleteAsync_ShouldOnlyDeleteSpecifiedLocalization_AndNotTheEquipment_IfOtherLocalizationsExist()
    {
        // Arrange
        var equipment = new Equipment { WeightKg = 20 };
        _context.Equipment.Add(equipment);
        _context.SaveChanges();

        var englishLocalization = new LocalizedEquipment
        {
            EquipmentId = equipment.Id,
            LanguageId = 1, // English
            Name = "dumbbell",
            Description = "For workouts"
        };
        var spanishLocalization = new LocalizedEquipment
        {
            EquipmentId = equipment.Id,
            LanguageId = 2, // Spanish
            Name = "pesa",
            Description = "Para entrenamientos"
        };

        _context.LocalizedEquipments.AddRange(englishLocalization, spanishLocalization);
        _context.SaveChanges();

        // Act
        var result = await _service.DeleteAsync("dumbbell", CancellationToken.None); // Intending to delete only the English description

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains(_context.Equipment, e => e.Id == equipment.Id); // Equipment should still exist
        Assert.DoesNotContain(_context.LocalizedEquipments, le => le.Name == "dumbbell" && le.LanguageId == 1); // English localization should be removed
        Assert.Contains(_context.LocalizedEquipments, le => le.Name == "pesa" && le.LanguageId == 2); // Spanish localization should remain
    }
    [Fact]
    public async Task GetAllByLanguageAsync_ShouldReturnEquipmentForSpecificLanguage()
    {
        DatabaseHelpers.SeedEquipmentAndAssociateThemWithLanguages(_context);

        // Arrange
        var languageCode = "en"; // English

        // Act
        var result = await _service.GetAllByLanguageAsync(languageCode, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Count); // Expect 3 equipment entries for English
        Assert.All(result.Value, erd =>
        {
            Assert.Equal(languageCode, erd.LanguageCode);
        });
    }
    [Fact]
    public async Task GetAllByLanguageAsync_ShouldReturnEmpty_WhenLanguageCodeIsInvalid()
    {
        // Arrange
        var languageCode = "xx"; // Invalid language code

        // Act
        var result = await _service.GetAllByLanguageAsync(languageCode, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value); // Should return an empty list
    }
    [Fact]
    public async Task GetAllByLanguageAsync_ShouldReturnAllEquipment_WhenNoLanguageCodeIsSpecified()
    {
        DatabaseHelpers.SeedEquipmentAndAssociateThemWithLanguages(_context);
        // Arrange
        string languageCode = null; // No language code specified

        // Act
        var result = await _service.GetAllByLanguageAsync(languageCode, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(9, result.Value.Count); // Should return all 9 entries (3 equipment * 3 languages)
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnSingleEquipment_WhenNameExists()
    {
        DatabaseHelpers.SeedEquipmentAndAssociateThemWithLanguages(_context);

        // Arrange
        var name = "dumbbell";  // always in lower case

        // Act
        var result = await _service.GetByNameAsync(name, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(name, result.Value.Name);  
    }
    [Fact]
    public async Task GetByNameAsync_ShouldReturnFailure_WhenEquipmentNameDoesNotExist()
    {
        // Arrange
        var name = "Nonexistent";

        // Act
        var result = await _service.GetByNameAsync(name, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("No equipment found with the specified name.", result.ErrorMessage);
    }

    
}