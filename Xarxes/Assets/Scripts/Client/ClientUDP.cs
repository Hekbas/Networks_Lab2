using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;

public class ClientUDP : MonoBehaviour
{
    private Socket socket;
    private IPEndPoint ipep;

    [SerializeField] private GameObject ui_text_obj;
    [SerializeField] private GameObject ui_inputfield_obj;

    private TextMeshProUGUI ui_text;
    private string clientText;

    [SerializeField] private GameObject ui_chat_obj;
    [SerializeField] private TMP_InputField nickname;
    [SerializeField] private TMP_InputField serverIP;
    [SerializeField] private TMP_InputField chat_message;
    [SerializeField] private TMP_Text online_players;


    enum connection_status
    {
        initialize,
        connected,
        shutdown,
        disconnected
    }

    connection_status cs = connection_status.disconnected;

    void Start()
    {
        ui_text = ui_text_obj.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (cs == connection_status.initialize)
        {
            ui_chat_obj.SetActive(true);
            cs = connection_status.connected;
        }

        ui_text.text = clientText;
    }

    private void OnDestroy()
    {
        cs = connection_status.shutdown;
        socket?.Close();
    }

    public void StartClient()
    {
        Thread mainThread = new Thread(Synchronize);
        mainThread.Start();
    }

    void Synchronize()
    {
        try
        {
            ipep = new IPEndPoint(IPAddress.Parse(serverIP.text), 9050);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }
        catch (SocketException e)
        {
            clientText += $"Error trying to connect to server {serverIP}: {e}\n";
            throw;
        }

        string handshake = nickname.text;
        byte[] data = new byte[1024];
        data = Encoding.ASCII.GetBytes(handshake);

        try
        {
            socket.SendTo(data, SocketFlags.None, ipep);
        }
        catch (SocketException e)
        {
            clientText += $"Error trying to connect to server {serverIP}: {e}\n";
            throw;
        }

        cs = connection_status.initialize;
        Thread receive = new Thread(Acknowledgement);
        receive.Start();
    }

    void Acknowledgement()
    {
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint Remote = (EndPoint)(sender);
        byte[] data = new byte[1024];

        try
        {
            while (cs != connection_status.shutdown)
            {
                int recv = socket.ReceiveFrom(data, ref Remote);
                if (recv > 0)
                {
                    string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);
                    clientText += $"\nMessage received from {Remote}: {receivedMessage}";
                }
            }
        }
        catch (SocketException e)
        {
            clientText += $"Error receiving data: {e}\n";
        }
        finally
        {
            socket?.Close();
        }
    }

    // ----------- Loby chat ----------- //
    public void SendChatMessage(string message, string sender = null)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        string s = sender + ": " + message;

        byte[] data = Encoding.ASCII.GetBytes(message);
        socket.SendTo(data, ipep);
    }

    public void SendMessageToServer()
    {
        SendChatMessage(chat_message.text, nickname.text);
        Debug.Log(this.chat_message.text);
    }
}