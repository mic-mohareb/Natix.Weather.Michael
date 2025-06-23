# Natix Weather API

clean-architecture .NET 8 Web API that fetches real-time weather data from the Open-Meteo API and caches it via Redis with Docker setup.

---

## Clean Architecture Structure

| Layer            | Project Name                  | Responsibility |
|------------------|-------------------------------|----------------|
| **Domain**       | `Natix.Weather.Domain`        | Core models, DTOs, interfaces (`IWeatherService`, `IWeatherProvider`) |
| **Application**  | `Natix.Weather.Application`   | Orchestrates logic (e.g. `WeatherService`), queries, commands |
| **Infrastructure**| `Natix.Weather.Infrastructure`| External concerns: `OpenMeteoWeatherProvider`, Redis caching, Polly, config |
| **API**          | `Natix.Weather.API`           | Controller layer, Swagger, DI wiring |
| **Testing**      | `Natix.Weather.Tests`         | Unit & integration tests for all boundaries |

---

## Design Patterns

| Pattern                    | Usage in Code |
|---------------------------|---------------|
| **Dependency Injection**   | All service and provider registrations in `Program.cs` (`AddScoped`, `AddSingleton`, etc.) |
| **Factory Pattern**        | `WeatherProviderFactory` resolves `IWeatherProvider` from string (e.g. "open-meteo") |
| **Strategy Pattern**       | `IWeatherProvider` interface has multiple implementations (e.g. `OpenMeteoWeatherProvider`) |
| **Repository Pattern**     | `WeatherCacheRepository` abstracts Redis caching logic |
| **Unit of Work**           | `IWeatherService.SaveTodayWeatherAsync()` manages coordinated write operations |
| **Adapter**                | `OpenMeteoWeatherProvider` transforms raw API JSON into domain-safe `WeatherDto` |
| **Resilience (Polly)**     | `PollyPolicies.Create()` defines retry + circuit breaker policies for API calls |
| **DTO Mapping**            | Domain-layer `WeatherDto` aggregates temperature, code, hour, and metadata |
| **Configuration Options**  | `WeatherProviderConfig` bound via `IOptions<T>` in `OpenMeteoWeatherProvider` |

---


## How to Run Locally

docker compose up --build


## Technologies & Frameworks & Libraries

| Library / Tool             | Purpose                                       |
|----------------------------|-----------------------------------------------|
| **.NET 8**                 | Core runtime                                  |
| **System.Text.Json**       | JSON (de)serialization                        |
| **Polly**                  | Retry and circuit breaker                     |
| **StackExchange.Redis**    | Redis client                                  |
| **Swashbuckle.AspNetCore** | Swagger & OpenAPI UI                          |
| **Microsoft.Extensions***  | Dependency injection, options, HTTP client    |
| **Docker Compose ***       | one command for run it locally or on server   |


---

## System Architecture Diagram

```plaintext
                                      ┌────────────────────────────┐
                                      │   Client HTTP Request      │
                                      │ GET /api/weather/{city}    │
                                      └────────────┬───────────────┘
                                                   │
                                    ┌──────────────▼──────────────┐
                                    │   WeatherController.cs      │
                                    │   (Natix.Weather.API)       │    
                                    │   Dockerized ASP.NET 8 API  │ 
                                    └──────────────┬──────────────┘
                                                   │
                            ┌──────────────────────▼──────────────────────┐
                            │   WeatherService.FetchTodayWeatherAsync     │
                            │   (Natix.Weather.Application)               │
                            └──────────────┬──────────────────────────────┘
                                           │
                        ┌──────────────────▼────────────────────┐
                        │  Check Redis via WeatherCacheRepo     │
                        │  (Natix.Weather.Infrastructure)       │
                        └──────────────┬────────────────────────┘
                                       │
                  ┌────────────────────▼────────────────────┐
                  │ Cached result found?                    │
                  ├──────────────┬──────────────┐           │
                  │              │              │           │
                  │ Yes          ▼              ▼           │
                  │      Return WeatherDto  →  Fetch via     │
                  │                        OpenMeteoProvider │
                  ▼                             │            ▼
        ┌──────────────────┐       ┌────────────────────────────────────┐
        │ Return response  │◄──────│ OpenMeteoWeatherProvider           │
        └──────────────────┘       │ (Natix.Weather.Infrastructure)     │
                                   └─────────────────┬──────────────────┘
                                                    │
                    ┌───────────────────────────────▼────────────────────┐
                    │         Resilience Layer (PollyPolicies)           │
                    │      - Retry: 3 attempts (exp backoff)             │
                    │      - Circuit Breaker:                            │
                    │        • Threshold: 50% failure rate               │
                    │        • Window: 10s, Minimum: 4 calls             │
                    │        • Break duration: 30s                       │
                    └───────────────────────────────┬────────────────────┘
                                                    │
                    ┌───────────────────────────────▼────────────────────┐
                    │ Call Open-Meteo Geocoding API                      │
                    │ Deserialize to GeoResult → GeoEntry               │
                    └───────────────────────────────┬────────────────────┘
                                                    │
                    ┌───────────────────────────────▼────────────────────┐
                    │ Call Forecast API with lat/lon                     │
                    │ Deserialize to ForecastResult → HourlyData         │
                    └───────────────────────────────┬────────────────────┘
                                                    │
                    ┌───────────────────────────────▼────────────────────┐
                    │ Compose WeatherDto & Cache it                      │
                    │ Save via WeatherCacheRepository                    │
                    └───────────────────────────────┬────────────────────┘
                                                    │
                                      ┌─────────────▼────────────┐
                                      │  Return WeatherDto       │
                                      │  to WeatherController    │
                                      └─────────────┬────────────┘
                                                    │
                                      ┌─────────────▼────────────┐
                                      │   Return JSON response   │
                                      │   to client via HTTP     │
                                      └──────────────────────────┘
