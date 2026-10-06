using Auth.Application.Contracts;

namespace Auth.Application.Services;

public interface IAuthService
{
    Task<AccessToken> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
