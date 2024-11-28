using Common;
using PollyResilience.Client.Services.Abstract;

namespace PollyResilience.Client.Services.Concrete;

    public class StrategyService(HttpClient client):IStrategyService
    {
        private const string RetryEndpoint = "api/strategy/retry";
        private const string CircuitBreakerEndpoint = "api/strategy/circuit-breaker";
        private const string TimeoutEndpoint = "api/strategy/timeout";
        private const string RateLimiterEndpoint = "api/strategy/rate-limiter";
        private const string FallbackEndpoint = "api/strategy/fallback";
        private const string HedgingEndpoint = "api/strategy/hedging";

        public Task<ApiResponse<Reservation>> Retry() => ProcessApiResponse.ExecuteRequest<Reservation>( () => client.GetAsync(RetryEndpoint));
        public Task<ApiResponse<Reservation>> CircuitBreaker() => ProcessApiResponse.ExecuteRequest<Reservation>(() => client.GetAsync(CircuitBreakerEndpoint));
        public Task<ApiResponse<Reservation>> Timeout() => ProcessApiResponse.ExecuteRequest<Reservation>(() => client.GetAsync(TimeoutEndpoint));
        public Task<ApiResponse<Reservation>> RateLimiter() => ProcessApiResponse.ExecuteRequest<Reservation>(() => client.GetAsync(RateLimiterEndpoint));
        public Task<ApiResponse<Reservation>> Fallback() => ProcessApiResponse.ExecuteRequest<Reservation>(() => client.GetAsync(FallbackEndpoint));
        public Task<ApiResponse<Reservation>> Hedging()=> ProcessApiResponse.ExecuteRequest<Reservation>(() => client.GetAsync(HedgingEndpoint));
    }