using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Enumeration;
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
            byte[] fileBytes = new byte[1024];

            Handler.Receive(fileBytes);
            string notifyStr = Encoding.ASCII.GetString(fileBytes);

            if (notifyStr.StartsWith("text"))
            {
                Console.WriteLine("Received text: {0}", ReceiveText());
            }
            else if (notifyStr.StartsWith("file"))
            {
                string path = Path.GetFileName(notifyStr.Split(":").Last());
                Handler.Send(Encoding.ASCII.GetBytes("OK"));
                ReceiveFile(path);
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
            string filename = fi.FullName;

            byte[] fileBytes = new byte[1024];

            int bytesRec = Handler.Receive(fileBytes, fileBytes.Length, SocketFlags.None);

            BinaryWriter bWrite = new(File.Open(filename, FileMode.Append));

            while (bytesRec > 0)
            {
                bytesRec = Handler.Receive(fileBytes, fileBytes.Length, SocketFlags.None);

                if (bytesRec != 0)
                    bWrite.Write(fileBytes);
            }

            bWrite.Close();

            Console.WriteLine("Received file: {0}", path);
        }
    }
}
