using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ClientHandler : MonoBehaviour
{
    [SerializeField] List<NetworkClient> clients = new List<NetworkClient>();
    [SerializeField] List<NetworkClient> uninstantiatedClients = new List<NetworkClient>();
    [SerializeField] List<GameObject> DigitalTwin = new List<GameObject>();
    [SerializeField] GameObject DigitalTwinPrefab;

    public MessageQueue MessageQueue = null;

    private bool isPulsing = false;

    const bool Uninstatiated = false;
    const bool Instatiated = true;

    // Start is called before the first frame update
    void Start()
    {
        MessageQueue = new MessageQueue();

    }

    private void Update()
    {
        if (isPulsing == false)
        {
            StartCoroutine(PulseClients());
        }

    }

    public IEnumerator PulseClients()
    {
        isPulsing = true;
        for (int i = 0; i < clients.Count; i++)
        {
            if (!clients[i].isAlive())
            {
                clients.RemoveAt(i);
                GameObject temp = DigitalTwin[i];
                DigitalTwin.RemoveAt(i);
                Destroy(temp, 1);
                i--;
                GetComponent<ServerScript>().ReduceClientCount();
            }
        }
        yield return new WaitForSeconds(5);
        isPulsing = false;
    }

    public bool AcceptClient(TcpClient client, int ID)
    {
        if(client == null)
        {
            return false;
        }

        uninstantiatedClients.Add(new NetworkClient(client, ID, MessageQueue, Uninstatiated));
        return true;
    }

    public bool hasUninstatiatedClients()
    {
        return uninstantiatedClients.Count > 0;
    }

    enum Direction
    {
        Up = 0,
        Down = 1
    }

    Direction lastDirection = Direction.Up;
    int x = 0;
    int z = 3;

    public void SpawnDigitalTwin()
    {
        List<NetworkClient> clientsToBeRemoved = new List<NetworkClient>();
        foreach (var client in uninstantiatedClients)
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
            obj.name = DigitalTwinPrefab.name + " | " + client.ID; // Makes it a bit easier.
            DigitalTwin.Add(obj);

            client.IsInstatiated();
            clients.Add(client);
        }
        uninstantiatedClients.Clear();
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
    public int ID { get; private set;}
    public bool isUninstatiated { get; private set; }

    TcpClient client;
    MessageQueue messageQueue = null;
    Thread thread = null;

    public NetworkClient(TcpClient client, int ID, MessageQueue queue, bool isUnInstatiated)
    {
        this.client = client;
        this.ID = ID;
        this.messageQueue = queue;
        this.isUninstatiated = isUnInstatiated;

        thread = new Thread(StartCommunication);
        thread.Start();
    }

    public void IsInstatiated()
    {
        isUninstatiated = false;
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

    byte[] heartBeatData = Encoding.ASCII.GetBytes("{\"Action\":\"Heartbeat\"}");

    public bool isAlive()
    {
        int bytes = 0;
        try
        {
            bytes = client.Client.Send(heartBeatData);
        }
        catch (SocketException ex)
        {
            Debug.LogError(ex.Message + " | Client was not formally disconnected.");
            bytes = -1;
        }
       return bytes > 0;
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
            Debug.Log(data);
            data = data.Replace("{", "");
            data = data.Replace("}", "");
            string[] splitData = data.Split(','); // Split the data into the segments
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