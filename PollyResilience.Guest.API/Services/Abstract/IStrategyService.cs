using Common;

namespace PollyResilience.Client.Services.Abstract;

public interface IStrategyService
{
    Task<ApiResponse<Reservation>> Retry();
    Task<ApiResponse<Reservation>> CircuitBreaker();
     Task<ApiResponse<Reservation>> Timeout();
     Task<ApiResponse<Reservation>> RateLimiter();
     Task<ApiResponse<Reservation>> Fallback();
     Task<ApiResponse<Reservation>> Hedging();
}