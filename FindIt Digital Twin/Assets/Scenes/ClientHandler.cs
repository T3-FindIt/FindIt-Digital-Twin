using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class ClientHandler : MonoBehaviour
{

    List<NetworkClient> clients = new List<NetworkClient>();

    public MessageQueue MessageQueue = null;

    // Start is called before the first frame update
    void Start()
    {
        MessageQueue = new MessageQueue();
    }

    private void Update()
    {
        foreach (var item in clients)
        {
            if (!item.isAlive())
            {
                clients.Remove(item);
            }
        }
    }

    public bool AcceptClient(TcpClient client, int ID)
    {
        if(client == null)
        {
            return false;
        }

        clients.Add(new NetworkClient(client, ID, MessageQueue));

        return true;
    }

}

public class NetworkClient
{
    TcpClient client;
    int ID = 0;
    MessageQueue messageQueue = null;
    Thread thread = null;

    public NetworkClient(TcpClient client, int ID, MessageQueue queue)
    {
        this.client = client;
        this.ID = ID;
        this.messageQueue = queue;

        thread = new Thread(StartCommunication);
        thread.Start();
    }

    void StartCommunication()
    {
        while (true)
        {
            if (client.Connected == false)
            {
                // Cleanup thread
                client.Close();

                thread.Abort();
                return;
            }

            if (client.Available > 0)
            {
                NetworkStream stream = client.GetStream();

                byte[] bytes = new byte[client.Available];

                stream.Read(bytes, 0, bytes.Length);

                string message = Encoding.ASCII.GetString(bytes);

                message = ID + " : " + message;

                messageQueue.AddMessage(message);

                //Cleanup the stream
                stream.Flush();
                stream.Close();
                stream.Dispose();
            }
        }
    }

    public bool isAlive()
    {
        return client.Connected;
    }
}

public class MessageQueue
{
    List<string> queue = null;

    public MessageQueue()
    {
        queue = new List<string>();
    }

    public string GetNextMessage()
    {
        if(queue.Count > 0)
        {
            string message = queue[0];
            queue.RemoveAt(0);
            return message;
        }
        else
        {
            return null;
        }
    }

    public void AddMessage(string message)
    {
        queue.Add(message);
    }

    public bool MessageAvailable()
    {
        return queue.Count > 0;
    }
}