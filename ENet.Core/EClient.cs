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
}