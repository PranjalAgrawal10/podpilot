using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Application.Models.Gateway;

namespace PodPilot.Infrastructure.Gateway;

/// <summary>
/// Streams HTTP requests to upstream inference backends.
/// </summary>
public sealed class StreamingProxy : IStreamingProxy
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<StreamingProxy> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamingProxy"/> class.
    /// </summary>
    public StreamingProxy(
        IHttpClientFactory httpClientFactory,
        ILogger<StreamingProxy> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<GatewayProxyResult> ForwardAsync(
        GatewayProxyOptions options,
        Stream responseBody,
        Func<GatewayProxyResult, Task>? onResponseHeaders = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var targetUrl = GatewayUrlHelper.Combine(options.BaseUrl, options.Path);
        var client = httpClientFactory.CreateClient(nameof(StreamingProxy));
        client.Timeout = options.Timeout;

        using var request = new HttpRequestMessage(new HttpMethod(options.Method), targetUrl);

        if (options.Body is not null && options.Method is not "GET" and not "HEAD")
        {
            request.Content = new StreamContent(options.Body);
            if (options.Headers.TryGetValue("Content-Type", out var contentType))
            {
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
            }
        }

        foreach (var header in options.Headers)
        {
            if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)
                || header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase)
                || header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase)
                || header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        logger.LogInformation("Forwarding gateway request to {TargetUrl}", targetUrl);

        using var response = await client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        var responseHeaders = response.Headers
            .Concat(response.Content.Headers)
            .GroupBy(h => h.Key)
            .ToDictionary(g => g.Key, g => string.Join(", ", g.SelectMany(v => v.Value)));

        var isStreaming = responseHeaders.TryGetValue("Content-Type", out var ct)
            && (ct.Contains("text/event-stream", StringComparison.OrdinalIgnoreCase)
                || ct.Contains("application/x-ndjson", StringComparison.OrdinalIgnoreCase)
                || ct.Contains("application/json", StringComparison.OrdinalIgnoreCase));

        var result = new GatewayProxyResult
        {
            StatusCode = (int)response.StatusCode,
            Headers = responseHeaders,
            IsStreaming = isStreaming,
        };

        if (onResponseHeaders is not null)
        {
            await onResponseHeaders(result);
        }

        await using var upstreamStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await upstreamStream.CopyToAsync(responseBody, cancellationToken);
        await responseBody.FlushAsync(cancellationToken);

        stopwatch.Stop();
        result.ForwardLatencyMs = (int)stopwatch.ElapsedMilliseconds;
        return result;
    }
}
