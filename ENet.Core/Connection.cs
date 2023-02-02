using System.Net.Sockets;
using ENet.Core.Middleware;
using ENet.Core.Processors;

namespace ENet.Core;

public class Connection
{
    public Socket Socket { get; set; }
    public DateTime LastUpdate { get; set; }
    
    public Connection(Socket socket)
    {
        Socket = socket;
        LastUpdate = DateTime.Now;
    }
}