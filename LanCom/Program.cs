using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class SocketListener
{
    public static int Main(string[] args)
    {
        if (args.Length == 0)
            return -1;
        switch (args[0])
        {
            case "server":
                StartServer();
                break;
            case "client":
                StartClient();
                break;
            default:
                break;
        }
        return 0;
    }

    public static void StartServer()
    {
        IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 10897);
        Console.WriteLine("Server Start");

        Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(localEndPoint);
        listener.Listen(10);

        while (true)
        {
            Console.WriteLine("Waiting for a connection...");
            Socket handler = listener.Accept();

            Console.WriteLine(handler.RemoteEndPoint);

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
    }

    public static void StartClient()
    {
        IPEndPoint ip = new IPEndPoint(IPAddress.Parse("MY IP HERE"), 10897);
        Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        server.Connect(ip);
    }
}
