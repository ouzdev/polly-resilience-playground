using System.Net;
using System.Text.Json.Serialization;
using Common;
using Microsoft.Extensions.Http.Resilience;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Polly;
using Polly.Fallback;
using Polly.Hedging;
using Polly.Resilience.Playground.Guest.API.Services.Concrete;
using PollyResilience.Client.Extensions.Strategies;
using PollyResilience.Client.Services.Abstract;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(opt =>
{
    opt.DocumentFilter<LowerCaseDocumentFilter>();

});

builder.Services.AddOpenTelemetry().WithMetrics(opts => opts
    .AddHttpClientInstrumentation()
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("PollyResilience.Guest.Api"))
    .AddMeter("Polly")
    .AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri(builder.Configuration.GetValue<string>("OtlpEndpointUri") ?? throw new InvalidOperationException());
    }));

builder.Services.AddTransient<ProcessApiResponse>();


builder.Services.AddHttpClient<IStrategyService, StrategyService>(ResiliencePipelineKey.StrategyHttpService.ToString(), client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("ExternalStrategyServiceApiEndpoint") ?? throw new InvalidOperationException());
}).AddResilienceHandler(ResiliencePipelineKey.StrategyHttpService.ToString(), (pipelineBuilder, c) =>
{
    var logger = c.ServiceProvider.GetService<ILogger<Program>>();

    pipelineBuilder
        //Aynı anda 5 istek işlenir ve 15 istek kuyrukta bekletilir bunun haricindeki istekler direk olarak iptal edilir.
        .AddConcurrencyLimiter(permitLimit: 5, queueLimit: 15)
        //Eğer servisten 30 saniye içinde yanıt gelmez ise istek timeout a düşer.
        .AddTimeoutStrategy(TimeSpan.FromSeconds(30), logger!)
        //Başarısız istekler tekrardan denenir. Burada maksimum 2 kez istek atmaya çalışılır eğer başarılı cevap gelmez ise hata dönülür.
        .AddRetryStrategy(maxRetryAttempts: 5, logger!)
        .AddHedging(new HttpHedgingStrategyOptions
        {
            MaxHedgedAttempts = 2, // İki paralel deneme
            Delay = TimeSpan.FromSeconds(5), // 1 saniye sonra ikinci istek başlatılır
            ShouldHandle = attempt => ValueTask.FromResult(attempt.Outcome switch
            {
                // Zaman aşımı durumunda hedge başlat
                { Result.StatusCode: HttpStatusCode.InternalServerError } => true,
                    
                _=>false
            }) 
            
        })
        .AddCircuitBreakerStrategy(failureRatio: 0.5, minimumThroughput: 5, breakDuration: TimeSpan.FromSeconds(15), logger!)
        .AddFallback(options: new FallbackStrategyOptions<HttpResponseMessage>
        {
            Name = "Fallback Strategy", // Fallback stratejisi adı

            FallbackAction = _ =>
            {
                // Fallback devreye girdiğinde varsayılan bir yanıt döndürülüyor
                var fallbackResponse = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    Content = new StringContent("Fallback response: Service is unavailable. Please try again later.")
                };

                logger?.LogWarning("Fallback executed for {PipelineKey}", ResiliencePipelineKey.StrategyHttpService.ToString());

                return new ValueTask<Outcome<HttpResponseMessage>>(Outcome.FromResult(fallbackResponse));
            }
        }).Build();

});


//Resilience Pipeline
builder.Services.AddResiliencePipeline("timeout", builder =>
{
    builder.AddTimeout(TimeSpan.FromSeconds(10));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
