using CookEase.Application.Users;
using CookEase.Domain.Entities;
using CookEase.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CookEase.Infrastructure.Persistence;

public sealed class CookEaseDbContext(
    DbContextOptions<CookEaseDbContext> options,
    ICurrentUser currentUser)
    : IdentityUserContext<ApplicationUser, Guid>(options)
{
    private Guid? CurrentUserId => currentUser.UserId;

    // Users is inherited from Identity; UserProfiles contains the domain users.
    public DbSet<User> UserProfiles => Set<User>();
    public DbSet<FridgeItem> FridgeItems => Set<FridgeItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CookEaseDbContext).Assembly);

        // Reference this context's user so EF's cached model works across requests.
        modelBuilder.Entity<FridgeItem>()
            .HasQueryFilter(item => CurrentUserId != null && item.UserId == CurrentUserId);
    }
}
