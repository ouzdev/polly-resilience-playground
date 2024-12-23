using System.Net;
using Polly;
using Polly.CircuitBreaker;
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
            FailureRatio = failureRatio,
            MinimumThroughput = minimumThroughput,
            BreakDuration = breakDuration,
            ShouldHandle = response =>
            {
                var statusCode = response.Outcome.Result?.StatusCode;
                return ValueTask.FromResult(statusCode is >= HttpStatusCode.InternalServerError && statusCode != HttpStatusCode.ServiceUnavailable);
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
        builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
        {
            ShouldHandle = args =>
            {
                if (args.Outcome.Result != null &&
                    (int)args.Outcome.Result.StatusCode >= 500 &&
                    (int)args.Outcome.Result.StatusCode < 600 &&
                    args.Outcome.Result.StatusCode != HttpStatusCode.ServiceUnavailable)
                {
                    return new ValueTask<bool>(true);
                }

                return new ValueTask<bool>(false);
            },
            MaxRetryAttempts = maxRetryAttempts,
            OnRetry = args =>
            {
                logger!.LogError(", Attempt: {0}", args.AttemptNumber);
                return default;
            },
            BackoffType = DelayBackoffType.Constant,

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
            context.Tags.Add(new KeyValuePair<string, object>("circuit.breaker.open.duration", onCircuitOpenedArgs.BreakDuration));
        }
    }
}

public class PipelineEndpointEnricher : MeteringEnricher
{
    public override void Enrich<TResult, TArgs>(in EnrichmentContext<TResult, TArgs> context)
    {
        if (context.TelemetryEvent.Arguments is not HttpRequestMessage httpRequest) return;

        var endpoint = httpRequest.RequestUri?.AbsolutePath ?? "Unknown";

        context.Tags.Add(new KeyValuePair<string, object>("http.endpoint", endpoint));
    }
}
