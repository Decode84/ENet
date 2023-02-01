using System.Net.Sockets;
using System.Text;

namespace ENet.Client;

public static class Program
{
    private static readonly Socket ClientSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    
    public static void Main()
    {
        var hostname = "localhost";
        ClientSocket.Connect(hostname, 5000);

        while (true)
        {
            Okay();
        }
    }

    private static void Okay()
    {
        Console.Write("Enter string: ");
        var data = Console.ReadLine();
        
        SendString($"{data}\n");
    }
    
    private static void SendString(string text)
    {
         var data = Encoding.ASCII.GetBytes(text);
         ClientSocket.Send(data, 0, data.Length, SocketFlags.None);
    }
}
