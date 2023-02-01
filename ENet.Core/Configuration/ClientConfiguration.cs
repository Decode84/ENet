namespace ENet.Core.Configuration;

// Can be used for however the client should be structured.
public abstract class ClientConfiguration
{
    public abstract string Host { get; }
    public abstract ushort Port { get; }

    public abstract string Username { get; }
    public abstract string Password { get; }
}