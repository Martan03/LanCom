using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LanCom
{
    internal class Client
    {
        private string ip { get; set; }
        private string[] args { get; set; }
        private Socket sender { get; set; }

        public Client(string[] args, string ip)
        {
            this.ip = ip;
            this.args = args;
        }

        public void RunClient()
        {
            StartClient();
            Console.WriteLine("Connected to {0}", sender.RemoteEndPoint.ToString());

            SelectOption();

            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        private void StartClient(int port = 11000)
        {
            IPEndPoint remEP = new(IPAddress.Parse(ip), port);
            sender = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(remEP);
        }

        private void SelectOption()
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Invalid usage, type: LanCom help to show help.");
                return;
            }

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
            sender.Send(Encoding.ASCII.GetBytes("0<EOF>"));

            byte[] msgBytes = Encoding.ASCII.GetBytes(msg + "<EOF>");
            int bytesSent = sender.Send(msgBytes);
        }

        private void SendFile(string path)
        {
            if (!File.Exists(path))
                throw new Exception("File not found");

            _SendFile(path, Path.GetFileName(path));
        }

        private void _SendFile(string path, string relPath)
        {
            byte[] fileData = File.ReadAllBytes(path);

            string notifyString = "file:" + relPath;
            byte[] notifyData = Encoding.ASCII.GetBytes(notifyString);

            sender.Send(notifyData);

            byte[] response = new byte[1024];
            sender.Receive(response);
            string res = Encoding.ASCII.GetString(response);

            if (res.Contains("OK"))
            {
                sender.Send(fileData);
                Console.WriteLine("File [" + path + "] transferred");
            }
        }

        private void SendDir(string path)
        {
            if (!Directory.Exists(path))
            {
                Console.WriteLine("Directory not found");
                return;
            }

            sender.Send(Encoding.ASCII.GetBytes("2<EOF>"));

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
            _SendFile(path, relPath);
        }
    }
}
