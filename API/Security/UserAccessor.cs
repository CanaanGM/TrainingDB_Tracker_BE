
using System.Security.Claims;
using SharedLibrary.Core;

namespace API.Security;


public interface IUserAccessor
{
    Result<int> GetUserId();
    Result<string> GetUsername();
}
public class UserAccessor : IUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }


    public Result<string> GetUsername()
    {
        var userName = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
        return userName is null
            ? Result<string>.Failure("user is not authenticated.")
            : Result<string>.Success(userName);
    }

    public Result<int> GetUserId()
    {
        var userIdClaim = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId))
            return Result<int>.Failure("user is not authenticated."); // Or you can return a 403 Forbidden status code
        
        return Result<int>.Success(currentUserId);
    }
}