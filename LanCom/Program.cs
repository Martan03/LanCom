using LanCom;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class SocketListener
{
    public static int Main(string[] args)
    {
        //StartCom(args);

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
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("Welcome to ");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("LanCom!\n");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("Help:");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("LanCom receive");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(" - starts a server and starts receiving\n");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("LanCom send text <message> <ip>");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(" - sends a text to the server on IP\n");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("LanCom send file <path> <ip>");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(" - sends a file to the server on IP\n");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("LanCom send dir <path> <ip>");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(" - sends a directory to the server on IP\n");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("LanCom config ip:<ip> dir:<path> add:<shortcut>-<ip> remove:<shortcut>");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(" - saves given ip/dir as default and adds/removes shortcut\n");
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
            {
                string[] data = code.Split("-");
                if (settings.IPShortcuts == null)
                    settings.IPShortcuts = new Dictionary<string, string>();
                if (settings.IPShortcuts.ContainsKey(data[0]))
                    settings.IPShortcuts[data[0]] = data[1];
                else
                    settings.IPShortcuts.Add(data[0], data[1]);
            }
            else if (arg.StartsWith("remove:")
                     && settings.IPShortcuts != null
                     && settings.IPShortcuts.ContainsKey(code.Split("-")[0]))
                settings.IPShortcuts.Remove(code);
        }
        settings.SaveSettings();
    }
}
