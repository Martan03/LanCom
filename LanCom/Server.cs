using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization.Metadata;
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
            SelectOption();

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

        private void SelectOption()
        {
            string startCom = ReceiveText();
            char option = startCom[0];
            startCom.Replace("<EOF>", "").Substring(1);

            switch (option)
            {
                case '0':
                    Console.WriteLine("Received text: {0}", ReceiveText());
                    break;
                case '1':
                    ReceiveFile(startCom);
                    break;
                default:
                    break;
            }
        }

        private string ReceiveText()
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
            return data;
        }

        private void ReceiveFile(string filename, string dir = "")
        {
            byte[] fileBytes = new byte[1024];
            int bytesRec;

            BinaryWriter bWrite = new(File.Open(dir + filename, FileMode.Append));

            while((bytesRec = Handler.Receive(fileBytes)) > 0)
            {
                bWrite.Write(fileBytes);
            }

            bWrite.Close();
            Console.WriteLine("File received");
        }
    }
}
