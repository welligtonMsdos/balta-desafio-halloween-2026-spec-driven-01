using System.Security.Claims;

namespace Auth.Api.Authorization;

public static class UserOwnership
{
    public static bool IsOwner(ClaimsPrincipal principal, Guid userId) =>
        Guid.TryParse(principal.FindFirstValue("sub"), out var subjectId) && subjectId == userId;
}
