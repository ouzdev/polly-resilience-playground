using Common;
using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.Registry;
using PollyResilience.Client.Services.Abstract;

namespace PollyResilience.Client.Services.Concrete;

    public class StrategyService(HttpClient client,ResiliencePipelineProvider<string>  pipelineProvider):IStrategyService
    {
        private const string RetryEndpoint = "api/strategy/retry";
        private const string CircuitBreakerEndpoint = "api/strategy/circuit-breaker";
        private const string TimeoutEndpoint = "api/strategy/timeout";
        private const string RateLimiterEndpoint = "api/strategy/rate-limiter";

        public Task<ApiResponse<Reservation>> Retry()
        {
           
            return ProcessApiResponse.ExecuteRequest<Reservation>( () => client.GetAsync(RetryEndpoint));
        }
        public Task<ApiResponse<Reservation>> CircuitBreaker() => ProcessApiResponse.ExecuteRequest<Reservation>(() => client.GetAsync(CircuitBreakerEndpoint));
        public Task<ApiResponse<Reservation>> Timeout() => ProcessApiResponse.ExecuteRequest<Reservation>(() => client.GetAsync(TimeoutEndpoint));
        public Task<ApiResponse<Reservation>> RateLimiter() => ProcessApiResponse.ExecuteRequest<Reservation>(() => client.GetAsync(RateLimiterEndpoint));
        public async Task TimeoutPipeline()
        {
            var timeoutPipeline = pipelineProvider.GetPipeline("timeout");
           await timeoutPipeline.ExecuteAsync(static async cancellation => await Task.Delay(100, cancellation));
           
        }
    }
    
    
    
    