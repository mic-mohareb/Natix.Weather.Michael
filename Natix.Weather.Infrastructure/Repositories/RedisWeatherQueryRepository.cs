using System.Text.Json;
using StackExchange.Redis;
using Natix.Weather.Domain.Queries;
using Natix.Weather.Infrastructure.Interfaces;

namespace Natix.Weather.Infrastructure.Repositories;

public class RedisWeatherQueryRepository : IWeatherQueryRepository
{
    private readonly IDatabase _redis;

    public RedisWeatherQueryRepository(IConnectionMultiplexer multiplexer)
    {
        _redis = multiplexer.GetDatabase();
    }

    public async Task<WeatherDto?> GetAsync(string city, DateTime date)
    {
        var key = $"weather:{city.ToLowerInvariant()}:{date:yyyyMMdd}";
        var json = await _redis.StringGetAsync(key);

        if (json.IsNullOrEmpty) return null;

        return JsonSerializer.Deserialize<WeatherDto>(json);
    }
}
