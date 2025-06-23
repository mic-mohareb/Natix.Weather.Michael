using Microsoft.AspNetCore.Mvc;
using Natix.Weather.Application.Interfaces;
using Natix.Weather.Domain.Commands;
using Natix.Weather.Domain.Queries;

namespace Natix.Weather.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;

    public WeatherController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    /// <summary>
    /// Gets today's weather forecast for a given city.
    /// </summary>
    [HttpGet("{city}")]
    public async Task<ActionResult<WeatherDto>> GetWeather(string city)
    {
        var result = await _weatherService.GetTodayWeatherAsync(city);
        return Ok(result);
    }
}
