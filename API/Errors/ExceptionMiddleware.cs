using System.Net;
using System.Text.Json;

namespace API.Errors;

public class ExceptionMiddleware(
    RequestDelegate _next,
    ILogger<ExceptionMiddleware> _logger,
    IHostEnvironment _env)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = _env.IsDevelopment() ?
                new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString()) :
                new ApiException(context.Response.StatusCode, ex.Message, "Internal Server Error");

            var jason = JsonSerializer.Serialize(response, JsonOptions);

            await context.Response.WriteAsync(jason);
        }
    }
}
