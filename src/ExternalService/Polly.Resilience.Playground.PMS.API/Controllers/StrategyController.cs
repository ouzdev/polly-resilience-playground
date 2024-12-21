﻿using Common;
using Microsoft.AspNetCore.Mvc;

namespace Polly.Resilience.Playground.PMS.API.Controllers;

public class StrategyController(ILogger<StrategyController> logger) : BaseController<StrategyController>(logger)
{
    private static int _failureCount;
    private static int _requestCount;
    
    [HttpGet("retry")]
    public IActionResult Retry()
    {
        var random = new Random();
        
        var shouldFail = random.Next(0, 2) == 0; 

        if (shouldFail)
        {
            Logger.LogError("Simulated failure occurred.");
            
            return StatusCode(500, "Simulated failure.");
        }

        var res = new Reservation
        {
            Id = Guid.NewGuid(),
            ArrivalDate = DateTime.Now,
            DepartureDate = DateTime.Now.AddDays(3),
            ConfirmationNo = "12345678",
        };

        Logger.LogInformation("Reservation generated successfully: {ConfirmationNo}", res.ConfirmationNo);
        return Ok(res);
    }
    
    [HttpGet("circuit-breaker")]
    public IActionResult CircuitBreaker()
    {
       
        if (_failureCount < 5)
        {
            _failureCount++;
            
            Logger.LogError("Simulated failure occurred in Circuit Breaker test. Failure count: {FailureCount}", _failureCount);

            return StatusCode(500, "Simulated failure.");
        }

        _failureCount = 0;
        var res = new Reservation
        {
            Id = Guid.NewGuid(),
            ArrivalDate = DateTime.Now,
            DepartureDate = DateTime.Now.AddDays(3),
            ConfirmationNo = "CIRCUIT1234",
        };

        Logger.LogInformation("Reservation (Circuit Breaker test) generated successfully: {ConfirmationNo}", res.ConfirmationNo);
        return Ok(res);
    }
    
    [HttpGet("rate-limiter")]
    public IActionResult RateLimiter()
    {
        // Thread-safe artırım
        var currentCount = Interlocked.Increment(ref _requestCount);

        Logger.LogInformation($"Rate Limiter Test generated successfully. Request Count: {currentCount}");

        return Ok();
    }
    
    [HttpGet("timeout")]
    public IActionResult Timeout()
    {
        Thread.Sleep(31000);
        return Ok();
    }
    
    [HttpGet("hedging-1")]
    public IActionResult Hedging1()
    {
        Logger.LogInformation("Endpoint hedging-1 is processing the request...");
        Thread.Sleep(5000); // 5 saniye gecikme simülasyonu
        return Ok("Response from hedging-1");
    }

    [HttpGet("hedging-2")]
    public IActionResult Hedging2()
    {
        Logger.LogInformation("Endpoint hedging-2 is processing the request...");
        Thread.Sleep(3000); // 3 saniye gecikme simülasyonu
        return Ok("Response from hedging-2");
    }

    [HttpGet("hedging-3")]
    public IActionResult Hedging3()
    {
        Logger.LogInformation("Endpoint hedging-3 is processing the request...");
        Thread.Sleep(1500); // 1.5 saniye gecikme simülasyonu
        return Ok("Response from hedging-3");
    }
}