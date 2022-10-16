using LanCom;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class SocketListener
{
    public static int Main(string[] args)
    {
        StartCom(args);

        return 0;
    }

    private static void StartCom(string[] args)
    {
        if (args.Length == 0)
        {
            Help();
            return;
        }
        switch(args[0])
        {
            case "receive":
                Server server = new();
                server.RunServer();
                break;
            case "send":
                Client client = new(args.Skip(1).ToArray(), "192.168.1.12");
                client.RunClient();
                break;
            default:
                Console.WriteLine("Invalid usage, type: LanCom help to show help!");
                break;
        }
    }

    private static void Help()
    {
        Console.WriteLine("Not implemented yet");
    }
}
