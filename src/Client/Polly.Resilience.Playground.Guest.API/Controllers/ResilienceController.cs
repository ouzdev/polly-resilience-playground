using Common;
using Microsoft.AspNetCore.Mvc;
using PollyResilience.Client.Controllers;
using PollyResilience.Client.Services.Abstract;

namespace Polly.Resilience.Playground.Guest.API.Controllers;

public class ResilienceController(ILogger<ResilienceController> logger, IStrategyService strategyService) : BaseController<ResilienceController>(logger)
{
    [HttpGet("retry")]
    public Task<ApiResponse<Reservation>> Retry() => strategyService.Retry();
    [HttpGet("circuit-breaker")]
    public Task<ApiResponse<Reservation>> CircuitBreaker()
    {
        return strategyService.CircuitBreaker();
    }
    [HttpGet("timeout")]
    public Task<ApiResponse<Reservation>> Timeout()
    {
        return strategyService.Timeout();
    }
    [HttpGet("rate-limiter")]
    public Task<ApiResponse<Reservation>> RateLimiter() => strategyService.RateLimiter();
    [HttpGet("hedging")]
    public Task<ApiResponse<Reservation>> Hedging() => strategyService.Hedging();

    [HttpGet("resilience-pipeline")]
    public async Task<ActionResult> TimeoutPipeline()
    {
        await strategyService.TimeoutPipeline();

        return StatusCode(StatusCodes.Status200OK);
    }

}
