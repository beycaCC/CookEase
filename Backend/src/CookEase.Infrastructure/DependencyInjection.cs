using CookEase.Application.Users;
using CookEase.Infrastructure.Identity;
using CookEase.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CookEase.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddDbContext<CookEaseDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddIdentityApiEndpoints<ApplicationUser>(options =>
            options.User.RequireUniqueEmail = true)
            .AddEntityFrameworkStores<CookEaseDbContext>();
        services.AddAuthorization();

        services.AddHealthChecks()
            .AddDbContextCheck<CookEaseDbContext>("postgresql");

        return services;
    }
}
