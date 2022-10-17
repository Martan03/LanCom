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
        private string[] args { get; set; }
        private int sendNum { get; set; }
        private string ip { get; set; }
        private Socket? sender { get; set; }
        private Settings settings { get; set; }

        public Client(string[] args)
        {
            this.args = args;
            sendNum = 1;
            settings = new Settings();
            ip = settings.defaultIP ?? "127.0.0.1";
        }

        public void RunClient()
        {
            string arg = "";
            if (args.Length == 0)
            {
                arg = "./";
            }
            else if (args.Length == 1)
            {
                if (IsIP(args[0]))
                    ip = args[0];
                else
                    arg = args[0];
            }
            else if (args.Length >= 2)
            {
                arg = args[0];
                if (!IsIP(args[1]))
                {
                    Console.WriteLine("{0} is not a valid IP", args[1]);
                    return;
                }
                ip = args[1];
            }

            if (!Directory.Exists(arg) && !File.Exists(arg))
            {
                SendText(arg);
                return;
            }

            FileAttributes attr = File.GetAttributes(arg);
            if (attr.HasFlag(FileAttributes.Directory))
                SendDir(arg);
            else
                SendFile(arg);
        }

        private bool StartClient(int port = 11000)
        {
            try
            {
                IPEndPoint remEP = new(IPAddress.Parse(ip), port);
                sender = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sender.Connect(remEP);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error connecting: {0}", e.Message);
                return false;
            }
            return true;
        }

        private void SendText(string msg)
        {
            if (!StartClient() || sender is null)
                return;

            Console.WriteLine("Connected to {0}", sender.RemoteEndPoint?.ToString());

            sender.Send(Encoding.ASCII.GetBytes("0:" + sendNum + ":<EOF>"));
            sendNum--;

            byte[] msgBytes = Encoding.ASCII.GetBytes(msg + "<EOF>");
            int bytesSent = sender.Send(msgBytes);

            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        private void SendFile(string path)
        {
            if (StartClient() || sender is null)
                return;

            Console.WriteLine("Connected to {0}", sender.RemoteEndPoint?.ToString());

            _SendFile(path, Path.GetFileName(path));

            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        private void _SendFile(string path, string relPath)
        {
            if (!File.Exists(path) || sender is null)
                return;

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
                return;

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
            if (!StartClient() || sender is null)
                return;

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

        private bool IsIP(string path)
        {
            IPAddress? address;
            return IPAddress.TryParse(path, out address);
        }
    }
}