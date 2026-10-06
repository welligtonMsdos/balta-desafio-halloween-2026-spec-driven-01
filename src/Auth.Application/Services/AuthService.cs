using Auth.Application.Abstractions;
using Auth.Application.Contracts;
using Auth.Application.Exceptions;
using Auth.Application.Validation;

namespace Auth.Application.Services;

public sealed class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : IAuthService
{
    public async Task<AccessToken> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = EmailAddress.Create(request.Email);
        var user = await userRepository.GetByNormalizedEmailAsync(email.NormalizedValue, cancellationToken);

        if (user is null || !passwordHasher.Verify(user, request.Password))
        {
            throw new InvalidCredentialsException();
        }

        return tokenService.Create(user.Id, user.Email);
    }
}
