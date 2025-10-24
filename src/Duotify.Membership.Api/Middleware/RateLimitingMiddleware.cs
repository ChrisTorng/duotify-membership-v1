using System.Collections.Concurrent;

namespace Duotify.Membership.Api.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, RateLimitEntry> _rateLimitStore = new();
    private const int MaxRequestsPerMinute = 60;
    private const int VerificationResendLimit = 3;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = context.Request.Path.ToString().Contains("/verification-code/resend")
            ? ExtractMemberId(context.Request.Path)
            : context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (string.IsNullOrEmpty(clientId))
        {
            clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        var key = context.Request.Path.ToString().Contains("/verification-code/resend")
            ? $"resend_{clientId}"
            : $"ip_{clientId}";

        if (!_rateLimitStore.TryGetValue(key, out var entry))
        {
            entry = new RateLimitEntry { FirstRequestTime = DateTime.UtcNow, Count = 0 };
            _rateLimitStore.TryAdd(key, entry);
        }

        var elapsed = DateTime.UtcNow - entry.FirstRequestTime;
        
        if (elapsed.TotalMinutes > 1)
        {
            entry.FirstRequestTime = DateTime.UtcNow;
            entry.Count = 0;
        }

        var limit = key.StartsWith("resend_") ? VerificationResendLimit : MaxRequestsPerMinute;

        if (entry.Count >= limit)
        {
            _logger.LogWarning("Rate limit exceeded for {Key}", key);
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";
            
            var response = new { error = "Too many requests. Please try again later." };
            await context.Response.WriteAsJsonAsync(response);
            return;
        }

        entry.Count++;
        await _next(context);
    }

    private string ExtractMemberId(PathString path)
    {
        var parts = path.ToString().Split('/');
        if (parts.Length >= 3 && Guid.TryParse(parts[3], out _))
        {
            return parts[3];
        }
        return string.Empty;
    }

    private class RateLimitEntry
    {
        public DateTime FirstRequestTime { get; set; }
        public int Count { get; set; }
    }
}
