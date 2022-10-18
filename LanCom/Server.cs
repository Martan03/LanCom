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
        private Socket? Listener { get; set; }

        private Socket? Handler { get; set; }

        private int repeats { get; set; }
        private string defaultDir { get; set; }

        public Server()
        {
            Settings settings = new Settings();
            settings.LoadSettings();
            defaultDir = settings.defaultDir ?? "./";
            repeats = 1;
        }

        /// <summary>
        /// Runs a server and wait for a connection
        /// </summary>
        public void RunServer()
        {
            if (!StartServer() || Listener is null)
                return;

            Console.WriteLine("Server started");
            ShowIP();

            while (repeats > 0)
            {
                Handler = Listener.Accept();
                SelectOption();
                Handler.Close();
                repeats--;
            }

            Listener.Close();
        }

        /// <summary>
        /// Starts server
        /// </summary>
        /// <param name="port">port number</param>
        /// <returns>true on success, else false</returns>
        private bool StartServer(int port = 11000)
        {
            try
            {
                IPEndPoint localEndPoint = new(IPAddress.Any, port);

                Listener = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Listener.Bind(localEndPoint);
                Listener.Listen(10);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error creating server: {0}", e.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Switches between receiving files and text
        /// </summary>
        private void SelectOption()
        {
            string startCom = ReceiveText();
            char option = startCom[0];

            startCom = startCom.Substring(startCom.IndexOf(":") + 1);
            repeats = Int32.Parse(startCom.Substring(0, startCom.IndexOf(":")));

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
                    break;
            }
        }

        /// <summary>
        /// Receives text from connected client
        /// </summary>
        /// <returns>received text</returns>
        private string ReceiveText()
        {
            if (Handler is null)
            {
                Console.WriteLine("Error maintaining connection with device.");
                return "";
            }

            string data = "";
            byte[] bytes;

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

        /// <summary>
        /// Receives file from connected client
        /// </summary>
        /// <param name="path">file path to save it</param>
        private void ReceiveFile(string path)
        {
            if (Handler is null)
            {
                Console.WriteLine("Error maintaining connection with device.");
                return;
            }

            path = defaultDir + "/" + path;

            FileInfo fi = new FileInfo(path);
            fi.Directory?.Create();

            byte[] fileBytes = new byte[1024];

            BinaryWriter bWrite = new(File.Open(path, FileMode.Append));

            while (Handler.Receive(fileBytes) > 0)
            {
                bWrite.Write(fileBytes);
            }

            bWrite.Close();
        }

        /// <summary>
        /// Shows local ip address
        /// </summary>
        private void ShowIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    Console.WriteLine(ip.ToString());
                }
            }
        }
    }
}