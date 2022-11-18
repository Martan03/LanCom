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
            case "help":
                Help();
                break;
            default:
                Console.WriteLine("Invalid usage, type: LanCom help to show help!");
                break;
        }
    }

    private static void Help()
    {
        Console.WriteLine("Welcome to \u001b[36mLanCom\u001b[0m help!");
        Console.WriteLine("\n\u001b[33mUsage:\u001b[0m");
        Console.WriteLine("   lancom {\u001b[33;1mCOMMAND\u001b[0m}");
        Console.WriteLine("\n\u001b[33mCommands:\u001b[0m");
        Console.WriteLine("   \u001b[33;1mreceive\u001b[0m                 starts server for receiving");
        Console.WriteLine("   \u001b[33;1msend\u001b[0m [\u001b[36;1marg\u001b[0m] [\u001b[36;1mip\u001b[0m]         starts client for sending");
        Console.WriteLine("      \u001b[36;1marg\u001b[0m                  path to file/folder or message to be send");
        Console.WriteLine("      \u001b[36;1mip\u001b[0m                   ip of the server");
        Console.WriteLine("   \u001b[33;1mconfig\u001b[0m [\u001b[36;1margs\u001b[0m]\u001b[0m           starts server for receiving");
        Console.WriteLine("      \u001b[36;1mip\u001b[0m:<ip>              saves IP as default for sending");
        Console.WriteLine("      \u001b[36;1mdir\u001b[0m:<path>           saves directory as default for saving");
        Console.WriteLine("      \u001b[36;1madd\u001b[0m:<shortcut>-<ip>  adds shortcut for given IP");
        Console.WriteLine("      \u001b[36;1mremove\u001b[0m:<shortcut>    removes shortcut");
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
