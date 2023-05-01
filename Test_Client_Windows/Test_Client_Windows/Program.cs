using System;
using System.Net.Sockets;
using System.Net;

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
            string IP = "192.168.56.1";

            TcpClient tcpClient = new TcpClient();
            
            tcpClient.Connect(IP, 6900);

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
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(message);
                networkStream.Write(buffer, 0, buffer.Length);
            }
        }
    }
}