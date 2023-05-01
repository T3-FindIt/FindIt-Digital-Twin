using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class ServerScript : MonoBehaviour
{
    TcpListener listener = null;
    TcpClient tcpClient = null;

    [SerializeField] string IP = null;
    [SerializeField] int port = 6900; // Should be fine.
    [SerializeField] int clientID = 0;

    MessageQueue messageQueue = null;

    ClientHandler clientHandler = null;
    void Start()
    {
        Debug.Log("Starting Demo!");
        if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
        {
            Debug.Log("No network available, please check connections!");
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

        Debug.Log("Server starting!");
        tcpClient = default(TcpClient);
    }

    // Update is called once per frame
    void Update()
    {
        if(listener.Server.Connected)
        {
            tcpClient = listener.AcceptTcpClient();
            Debug.Log("Client connected!");
            clientHandler.AcceptClient(tcpClient, clientID);
            clientID++;
        }

        if (messageQueue.MessageAvailable())
        {
            string msg = messageQueue.GetNextMessage();
            Debug.Log("Message received:\n"+ msg);
        }
    }
}
