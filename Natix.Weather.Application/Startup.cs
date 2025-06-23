using Microsoft.Extensions.DependencyInjection;
using Polly;
using Natix.Weather.Application.Interfaces;
using Natix.Weather.Application.Services;
using Natix.Weather.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Natix.Weather.Application;

public static class Startup
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        //Register Infrastructure
        services.AddInfrastructure(configuration);

        // Register domain service layer
        services.AddScoped<IWeatherService, WeatherService>();

        return services;
    }
}
