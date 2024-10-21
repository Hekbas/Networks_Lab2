using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
using System;
using System.Collections.Generic;
//using UnityEngine.tvOS;

public class ServerUDP : MonoBehaviour
{
    Socket socket;
    List<EndPoint> clients = new List<EndPoint>();
    object lockObj = new object();

    public GameObject UItextObj;
    TextMeshProUGUI UItext;

    [SerializeField] private TMP_InputField serverName;
    string serverText;

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

            Thread newConnection = new Thread(Synchronize);
            newConnection.Start();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to start the server: {ex.Message}");
        }
    }

    void Synchronize()
    {
        byte[] data = new byte[1024];
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint Remote = (EndPoint)(sender);

        lock (lockObj)
        {
            serverText += "\n" + "Waiting for new Client...";
        }

        while (true)
        {
            try
            {
                // Receive data from the socket
                int recv = socket.ReceiveFrom(data, ref Remote);
                string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);

                if (recv > 0)
                {
                    lock (lockObj)
                    {
                        serverText += "\n" + $"Message received from {Remote}: {receivedMessage}";
                    }

                    // Check if the client is new, add to the list if so
                    lock (lockObj)
                    {
                        if (!clients.Contains(Remote))
                        {
                            clients.Add(Remote);
                            serverText += "\n" + $"New client connected: {Remote}";
                        }
                    }

                    // Respond to the client
                    Thread answer = new Thread(() => Acknowledgement(Remote));
                    answer.Start();
                }
            }
            catch (SocketException ex)
            {
                lock (lockObj)
                {
                    serverText += "\n" + $"Error receiving data: {ex.Message}";
                }
            }
        }
    }

    void Acknowledgement(EndPoint Remote)
    {
        byte[] data = Encoding.ASCII.GetBytes($"Successfully connected to: {serverName.text}");

        try
        {
            socket.SendTo(data, Remote);
            lock (lockObj)
            {
                serverText += "\n" + $"Sent confirmation to {Remote}";
            }
        }
        catch (SocketException ex)
        {
            lock (lockObj)
            {
                serverText += "\n" + $"Error sending data to {Remote}: {ex.Message}";
            }
        }
    }
}
