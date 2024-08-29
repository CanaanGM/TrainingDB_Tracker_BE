using API.Controllers;
using DataLibrary.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SharedLibrary.Dtos;
using TrainingTests.helpers;

namespace TrainingTests.APIControllersIntegrationTests;

public class MeasurementsControllerIntegrationTests : ControllerBaseIntegrationTestClass
{
    private readonly MeasurementsController _controller;
    private readonly IMeasurementsService _service;
    private readonly Mock<ILogger<MeasurementsService>> _loggerMock;
    
    public MeasurementsControllerIntegrationTests()
    {
        _loggerMock = new Mock<ILogger<MeasurementsService>>();
        _service = new MeasurementsService(_context, _mapper, _loggerMock.Object);

        services.AddSingleton<IMeasurementsService>();
        _controller = new MeasurementsController(_service)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = serviceProvider }
            }
        };
    }

    [Fact]
    public async Task GetMeasurementsAsync_NotEmpty_Should_Return_NotEmptyList()
    {
        ProductionDatabaseHelpers.SeedDummyUsers(_context);
        ProductionDatabaseHelpers.SeedMeasurements(_context);

        var result = await _controller.GetMeasurementsAsync(1, new CancellationToken());

        var OkResult = Assert.IsType<OkObjectResult>(result);
        var measurements = Assert.IsAssignableFrom<List<MeasurementsReadDto>>(OkResult.Value);
        Assert.NotEmpty(measurements);
    }
    [Fact]
    public async Task GetMeasurementsAsync_Empty_Should_Return_EmptyList()
    {
        ProductionDatabaseHelpers.SeedDummyUsers(_context);

        var result = await _controller.GetMeasurementsAsync(1, new CancellationToken());

        var OkResult = Assert.IsType<OkObjectResult>(result);
        var measurements = Assert.IsAssignableFrom<List<MeasurementsReadDto>>(OkResult.Value);
        Assert.Empty(measurements);
    }

    [Fact]
    public async Task UpdateAsync_Success()
    {
        ProductionDatabaseHelpers.SeedDummyUsers(_context);
        ProductionDatabaseHelpers.SeedMeasurements(_context);

        var newMeasurements = MeasurementsDtoFactory.CreateOneWithBMIAndWeight();

        var result = await _controller.UpdateMeasurementsAsync(1, 1, newMeasurements, new CancellationToken());

        var okResult = Assert.IsType<NoContentResult>(result);


    }
    
    [Fact]
    public async Task CreateMeasurementsAsync_Should_CreateMeasurements_When_DataIsValid()
    {
        ProductionDatabaseHelpers.SeedDummyUsers(_context);
        var newMeasurement = MeasurementsDtoFactory.CreateOneWithBMIAndWeight();

        var result = await _controller.CreateMeasurementsAsync(1, newMeasurement, new CancellationToken());

        var createdAtResult = Assert.IsType<CreatedAtRouteResult>(result);
        Assert.NotNull(createdAtResult);
        var createdMeasurementId =  createdAtResult.Value;
        Assert.NotNull(createdMeasurementId);

        var createdMeasurement = await _context.Measurements
            .FindAsync(1);
        Assert.NotNull(createdMeasurement);
    }

    [Fact]
    public async Task UpdateMeasurementsAsync_Should_UpdateMeasurement_When_DataIsValid()
    {
        ProductionDatabaseHelpers.SeedDummyUsers(_context);
        ProductionDatabaseHelpers.SeedMeasurements(_context);
        var updateMeasurement = MeasurementsDtoFactory.CreateOneWithBMIAndWeight();

        var result = await _controller.UpdateMeasurementsAsync(1, 1, updateMeasurement, new CancellationToken());

        Assert.IsType<NoContentResult>(result);

        var updatedMeasurement = await _context.Measurements.FindAsync(1);
        Assert.NotNull(updatedMeasurement);
        Assert.Equal(
            updateMeasurement.Minerals + updateMeasurement.Protein
            + updateMeasurement.TotalBodyWater + updateMeasurement.BodyFatMass
            , updatedMeasurement.BodyWeight);
    }

    [Fact]
    public async Task DeleteMeasurementsAsync_Should_DeleteMeasurement_When_Exists()
    {
        ProductionDatabaseHelpers.SeedDummyUsers(_context);
        ProductionDatabaseHelpers.SeedMeasurements(_context);

        var result = await _controller.DeleteMeasurementsAsync(1, 1, new CancellationToken());

        Assert.IsType<NoContentResult>(result);

        var deletedMeasurement = await _context.Measurements.FindAsync(1);
        Assert.Null(deletedMeasurement);
    }
}