using Auth.Application.Abstractions;
using Auth.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace Auth.Infrastructure.Security;

public sealed class AspNetPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> passwordHasher = new();

    public string Hash(User user, string password) => passwordHasher.HashPassword(user, password);

    public bool Verify(User user, string password) =>
        passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) is not PasswordVerificationResult.Failed;
}
