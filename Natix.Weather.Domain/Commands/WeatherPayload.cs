using Natix.Weather.Domain.Models;

namespace Natix.Weather.Domain.Commands
{
    public class WeatherPayload
    {
        public string City { get; set; } = string.Empty;
        public DateTime FetchedAt { get; set; }
        public List<WeatherHour> Hours { get; set; } = new();
    }
}
