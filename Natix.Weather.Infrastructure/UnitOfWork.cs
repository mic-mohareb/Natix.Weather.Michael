using Natix.Weather.Infrastructure.Interfaces;

namespace Natix.Weather.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    public IWeatherQueryRepository WeatherReader { get; }
    public IWeatherCommandRepository WeatherWriter { get; }

    public UnitOfWork(
        IWeatherQueryRepository reader,
        IWeatherCommandRepository writer)
    {
        WeatherReader = reader;
        WeatherWriter = writer;
    }

    public Task CommitAsync()
    {
        // For Redis this is a no-op, but could be extended for transactional systems
        return Task.CompletedTask;
    }
}
