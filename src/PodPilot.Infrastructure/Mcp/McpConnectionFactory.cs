using Microsoft.Extensions.Logging;
using PodPilot.Application.Common.Interfaces;
using PodPilot.Domain.Entities;

namespace PodPilot.Infrastructure.Mcp;

/// <summary>
/// Creates HTTP JSON-RPC MCP connections with timeout configuration.
/// </summary>
public sealed class McpConnectionFactory : IMcpConnectionFactory
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILoggerFactory loggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="McpConnectionFactory"/> class.
    /// </summary>
    public McpConnectionFactory(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
    {
        this.httpClientFactory = httpClientFactory;
        this.loggerFactory = loggerFactory;
    }

    /// <inheritdoc />
    public Task<IMcpConnection> CreateAsync(
        McpServer server,
        string? decryptedCredential,
        CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(nameof(HttpJsonRpcMcpConnection));
        client.Timeout = TimeSpan.FromSeconds(Math.Clamp(server.TimeoutSeconds, 5, 300));
        IMcpConnection connection = new HttpJsonRpcMcpConnection(
            client,
            server.Endpoint,
            server.AuthScheme,
            decryptedCredential,
            loggerFactory.CreateLogger<HttpJsonRpcMcpConnection>());
        return Task.FromResult(connection);
    }
}
