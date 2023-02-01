using System.Net.Sockets;

namespace ENet.Core.Processors;

public sealed class ReceiveProcessor : IDisposable
{
    private readonly Socket _receive;
    private readonly SocketAsyncEventArgs _receiveArgs;

    public event EventHandler<SocketAsyncEventArgs>? OnReceived;
    public event EventHandler<SocketAsyncEventArgs>? OnReceivedFailed;
    
    /// <summary>
    /// Constructor for the ReceiveProcessor class
    /// </summary>
    /// <param name="socketAsyncEventArgs"></param>
    public ReceiveProcessor(Socket receive)
    {
        // Set the receive socket
        _receive = receive;

        // Initialize the _receiveArgs field with a new SocketAsyncEventArgs object
        _receiveArgs = new SocketAsyncEventArgs();

        // Set the event handler for the Completed event
        _receiveArgs.Completed += ReceiveCompleted;

        // // Set the input SocketAsyncEventArgs object as the UserToken of the _receiveArgs object
        // _receiveArgs.UserToken = socketAsyncEventArgs.UserToken;
    }
    
    /// <summary>
    /// Starts an asynchronous receive operation on a socket.
    /// </summary>
    /// <param name="eventArgs"></param>
    public async Task StartReceiveAsync(SocketAsyncEventArgs eventArgs)
    {
        // Create a new SocketAsyncEventArgs instance for receiving data
        var receiveArgs = new SocketAsyncEventArgs();

        // Set the buffer for the receive operation and specify the offset and number of bytes to receive
        receiveArgs.SetBuffer(new byte[1024], 0, 1024);

        // Add an event handler to be called when the receive operation completes
        receiveArgs.Completed += OnReceived;

        // Set the AcceptSocket property to the socket passed in the eventArgs parameter
        receiveArgs.AcceptSocket = eventArgs.AcceptSocket;

        // Start the receive operation. If the operation is already complete,
        // the callback will be invoked immediately.
        if (!receiveArgs.AcceptSocket.ReceiveAsync(receiveArgs))
            await ProcessReceive(receiveArgs);
    }
    
    /// <summary>
    /// Processes the received data
    /// </summary>
    /// <param name="eventArgs"></param>
    private async Task ProcessReceive(SocketAsyncEventArgs eventArgs)
    {
        // Check for any errors in the receive operation
        if (eventArgs.SocketError != SocketError.Success) return;

        try
        {
            OnReceived?.Invoke(this, eventArgs);
        }
        catch (Exception e)
        {
            OnReceivedFailed?.Invoke(this, eventArgs);
        }

        await StartReceiveAsync(eventArgs);
    }
    
    private async void ReceiveCompleted(object sender, SocketAsyncEventArgs eventArgs)
    {
        if (eventArgs == null) throw new ArgumentNullException(nameof(eventArgs));

        // If the receive operation completed successfully
        try
        {
            await ProcessReceive(eventArgs);
        }
        catch (Exception e)
        {
            OnReceivedFailed?.Invoke(this, eventArgs);
        }
    }
    
    public void Dispose() => _receiveArgs.Dispose();
}