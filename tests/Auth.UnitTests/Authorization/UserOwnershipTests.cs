using System.Security.Claims;
using Auth.Api.Authorization;

namespace Auth.UnitTests.Authorization;

public sealed class UserOwnershipTests
{
    [Fact]
    public void IsOwnerShouldReturnTrueForMatchingSubject()
    {
        var userId = Guid.NewGuid();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", userId.ToString())]));

        Assert.True(UserOwnership.IsOwner(principal, userId));
    }

    [Fact]
    public void IsOwnerShouldReturnFalseForDifferentSubject()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", Guid.NewGuid().ToString())]));

        Assert.False(UserOwnership.IsOwner(principal, Guid.NewGuid()));
    }
}
