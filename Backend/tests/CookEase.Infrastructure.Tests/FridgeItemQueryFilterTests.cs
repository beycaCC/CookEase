using System.Security.Claims;
using CookEase.Application.Users;
using CookEase.Domain.Entities;
using CookEase.Domain.Enums;
using CookEase.Infrastructure.Identity;
using CookEase.Infrastructure.Persistence;
using CookEase.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CookEase.Infrastructure.Tests;

public sealed class FridgeItemQueryFilterTests
{
    [Fact]
    public async Task Repositories_isolate_users_and_return_nothing_without_a_user()
    {
        var firstUser = new User(Guid.NewGuid(), "First user");
        var secondUser = new User(Guid.NewGuid(), "Second user");
        var firstItem = NewItem(firstUser.Id);
        var secondItem = NewItem(secondUser.Id);
        var options = new DbContextOptionsBuilder<CookEaseDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        await using (var seed = new CookEaseDbContext(options, new TestCurrentUser(null)))
        {
            seed.UserProfiles.AddRange(firstUser, secondUser);
            seed.FridgeItems.AddRange(firstItem, secondItem);
            await seed.SaveChangesAsync();
        }

        // Each request gets a fresh context, while EF reuses the same model.
        foreach (var (user, ownItem, otherItem) in new[]
        {
            (firstUser, firstItem, secondItem),
            (secondUser, secondItem, firstItem)
        })
        {
            await using var context = new CookEaseDbContext(options, new TestCurrentUser(user.Id));
            var repository = new FridgeRepository(context);

            Assert.Equal(ownItem.Id, Assert.Single(await repository.GetAllFridgesAsync()).Id);
            Assert.Null(await repository.GetFridgeByIdAsync(otherItem.Id));
            Assert.NotNull(await repository.GetFridgeByIdAsync(ownItem.Id));
            Assert.Equal(1, await context.FridgeItems.CountAsync());
            Assert.Equal(user.Id, Assert.Single(await context.FridgeItems.Include(item => item.User)
                .ToListAsync()).User.Id);
        }

        await using var anonymous = new CookEaseDbContext(options, new TestCurrentUser(null));
        var anonymousRepository = new FridgeRepository(anonymous);
        Assert.Empty(await anonymousRepository.GetAllFridgesAsync());
        Assert.Null(await anonymousRepository.GetFridgeByIdAsync(firstItem.Id));
    }

    [Fact]
    public void PostgreSql_queries_use_each_contexts_user_in_the_database_predicate()
    {
        var options = new DbContextOptionsBuilder<CookEaseDbContext>()
            .UseNpgsql("Host=localhost;Database=unused;Username=unused;Password=unused").Options;
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        using var first = new CookEaseDbContext(options, new TestCurrentUser(firstId));
        using var second = new CookEaseDbContext(options, new TestCurrentUser(secondId));

        var firstSql = first.FridgeItems.ToQueryString();
        var secondSql = second.FridgeItems.ToQueryString();
        Assert.Contains("WHERE", firstSql);
        Assert.Contains("\"UserId\" =", firstSql);
        Assert.Contains(firstId.ToString(), firstSql);
        Assert.Contains(secondId.ToString(), secondSql);
        Assert.DoesNotContain(firstId.ToString(), secondSql);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("invalid-id", true)]
    [InlineData("00000000-0000-0000-0000-000000000000", true)]
    [InlineData("d01e3fdc-ab07-4653-a1c2-8a2868382d13", false)]
    public void Untrusted_or_invalid_identity_has_no_current_user(string? id, bool authenticated)
    {
        var claims = id is null ? Array.Empty<Claim>() : [new Claim(ClaimTypes.NameIdentifier, id)];
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticated ? "Test" : null))
            }
        };

        Assert.Null(new CurrentUser(accessor).UserId);
        accessor.HttpContext = null;
        Assert.Null(new CurrentUser(accessor).UserId);
    }

    [Fact]
    public async Task Infrastructure_resolves_current_user_from_authenticated_claims()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddInfrastructure("Host=localhost;Database=unused;Username=unused;Password=unused");
        await using var app = builder.Build();
        using var scope = app.Services.CreateScope();
        var userId = Guid.NewGuid();
        var accessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        try
        {
            accessor.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "Test"))
            };

            Assert.Equal(userId, scope.ServiceProvider.GetRequiredService<ICurrentUser>().UserId);
            var context = scope.ServiceProvider.GetRequiredService<CookEaseDbContext>();
            Assert.Contains(userId.ToString(), context.FridgeItems.ToQueryString());
            Assert.Empty(context.Model.FindEntityType(typeof(ApplicationUser))!.GetDeclaredQueryFilters());
        }
        finally
        {
            accessor.HttpContext = null;
        }
    }

    private static FridgeItem NewItem(Guid userId) =>
        new(userId, "Milk", 1, MeasurementUnit.Liter, null, StorageLocation.Refrigerator);
}
