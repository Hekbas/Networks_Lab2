using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
//using UnityEngine.tvOS;

public class ClientTCP : MonoBehaviour
{
    public GameObject UItextObj;
    TextMeshProUGUI UItext;
    string clientText;
    Socket server;

    void Start()
    {
        UItext = UItextObj.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        UItext.text = clientText;
    }

    public void StartClient()
    {
        Thread connect = new Thread(Connect);
        connect.Start();
    }

    void Connect()
    {
        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);
        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try{
            server.Connect(ipep);
        }
        catch (SocketException e){
            clientText += "\nError trying to connect to server.";
            return;
        }

        Thread sendThread = new Thread(Send);
        sendThread.Start();

        Thread receiveThread = new Thread(Receive);
        receiveThread.Start();

    }

    void Send()
    {
        clientText += "\n Sending message...";
        byte[] data = Encoding.ASCII.GetBytes(clientText);

        server.Send(data);
    }

    void Receive()
    {
        byte[] data = new byte[1024];
        int recv = 0;

        recv = server.Receive(data);

        clientText += "\n" + Encoding.ASCII.GetString(data, 0, recv);
    }
}
