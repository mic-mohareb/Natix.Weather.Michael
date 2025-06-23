using System.Text.Json;
using StackExchange.Redis;
using Natix.Weather.Domain.Commands;
using Natix.Weather.Domain.Queries;
using Natix.Weather.Infrastructure.Interfaces;
using Natix.Weather.Domain.Models;

namespace Natix.Weather.Infrastructure.Repositories;

public class RedisWeatherCommandRepository : IWeatherCommandRepository
{
    private readonly IDatabase _redis;

    public RedisWeatherCommandRepository(IConnectionMultiplexer multiplexer)
    {
        _redis = multiplexer.GetDatabase();
    }

    public async Task SaveAsync(string city, DateTime date, WeatherPayload payload)
    {
        var dto = new WeatherDto
        {
            City = city,
            Date = date,
            UpdatedAt = payload.FetchedAt,
            Cached = true,
            Source = "external-weather-api",
            RetryAttempts = 0,
            Weather = payload.Hours
        };

        var json = JsonSerializer.Serialize(dto);
        var key = $"weather:{city.ToLowerInvariant()}:{date:yyyyMMdd}";
        var ttl = date.AddDays(1).Date - DateTime.UtcNow;

        await _redis.StringSetAsync(key, json, ttl);
    }
}
