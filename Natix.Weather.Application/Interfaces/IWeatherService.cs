using Natix.Weather.Domain.Commands;
using Natix.Weather.Domain.Queries;

namespace Natix.Weather.Application.Interfaces;

public interface IWeatherService
{
    Task<WeatherDto> GetTodayWeatherAsync(string city);
    Task SaveTodayWeatherAsync(string city, WeatherPayload payload);
}
