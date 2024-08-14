using DataLibrary.Context;
using Microsoft.Extensions.Logging;

namespace DataLibrary.Services;

public class UserService
{
    private readonly SqliteContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(SqliteContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    
    
    // user plans 
        // get active plan
        // set active plan
        // (set/adjust) active plan start/end date
        // (finish/start) plan
    // user profile
        // user password
            // create 
            // update
            // change
        // user images (blob or disk ?)
            // CRUD
            // set primary
    // user muscles 
        // cool down calculation
        // frequency 
    // user exercises 
}