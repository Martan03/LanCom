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
            string filename = path.Split("/").Last();

            if (!File.Exists(path))
                throw new Exception("File doesn't exist or is unreachable");

            sender.Send(Encoding.ASCII.GetBytes("1" + path + "<EOF>"));

            FileStream file = new FileStream(path, FileMode.Open);
            byte[] fileChunk = new byte[1024];
            int bytesCount;

            while((bytesCount = file.Read(fileChunk, 0, 1024)) > 0)
            {
                if (sender.Send(fileChunk, bytesCount, SocketFlags.None) != bytesCount)
                    throw new Exception("Error in sending the file");
            }

            file.Close();
        }
    }
}
