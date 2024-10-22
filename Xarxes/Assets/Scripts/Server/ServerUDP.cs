using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;


public class ServerUDP : MonoBehaviour
{
    Socket socket;
    List<Client> players = new List<Client>();
    object lockObj = new object();

    public GameObject UItextObj;
    TextMeshProUGUI UItext;

    [SerializeField] private TMP_InputField serverName;
    string serverText;

    struct Client
    {
        public EndPoint ep;
        public string name;
    }


    void Start()
    {
        UItext = UItextObj.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        lock (lockObj)
        {
            UItext.text = serverText;
        }
    }

    private void OnDestroy()
    {
        socket?.Close();
    }

    public void StartServer()
    {
        Debug.Log("Starting Server...");
        try
        {
            if (string.IsNullOrEmpty(serverName.text))
            {
                serverText = "Server name is not assigned. Please set a server name before starting the server.";
                throw new Exception("Server name is not assigned. Please set a server name before starting the server.");
            }

            serverText = $"Starting UDP Server... Server name: {serverName.text}";

            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(ipep);

            Thread newConnection = new Thread(Receive);
            newConnection.Start();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to start the server: {ex.Message}");
        }
    }


    void Receive()
    {
        byte[] data = new byte[1024];
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint remote = (EndPoint)(sender);

        lock (lockObj)
        {
            serverText += "\n" + "Waiting for new Client...";
        }

        while (true)
        {
            try
            {
                // Receive data from the socket
                int recv = socket.ReceiveFrom(data, ref remote);
                string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);

                if (recv > 0)
                {
                    lock (lockObj)
                    {
                        serverText += $"\nMessage received from {remote}: {receivedMessage}";
                    }

                    // Check message type (CONNECT or CHAT)
                    if (receivedMessage.StartsWith("CONNECT:"))
                    {
                        // Extract player name
                        string playerName = receivedMessage.Substring("CONNECT:".Length);

                        // Check if client is already in the list, if not, add to the list
                        lock (lockObj)
                        {
                            if (!CheckConnectedPlayers(remote, playerName))
                            {
                                players.Add(new Client { ep = remote, name = playerName });
                                serverText += $"\nNew client connected: {playerName} ({remote})";
                            }
                        }

                        // Acknowledge connection
                        SendToClient($"Welcome, {playerName}!", remote);
                    }
                    else if (receivedMessage.StartsWith("CHAT:"))
                    {
                        // Extract chat message
                        string chatMessage = receivedMessage.Substring("CHAT:".Length);

                        // Broadcast the chat message to all clients
                        BroadcastMessage(chatMessage, remote);
                    }
                }
            }
            catch (SocketException ex)
            {
                lock (lockObj)
                {
                    serverText += $"\nError receiving data: {ex.Message}";
                }
            }
        }
    }

    void Send(EndPoint Remote)
    {
        byte[] data = Encoding.ASCII.GetBytes($"Successfully connected to: {serverName.text}");

        try
        {
            socket.SendTo(data, Remote);
            lock (lockObj)
            {
                serverText += $"\nSent confirmation to {Remote}";
            }
        }
        catch (SocketException ex)
        {
            lock (lockObj)
            {
                serverText += $"\nError sending data to {Remote}: {ex.Message}";
            }
        }
    }

    void SendToClient(string message, EndPoint remote)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        socket.SendTo(data, remote);
    }

    void BroadcastMessage(string message, EndPoint sender)
    {
        lock (lockObj)
        {
            byte[] data = Encoding.ASCII.GetBytes($"CHAT:{message}");

            foreach (var player in players)
            {
                try
                {
                    socket.SendTo(data, player.ep);
                    serverText += $"\n[{sender}]: {message}";
                }
                catch (SocketException ex)
                {
                    serverText += $"\nError sending data to {player}: {ex.Message}";
                }
            }
        }
    }

    bool CheckConnectedPlayers(EndPoint endpoint, string name)
    {
        return players.Any(client => client.ep.Equals(endpoint) && client.name == name);
    }
}
