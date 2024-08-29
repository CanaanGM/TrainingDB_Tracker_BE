using AutoMapper;
using DataLibrary.Context;
using DataLibrary.Core;
using TestSupport.EfHelpers;

public class BaseTestClass : IDisposable
{
    protected DbContextOptionsDisposable<SqliteContext> _options;
    protected SqliteContext _context;
    protected Profiles _myProfile;
    protected MapperConfiguration _configuration;
    protected Mapper _mapper;

    protected BaseTestClass()
    {
        _options = SqliteInMemory.CreateOptions<SqliteContext>();
        _context = new SqliteContext(_options);
        _context.Database.EnsureCreated();
        _myProfile = new Profiles();
        _configuration = new MapperConfiguration(cfg => cfg.AddProfile(_myProfile));
        _mapper = new Mapper(_configuration);
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

    ~BaseTestClass()
    {
        Dispose(false);
    }
}