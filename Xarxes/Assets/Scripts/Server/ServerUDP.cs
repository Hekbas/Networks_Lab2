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
    string serverName;
    string serverText;

    void Start()
    {
        UItext = UItextObj.GetComponent<TextMeshProUGUI>();
        serverName = "Moss Eisley Canteen";
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

    public void startServer()
    {
        serverText = "Starting UDP Server...";

        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(ipep);

        Thread newConnection = new Thread(Receive);
        newConnection.Start();
    }
 
    void Receive()
    {
        int recv = 0;
        byte[] data = new byte[1024];
        
        serverText += "\n" + "Waiting for new Client...";

        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint Remote = (EndPoint)(sender);
        
        while (true)
        {
            // Receive data from the socket
            recv = socket.ReceiveFrom(data, ref Remote);
            string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);

            if (recv == 0)
                break;
            else
            {
                serverText += "\n" + $"Message received from {Remote}: ";
                serverText += Encoding.ASCII.GetString(data, 0, recv);
            }

            //answer
            Thread answer = new Thread(() => Send(Remote));
            answer.Start();
        }

    }

    void Send(EndPoint Remote)
    {
        byte[] data = new byte[1024];
        //data = Encoding.ASCII.GetBytes("General Kenobi!");
        data = Encoding.ASCII.GetBytes($"Successfully connected to: {serverName}");

        socket.SendTo(data, Remote);
    }
}
