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
        private Socket sender { get; set; }

        public Client(string ip)
        {
            this.ip = ip;
        }

        public void RunClient()
        {
            StartClient();
            Console.WriteLine("Connected to {0}", sender.RemoteEndPoint.ToString());

            SendFile("test.txt");

            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        private void StartClient(int port = 11000)
        {
            IPEndPoint remEP = new(IPAddress.Parse(ip), port);
            sender = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(remEP);
        }

        private void SendText(string msg)
        {
            byte[] msgBytes = Encoding.ASCII.GetBytes(msg + "<EOF>");
            int bytesSent = sender.Send(msgBytes);
        }

        private void SendFile(string filename)
        {
            byte[] fileBytes = File.ReadAllBytes(filename);
            sender.Send(fileBytes);
        }
    }
}
