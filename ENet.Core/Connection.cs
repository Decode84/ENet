using System.Net.Sockets;
using ENet.Core.Middleware;
using ENet.Core.Processors;

namespace ENet.Core;

public class Connection
{
    private readonly EClient _client;
    // private readonly ReceiveProcessor _receiveProcessor;
    private readonly RateLimiter _rateLimiter;
    
    public Connection(EClient client, ReceiveProcessor receiveProcessor, RateLimiter rateLimiter)
    {
        _client = client;
        _receiveProcessor = receiveProcessor;
        _rateLimiter = rateLimiter;
    }
    
    public async Task StartAsync()
    {
        // Create a new ReceiveProcessor instance & add event handlers
        var receiveArgs = new SocketAsyncEventArgs();
        receiveArgs.AcceptSocket = _client.Socket;
        receiveArgs.Completed += OnReceived;
        
        // Start the receive processor
        await _receiveProcessor.StartReceiveAsync(receiveArgs);
    }

    /// <summary>
    /// Event handler for the Completed event of a SocketAsyncEventArgs 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnReceived(object? sender, SocketAsyncEventArgs eventArgs)
    {
        // Get the number of bytes transferred in the receive operation
        long size = eventArgs.BytesTransferred;

        // If the size is 0 or less then the connection has been closed
        if (size <= 0)
        {
            Disconnect(eventArgs);
            return;
        }

        // Check if the data received exceeds the rate limit
        if (!_rateLimiter.HandleData(eventArgs.AcceptSocket?.RemoteEndPoint, size))
        {
            Disconnect(eventArgs);
            return;
        }

        try
        {
            // // List out the received data
            // var data = new byte[size];
            // Buffer.BlockCopy(eventArgs.Buffer, 0, data, 0, data.Length);
            // OnReceivedData(eventArgs.Buffer, 0, data.Length);

            // Start another receive operation
            await _receiveProcessor.StartReceiveAsync(eventArgs);
        }
        catch (Exception e)
        {
            Disconnect(eventArgs);
        }
    }
    
    /// <summary>
    /// Disconnects the connection from the server
    /// </summary>
    /// <param name="eventArgs"></param>
    private static void Disconnect(SocketAsyncEventArgs eventArgs)
    {
        // Close the socket
        eventArgs.AcceptSocket?.Close();

        // Dispose of the SocketAsyncEventArgs instance
        eventArgs.Dispose();

        // Print a message indicating that the connection was closed
        Console.WriteLine("Connection closed");
    }
}