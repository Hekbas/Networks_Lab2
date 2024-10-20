using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
using System;
//using UnityEngine.tvOS;

public class ServerUDP : MonoBehaviour
{
    Socket socket;

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
        UItext.text = serverText;
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
        
        serverText = serverText + "\n" + "Waiting for new Client...";

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
                serverText += serverText + "\n" + "Message received from {0}:" + Remote.ToString();
                serverText += serverText + "\n" + Encoding.ASCII.GetString(data, 0, recv);
            }

            //answer
            Thread answer = new Thread(() => Send(Remote));
            answer.Start();
        }

    }

    void Send(EndPoint Remote)
    {
        //TO DO 4
        //Use socket.SendTo to send a ping using the remote we stored earlier.
        byte[] data = new byte[1024];
        data = Encoding.ASCII.GetBytes("General Kenobi!");
        //data = Encoding.ASCII.GetBytes("Server name: " + serverName);

        socket.SendTo(data, Remote);
    }
}
