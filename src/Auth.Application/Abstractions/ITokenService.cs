using Auth.Application.Contracts;

namespace Auth.Application.Abstractions;

public interface ITokenService
{
    AccessToken Create(Guid userId, string email);
}
