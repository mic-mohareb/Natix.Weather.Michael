using Natix.Weather.Domain.Commands;

namespace Natix.Weather.Infrastructure.Interfaces;

public interface IWeatherCommandRepository
{
    Task SaveAsync(string city, DateTime date, WeatherPayload payload);
}
