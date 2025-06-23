using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;
using Polly.Wrap;
using Natix.Weather.Domain.Queries;

namespace Natix.Weather.Infrastructure.Resilience;

public static class PollyPolicies
{
    public static IAsyncPolicy<WeatherDto> Create()
    {
        // Retry with exponential backoff
        var retry = Policy<WeatherDto>
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (ex, delay, attempt, _) =>
                {
                });

        // Circuit breaker with failure rate logic
        var circuitBreaker = Policy<WeatherDto>
            .Handle<Exception>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,                      // 50% failure rate
                samplingDuration: TimeSpan.FromSeconds(10), // 10s window
                minimumThroughput: 4,                        // minimum 4 calls
                durationOfBreak: TimeSpan.FromSeconds(30),  // open for 30s
                onBreak: (ex, delay) => { },
                onReset: () => {  });

        return Policy.WrapAsync(retry, circuitBreaker);
    }
}
