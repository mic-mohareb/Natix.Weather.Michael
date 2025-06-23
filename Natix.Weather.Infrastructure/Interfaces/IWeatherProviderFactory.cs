namespace Natix.Weather.Infrastructure.Interfaces
{
    public interface IWeatherProviderFactory
    {
        IWeatherProvider Create(string providerName);
    }
}
