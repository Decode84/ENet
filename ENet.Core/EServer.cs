using System.Net;
using System.Net.Sockets;
using ENet.Core.Configuration;
using ENet.Core.Middleware;
using ENet.Core.Processors;

namespace ENet.Core;

public abstract class EServer : IDisposable
{
    private readonly Socket _socket;
    private readonly Socket _socket6;
    
    private readonly AcceptProcessor _acceptProcessor;
    private readonly ReceiveProcessor? _receiveProcessor;
    
    private ServerConfiguration Configuration { get; }
    private readonly RateLimiter _rateLimiter = new();
    
    private static bool IsRunning { get; set; }

    protected EServer(ServerConfiguration configuration)
    {
        // Set the Configuration property
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        
        // Create a new socket for the server
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        
        // Create a new AcceptProcessor instance & add event handlers 
        _acceptProcessor = new AcceptProcessor(_socket);
        _acceptProcessor.OnAccepted += OnAccepted;
        _acceptProcessor.OnAcceptFailed += OnAcceptFailed;
        
        // // Create a new ReceiveProcessor instance & add event handlers
        // _receiveProcessor = new ReceiveProcessor(_socket);
        // _receiveProcessor.OnReceived += OnReceived;
        // _receiveProcessor.OnReceivedFailed += OnReceivedFailed;
    }
    
    public async Task StartAsync()
    {
        // If the server is already running, throw an exception
        if (IsRunning)
            throw new InvalidOperationException("Server is already running");

        try
        {
            // Bind the socket to a local IP address and port
            _socket.Bind(new IPEndPoint(IPAddress.Any, Configuration.Port));

            // Raise the OnStarting event
            OnStarting();

            // Start listening for incoming connections
            _socket.Listen(Configuration.MaxConnections);

            IsRunning = true;

            // Raise the OnStarted event
            OnStarted();

            // Start accepting incoming connections
            await _acceptProcessor.StartAcceptAsync();
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Failed to start the server", e);
        }
    }
    
    /// <summary>
    /// Event handler for the OnAccepted event of the AcceptProcessor.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnAccepted(object? sender, SocketAsyncEventArgs e)
    {
        try
        {
            // If there was an error with the accept operation, return early
            if (e.SocketError != SocketError.Success) return;

            // var client = e.AcceptSocket;
            var client = new EClient(e.AcceptSocket);
            var endPoint = (IPEndPoint)client.Socket.RemoteEndPoint;

            OnConnected(client);

            // Initialize a SocketAsyncEventArgs object for receiving data
            // var receiveArgs = new SocketAsyncEventArgs();
            // receiveArgs.AcceptSocket = client.Socket;
            
            var connection = new Connection(client, _receiveProcessor, _rateLimiter);
            await connection.StartAsync();

            // Start receiving data from the client
            // await _receiveProcessor.StartReceiveAsync(receiveArgs);
        }
        catch (Exception exception)
        {
            // Invoke the OnAcceptFailed method
            OnAcceptFailed(sender, e);
        }
        finally
        {
            // Start listening for new client connections
            await _acceptProcessor.StartAcceptAsync();
        }
    }
    
    
    private void OnAcceptFailed(object? sender, SocketAsyncEventArgs e)
    {
        Disconnect(e);
    }
    
    private void OnReceivedFailed(object? sender, SocketAsyncEventArgs e)
    {
        
    }

    /// <summary>
    /// Stops the server.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void Stop()
    {
        OnStopping();

        try
        {
            _socket.Close();
            _socket.Dispose();
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Failed to stop the server", e);
        }

        OnStopped();
    }
    
    public void Dispose()
    {
        _socket.Close();
    }
    
    protected virtual void OnStarting() {}
    protected virtual void OnStarted() {}
    protected virtual void OnStopping() {}
    protected virtual void OnStopped() {}
    protected virtual void OnReceivedData(byte[] buffer, long offset, long size) {}
    protected virtual void OnConnected(EClient socket) {}
}