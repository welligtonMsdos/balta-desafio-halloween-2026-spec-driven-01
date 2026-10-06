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

    public async Task<UserResponse> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        return ToResponse(user);
    }

    public async Task<PageResult<UserResponse>> ListAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        if (page < 1 || pageSize is < 1 or > 100)
        {
            throw new ArgumentException("Os parâmetros de paginação são inválidos.");
        }

        var users = await userRepository.ListAsync(page, pageSize, cancellationToken);
        return new PageResult<UserResponse>(users.Items.Select(ToResponse).ToArray(), users.Page, users.PageSize, users.TotalCount);
    }

    public async Task<UserResponse> UpdateAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Informe e-mail ou senha para atualização.");
        }

        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var email = EmailAddress.Create(request.Email);
            var existingUser = await userRepository.GetByNormalizedEmailAsync(email.NormalizedValue, cancellationToken);
            if (existingUser is not null && existingUser.Id != user.Id)
            {
                throw new ConflictException("O e-mail já está em uso.");
            }

            user.ChangeEmail(email.Value, email.NormalizedValue);
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            PasswordPolicy.EnsureValid(request.Password);
            user.ChangePassword(passwordHasher.Hash(user, request.Password));
        }

        await userRepository.UpdateAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);
        return ToResponse(user);
    }

    private static UserResponse ToResponse(User user) => new(user.Id, user.Email, user.CreatedAtUtc);
}
