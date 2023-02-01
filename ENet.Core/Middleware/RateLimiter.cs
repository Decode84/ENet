using System.Net;

namespace ENet.Core.Middleware;

public sealed class RateLimiter
{
    // rate limiting window size
    private static readonly TimeSpan WindowSize = TimeSpan.FromMinutes(5);

    // maximum bytes allowed per window
    private static readonly int MaxBytesPerWindow = 1024 * 1024 * 10; // 10 MB

    // store rate limiting info for each endpoint
    private readonly Dictionary<EndPoint, RateLimitingInfo> _rateLimitingInfo = new();

    public bool HandleData(EndPoint? endpoint, long size)
    {
        // Check if there is already rate limiting info for the given endpoint
        if (!_rateLimitingInfo.TryGetValue(endpoint ?? throw new ArgumentNullException(nameof(endpoint)), out var info))
        {
            // If not, create a new rate limiting info object and add it to the dictionary
            info = new RateLimitingInfo();
            _rateLimitingInfo.Add(endpoint, info);
        }

        // Get the current time
        var now = DateTime.UtcNow;

        // Check if the current time minus the last receive time is greater than the window size
        if (now - info.LastReceive > WindowSize)
        {
            // Reset the bytes received in the current window to 0
            info.BytesReceivedInWindow = 0;
            // Update the last receive time to the current time
            info.LastReceive = now;
        }

        // increment the bytes received in the current window by the size of the incoming data
        info.BytesReceivedInWindow += size;

        // return whether the total number of bytes received in the current window
        // is less than or equal to the maximum allowed
        return info.BytesReceivedInWindow <= MaxBytesPerWindow;
    }

    /// <summary>
    /// Inner class to store rate limiting info for a specific endpoint
    /// </summary>
    private class RateLimitingInfo
    {
        // last time data was received from this endpoint
        public DateTime LastReceive { get; set; }

        // total bytes received from this endpoint in the current window
        public long BytesReceivedInWindow { get; set; }
    }
}