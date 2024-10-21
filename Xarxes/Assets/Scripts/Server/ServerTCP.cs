using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TMPro;
using System.Text;
using System;

public class ServerTCP : MonoBehaviour
{
    Socket socket;
    Thread mainThread = null;

    public GameObject UItextObj;
    TextMeshProUGUI UItext;
    string serverText;

    public struct User
    {
        public string name;
        public Socket socket;
    }

    void Start()
    {
        UItext = UItextObj.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        UItext.text = serverText;
    }

    public void startServer()
    {
        serverText = "Starting TCP Server...";

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        IPEndPoint localEp = new IPEndPoint(IPAddress.Any, 9050);
        socket.Bind(localEp);

        socket.Listen(10);

        mainThread = new Thread(CheckNewConnections);
        mainThread.Start();
    }

    void CheckNewConnections()
    {
        while(true)
        {
            User newUser = new User();
            newUser.name = "";

            newUser.socket = socket.Accept();

            IPEndPoint clientep = (IPEndPoint)newUser.socket.RemoteEndPoint;
            serverText = serverText + "\n"+ "Connected with " + clientep.Address.ToString() + " at port " + clientep.Port.ToString();

            Thread newConnection = new Thread(() => Receive(newUser));
            newConnection.Start();
        }
        //This users could be stored in the future on a list
        //in case you want to manage your connections

    }

    void Receive(User user)
    {
        byte[] data = new byte[1024];
        int recv = 0;

        while (true)
        {
            data = new byte[1024];
            recv = user.socket.Receive(data);

            if (recv == 0)
                break;
            else
            {
                serverText = serverText + "\n" + Encoding.ASCII.GetString(data, 0, recv);
            }

            Thread answer = new Thread(() => Send(user));
            answer.Start();
        }
    }

    void Send(User user)
    {
        byte[] data = Encoding.ASCII.GetBytes("Pong");
        user.socket.Send(data);
    }
}
