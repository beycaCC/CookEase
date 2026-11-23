using Microsoft.Extensions.DependencyInjection;
using CookEase.Domain.Interface;
using CookEase.Infrastructure.Repositories;

namespace CookEase.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IFridgeRepository, FridgeRepository>();
        return services;
    }
}