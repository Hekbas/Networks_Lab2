using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;

public class ClientUDP : MonoBehaviour
{
    private Socket socket;
    [SerializeField] private GameObject ui_text_obj;
    private TextMeshProUGUI ui_text;
    [SerializeField] public GameObject ui_inputfield_obj;
    string ui_serverIP;
    string clientText;


    void Start()
    {
        ui_text = ui_text_obj.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        ui_text.text = clientText;
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
        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ui_serverIP.ToString()), 9050);

        try
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }
        catch (SocketException e)
        {
            clientText += $"Error trying to connect to server {ui_serverIP}: {e}\n";
            throw;
        }

        string handshake = "Hello there";
        byte[] data = new byte[1024];
        data = Encoding.ASCII.GetBytes(handshake);

        try
        {
            socket.SendTo(data, SocketFlags.None, ipep);
        }
        catch (SocketException e)
        {
            clientText += $"Error trying to connect to server {ui_serverIP}: {e}\n";
            throw;
        }

        Thread receive = new Thread(Receive);
        receive.Start();

    }

    void Receive()
    {
        IPEndPoint sender = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
        EndPoint Remote = (EndPoint)(sender);
        byte[] data = new byte[1024];
        int recv = socket.ReceiveFrom(data, ref Remote);

        clientText += $"Message received from {Remote}: ";
        clientText += Encoding.ASCII.GetString(data, 0, recv) + "\n";
    }

    public void ReadInput(string s)
    {
        ui_serverIP = s;
        Debug.Log(ui_serverIP);
    }
}