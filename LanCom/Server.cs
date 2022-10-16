using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LanCom
{
    internal class Server
    {
        private Socket _listener;
        public Socket Listener
        {
            get { return _listener; }
            set { _listener = value; }
        }

        private Socket _handler;
        public Socket Handler
        {
            get { return _handler; }
            set { _handler = value; }
        }

        public void RunServer()
        {
            StartServer();
            Console.WriteLine("Server started");

            Handler = Listener.Accept();
            ReceiveFile();

            Handler.Shutdown(SocketShutdown.Both);
            Handler.Close();
        }

        private void StartServer(int port = 11000)
        {
            IPEndPoint localEndPoint = new (IPAddress.Any, port);

            Listener = new (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Listener.Bind(localEndPoint);
            Listener.Listen(10);
        }

        private void ReceiveText()
        {
            string data = null;
            byte[] bytes = null;

            while (true)
            {
                bytes = new byte[1024];
                int bytesRec = Handler.Receive(bytes);
                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                if (data.IndexOf("<EOF>") > -1)
                {
                    break;
                }
            }
            Console.WriteLine("Text received: {0}", data);
        }

        private void ReceiveFile()
        {
            byte[] clientData = new byte[1024 * 5000];
            int bytesRec = Handler.Receive(clientData);

            BinaryWriter bWrite = new(File.Open("test.txt", FileMode.Append));
            bWrite.Write(clientData);
            bWrite.Close();
            Console.WriteLine("File received");
        }
    }
}
