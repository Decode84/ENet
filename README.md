# ENet

ENet was developed as part of gaining an increased understanding of how sockets work. For this I chose to use C#, as it brings many options to the table, such as being able to use the following classes, TCPClient, TCPServer, Socket, IAsyncResult or SocketAsyncEventArgs.

![server client](https://i.imgur.com/XB37ZaZ.png)

When I first looked at two classes IAsyncResult and SocketAsyncEventArgs, I wondered what I would need. Since almost all online tutorials or open-source projects dealing with Socket in C# use IAsyncResult. Both classes are used for asynchronous programming. However, IAsyncResult is a more generalized interface and more flexible, whereas SocketAsyncEventArgs is specialized for use with the Socket Class. So the choice naturally fell on SocketAsyncEventArgs.

## Structure

The repository is set up in such a way that there is ENet.Core, ENet.Server and ENet.Client. Server and Client are only examples of how to use the Core library. The Core library consists of 1 main class EServer to be used by the server. Through it is where it all happens. In addition, there is a directory called Processors. In the processors directory there are two classes, AcceptProcessor and ReceiveProcessor. As the name might already suggest, these two classes actually take care of accepting a client and handling the incoming data from the client.  

## Processors

Accept Processor
```csharp
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
```
 
ReceiveProcessor
```csharp
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
```


    

