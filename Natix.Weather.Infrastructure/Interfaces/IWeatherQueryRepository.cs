using Natix.Weather.Domain.Queries;

namespace Natix.Weather.Infrastructure.Interfaces
{
    public interface IWeatherQueryRepository
    {
        Task<WeatherDto?> GetAsync(string city, DateTime date);
    }
}
