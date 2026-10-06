using Auth.Application.Contracts;

namespace Auth.Application.Services;

public interface IUserService
{
    Task<UserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken);
    Task<UserResponse> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<PageResult<UserResponse>> ListAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<UserResponse> UpdateAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid userId, CancellationToken cancellationToken);
}
