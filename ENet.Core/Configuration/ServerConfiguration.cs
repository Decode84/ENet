namespace ENet.Core.Configuration;

public class ServerConfiguration
{
    /// <summary>
    /// The port to listen on
    /// </summary>
    public ushort Port { get; init; } = 5000;

    /// <summary>
    /// The amount of connections allowed to be open at once.
    /// </summary>
    public int MaxConnections { get; init; } = DefaultBacklog;

    private const int DefaultBacklog = 10;
}