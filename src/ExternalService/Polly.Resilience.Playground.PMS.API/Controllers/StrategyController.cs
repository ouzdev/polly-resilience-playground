using Common;
using Microsoft.AspNetCore.Mvc;

namespace Polly.Resilience.Playground.PMS.API.Controllers;

public class StrategyController(ILogger<StrategyController> logger) : BaseController<StrategyController>(logger)
{
    private static int _failureCount;
    private static int _requestCount;
    private static int _hedgingRequestCount;
    
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
    
    [HttpGet("hedging")]
    public IActionResult Hedging()
    {
        var currentRequest = Interlocked.Increment(ref _hedgingRequestCount); // İstek sayısını artır

        Logger.LogInformation("Hedging endpoint processing request {RequestNumber}", currentRequest);

        
        var res = new Reservation
        {
            Id = Guid.NewGuid(),
            ArrivalDate = DateTime.Now,
            DepartureDate = DateTime.Now.AddDays(3),
            ConfirmationNo = "CIRCUIT1234",
        };
        
        if (currentRequest % 2 == 1) // Tek sayılı isteklerde uzun gecikme
        {
            Logger.LogWarning("Request {RequestNumber} is experiencing a simulated delay.", currentRequest);
            Thread.Sleep(10000); // 6 saniye gecikme (hedging delay değerini aşar)
            return Ok(res);
        }

        // Çift sayılı isteklerde hızlı yanıt
        Logger.LogInformation("Request {RequestNumber} completed successfully.", currentRequest);
        return Ok(res);
    }

}