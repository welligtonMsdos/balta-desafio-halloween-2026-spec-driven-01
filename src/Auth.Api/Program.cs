using Auth.Api.Errors;
using Auth.Api.Authorization;
using Auth.Application.Abstractions;
using Auth.Application.Contracts;
using Auth.Application.Services;
using Auth.Infrastructure;
using Auth.Infrastructure.Persistence;
using Auth.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwtOptions.Key) || Encoding.UTF8.GetByteCount(jwtOptions.Key) < 32)
{
    throw new InvalidOperationException("A configuração Jwt:Key deve possuir ao menos 32 bytes.");
}

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        options.MapInboundClaims = false;
    });
builder.Services.AddAuthorization();
var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("A connection string Postgres é obrigatória.");
builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.EnableSensitiveDataLogging(false);
    options.EnableDetailedErrors(false);
});
builder.Services.AddScoped<IUserRepository, PostgresUserRepository>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<IPasswordHasher, AspNetPasswordHasher>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.MapPost("/auth/register", async (RegisterUserRequest request, UserService userService, CancellationToken cancellationToken) =>
{
    var user = await userService.RegisterAsync(request, cancellationToken);
    return Results.Created($"/users/{user.Id}", user);
});

app.MapPost("/auth/login", async (LoginRequest request, IAuthService authService, CancellationToken cancellationToken) =>
{
    var token = await authService.LoginAsync(request, cancellationToken);
    return Results.Ok(new { accessToken = token.Value, tokenType = token.TokenType, expiresIn = token.ExpiresIn });
});

app.MapGet("/users/{userId:guid}", async (Guid userId, HttpContext context, UserService userService, CancellationToken cancellationToken) =>
{
    if (!UserOwnership.IsOwner(context.User, userId))
    {
        return Results.Forbid();
    }

    return Results.Ok(await userService.GetByIdAsync(userId, cancellationToken));
}).RequireAuthorization();

app.MapGet("/users", async (int? page, int? pageSize, UserService userService, CancellationToken cancellationToken) =>
{
    var users = await userService.ListAsync(page ?? 1, pageSize ?? 20, cancellationToken);
    return Results.Ok(users);
}).RequireAuthorization();

app.MapPut("/users/{userId:guid}", async (Guid userId, UpdateUserRequest request, HttpContext context, UserService userService, CancellationToken cancellationToken) =>
{
    if (!UserOwnership.IsOwner(context.User, userId))
    {
        return Results.Forbid();
    }

    return Results.Ok(await userService.UpdateAsync(userId, request, cancellationToken));
}).RequireAuthorization();

app.MapDelete("/users/{userId:guid}", async (Guid userId, HttpContext context, UserService userService, CancellationToken cancellationToken) =>
{
    if (!UserOwnership.IsOwner(context.User, userId))
    {
        return Results.Forbid();
    }

    await userService.DeleteAsync(userId, cancellationToken);
    return Results.NoContent();
}).RequireAuthorization();

app.Run();

public partial class Program;
