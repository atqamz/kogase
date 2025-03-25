using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Kogase.Engine.Data;
using Kogase.Engine.Services;

namespace Kogase.Engine.Middleware
{
    public class ApiKeyAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiKeyAuthMiddleware> _logger;

        public ApiKeyAuthMiddleware(RequestDelegate next, ILogger<ApiKeyAuthMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, KogaseDbContext dbContext, ApiKeyService apiKeyService)
        {
            // Only check API key for telemetry endpoints
            if (!context.Request.Path.Value?.StartsWith("/api/v1/telemetry") ?? false)
            {
                await _next(context);
                return;
            }

            // Try to get API key from header
            var apiKey = context.Request.Headers["X-API-Key"].ToString();

            // If not in header, try query string
            if (string.IsNullOrEmpty(apiKey))
            {
                apiKey = context.Request.Query["apiKey"].ToString();
            }

            // If no API key found, return unauthorized
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("API key missing for telemetry request: {Path}", context.Request.Path);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "API key is required" });
                return;
            }

            // Check if API key is valid
            if (!apiKeyService.IsValidApiKeyFormat(apiKey))
            {
                _logger.LogWarning("Invalid API key format: {ApiKey}", apiKey);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid API key format" });
                return;
            }

            // Check if the API key is registered
            var project = await dbContext.Projects.FirstOrDefaultAsync(p => p.ApiKey == apiKey);
            if (project == null)
            {
                _logger.LogWarning("Unknown API key: {ApiKey}", apiKey);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid API key" });
                return;
            }

            // Add project ID to request for use in controllers
            context.Items["ProjectId"] = project.Id;

            // Continue with the request
            await _next(context);
        }
    }
} 