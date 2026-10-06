namespace Auth.Domain.Users;

public sealed class User
{
    private User(Guid id, string email, string normalizedEmail, string passwordHash, DateTimeOffset createdAtUtc)
    {
        Id = id;
        Email = email;
        NormalizedEmail = normalizedEmail;
        PasswordHash = passwordHash;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; }
    public string Email { get; private set; }
    public string NormalizedEmail { get; private set; }
    public string PasswordHash { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }

    public static User Create(Guid id, string email, string normalizedEmail, string passwordHash, DateTimeOffset createdAtUtc) =>
        new(id, email, normalizedEmail, passwordHash, createdAtUtc);

    public void ChangeEmail(string email, string normalizedEmail)
    {
        Email = email;
        NormalizedEmail = normalizedEmail;
    }

    public void ChangePassword(string passwordHash) => PasswordHash = passwordHash;
}
