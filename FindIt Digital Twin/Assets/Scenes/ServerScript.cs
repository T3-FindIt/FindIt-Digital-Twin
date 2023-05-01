using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    string IP = null;
    int port = 6900; // Should be fine.
    TcpListener listener = null;
    int clientID = 0;
    TcpClient tcpClient = null;

    MessageQueue messageQueue = null;

    ClientHandler clientHandler = null;
    void Start()
    {
        if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
        {
            Console.WriteLine("No network available, please check connections!");
            return;
        }

        clientHandler = gameObject.GetComponent<ClientHandler>();
        if(clientHandler == null)
        {
            clientHandler = gameObject.AddComponent<ClientHandler>();
        }

        messageQueue = clientHandler.MessageQueue;

        IPHostEntry IpAdresses = Dns.GetHostEntry(Dns.GetHostName());

        foreach (var item in IpAdresses.AddressList)
        {
            if(item.AddressFamily == AddressFamily.InterNetwork)
            {
                IP = item.ToString();
            }
        }

        listener = new TcpListener(IPAddress.Parse(IP), port);
        listener.Start();

        Console.WriteLine("Server starting!");
        Console.WriteLine("Server isActive = {0}",listener.Server.Connected);
        tcpClient = default(TcpClient);
    }

    // Update is called once per frame
    void Update()
    {
        if(listener.Server.Connected)
        {
            tcpClient = listener.AcceptTcpClient();
            Console.WriteLine("Client connected!");
            clientHandler.AcceptClient(tcpClient, clientID);
            clientID++;
        }

        if (messageQueue.MessageAvailable())
        {
            string msg = messageQueue.GetNextMessage();
            Console.WriteLine("Message received:\n{0}", msg);
        }
    }
}
