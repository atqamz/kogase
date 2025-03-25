using Microsoft.AspNetCore.Builder;

namespace Kogase.Engine.Middleware
{
    public static class ApiKeyAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiKeyAuthMiddleware>();
        }
    }
} 