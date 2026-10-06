using Auth.Application.Validation;

namespace Auth.UnitTests.Validation;

public sealed class PasswordPolicyTests
{
    [Fact]
    public void EnsureValidShouldAcceptStrongPassword() =>
        PasswordPolicy.EnsureValid("SenhaForte@2026");

    [Theory]
    [InlineData("curta@1A")]
    [InlineData("senhaforte@2026")]
    [InlineData("SENHAFORTE@2026")]
    [InlineData("SenhaForte2026")]
    public void EnsureValidShouldRejectWeakPassword(string password) =>
        Assert.Throws<ArgumentException>(() => PasswordPolicy.EnsureValid(password));
}
