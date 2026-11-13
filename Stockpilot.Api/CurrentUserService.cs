using Stockpilot.Application.Contracts;
using System.Security.Claims;

namespace Stockpilot.Api;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userId = _httpContextAccessor?.HttpContext?.User?.FindFirst("Id")?.Value;

            if (!Guid.TryParse(userId, out var parsedUserId))
            {
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");
            }

            return parsedUserId;
        }
    }

    public string FullName
    {
        get
        {
            return _httpContextAccessor.HttpContext?
                .User.FindFirstValue("FullName")
                ?? string.Empty;
        }
    }

}