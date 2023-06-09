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
    [SerializeField] int clientID = 1;

    MessageQueue messageQueue = null;

    ClientHandler clientHandler = null;

    Thread thread;
    void Start()
    {
        if (string.IsNullOrEmpty(IP) || string.IsNullOrWhiteSpace(IP))
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                Debug.Log("No network available, please check connections!");
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

        clientHandler = gameObject.GetComponent<ClientHandler>();
        if (clientHandler == null)
        {
            clientHandler = gameObject.AddComponent<ClientHandler>();
        }

        listener = new TcpListener(IPAddress.Parse(IP), port);
        tcpClient = default;

        listener.Start();
        thread = new Thread(DetectIncomingClient);
        thread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            if (messageQueue == null)
            {
                messageQueue = clientHandler.MessageQueue;
            }

            if (messageQueue.MessageAvailable())
            {
                MessageQueue.DecodedMessage message = messageQueue.GetNextMessage();
                GameObject obj = clientHandler.FindTwinByID(message.ID);
                if (obj == null)
                {
                    Debug.Log("No objects with that ID found!");
                    return;
                }
                // Get the number of nodes from the message.

                int nodes = 0;
                
                for (int i = 0; i < message.data.Length; i++)
                {
                    if (message.data[i].Contains("Nodes"))
                    {
                        string[] value = message.data[i].Split(':');
                        if(value.Length == 2)
                        {
                            if (!Int32.TryParse(value[1], out nodes))
                            {
                                Debug.Log(value[1]);
                                throw new ArgumentOutOfRangeException("Invalid data!");
                            }
                        }
                        break;
                    }
                }

                NodeSpawner spawner = obj.GetComponent<NodeSpawner>();
                if (!spawner.HasNodes)
                {
                    spawner.SpawnNodes(nodes);
                }
            }
        }
        catch (ArgumentOutOfRangeException e)
        {
            Debug.LogError(e.ToString());
        }

        if (clientHandler.hasUninstatiatedClients())
        {
            Debug.Log("Spawning Digital Twin!");
            clientHandler.SpawnDigitalTwin();
        }
    }

    private void DetectIncomingClient()
    {
        Debug.Log("Waiting For A Client!");
        while (true)
        {
            tcpClient = listener.AcceptTcpClient();
            Debug.Log("Client connected with ID: " + clientID);
            if (!clientHandler.AcceptClient(tcpClient, clientID))
            {
                Debug.LogError("Client was null!");
            }
            clientID += 1;
        }
    }

    public void ReduceClientCount()
    {
        clientID -= 1;
    }

}