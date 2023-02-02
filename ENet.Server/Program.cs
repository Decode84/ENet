using System.Text;
using ENet.Core;
using ENet.Core.Configuration;

namespace ENet.Server;

internal class Server : EServer
{
    private EClient _socket;
    
    public Server(ServerConfiguration configuration) : base(configuration)
    {
    }

    protected override void OnStarting()
    {
        Console.WriteLine("Starting server");
    }

    protected override void OnStarted()
    {
        Console.WriteLine("Server started");
    }

    protected override void OnConnected(EClient socket)
    {
        Console.WriteLine("Client connected " + socket.Ip);
        _socket = socket;
    }

    protected override void OnReceivedData(byte[] buffer, long offset, long size)
     {
         string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
         Console.WriteLine("Received message: " + message);

         // Respond to the client
         string response = "Server: You said " + message;
         byte[] responseBytes = Encoding.UTF8.GetBytes(response);
         _socket.Send(responseBytes, 0, responseBytes.Length);
     }
}

public static class Program
{
    public static async Task Main()
    {
        var config = new ServerConfiguration()
        {
            Port = 5000,
            MaxConnections = 50,
        };

        using var server = new Server(config);
        await server.StartAsync();

        Console.ReadKey();
    }
}