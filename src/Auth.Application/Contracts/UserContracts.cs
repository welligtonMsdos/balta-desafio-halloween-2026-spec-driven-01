namespace Auth.Application.Contracts;

public sealed record RegisterUserRequest(string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record UpdateUserRequest(string? Email, string? Password);
public sealed record UserResponse(Guid Id, string Email, DateTimeOffset CreatedAtUtc);
public sealed record AccessToken(string Value, string TokenType, int ExpiresIn);
public sealed record PageResult<T>(IReadOnlyCollection<T> Items, int Page, int PageSize, int TotalCount);
