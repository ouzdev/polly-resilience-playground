using System.Collections.Immutable;
using System.Net;
using System.Net.Sockets;
using Polly;
using Polly.CircuitBreaker;
using Polly.RateLimiting;
using Polly.Retry;
using Polly.Telemetry;
using Polly.Timeout;

namespace PollyResilience.Client.Extensions.Strategies;

public static class ResiliencePipelineBuilderExtensions
{
    public static ResiliencePipelineBuilder<HttpResponseMessage> AddTimeoutStrategy(this ResiliencePipelineBuilder<HttpResponseMessage> builder,TimeSpan timeoutDuration,ILogger logger)
    {
        builder.AddTimeout(new TimeoutStrategyOptions
        {
            Timeout = timeoutDuration,
            Name = $"Timeout {timeoutDuration.TotalMilliseconds}ms",
            OnTimeout = args =>
            {
                logger.LogWarning("Timeout occurred after {Timeout} for operation: {OperationKey}", args.Timeout, args.Context.OperationKey);
                return ValueTask.CompletedTask;
            }
        });

        return builder;
    }

    public static ResiliencePipelineBuilder<HttpResponseMessage> AddCircuitBreakerStrategy(this ResiliencePipelineBuilder<HttpResponseMessage> builder,double failureRatio,int minimumThroughput,TimeSpan breakDuration,ILogger logger)
    {
        builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
        {
            FailureRatio = failureRatio, // %50 hata oranı
            MinimumThroughput = minimumThroughput, // En az 4 istek sonrası değerlendirme başlar
            BreakDuration = breakDuration, // Servis 1 dakika devre dışı kalacak.
            ShouldHandle = response =>
            {
                var statusCode = response.Outcome.Result?.StatusCode;
                return ValueTask.FromResult(statusCode != null && (int)statusCode >= 500);
            },
            OnOpened = args =>
            {
                logger.LogWarning("Circuit breaker tripped for {BreakDuration}. Reason: {Reason}", args.BreakDuration,
                    args.Outcome.Exception?.ToString() ?? "N/A");
                return ValueTask.CompletedTask;
            },
            OnClosed = args =>
            {
                logger.LogInformation($"Circuit breaker reset. Operation Key: {args.Context.OperationKey}");
                return ValueTask.CompletedTask;
            },
            OnHalfOpened = args =>
            {

                logger.LogInformation($"Circuit breaker half. Operation Key: {args.Context.OperationKey}");
                return ValueTask.CompletedTask;
            }
        });
        return builder;
    }
    


    public static ResiliencePipelineBuilder<HttpResponseMessage> AddRetryStrategy(this ResiliencePipelineBuilder<HttpResponseMessage> builder, int maxRetryAttempts, ILogger logger)
    {
        
        var strategyExceptions = new[]
        {
            typeof(TimeoutRejectedException),
            typeof(BrokenCircuitException),
            typeof(RateLimiterRejectedException),
        }.ToImmutableArray();
        
        builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
        {
            ShouldHandle = args =>
            {
                // Sadece HTTP durum kodu kontrol edilir
                if (args.Outcome.Result is HttpResponseMessage response &&
                    (int)response.StatusCode >= 500) // 500 ve üzeri durum kodları için retry
                {
                    return new ValueTask<bool>(true);
                }

                return new ValueTask<bool>(false); // Retry tetiklenmez
            },
            
            MaxRetryAttempts = maxRetryAttempts,
            DelayGenerator = static args =>
            {
                var delay = args.AttemptNumber switch
                {
                    0 => TimeSpan.Zero,
                    1 => TimeSpan.FromSeconds(1),
                    _ => TimeSpan.FromSeconds(5)
                };
                return new ValueTask<TimeSpan?>(delay);
            },
            OnRetry = args =>
            {
                logger!.LogError(", Attempt: {0}", args.AttemptNumber);
                return default;
            },
            BackoffType = DelayBackoffType.Constant
        });

        return builder;
    }

}

//Custom metrik eklemek için kullanılır...
public class CircuitBreakerMetersEnricher : MeteringEnricher
{
    public override void Enrich<TResult, TArgs>(in EnrichmentContext<TResult, TArgs> context)
    {
        if (context.TelemetryEvent.Arguments is OnCircuitOpenedArguments<TResult> onCircuitOpenedArgs)
        {
            context.Tags.Add(new KeyValuePair<string, object?>("circuit.breaker.open.duration", onCircuitOpenedArgs.BreakDuration));
        }
    }
}

public class PipelineEndpointEnricher : MeteringEnricher
{
    public override void Enrich<TResult, TArgs>(in EnrichmentContext<TResult, TArgs> context)
    {
        if (context.TelemetryEvent.Arguments is not HttpRequestMessage httpRequest) return;

        var endpoint = httpRequest.RequestUri?.AbsolutePath ?? "Unknown";

        context.Tags.Add(new KeyValuePair<string, object?>("http.endpoint", endpoint));
    }
}
