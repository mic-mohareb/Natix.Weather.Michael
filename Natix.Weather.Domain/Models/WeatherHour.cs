namespace Natix.Weather.Domain.Models
{
    public class WeatherHour
    {
        public int Hour { get; set; }
        public int Temperature { get; set; }
        public string Condition { get; set; } = string.Empty;
    }
}
