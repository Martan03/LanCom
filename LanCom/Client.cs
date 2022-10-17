using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace LanCom
{
    internal class Client
    {
        private string? ip { get; set; }
        private string[] args { get; set; }
        private Socket? sender { get; set; } = null;
        private int sendNum { get; set; }
        private Settings settings { get; set; }

        public Client(string[] args)
        {
            this.args = args;
            sendNum = 1;
            settings = new Settings();
            ip = settings.defaultIP;
        }

        private bool StartClient(int port = 11000)
        {
            IPEndPoint remEP = new(IPAddress.Parse(ip), port);
            sender = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(remEP);

            return true;
        }

        public void RunClient()
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Invalid usage, type: LanCom help to show help.");
                return;
            }

            if (args.Length < 3 && settings.defaultIP == null)
            {
                Console.WriteLine("No Default IP set nor any IP given.");
                return;
            }

            if (args.Length >= 3)
                ip = args[2];

            switch (args[0])
            {
                case "text":
                    SendText(args[1]);
                    break;
                case "file":
                    SendFile(args[1]);
                    break;
                case "dir":
                    SendDir(args[1]);
                    break;
                default:
                    break;
            }
        }

        private void SendText(string msg)
        {
            StartClient();
            Console.WriteLine("Connected to {0}", sender?.RemoteEndPoint?.ToString());

            sender.Send(Encoding.ASCII.GetBytes("0:" + sendNum + ":<EOF>"));
            sendNum--;

            byte[] msgBytes = Encoding.ASCII.GetBytes(msg + "<EOF>");
            int bytesSent = sender.Send(msgBytes);

            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        private void SendFile(string path)
        {
            StartClient();
            Console.WriteLine("Connected to {0}", sender?.RemoteEndPoint?.ToString());

            _SendFile(path, Path.GetFileName(path));

            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        private void _SendFile(string path, string relPath)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("File not found: {0}", path);
                return;
            }

            sender.Send(Encoding.ASCII.GetBytes("1:" + sendNum + ":" + relPath + "<EOF>"));
            sendNum--;

            FileStream file = new FileStream(path, FileMode.Open);
            byte[] fileChunk = new byte[1024];
            int bytesCount;

            while ((bytesCount = file.Read(fileChunk, 0, 1024)) > 0)
            {
                if (sender.Send(fileChunk, bytesCount, SocketFlags.None) != bytesCount)
                    throw new Exception("Error in sending the file");
            }

            file.Close();
        }

        private void SendDir(string path)
        {
            if (!Directory.Exists(path))
            {
                Console.WriteLine("Directory not found: {0}", path);
                return;
            }

            sendNum = CountFiles(path, 0);

            string dir = Path.GetFullPath(Path.Combine(path, @"../"));
            ProcessDir(dir, path);
        }

        private void ProcessDir(string startDir, string path)
        {
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
                ProcessFile(file, file.Replace(startDir, ""));

            string[] dirs = Directory.GetDirectories(path);
            foreach (string dir in dirs)
                ProcessDir(startDir, dir);
        }

        private void ProcessFile(string path, string relPath)
        {
            StartClient();

            _SendFile(path, relPath);

            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        private int CountFiles(string path, int n)
        {
            string[] dirs = Directory.GetDirectories(path);
            foreach (string dir in dirs)
                n += CountFiles(dir, n);

            n += Directory.GetFiles(path).Length;

            return n;
        }
    }
}