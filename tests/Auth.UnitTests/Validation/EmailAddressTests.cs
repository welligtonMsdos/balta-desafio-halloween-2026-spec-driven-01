using Auth.Application.Validation;

namespace Auth.UnitTests.Validation;

public sealed class EmailAddressTests
{
    [Fact]
    public void CreateShouldTrimAndNormalizeEmail()
    {
        var email = EmailAddress.Create("  USER@Example.COM  ");

        Assert.Equal("USER@Example.COM", email.Value);
        Assert.Equal("user@example.com", email.NormalizedValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    public void CreateShouldRejectInvalidEmail(string value) =>
        Assert.Throws<ArgumentException>(() => EmailAddress.Create(value));
}
