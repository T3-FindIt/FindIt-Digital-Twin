using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
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
                GetComponent<ServerScript>().ReduceClientCount();
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
    int z = 3;

    public void SpawnDigitalTwin(int id)
    {
        if (lastDirection == Direction.Down)
        {
            x += 1;
            lastDirection = Direction.Up;
        }
        else
        {
            x -= 1;
            lastDirection = Direction.Down;
        }

        Vector3 position = new Vector3(x, 0, z);

        GameObject obj = Instantiate(DigitalTwinPrefab, position, Quaternion.identity);
        obj.name = DigitalTwinPrefab.name + " | " + id; // Makes it a bit easier.
        DigitalTwin.Add(obj);
    }

    public GameObject FindTwinByID(int id)
    {
        foreach (var item in DigitalTwin)
        {
            if(item.name.Contains(id.ToString()))
            {
                return item;
            }
        }

        return null;
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

                string data = Encoding.ASCII.GetString(bytes);

                MessageQueue.Message message = new MessageQueue.Message(ID, data);

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
    List<Message> queue = null;

    public MessageQueue()
    {
        queue = new List<Message>();
    }

    public class Message
    {        
        public int ID { get; private set; }
        public string message { get; private set; }
        public Message(int id, string message)
        {
            this.ID = id;
            this.message = message;
        }
    }

    public class DecodedMessage
    {
        public int ID { get; private set; }
        public string[] data {get; private set;}
        public DecodedMessage(int id, string[] data)
        {
            this.ID = id;
            this.data = data;
        }
    }

    public DecodedMessage GetNextMessage()
    {
        if(queue.Count > 0)
        {
            Message returnMessage = queue[0];
            queue.RemoveAt(0);
            // Conver the data into a list of strings.
            string data = returnMessage.message.Trim();
            data.Remove(0); // Remove the first character, which is a '{'
            data.Remove(data.Length - 1); // Remove the last character, which is a '}'
            string[] splitData = data.Split(',');
            return new DecodedMessage(returnMessage.ID, splitData);
        }
        else
        {
            return null;
        }
    }

    public void AddMessage(Message message)
    {
        queue.Add(message);
    }

    public bool MessageAvailable()
    {
        return queue.Count > 0;
    }
}