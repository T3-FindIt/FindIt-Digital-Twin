using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class ClientHandler : MonoBehaviour
{
    [SerializeField] List<NetworkClient> clients = new List<NetworkClient>();
    [SerializeField] List<GameObject> DigitalTwin = new List<GameObject>();
    [SerializeField] GameObject DigitalTwinPrefab;

    public MessageQueue MessageQueue = null;

    public bool hasUninstatiatedClient { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        MessageQueue = new MessageQueue();
        hasUninstatiatedClient = false;
    }

    private void Update()
    {
        for (int i = 0; i < clients.Count; i++)
        {
            if (!clients[i].isAlive())
            {
                clients.RemoveAt(i);
                Destroy(DigitalTwin[i],1);
                DigitalTwin.RemoveAt(i);
                i--;
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
        hasUninstatiatedClient = true;
        return true;
    }

    enum Direction
    {
        Up = 0,
        Down = 1
    }

    Direction lastDirection = Direction.Up;
    int x = 0;
    int z = 0;

    public void SpawnDigitalTwin(int id)
    {
        if (lastDirection == Direction.Down)
        {
            x += 1;
        }

        if (id % 4 == 0)
        {
            z += 1;
        }

        Vector3 position = new Vector3(x, 0, z);

        GameObject obj = Instantiate(DigitalTwinPrefab, position, Quaternion.identity);
        DigitalTwin.Add(obj);
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
        Debug.Log("Client Connected, Starting communication");
        NetworkStream stream = client.GetStream();
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

                byte[] bytes = new byte[client.Available];

                stream.Read(bytes, 0, bytes.Length);

                string message = Encoding.ASCII.GetString(bytes);

                message = ID + " : " + message;

                messageQueue.AddMessage(message);

                //Cleanup the stream
                stream.Flush();
            }
            Thread.Sleep(50);
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