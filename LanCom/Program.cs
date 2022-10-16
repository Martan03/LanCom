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
                Client client = new(args.Skip(1).ToArray());
                client.RunClient();
                break;
            case "config":
                Config(args.Skip(1).ToArray());
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

    private static void Config(string[] args)
    {
        Settings settings = new();
        foreach (string arg in args)
        {
            string code = arg.Split(":").Last();
            if (arg.StartsWith("ip:"))
                settings.defaultIP = code;
            else if (arg.StartsWith("dir:"))
                settings.defaultDir = code;
            else if (arg.StartsWith("add:"))
                settings.IPShortcuts.Add(code.Split("-")[0], code.Split("-")[1]);
            else if (arg.StartsWith("remove:"))
                settings.IPShortcuts.Remove(code);
        }
        settings.SaveSettings();
    }
}
