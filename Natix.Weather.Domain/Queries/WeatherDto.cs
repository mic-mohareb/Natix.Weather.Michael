using Natix.Weather.Domain.Models;

namespace Natix.Weather.Domain.Queries
{
    public class WeatherDto
    {
        public string City { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Source { get; set; } = "external-weather-api";
        public bool Cached { get; set; }
        public int RetryAttempts { get; set; }
        public List<WeatherHour> Weather { get; set; } = new();
    }
}
