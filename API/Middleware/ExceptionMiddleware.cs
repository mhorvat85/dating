using System.Net;
using System.Text.Json;
using API.Errors;

namespace API.Middleware;

public class ExceptionMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ILogger<ExceptionMiddleware> _logger;
  private readonly IHostEnvironment _env;
  private static readonly JsonSerializerOptions _options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

  public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
  {
    _next = next;
    _logger = logger;
    _env = env;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (Exception ex)
    {
      _logger.LogError("{Exception} {Message}", ex, ex.Message);
      context.Response.ContentType = "application/json";
      context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

      var response = _env.IsDevelopment()
        ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
        : new ApiException(context.Response.StatusCode, ex.Message, "Internal server error");

      var json = JsonSerializer.Serialize(response, _options);

      await context.Response.WriteAsync(json);
    }
  }
}
