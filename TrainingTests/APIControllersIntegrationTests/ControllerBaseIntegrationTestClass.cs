using API.Security;
using AutoMapper;
using DataLibrary.Context;
using DataLibrary.Core;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TestSupport.EfHelpers;

namespace TrainingTests.APIControllersIntegrationTests;

public class ControllerBaseIntegrationTestClass : IDisposable
{
    protected readonly DbContextOptionsDisposable<SqliteContext> _options;
    protected readonly SqliteContext _context;
    protected readonly Profiles _myProfile;
    protected readonly MapperConfiguration _configuration;
    protected readonly Mapper _mapper;
    protected readonly Mock<IUserAccessor> _userAccessorMock;
    protected readonly ServiceProvider serviceProvider;
    protected readonly ServiceCollection services;

    protected ControllerBaseIntegrationTestClass()
    {
        _options = SqliteInMemory.CreateOptions<SqliteContext>();
        _context = new SqliteContext(_options);
        _context.Database.EnsureCreated();
        _myProfile = new Profiles();
        _configuration = new MapperConfiguration(cfg => cfg.AddProfile(_myProfile));
        _mapper = new Mapper(_configuration);
        _userAccessorMock = new Mock<IUserAccessor>();
        services = new ServiceCollection();
        services.AddSingleton<IUserAccessor>(_userAccessorMock.Object);
        serviceProvider = services.BuildServiceProvider();

    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _context?.Dispose();
        }
    }

    ~ControllerBaseIntegrationTestClass()
    {
        Dispose(false);
    }
}