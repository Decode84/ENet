using System.Net;
using System.Net.Sockets;

namespace ENet.Core;

public class EClient
{
    public Socket Socket { get; set; }
    public string Ip { get; set; }
    private int Port { get; set; }

    public EClient(Socket socket)
    {
        Socket = socket;
        
        Ip = ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
        Port = ((IPEndPoint)socket.RemoteEndPoint).Port;
    }
    
    public void Send(byte[] buffer, int offset, int size)
    {
        Socket.BeginSend(buffer, offset, size, SocketFlags.None, SendCallback, Socket);
    }

    private void SendCallback(IAsyncResult result)
    {
        try
        {
            Socket socket = (Socket)result.AsyncState;
            int bytesSent = socket.EndSend(result);
            Console.WriteLine("Sent {0} bytes to client {1}", bytesSent, Ip);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error sending data to client: " + e.Message);
        }
    }
}