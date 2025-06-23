using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Natix.Weather.Infrastructure.Repositories;
using Natix.Weather.Infrastructure.Providers;
using Natix.Weather.Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Polly;
using Natix.Weather.Domain.Config;
using Microsoft.Extensions.Options;
using Natix.Weather.Domain.Queries;
using Natix.Weather.Infrastructure.Resilience;

namespace Natix.Weather.Infrastructure;

public static class Startup
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Redis
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));

        // Repositories
        services.AddScoped<IWeatherQueryRepository, RedisWeatherQueryRepository>();
        services.AddScoped<IWeatherCommandRepository, RedisWeatherCommandRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // External weather provider + factory
        services.AddHttpClient<OpenMeteoWeatherProvider>();
        services.AddScoped<IWeatherProvider>(sp => sp.GetRequiredService<OpenMeteoWeatherProvider>());

        services.AddScoped<IWeatherProviderFactory, WeatherProviderFactory>();
        services.AddSingleton<IAsyncPolicy<WeatherDto>>(PollyPolicies.Create());

        // Config
        services.Configure<WeatherProviderConfig>(configuration.GetSection("WeatherProvider"));


        return services;
    }
}
