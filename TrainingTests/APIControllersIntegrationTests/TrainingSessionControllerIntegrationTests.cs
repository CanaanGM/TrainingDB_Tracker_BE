using API.Controllers;
using API.Security;
using DataLibrary.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SharedLibrary.Core;
using SharedLibrary.Dtos;
using TrainingTests.helpers;

namespace TrainingTests.APIControllersIntegrationTests;

public class TrainingSessionControllerIntegrationTests : ControllerBaseIntegrationTestClass
{
	private readonly TrainingSessionController _controller;
	private readonly ITrainingSessionService _trainingSessionService;
	private readonly Mock<ILogger<TrainingSessionService>> _trainingSessionServiceLoggerMock;
	private Mock<IUserAccessor> _userAccessorMock;

	public TrainingSessionControllerIntegrationTests()
	{
		_trainingSessionServiceLoggerMock = new Mock<ILogger<TrainingSessionService>>();
		_userAccessorMock = new Mock<IUserAccessor>();
		_trainingSessionService =
			new TrainingSessionService(_context, _mapper, _trainingSessionServiceLoggerMock.Object);


		services.AddSingleton<ITrainingSessionService>(_trainingSessionService);
		_userAccessorMock.Setup(x => x.GetUserId()).Returns(Result<int>.Success(1));
		_controller = new TrainingSessionController(_trainingSessionService, _userAccessorMock.Object)
		{
			ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext { RequestServices = serviceProvider }
			}
		};
	}

	[Fact]
	public async Task GetTrainingSessionsAsync_ShouldReturnEmpty_WhenDateRangeHasNoSessions()
	{
		ProductionDatabaseHelpers.SeedProductionData(_context);
		ProductionDatabaseHelpers.SeedDummyUsers(_context);

		var startDate = "2-11-2023";
		var endDate = "2-12-2023";

		var result = await _controller.GetTrainingSessionsAsync(startDate, endDate);

		var okResult = Assert.IsType<OkObjectResult>(result);
		var sessions = Assert.IsAssignableFrom<PaginatedList<TrainingSessionReadDto>>(okResult.Value);

		Assert.NotNull(sessions);
		Assert.Equal(1, sessions.Metadata.CurrentPage);
		Assert.Equal(0, sessions.Metadata.TotalPages);
		Assert.Equal(10, sessions.Metadata.PageSize);
		Assert.Equal(0, sessions.Metadata.TotalCount);
		Assert.Empty(sessions.Items);
	}

	[Fact]
	public async Task GetTrainingSessionsAsync_ShouldReturnSessions_WhenUserHasSessions()
	{
		ProductionDatabaseHelpers.SeedProductionData(_context);
		ProductionDatabaseHelpers.SeedDummyUsers(_context);
		var newSession = TrainingSessionDtoFactory.CreateLegsSessionDto();
		await _trainingSessionService.CreateSessionAsync(1, newSession, new CancellationToken());

		var result = await _controller.GetTrainingSessionsAsync(null, null);

		var okResult = Assert.IsType<OkObjectResult>(result);
		var sessions = Assert.IsAssignableFrom<PaginatedList<TrainingSessionReadDto>>(okResult.Value);

		Assert.NotNull(sessions);
		Assert.Equal(1, sessions.Metadata.CurrentPage);
		Assert.Equal(1, sessions.Metadata.TotalPages);
		Assert.Equal(10, sessions.Metadata.PageSize);
		Assert.Equal(1, sessions.Metadata.TotalCount);

		var session = sessions.Items.FirstOrDefault();
		Assert.NotNull(session);
		Assert.Equal(newSession.Notes, session.Notes);
		Assert.Equal(newSession.Mood, session.Mood);
	}

	[Fact]
	public async Task CreateTrainingSessionAsync_ShouldCreateSession_WhenDataIsValid()
	{
		ProductionDatabaseHelpers.SeedProductionData(_context);
		ProductionDatabaseHelpers.SeedDummyUsers(_context);
		var newSession = TrainingSessionDtoFactory.CreateCorrectSessionDtoMixedCardio();

		var result = await _controller.CreateTrainingSessionAsync(1, newSession, new CancellationToken());

		var createdResult = Assert.IsType<CreatedResult>(result);
		Assert.NotNull(createdResult);

		var createdSessions = await _context.TrainingSessions.Where(ts => ts.UserId == 1).ToListAsync();
		Assert.Single(createdSessions);
		Assert.Equal(newSession.Notes, createdSessions[0].Notes);
	}

	[Fact]
	public async Task UpdateTrainingSessionAsync_ShouldUpdateSession_WhenDataIsValid()
	{
		ProductionDatabaseHelpers.SeedProductionData(_context);
		ProductionDatabaseHelpers.SeedDummyUsers(_context);
		var newSession = TrainingSessionDtoFactory.CreateLegsSessionDto();
		await _trainingSessionService.CreateSessionAsync(1, newSession, new CancellationToken());
		var updatedSession = TrainingSessionDtoFactory.CreateUpdateDto();

		var result = await _controller.UpdateTrainingSessionAsync(1, 1, updatedSession, new CancellationToken());

		Assert.IsType<NoContentResult>(result);

		var updatedSessionFromDb = await _context.TrainingSessions.FirstOrDefaultAsync(ts => ts.Id == 1);
		Assert.NotNull(updatedSessionFromDb);
		Assert.Equal(updatedSession.Notes, updatedSessionFromDb.Notes);
		Assert.Equal(updatedSession.Mood, updatedSessionFromDb.Mood);
	}

	[Fact]
	public async Task DeleteTrainingSessionAsync_ShouldDeleteSession_WhenExists()
	{
		ProductionDatabaseHelpers.SeedProductionData(_context);
		ProductionDatabaseHelpers.SeedDummyUsers(_context);
		var newSession = TrainingSessionDtoFactory.CreateLegsSessionDto();
		await _trainingSessionService.CreateSessionAsync(1, newSession, new CancellationToken());

		var result = await _controller.DeleteTrainingSessionAsync(1, 1, new CancellationToken());

		Assert.IsType<NoContentResult>(result);

		var deletedSession = await _context.TrainingSessions.FirstOrDefaultAsync(ts => ts.Id == 1);
		Assert.Null(deletedSession);
	}

	[Fact]
	public async Task CreateTrainingSessionBulkAsync_ShouldCreateMultipleSessions_WhenDataIsValid()
	{
		ProductionDatabaseHelpers.SeedProductionData(_context);
		ProductionDatabaseHelpers.SeedDummyUsers(_context);
		var newSessions = new List<TrainingSessionWriteDto>
		{
			TrainingSessionDtoFactory.CreateLegsSessionDto(),
			TrainingSessionDtoFactory.CreateCorrectSessionDtoMixedCardio(),
			TrainingSessionDtoFactory.CreateUpdateDto()
		};

		var result = await _controller.CreateTrainingSessionBulkAsync(1, newSessions, new CancellationToken());

		var createdResult = Assert.IsType<CreatedResult>(result);
		Assert.NotNull(createdResult);

		var createdSessions = await _context.TrainingSessions.Where(ts => ts.UserId == 1).ToListAsync();
		Assert.Equal(newSessions.Count, createdSessions.Count);
	}
}