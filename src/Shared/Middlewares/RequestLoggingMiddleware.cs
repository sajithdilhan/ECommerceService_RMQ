using Confluent.Kafka;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Shared.Middlewares;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;
        var method = context.Request.Method;

        logger.LogInformation("Incoming request {Method} {Path}", method, path);

        var sw = Stopwatch.StartNew();

        try
        {
            await next(context);
            sw.Stop();

            logger.LogInformation("Completed {Method} {Path} with {Status} in {Elapsed}ms", method, path, context.Response.StatusCode, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "Error in {Method} {Path} after {Elapsed}ms", method, path, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
