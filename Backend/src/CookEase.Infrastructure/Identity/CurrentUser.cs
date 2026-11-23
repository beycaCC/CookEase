using System.Security.Claims;
using CookEase.Application.Users;
using Microsoft.AspNetCore.Http;

namespace CookEase.Infrastructure.Identity;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            return Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
                && userId != Guid.Empty
                    ? userId
                    : null;
        }
    }
}
