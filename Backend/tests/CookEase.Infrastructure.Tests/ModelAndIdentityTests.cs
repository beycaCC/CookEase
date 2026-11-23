using CookEase.Application.Users;
using CookEase.Domain.Entities;
using CookEase.Domain.Enums;
using CookEase.Infrastructure.Identity;
using CookEase.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace CookEase.Infrastructure.Tests;

public sealed class ModelAndIdentityTests
{
    private const string ConnectionString =
        "Host=localhost;Database=cookease_model_tests;Username=unused;Password=unused";

    [Fact]
    public void Inventory_allows_fractional_amounts_unknown_dates_and_separate_batches()
    {
        var account = new ApplicationUser();
        var profile = new User(account.Id, " Christine ");
        var first = NewItem(profile.Id, amount: 0.0001m);
        var second = NewItem(profile.Id);

        Assert.NotEqual(Guid.Empty, account.Id);
        Assert.Equal(account.Id, profile.Id);
        Assert.Equal("Christine", profile.DisplayName);
        Assert.Equal("Milk", first.ItemName);
        Assert.Equal(0.0001m, first.Amount);
        Assert.Null(first.ExpirationDate);
        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(TimeSpan.Zero, profile.CreatedAt.Offset);
        Assert.Equal(TimeSpan.Zero, first.AddedAt.Offset);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Inventory_rejects_nonpositive_amounts(decimal amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => NewItem(Guid.NewGuid(), amount: amount));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \t\n")]
    public void Entities_reject_blank_names(string name)
    {
        Assert.Throws<ArgumentException>(() => new User(Guid.NewGuid(), name));
        Assert.Throws<ArgumentException>(() => NewItem(Guid.NewGuid(), name));
    }

    [Fact]
    public void Entities_reject_missing_owners_and_overlong_names()
    {
        Assert.Throws<ArgumentException>(() => new User(Guid.Empty, "Christine"));
        Assert.Throws<ArgumentException>(() => NewItem(Guid.Empty));
        Assert.Throws<ArgumentException>(() => new User(Guid.NewGuid(), new string('x', 201)));
        Assert.Throws<ArgumentException>(() => NewItem(Guid.NewGuid(), new string('x', 201)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(999)]
    public void Inventory_rejects_undefined_enum_values(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FridgeItem(
            Guid.NewGuid(), "Milk", 1, (MeasurementUnit)value, null, StorageLocation.Refrigerator));
        Assert.Throws<ArgumentOutOfRangeException>(() => new FridgeItem(
            Guid.NewGuid(), "Milk", 1, MeasurementUnit.Liter, null, (StorageLocation)value));
    }

    [Fact]
    public void Model_links_accounts_profiles_and_owned_inventory_without_shadow_keys()
    {
        using var context = CreateContext();
        var profile = context.Model.FindEntityType(typeof(User))!;
        var item = context.Model.FindEntityType(typeof(FridgeItem))!;
        var account = context.Model.FindEntityType(typeof(ApplicationUser))!;

        var profileKey = Assert.Single(profile.GetForeignKeys());
        Assert.Equal(account, profileKey.PrincipalEntityType);
        Assert.Equal(nameof(User.Id), Assert.Single(profileKey.Properties).Name);
        Assert.True(profileKey.IsUnique);
        Assert.True(profileKey.IsRequired);
        Assert.Equal(DeleteBehavior.Cascade, profileKey.DeleteBehavior);

        var ownerKey = Assert.Single(item.GetForeignKeys());
        Assert.Equal(profile, ownerKey.PrincipalEntityType);
        Assert.Equal(nameof(FridgeItem.UserId), Assert.Single(ownerKey.Properties).Name);
        Assert.False(ownerKey.IsUnique);
        Assert.True(ownerKey.IsRequired);
        Assert.Equal(DeleteBehavior.Cascade, ownerKey.DeleteBehavior);
        Assert.DoesNotContain(item.GetProperties(), property => property.IsShadowProperty());
        Assert.Contains(item.GetIndexes(), index => !index.IsUnique &&
            index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(FridgeItem.UserId), nameof(FridgeItem.ExpirationDate)]));
        Assert.Contains(account.GetIndexes(), index => index.IsUnique &&
            index.Properties.Single().Name == nameof(ApplicationUser.NormalizedEmail));

        var user = new User(Guid.NewGuid(), "Christine");
        var fridgeItem = NewItem(user.Id);
        context.UserProfiles.Add(user);
        context.FridgeItems.Add(fridgeItem);
        Assert.Same(user, fridgeItem.User);
        Assert.Contains(fridgeItem, user.FridgeItems);
    }

    [Fact]
    public void PostgreSql_schema_preserves_dates_amounts_and_database_constraints()
    {
        using var context = CreateContext();
        var sql = context.Database.GenerateCreateScript();

        Assert.Contains("CREATE TABLE \"AspNetUsers\"", sql);
        Assert.Contains("CREATE TABLE \"UserProfiles\"", sql);
        Assert.Contains("CREATE TABLE \"FridgeItems\"", sql);
        Assert.Contains("\"ExpirationDate\" date", sql);
        Assert.Contains("\"AddedAt\" timestamp with time zone NOT NULL", sql);
        Assert.Contains("\"Amount\" numeric NOT NULL", sql);
        Assert.Contains("CHECK (\"Amount\" > 0)", sql);
        Assert.Contains("CK_FridgeItems_ItemName_NotBlank", sql);
        Assert.Contains("CK_FridgeItems_Unit_Valid", sql);
        Assert.Contains("CK_FridgeItems_StorageLocation_Valid", sql);
        Assert.Contains("CK_UserProfiles_DisplayName_NotBlank", sql);
        Assert.DoesNotContain("CREATE TABLE \"AspNetRoles\"", sql);
    }

    [Fact]
    public async Task Identity_resolves_database_stores_password_hashing_and_authentication_schemes()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddInfrastructure(ConnectionString);
        builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
        await using var app = builder.Build();
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var manager = services.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.True(manager.SupportsUserPassword);
        Assert.True(manager.SupportsUserEmail);
        Assert.True(services.GetRequiredService<IOptions<IdentityOptions>>().Value.User.RequireUniqueEmail);
        Assert.NotNull(services.GetRequiredService<SignInManager<ApplicationUser>>());

        var account = new ApplicationUser();
        var hash = manager.PasswordHasher.HashPassword(account, "Test-Password-Only-123!");
        Assert.NotEqual("Test-Password-Only-123!", hash);
        Assert.Equal(PasswordVerificationResult.Success,
            manager.PasswordHasher.VerifyHashedPassword(account, hash, "Test-Password-Only-123!"));
        Assert.Equal(PasswordVerificationResult.Failed,
            manager.PasswordHasher.VerifyHashedPassword(account, hash, "incorrect"));

        var schemes = services.GetRequiredService<IAuthenticationSchemeProvider>();
        Assert.NotNull(await schemes.GetSchemeAsync(IdentityConstants.BearerScheme));
        Assert.NotNull(await schemes.GetSchemeAsync(IdentityConstants.ApplicationScheme));
    }

    private static FridgeItem NewItem(Guid userId, string name = " Milk ", decimal amount = 1) =>
        new(userId, name, amount, MeasurementUnit.Liter, null, StorageLocation.Refrigerator);

    private static CookEaseDbContext CreateContext() => new(
        new DbContextOptionsBuilder<CookEaseDbContext>().UseNpgsql(ConnectionString).Options,
        new TestCurrentUser(null));
}

internal sealed record TestCurrentUser(Guid? UserId) : ICurrentUser;
