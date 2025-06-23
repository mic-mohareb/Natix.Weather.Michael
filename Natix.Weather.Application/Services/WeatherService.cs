using Natix.Weather.Application.Interfaces;
using Natix.Weather.Domain.Commands;
using Natix.Weather.Domain.Queries;
using Natix.Weather.Infrastructure.Interfaces;
using Polly;

namespace Natix.Weather.Application.Services;

public class WeatherService : IWeatherService
{
    private readonly IWeatherQueryRepository _reader;
    private readonly IWeatherCommandRepository _writer;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWeatherProviderFactory _providerFactory;
    private readonly IAsyncPolicy<WeatherDto> _resilience;

    public WeatherService(
        IWeatherQueryRepository reader,
        IWeatherCommandRepository writer,
        IUnitOfWork unitOfWork,
        IWeatherProviderFactory providerFactory,
        IAsyncPolicy<WeatherDto> resilience)
    {
        _reader = reader;
        _writer = writer;
        _unitOfWork = unitOfWork;
        _providerFactory = providerFactory;
        _resilience = resilience;
    }

    public async Task<WeatherDto> GetTodayWeatherAsync(string city)
    {
        var date = DateTime.UtcNow.Date;

        var cached = await _reader.GetAsync(city, date);
        if (cached is not null)
            return cached;

        var provider = _providerFactory.Create("default");

        var fresh = await _resilience.ExecuteAsync(() => provider.FetchTodayWeatherAsync(city));

        var payload = new WeatherPayload
        {
            City = city,
            FetchedAt = DateTime.UtcNow,
            Hours = fresh.Weather
        };

        await SaveTodayWeatherAsync(city, payload);

        fresh.Cached = false;
        fresh.UpdatedAt = payload.FetchedAt;

        return fresh;
    }

    public async Task SaveTodayWeatherAsync(string city, WeatherPayload payload)
    {
        var date = DateTime.UtcNow.Date;
        await _writer.SaveAsync(city, date, payload);
        await _unitOfWork.CommitAsync();
    }
}
