using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Authentication;
using System.Text.Json;

namespace Shared.Middlewares;

public class ApiKeyMiddleware(RequestDelegate next, IApiKeyValidation apiKeyValidation, ILogger<ApiKeyMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var userApiKey = context.Request.Headers[Common.Constants.ApiKeyHeaderName].FirstOrDefault();
        if (!apiKeyValidation.IsValidApiKey(userApiKey))
        {
            logger.LogWarning("Unauthorized access attempt with key: {UserApiKey}", userApiKey);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var error = new
            {
                error = "Unauthorized",
                message = "Invalid API Key"
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(error));
            return;
        }
        await next(context);
    }
}
