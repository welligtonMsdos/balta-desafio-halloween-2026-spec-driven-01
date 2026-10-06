using Auth.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence;

public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<User>();
        user.ToTable("users");
        user.HasKey(entity => entity.Id);
        user.Property(entity => entity.Id).HasColumnName("id");
        user.Property(entity => entity.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
        user.Property(entity => entity.NormalizedEmail).HasColumnName("normalized_email").HasMaxLength(320).IsRequired();
        user.Property(entity => entity.PasswordHash).HasColumnName("password_hash").IsRequired();
        user.Property(entity => entity.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        user.HasIndex(entity => entity.NormalizedEmail).HasDatabaseName("ux_users_normalized_email").IsUnique();
    }
}
