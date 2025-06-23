using Natix.Weather.Domain.Models;
using Natix.Weather.Domain.Queries;

namespace Natix.Weather.Infrastructure.Interfaces;

public interface IWeatherProvider
{
    Task<WeatherDto> FetchTodayWeatherAsync(string city);
}
