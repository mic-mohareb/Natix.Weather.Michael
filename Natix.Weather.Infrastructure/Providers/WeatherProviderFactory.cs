using Microsoft.Extensions.DependencyInjection;
using Natix.Weather.Infrastructure.Interfaces;

namespace Natix.Weather.Infrastructure.Providers;

public class WeatherProviderFactory : IWeatherProviderFactory
{
    private readonly IServiceProvider _services;

    public WeatherProviderFactory(IServiceProvider services)
    {
        _services = services;
    }

    public IWeatherProvider Create(string providerName) =>
        providerName switch
        {
            "default" => _services.GetRequiredService<IWeatherProvider>(),
            _ => throw new InvalidOperationException($"No provider found for {providerName}")
        };
}
