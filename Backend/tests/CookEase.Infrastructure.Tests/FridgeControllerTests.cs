using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using CookEase.Application.Dtos.FridgeItems;
using CookEase.Domain.Entities;
using CookEase.Infrastructure.Identity;
using CookEase.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace CookEase.Infrastructure.Tests;

public sealed class FridgeControllerTests
{
    private const string Route = "/api/fridgeItems";

    [Fact]
    public async Task Owner_can_create_read_update_and_delete_without_changing_ownership_or_creation_date()
    {
        await using var factory = new FridgeApiFactory();
        var ownerId = Guid.NewGuid();
        using var client = factory.CreateUserClient(ownerId);
        var response = await client.PostAsJsonAsync(Route, new
        {
            ItemName = " Milk ", Amount = 1.25m, Unit = 5,
            ExpirationDate = "2026-12-01", StorageLocation = 1,
            UserId = Guid.NewGuid(), Id = Guid.NewGuid()
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = (await response.Content.ReadFromJsonAsync<GetFridgeItemsDto>())!;
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("Milk", created.ItemName);
        Assert.Equal(1.25m, created.Amount);
        Assert.NotEqual(default, created.AddedAt);
        Assert.Equal($"{Route}/{created.Id}", response.Headers.Location!.AbsolutePath);
        Assert.DoesNotContain("userId", await response.Content.ReadAsStringAsync());

        var fetched = await client.GetFromJsonAsync<GetFridgeItemsDto>(response.Headers.Location);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal(created.Id, Assert.Single((await client.GetFromJsonAsync<GetFridgeItemsDto[]>(Route))!).Id);

        var update = await client.PutAsJsonAsync($"{Route}/{created.Id}", new
        {
            ItemName = "Frozen milk", Amount = 0.5m, Unit = 5,
            ExpirationDate = (string?)null, StorageLocation = 2, UserId = Guid.NewGuid()
        });
        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);
        var updated = (await client.GetFromJsonAsync<GetFridgeItemsDto>($"{Route}/{created.Id}"))!;
        Assert.Equal("Frozen milk", updated.ItemName);
        Assert.Equal(0.5m, updated.Amount);
        Assert.Null(updated.ExpirationDate);
        Assert.Equal(created.AddedAt, updated.AddedAt);

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CookEaseDbContext>();
            Assert.Equal(ownerId, (await context.FridgeItems.IgnoreQueryFilters().SingleAsync()).UserId);
        }

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"{Route}/{created.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"{Route}/{created.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"{Route}/{created.Id}")).StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<GetFridgeItemsDto[]>(Route))!);
    }

    [Fact]
    public async Task Other_users_cannot_read_update_or_delete_an_item()
    {
        await using var factory = new FridgeApiFactory();
        using var owner = factory.CreateUserClient(Guid.NewGuid());
        using var other = factory.CreateUserClient(Guid.NewGuid());
        var created = (await (await owner.PostAsJsonAsync(Route, ValidRequest()))
            .Content.ReadFromJsonAsync<GetFridgeItemsDto>())!;

        Assert.Empty((await other.GetFromJsonAsync<GetFridgeItemsDto[]>(Route))!);
        foreach (var id in new[] { created.Id, Guid.NewGuid() })
        {
            Assert.Equal(HttpStatusCode.NotFound, (await other.GetAsync($"{Route}/{id}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound,
                (await other.PutAsJsonAsync($"{Route}/{id}", ValidRequest())).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await other.DeleteAsync($"{Route}/{id}")).StatusCode);
        }

        Assert.Equal(HttpStatusCode.OK, (await owner.GetAsync($"{Route}/{created.Id}")).StatusCode);
    }

    [Theory]
    [InlineData("", 1, 5, 1)]
    [InlineData("   ", 1, 5, 1)]
    [InlineData("Milk", 0, 5, 1)]
    [InlineData("Milk", -1, 5, 1)]
    [InlineData("Milk", 1, 0, 1)]
    [InlineData("Milk", 1, 999, 1)]
    [InlineData("Milk", 1, 5, 0)]
    [InlineData("Milk", 1, 5, 999)]
    public async Task Invalid_create_and_update_requests_return_400_without_changing_inventory(
        string name, int amount, int unit, int storage)
    {
        await using var factory = new FridgeApiFactory();
        using var client = factory.CreateUserClient(Guid.NewGuid());
        var created = (await (await client.PostAsJsonAsync(Route, ValidRequest()))
            .Content.ReadFromJsonAsync<GetFridgeItemsDto>())!;
        var invalid = new { ItemName = name, Amount = amount, Unit = unit, StorageLocation = storage };

        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync(Route, invalid)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await client.PutAsJsonAsync($"{Route}/{created.Id}", invalid)).StatusCode);
        var remaining = Assert.Single((await client.GetFromJsonAsync<GetFridgeItemsDto[]>(Route))!);
        Assert.Equal("Milk", remaining.ItemName);
        Assert.Equal(1, remaining.Amount);
    }

    [Theory]
    [InlineData("GET", false)]
    [InlineData("GET", true)]
    [InlineData("POST", false)]
    [InlineData("PUT", true)]
    [InlineData("DELETE", true)]
    public async Task All_inventory_endpoints_require_authentication(string method, bool withId)
    {
        await using var factory = new FridgeApiFactory();
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(new HttpMethod(method),
            withId ? $"{Route}/{Guid.NewGuid()}" : Route);
        if (method is "POST" or "PUT")
        {
            request.Content = JsonContent.Create(ValidRequest());
        }

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.SendAsync(request)).StatusCode);
    }

    private static object ValidRequest() =>
        new { ItemName = "Milk", Amount = 1, Unit = 5, StorageLocation = 1 };
}

internal sealed class FridgeApiFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:CookEase", "Host=unused;Database=unused;Username=unused");
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<CookEaseDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<CookEaseDbContext>>();
            services.AddDbContext<CookEaseDbContext>(options => options.UseInMemoryDatabase(databaseName));
            services.AddDataProtection().UseEphemeralDataProtectionProvider();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", _ => { });
        });
    }

    public HttpClient CreateUserClient(Guid userId)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-User", userId.ToString());
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CookEaseDbContext>();
        context.Users.Add(new ApplicationUser { Id = userId });
        context.UserProfiles.Add(new User(userId, "Test user"));
        context.SaveChanges();
        return client;
    }
}

internal sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Guid.TryParse(Request.Headers["X-Test-User"], out var userId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())], Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }
}
