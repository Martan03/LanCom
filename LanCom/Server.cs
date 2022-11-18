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

        private bool repeat { get; set; }
        private string defaultDir { get; set; }

        public Server()
        {
            Settings settings = new Settings();
            settings.LoadSettings();
            defaultDir = settings.defaultDir ?? "./";
            repeat = true;
        }

        /// <summary>
        /// Runs a server and wait for a connection
        /// </summary>
        public void RunServer()
        {
            if (!StartServer() || Listener is null)
                return;

            Console.WriteLine("Server started:");
            ShowIP();

            while (repeat)
            {
                Listener.Listen(10);
                Handler = Listener.Accept();
                SelectOption();
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
            byte[] notifyArray = new byte[1024];

            Handler!.Receive(notifyArray);
            string notifyStr = Encoding.UTF8.GetString(notifyArray);
            string temp = "9Vr3Hjqn0v:";

            if (notifyStr.StartsWith(temp + "file "))
            {
                string path = notifyStr.Replace(temp + "file ", "");

                path = string.Concat(path.Split(Path.GetInvalidPathChars())).Trim();

                FileInfo fi = new(path);
                fi.Directory?.Create();
                File.Create(path).Close();

                Handler.Send(Encoding.UTF8.GetBytes("OK"));

                ReceiveFile(path);

                Handler.Close();
                Console.WriteLine("Received file [{0}]...", path);
            }
            else if (notifyStr.StartsWith(temp + "text "))
            {
                Console.WriteLine("Received text: {0}", notifyStr.Replace(temp + "text ", ""));
                Handler.Close();
            }
            else if (notifyStr.StartsWith(temp + "end "))
            {
                Handler.Close();
                repeat = false;
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
            byte[] fileBytes = new byte[1024];
            int bytesRec = Handler!.Receive(fileBytes, fileBytes.Length, SocketFlags.None);

            BinaryWriter bWrite = new(File.Open(path, FileMode.Append));
            bWrite.Write(fileBytes);

            while (bytesRec > 0)
            {
                bytesRec = Handler.Receive(fileBytes, fileBytes.Length, 0);
                if (bytesRec != 0)
                    bWrite.Write(fileBytes, 0, bytesRec);
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
                    Console.WriteLine(ip.ToString());
            }
        }
    }
}