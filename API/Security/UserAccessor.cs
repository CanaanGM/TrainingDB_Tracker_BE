
using System.Security.Claims;

namespace API.Security;


public interface IUserAccessor
{
    int GetUserId();
    string GetUsername();
}
public class UserAccessor : IUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }


    public string GetUsername()
    {
        return _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
    }

    public int GetUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId))
        {
            return 0; // Or you can return a 403 Forbidden status code
        }
        return currentUserId;
    }
}