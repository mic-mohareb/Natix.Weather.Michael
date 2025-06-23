using System.Text.Json;
using System.Text.Json.Serialization;
using Natix.Weather.Domain.Config;
using Natix.Weather.Domain.Models;
using Natix.Weather.Domain.Queries;
using Natix.Weather.Infrastructure.Interfaces;
using Microsoft.Extensions.Options;

namespace Natix.Weather.Infrastructure.Providers;

public class OpenMeteoWeatherProvider : IWeatherProvider
{
    private readonly HttpClient _httpClient;
    private readonly WeatherProviderConfig _config;

    public OpenMeteoWeatherProvider(HttpClient httpClient, IOptions<WeatherProviderConfig> options)
    {
        _httpClient = httpClient;
        _config = options.Value;
    }

    public async Task<WeatherDto> FetchTodayWeatherAsync(string city)
    {
        // Step 1:  Get lat/lng by city 
        var geoUrl = $"{_config.GeocodingBaseUrl.TrimEnd('/')}/search?name={Uri.EscapeDataString(city)}";
        var geoResponse = await _httpClient.GetStringAsync(geoUrl);
        var geoData = JsonSerializer.Deserialize<GeoResult>(geoResponse);

        if (geoData?.Results == null || geoData.Results.Count == 0)
        {
            // Optional: log details from geoResponse
            throw new KeyNotFoundException($"No location data returned for '{city}'.");
        }
        var location = geoData.Results[0];

        // Step 2: Fetch forecast by lat/lng
        var forecastUrl = $"{_config.ForecastBaseUrl.TrimEnd('/')}" +
                          $"?latitude={location.Latitude}&longitude={location.Longitude}" +
                          $"&hourly=temperature_2m,weathercode" +
                          $"&forecast_days=1&timezone=auto";

        var forecastResponse = await _httpClient.GetStringAsync(forecastUrl);
        var forecast = JsonSerializer.Deserialize<ForecastResult>(forecastResponse);

        var hours = forecast?.Hourly?.Time.Select((t, i) => new WeatherHour
        {
            Hour = DateTime.Parse(t).Hour,
            Temperature = (int)Math.Round(forecast.Hourly.Temperature2M[i]),
            Condition = MapWeatherCode(forecast.Hourly.WeatherCode[i])
        }).ToList() ?? new();

        return new WeatherDto
        {
            City = location.Name,
            Date = DateTime.UtcNow.Date,
            Cached = false,
            UpdatedAt = DateTime.UtcNow,
            Source = "open-meteo",
            RetryAttempts = 0,
            Weather = hours
        };
    }

    private static string MapWeatherCode(int code) => code switch
    {
        0 => "Clear",
        1 or 2 => "Partly Cloudy",
        3 => "Overcast",
        45 or 48 => "Fog",
        51 or 53 or 55 => "Drizzle",
        61 or 63 or 65 => "Rain",
        71 or 73 or 75 => "Snow",
        95 or 96 or 99 => "Thunderstorm",
        _ => "Unknown"
    };

    private record GeoResult(
        [property: JsonPropertyName("results")] List<GeoEntry>? Results);

    private record GeoEntry(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("latitude")] double Latitude,
        [property: JsonPropertyName("longitude")] double Longitude);

    private record ForecastResult(
    [property: JsonPropertyName("hourly")] HourlyData Hourly);

    private record HourlyData(
        [property: JsonPropertyName("time")] List<string> Time,
        [property: JsonPropertyName("temperature_2m")] List<double> Temperature2M,
        [property: JsonPropertyName("weathercode")] List<int> WeatherCode);
}
