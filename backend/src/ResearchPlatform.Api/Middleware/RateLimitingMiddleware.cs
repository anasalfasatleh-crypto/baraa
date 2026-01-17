using System.Collections.Concurrent;

namespace ResearchPlatform.Api.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, Queue<DateTime>> _requestHistory = new();
    private readonly int _requestLimit;
    private readonly TimeSpan _timeWindow;

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;

        // Read from configuration or use defaults
        _requestLimit = configuration.GetValue<int>("RateLimit:RequestsPerWindow", 100);
        _timeWindow = TimeSpan.FromMinutes(configuration.GetValue<int>("RateLimit:WindowMinutes", 1));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip rate limiting for health checks
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        var identifier = GetClientIdentifier(context);
        var now = DateTime.UtcNow;

        // Get or create request history for this client
        var requestTimes = _requestHistory.GetOrAdd(identifier, _ => new Queue<DateTime>());

        bool isRateLimited;
        int currentCount;

        lock (requestTimes)
        {
            // Remove old requests outside the time window
            while (requestTimes.Count > 0 && requestTimes.Peek() < now - _timeWindow)
            {
                requestTimes.Dequeue();
            }

            // Check if rate limit exceeded
            currentCount = requestTimes.Count;
            isRateLimited = currentCount >= _requestLimit;

            if (!isRateLimited)
            {
                // Add current request
                requestTimes.Enqueue(now);
            }
        }

        // Handle rate limiting outside of lock
        if (isRateLimited)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.Add("Retry-After", _timeWindow.TotalSeconds.ToString());

            _logger.LogWarning(
                "Rate limit exceeded for {Identifier}. {RequestCount} requests in {TimeWindow}",
                identifier,
                currentCount,
                _timeWindow
            );

            await context.Response.WriteAsJsonAsync(new
            {
                error = "Rate limit exceeded",
                retryAfter = _timeWindow.TotalSeconds
            });

            return;
        }

        // Add rate limit headers
        context.Response.Headers.Add("X-RateLimit-Limit", _requestLimit.ToString());
        context.Response.Headers.Add("X-RateLimit-Remaining", (_requestLimit - requestTimes.Count).ToString());
        context.Response.Headers.Add("X-RateLimit-Reset", (now + _timeWindow).ToString("o"));

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Try to get user ID from claims first
        var userId = context.User?.FindFirst("sub")?.Value
                     ?? context.User?.FindFirst("userId")?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        // Fall back to IP address
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ipAddress}";
    }

    // Cleanup old entries periodically (can be called by a background service)
    public static void Cleanup()
    {
        var cutoff = DateTime.UtcNow.AddHours(-1);
        var keysToRemove = new List<string>();

        foreach (var kvp in _requestHistory)
        {
            lock (kvp.Value)
            {
                if (kvp.Value.Count == 0 || kvp.Value.All(t => t < cutoff))
                {
                    keysToRemove.Add(kvp.Key);
                }
            }
        }

        foreach (var key in keysToRemove)
        {
            _requestHistory.TryRemove(key, out _);
        }
    }
}
