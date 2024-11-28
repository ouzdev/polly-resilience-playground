using Common;
using Microsoft.AspNetCore.Mvc;
using PollyResilience.Client.Services.Abstract;

namespace PollyResilience.Client.Controllers;

public class ResilienceController(ILogger<ResilienceController> logger,IStrategyService strategyService) : BaseController<ResilienceController>(logger)
{
    [HttpGet("retry")]
    public Task<ApiResponse<Reservation>> Retry() => strategyService.Retry();
    [HttpGet("circuit-breaker")]
    public Task<ApiResponse<Reservation>> CircuitBreaker() => strategyService.CircuitBreaker();
    [HttpGet("timeout")]
    public Task<ApiResponse<Reservation>> Timeout() => strategyService.Timeout();
    [HttpGet("rate-limiter")]
    public Task<ApiResponse<Reservation>> RateLimiter() => strategyService.RateLimiter();
    [HttpGet("fallback")]
    public Task<ApiResponse<Reservation>> Fallback() => strategyService.Fallback();
    [HttpGet("hedging")]
    public Task<ApiResponse<Reservation>> Hedging() => strategyService.Hedging();
}