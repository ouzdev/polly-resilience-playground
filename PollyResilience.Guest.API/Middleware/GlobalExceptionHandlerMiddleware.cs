using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PollyResilience.Client.Middleware;
public class GlobalExceptionHandlerMiddleware(RequestDelegate next,
    ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    public async Task Invoke(HttpContext context, IWebHostEnvironment env)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
                return;

            try
            {
                var apiError = ex switch
                {
                    ApiException apiException => new ApiError
                    {
                        StatusCode = apiException.StatusCode,
                        Errors = [apiException.Code]
                    },
                    _ => CreateInternalError(env.IsProduction(), ex)
                };
                
                logger.LogError(ex, "Exception : {Type}", ex.GetType().Name);

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var responseJson = JsonSerializer.Serialize(apiError, options);


                context.Response.ContentType = MediaTypeNames.Application.Json;
                context.Response.StatusCode = apiError.StatusCode;
                await context.Response.WriteAsync(responseJson);
            }
            catch (Exception ex2)
            {
                logger.LogError(0, ex2, "Global Exception Handler Response Creation Catcher");
            }
        }
    }
    
    /// <summary>
    /// Creates an ApiError object. Hides error details in production env, shows detailed messages in other environments.
    /// </summary>
    /// <param name="isProduction">True if environment is production.</param>
    /// <param name="ex">The exception to process.</param>
    /// <returns>ApiError object with appropriate error messages.</returns>
    private static ApiError CreateInternalError(bool isProduction, Exception ex)
    {
        var apiError = new ApiError
        {
            StatusCode = StatusCodes.Status500InternalServerError,
            Errors = [isProduction ? "AnErrorOccuredDuringTheOperation" : ex.Message]
        };

        if (isProduction)
            return apiError;

        if (ex.InnerException != null)
            apiError.Errors.Add(ex.InnerException.Message);

        if (!string.IsNullOrEmpty(ex.StackTrace))
            apiError.Errors.Add(ex.StackTrace);

        return apiError;
    }
}

[Serializable]
public class ApiException : Exception
{
    public string Code { get; set; }
    public int StatusCode { set; get; }
}

public class ApiError
{
    public Guid ErrorId { get; } = Guid.NewGuid();
    public int StatusCode { set; get; }

    public List<string> Errors { get; set; } = new List<string>();
    public string PrivateMessage { get; set; }
}