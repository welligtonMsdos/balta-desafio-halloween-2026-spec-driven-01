using Auth.Application.Abstractions;
using Auth.Application.Contracts;
using Auth.Application.Exceptions;
using Auth.Application.Validation;
using Auth.Domain.Users;

namespace Auth.Application.Services;

public sealed class UserService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IClock clock)
{
    public async Task<UserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var email = EmailAddress.Create(request.Email);
        PasswordPolicy.EnsureValid(request.Password);

        if (await userRepository.GetByNormalizedEmailAsync(email.NormalizedValue, cancellationToken) is not null)
        {
            throw new ConflictException("O e-mail já está em uso.");
        }

        var user = User.Create(Guid.NewGuid(), email.Value, email.NormalizedValue, string.Empty, clock.UtcNow);
        user.ChangePassword(passwordHasher.Hash(user, request.Password));

        await userRepository.AddAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);

        return ToResponse(user);
    }

    private static UserResponse ToResponse(User user) => new(user.Id, user.Email, user.CreatedAtUtc);
}
