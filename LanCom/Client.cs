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
            settings.LoadSettings();
            ip = settings.defaultIP ?? "127.0.0.1";
        }

        /// <summary>
        /// Runs client depending on the arguments given
        /// </summary>
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
            }
            else
            {
                FileAttributes attr = File.GetAttributes(arg);
                if (attr.HasFlag(FileAttributes.Directory))
                    SendDir(arg);
                else
                    SendFile(arg);
            }

            if (StartClient() && sender is not null)
            {
                string notifyString = "9Vr3Hjqn0v:end ";
                byte[] notifyData = Encoding.UTF8.GetBytes(notifyString);

                sender.Send(notifyData, 0, notifyData.Length, 0);
            }
        }

        /// <summary>
        /// Connects to the server on given ip
        /// </summary>
        /// <param name="port">number of port of communication</param>
        /// <returns>true on success, else false</returns>
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

        /// <summary>
        /// Sends text to the server
        /// </summary>
        /// <param name="msg">text to be send</param>
        private void SendText(string msg)
        {
            if (!StartClient() || sender is null)
                return;

            string notifyString = "9Vr3Hjqn0v:text " + msg;
            byte[] notifyData = Encoding.UTF8.GetBytes(notifyString);

            sender.Send(notifyData, 0, notifyData.Length, 0);

            sender.Close();
        }

        /// <summary>
        /// Prepares sending file to the server
        /// </summary>
        /// <param name="path">file path</param>
        private void SendFile(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("File doesn't exist or can't be accessed");
                return;
            }

            _SendFile(path, Path.GetFileName(path));
        }

        /// <summary>
        /// Sends file to the server
        /// </summary>
        /// <param name="path">file path</param>
        /// <param name="relPath">path where file will be saved on server</param>
        private void _SendFile(string path, string relPath)
        {
            try
            {
                if (!StartClient() || sender is null)
                    throw new Exception();

                byte[] fileData = File.ReadAllBytes(path);

                string notifyString = "9Vr3Hjqn0v:file " + relPath;
                byte[] notifyData = Encoding.UTF8.GetBytes(notifyString);

                sender.Send(notifyData, 0, notifyData.Length, 0);

                byte[] response = new byte[512];
                sender.Receive(response);

                notifyString = Encoding.UTF8.GetString(response);
                if (notifyString.Contains("OK"))
                {
                    sender.Send(fileData, 0, fileData.Length, 0);

                    sender.Close();

                    Console.WriteLine("File [{0}] transferred", path);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error sending: {0}", e.Message);
            }
        }

        /// <summary>
        /// Prepares sending folder to the server
        /// </summary>
        /// <param name="path">path to the folder</param>
        private void SendDir(string path)
        {
            if (!Directory.Exists(path))
                return;

            sendNum = CountFiles(path, 0);

            string dir = Path.GetFullPath(Path.Combine(path, @"../"));
            ProcessDir(dir, path);
        }

        /// <summary>
        /// Processes given dir, searches all directories for files and sends them
        /// </summary>
        /// <param name="startDir">Start dir of search</param>
        /// <param name="path">Path that is being searched</param>
        private void ProcessDir(string startDir, string path)
        {
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
                _SendFile(file, file.Replace(startDir, ""));

            string[] dirs = Directory.GetDirectories(path);
            foreach (string dir in dirs)
                ProcessDir(startDir, dir);
        }

        /// <summary>
        /// Counts files in all directories
        /// </summary>
        /// <param name="path">directory to be search</param>
        /// <param name="n">number of found files</param>
        /// <returns>number of found files</returns>
        private int CountFiles(string path, int n)
        {
            string[] dirs = Directory.GetDirectories(path);
            foreach (string dir in dirs)
                n += CountFiles(dir, n);

            n += Directory.GetFiles(path).Length;

            return n;
        }

        /// <summary>
        /// Checks if given string is ip
        /// </summary>
        /// <param name="str">string to be checked</param>
        /// <returns>true if is ip, else false</returns>
        private bool IsIP(string str)
        {
            IPAddress? address;
            return IPAddress.TryParse(str, out address);
        }
    }
}