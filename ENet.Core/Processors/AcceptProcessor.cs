using System.Net.Sockets;

namespace ENet.Core.Processors;

public class AcceptProcessor : IDisposable
{
    private readonly Socket _listen;
    private readonly SocketAsyncEventArgs _acceptArgs;

    public event EventHandler<SocketAsyncEventArgs>? OnAccepted;
    public event EventHandler<SocketAsyncEventArgs>? OnAcceptFailed;
    
    /// <summary>
    /// Constructor for the AcceptProcessor
    /// </summary>
    /// <param name="listen"></param>
    public AcceptProcessor(Socket listen)
    {
        // Set the listen socket
        _listen = listen;

        // Create a new SocketAsyncEventArgs instance for accepting connections
        _acceptArgs = new SocketAsyncEventArgs();

        // Add an event handler to be called when the accept operation completes
        _acceptArgs.Completed += AcceptCompleted;
    }
    
    /// <summary>
    /// starts an asynchronous accept operation on the _listen socket. 
    /// </summary>
    public async Task StartAcceptAsync()
    {
        // Set the AcceptSocket property to null to ensure that the context object is not
        if (_acceptArgs.AcceptSocket is not null)
            _acceptArgs.AcceptSocket = null;

        // Start an asynchronous accept operation
        if (!_listen.AcceptAsync(_acceptArgs))
            await ProcessAccept(_acceptArgs);
    }
    
    /// <summary>
    /// Process the connection when an accept operation completes.
    /// If there was an error in the accept operation, it returns immediately. 
    /// </summary>
    /// <param name="eventArgs"></param>
    private async Task ProcessAccept(SocketAsyncEventArgs eventArgs)
    {
        // If there was an error in the accept operation, return
        if (eventArgs.SocketError != SocketError.Success) return;

        try
        {
            // Raise the OnAccepted event
            OnAccepted?.Invoke(this, eventArgs);
        }
        catch (Exception e)
        {
            // If there is an exception, raise the OnAcceptFailed event
            OnAcceptFailed?.Invoke(this, eventArgs);
        }

        // Start another accept operation
        await StartAcceptAsync();
    }
    
    /// <summary>
    /// Event handler for the Completed event of the SocketAsyncEventArgs instance.
    /// It is called when an accept operation completes
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    private async void AcceptCompleted(object sender, SocketAsyncEventArgs e)
    {
        // Validate the input parameters
        if (e == null) throw new ArgumentNullException(nameof(e));

        if (sender is null)
            throw new ArgumentNullException(nameof(sender));

        if (e.LastOperation != SocketAsyncOperation.Accept)
            throw new ArgumentException("The last operation completed on the socket was not a accept");
        
        try
        {
            // Process the accept operation
            await ProcessAccept(e);
        }
        catch (Exception exception)
        {
            // If there is an exception, raise the OnAcceptFailed event
            OnAcceptFailed?.Invoke(this, e);
        }
    }
    
    /// <summary>
    /// The StopAccept method closes the _listen socket
    /// disposes of the SocketAsyncEventArgs instance
    /// </summary>
    public void StopAccept()
    {
        // Close the listen socket
        _listen.Close();
        
        // Dispose of the SocketAsyncEventArgs instance
        Dispose();
    }

    /// <summary>
    /// Disposes of the SocketAsyncEventArgs instance.
    /// </summary>
    public void Dispose()
    {
        // Dispose of the SocketAsyncEventArgs instance
        _acceptArgs.Dispose();
    }
}