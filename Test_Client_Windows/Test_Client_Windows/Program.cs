using System;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;

namespace Test_Client_Windows
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                Console.WriteLine("No network available, please check connections!");
                return;
            }

            string IP = "";
            if (string.IsNullOrEmpty(IP) || string.IsNullOrWhiteSpace(IP))
            {
                if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                {
                    Console.WriteLine("No network available, please check connections!");
                    return;
                }

                IPHostEntry IpAdresses = Dns.GetHostEntry(Dns.GetHostName());

                foreach (var item in IpAdresses.AddressList)
                {
                    if (item.AddressFamily == AddressFamily.InterNetwork)
                    {
                        IP = item.ToString();
                        break;
                    }
                }
            }

            TcpClient tcpClient = new TcpClient();
            
            tcpClient.Connect(IP, 4200);

            if (tcpClient.Connected)
            {
                Console.WriteLine("Connected to server!");
            }
            else
            {
                Console.WriteLine("Failed to connect to server!");
                return;
            }

            NetworkStream networkStream = tcpClient.GetStream();

            while (true)
            {
                string message = Console.ReadLine();
                if (message == null)
                {
                    break;
                }

                if (message == "Exit")
                {
                    tcpClient.Close();
                    return;
                }

                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(message);
                networkStream.Write(buffer, 0, buffer.Length);
            }
        }
    }
}