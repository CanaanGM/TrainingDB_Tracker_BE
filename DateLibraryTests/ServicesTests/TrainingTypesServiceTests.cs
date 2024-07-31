using AutoMapper;
using DataLibrary.Context;
using DataLibrary.Core;
using DataLibrary.Dtos;
using DataLibrary.Helpers;
using DataLibrary.Services;
using DateLibraryTests.helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TestSupport.EfHelpers;

namespace DateLibraryTests.ServicesTests;

public class TrainingTypesServiceTests : BaseTestClass
{
    private Mock<ILogger<TrainingTypesService>> logger;
    private readonly TrainingTypesService _service;

    public TrainingTypesServiceTests()
    {
        logger = new Mock<ILogger<TrainingTypesService>>();
        _service = new TrainingTypesService(_context, _mapper, logger.Object);
        DatabaseHelpers.SeedLanguages(_context);
    }

    [Theory]
    [InlineData("Cardio")]
    public async Task Creating_English_Returns_Success(string @string)
    {
        var newTypeDto = new TrainingTypeUpdateDto
        {
            Name = @string,
            LanguageCode = "en"
        };

        var creationResult = await _service.UpsertAsync(newTypeDto, new CancellationToken());

        Assert.True(creationResult.Value >= 1); // a new ID has been assigned
        var newType = _context.TrainingTypes
            .Include(x => x.Language)
            .FirstOrDefault(x => x.Id == creationResult.Value);
        Assert.NotNull(newType);
        Assert.True(newType.Name == Utils.NormalizeString(@string)); // normalizing works
        Assert.True(newType.Language.Code == "en"); 
    }

    [Theory]
    [InlineData("استحمال")]
    public async Task Creating_Arabic_Returns_Success(string @string)
    {
        var newTypeDto = new TrainingTypeUpdateDto
        {
            Name = @string,
            LanguageCode = "ar"
        };

        var creationResult = await _service.UpsertAsync(newTypeDto, new CancellationToken());

        Assert.True(creationResult.Value >= 1); // a new ID has been assigned
        var newType = _context.TrainingTypes
            .Include(x => x.Language)
            .FirstOrDefault(x => x.Id == creationResult.Value);
        Assert.NotNull(newType);
        Assert.True(newType.Name == Utils.NormalizeString(@string)); // normalizing works
        Assert.True(newType.Language.Code == "ar"); 
    }
    [Fact]
    public async Task GettingAll_no_types_returns_Success()
    {
        var result = await _service.GetAllAsync(new CancellationToken());

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetAll_returns_success()
    {
        _context.TrainingTypes.Add(new DataLibrary.Models.TrainingType { Name = "cardio", LanguageId = 1 });
        _context.TrainingTypes.Add(new DataLibrary.Models.TrainingType { Name = "strength",LanguageId = 1});
        _context.SaveChanges();

        var result = await _service.GetAllAsync(new CancellationToken());

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);
        Assert.Equal(2, result.Value.Count());
        Assert.Equal("cardio", result.Value[0].Name);
        Assert.Equal("strength", result.Value[1].Name);
    }

    [Fact]
    public async Task Update_record_returns_success()
    {
        _context.TrainingTypes.Add(new DataLibrary.Models.TrainingType { Name = "cardio", LanguageId = 1});
        _context.SaveChanges();

        var result = await _service.UpsertAsync(
            new TrainingTypeUpdateDto { Name = "strength", LanguageCode = "en"}, 
            new CancellationToken()
            );
        
        Assert.True(result.IsSuccess);
        var updatedType = _context.TrainingTypes
            .Include(x => x.Language)
            .FirstOrDefault(x => x.Name == "strength");
        Assert.NotNull(updatedType);
        Assert.NotNull(updatedType.Language);
        Assert.Equal("en", updatedType.Language.Code);
    }

    [Fact]
    public async Task Update_Language_record_returns_success()
    {
        _context.TrainingTypes.Add(new DataLibrary.Models.TrainingType { Name = "cardio", LanguageId = 1});
        _context.SaveChanges();

        var result = await _service.UpsertAsync(
            new TrainingTypeUpdateDto { Name = "cardio", LanguageCode = "ar", NewName = "استحمال"}, 
            new CancellationToken()
        );
        
        Assert.True(result.IsSuccess);
        var updatedType = _context.TrainingTypes
            .Include(x => x.Language)
            .FirstOrDefault(x => x.Name == "استحمال");
        Assert.NotNull(updatedType);
        Assert.Equal("استحمال", updatedType.Name);
        Assert.NotNull(updatedType.Language);
        Assert.Equal("ar", updatedType.Language.Code);
    }
    
    [Fact]
    public async Task Delete_type_returns_success()
    {
        _context.TrainingTypes.Add(new DataLibrary.Models.TrainingType { Name = "cardio", LanguageId = 1});
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        var result = await _service.DeleteAsync(1, new CancellationToken());

        Assert.True(result.IsSuccess);
        Assert.Null(_context.TrainingTypes.FirstOrDefault(x => x.Name == "cardio"));
    }


    [Fact]
    public async Task CreateBulk_returns_Success()
    {
        var listOfTypes = new List<TrainingTypeWriteDto>
        {
            new TrainingTypeWriteDto { Name = "CaRdIo", LanguageCode = "En"},
            new TrainingTypeWriteDto { Name = "STRENGTH", LanguageCode = "eN"},
            new TrainingTypeWriteDto { Name = "BodyBuilding", LanguageCode = "En"},
            new TrainingTypeWriteDto { Name = "استحمال", LanguageCode = "AR"},
            new TrainingTypeWriteDto { Name = "Martial Arts", LanguageCode = "En"},
            new TrainingTypeWriteDto { Name = "Yoga", LanguageCode = "En"},
        };

        var result = await _service.CreateBulkAsync(listOfTypes, new CancellationToken());


        Assert.True(result.IsSuccess);
        var createdTypes = _context.TrainingTypes.ToList();
        Assert.NotNull(createdTypes);
        Assert.Equal(6, createdTypes.Count());
        Assert.Equal("cardio", createdTypes[0].Name);
        Assert.Equal("strength", createdTypes[1].Name);
        Assert.Equal("استحمال", createdTypes[3].Name);
        Assert.Equal("martial arts", createdTypes[4].Name);
        Assert.Equal("en", createdTypes[0].Language.Code);
        Assert.Equal("en", createdTypes[1].Language.Code);
        Assert.Equal("ar", createdTypes[3].Language.Code);
    }
}