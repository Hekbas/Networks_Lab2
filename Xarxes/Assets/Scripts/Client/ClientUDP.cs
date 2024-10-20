using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;

public class ClientUDP : MonoBehaviour
{
    Socket socket;
    public GameObject UItextObj;
    TextMeshProUGUI UItext;
    string clientText;

    // Start is called before the first frame update
    void Start()
    {
        UItext = UItextObj.GetComponent<TextMeshProUGUI>();

    }

    void Update()
    {
        UItext.text = clientText;
    }

    private void OnDestroy()
    {
        socket?.Close();
    }

    public void StartClient()
    {
        Thread mainThread = new Thread(Send);
        mainThread.Start();
    }

    void Send()
    {
        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        string handshake = "Hello there";
        byte[] data = new byte[1024];
        data = Encoding.ASCII.GetBytes(handshake);

        socket.SendTo(data, SocketFlags.None, ipep);

        Thread receive = new Thread(Receive);
        receive.Start();

    }

    void Receive()
    {
        Debug.Log("Recieve!");
        IPEndPoint sender = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
        EndPoint Remote = (EndPoint)(sender);
        byte[] data = new byte[1024];
        int recv = socket.ReceiveFrom(data, ref Remote);

        clientText += ("Message received from {0}: " + Remote.ToString());
        clientText += "\n" + Encoding.ASCII.GetString(data, 0, recv);
    }
}