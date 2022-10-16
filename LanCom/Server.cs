using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

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

            while (true)
            {
                Handler = Listener.Accept();
                SelectOption();
                Handler.Close();
            }

            Listener.Shutdown(SocketShutdown.Both);
            Listener.Close();
        }

        private void StartServer(int port = 11000)
        {
            IPEndPoint localEndPoint = new(IPAddress.Any, port);

            Listener = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Listener.Bind(localEndPoint);
            Listener.Listen(10);
        }

        private void SelectOption()
        {
            string startCom = ReceiveText();
            char option = startCom[0];

            startCom = startCom.Substring(startCom.IndexOf(":") + 1);
            int repeats = Int32.Parse(startCom.Substring(0, startCom.IndexOf(":")));

            startCom = startCom.Split(":").Last();

            switch (option)
            {
                case '0':
                    Console.WriteLine("Received text: {0}", ReceiveText());
                    break;
                case '1':
                    ReceiveFile(startCom);
                    Console.WriteLine("Received file: {0}", startCom);
                    break;
                default:
                    Listener.Shutdown(SocketShutdown.Both);
                    Listener.Close();
                    break;
            }

            if (repeats <= 1)
            {
                Handler.Close();
                Listener.Shutdown(SocketShutdown.Both);
                Listener.Close();
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

        private void ReceiveFile(string path)
        {
            FileInfo fi = new FileInfo(path);
            fi.Directory.Create();

            byte[] fileBytes = new byte[1024];

            BinaryWriter bWrite = new(File.Open(path, FileMode.Append));

            while (Handler.Receive(fileBytes) > 0)
            {
                bWrite.Write(fileBytes);
            }

            bWrite.Close();
        }
    }
}