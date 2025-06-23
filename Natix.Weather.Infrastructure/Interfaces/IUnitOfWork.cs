namespace Natix.Weather.Infrastructure.Interfaces;

public interface IUnitOfWork
{
    IWeatherQueryRepository WeatherReader { get; }
    IWeatherCommandRepository WeatherWriter { get; }
    Task CommitAsync();
}
