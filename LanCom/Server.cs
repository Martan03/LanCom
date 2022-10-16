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

            switch (option)
            {
                case '0':
                    Console.WriteLine("Received text: {0}", ReceiveText());
                    break;
                case '1':
                    ReceiveFile();
                    Console.WriteLine("File received");
                    break;
                case '2':
                    ReceiveDir();
                    Console.WriteLine("Directory received");
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
            return data.Replace("<EOF>", "");
        }

        private void ReceiveFile()
        {
            byte[] fileBytes = new byte[1024];

            Handler.Receive(fileBytes);
            int filenameLen = BitConverter.ToInt32(fileBytes, 0);
            string filename = Encoding.ASCII.GetString(fileBytes, 4, filenameLen);
            int fileLen = BitConverter.ToInt32(fileBytes, 4 + filenameLen);

            FileInfo file = new(filename);
            file.Directory.Create();

            BinaryWriter bWrite = new(File.Open(filename, FileMode.Append));
            int bytesRec = 0;

            while (bytesRec <= fileLen)
            {
                bytesRec += Handler.Receive(fileBytes);
                bWrite.Write(fileBytes);
            }

            bWrite.Close();
        }

        private void ReceiveDir()
        {
            string fileInfo;
            while (true)
            {
                ReceiveFile();
            }
        }
    }
}
