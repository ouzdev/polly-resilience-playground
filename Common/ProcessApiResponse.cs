using System.Net;
using System.Text;
using System.Text.Json;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace Common;

public class ProcessApiResponse
{
    public static async Task<ApiResponse<T>> ExecuteRequest<T>(Func<Task<HttpResponseMessage>> action)
    {
        try
        {
            var response = await action();

            var content = await response.Content.ReadAsStringAsync();

            if (response != null && response.StatusCode != HttpStatusCode.OK)
                throw new HttpRequestException(content, null, response.StatusCode);


            var data = response.IsSuccessStatusCode && !string.IsNullOrEmpty(content)
                ? JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
                : default;

            return new ApiResponse<T>
            {
                StatusCode = response.StatusCode,
                IsSuccess = response.IsSuccessStatusCode,
                Data = data,
                ErrorMessage = response.IsSuccessStatusCode ? null : response.ReasonPhrase
            };
        }
        catch (BrokenCircuitException brokenCircuitException)
        {
            var retryAfterMessage = brokenCircuitException.RetryAfter.HasValue
                ? $" Circuit breaker will close after {brokenCircuitException.RetryAfter.Value.TotalSeconds} seconds." : " Retry time is not specified.";

            return new ApiResponse<T>
            {
                IsSuccess = false,
                StatusCode = HttpStatusCode.InternalServerError,
                ErrorMessage = $"{brokenCircuitException.Message}{retryAfterMessage}"
            };
        }
        catch (TimeoutRejectedException timeoutRejectedException)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                StatusCode = HttpStatusCode.RequestTimeout, // 408: Request Timeout
                ErrorMessage = $"The request was timed out: {timeoutRejectedException.Message}. Please try again later."
            };
        }
        catch (HttpRequestException httpRequestException)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                StatusCode = httpRequestException.StatusCode ?? HttpStatusCode.InternalServerError,
                ErrorMessage = httpRequestException.Message
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                StatusCode = HttpStatusCode.InternalServerError,
                ErrorMessage = $"An unexpected error occured. Message: {ex.Message}",
            };
        }
    }
}