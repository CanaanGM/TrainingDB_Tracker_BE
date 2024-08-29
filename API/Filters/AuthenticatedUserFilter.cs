using API.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters;

public class AuthenticatedUserFilter : IActionFilter
{
    private readonly IUserAccessor _userAccessor;
    private readonly ILogger<AuthenticatedUserFilter> _logger;

    public AuthenticatedUserFilter(IUserAccessor userAccessor, ILogger<AuthenticatedUserFilter> logger)
    {
        _userAccessor = userAccessor;
        _logger = logger;
    }
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var userResult = _userAccessor.GetUserId();
        if (!userResult.IsSuccess)
        {
            _logger.LogWarning("User is not authenticated.");
            context.Result = new UnauthorizedResult(); 
        }
        else
        {
            // Add the user ID to the action arguments if needed
            context.ActionArguments["userId"] = userResult.Value;
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // no need
    }
}