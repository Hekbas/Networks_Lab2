﻿using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;

public class ClientUDP : MonoBehaviour
{
    private Socket socket;
    private IPEndPoint ipep;

    [SerializeField] private GameObject ui_chat_obj;
    [SerializeField] private GameObject ui_inputfield_obj;
    [SerializeField] private GameObject ui_text_client_obj;
    [SerializeField] private GameObject ui_text_chat_obj;
    TextMeshProUGUI ui_text_client;
    TextMeshProUGUI ui_text_chat;
    string clientText;
    string chatText;

    [SerializeField] private TMP_InputField nickname;
    [SerializeField] private TMP_InputField serverIP;
    [SerializeField] private TMP_InputField chat_message;


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
        clientText = "";
        chatText = "";
        ui_text_client = ui_text_client_obj.GetComponent<TextMeshProUGUI>();
        ui_text_chat = ui_text_chat_obj.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (cs == connection_status.initialize)
        {
            ui_chat_obj.SetActive(true);
            cs = connection_status.connected;
        }

        ui_text_client.text = clientText;
        ui_text_chat.text = chatText;
    }

    private void OnDestroy()
    {
        cs = connection_status.shutdown;
        socket?.Close();
    }

    public void StartClient()
    {
        Thread mainThread = new Thread(Connect);
        mainThread.Start();
    }

    void Connect()
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

        try
        {
            SendConnectionRequest(nickname.text);
        }
        catch (SocketException e)
        {
            clientText += $"Error trying to connect to server {serverIP}: {e}\n";
            throw;
        }

        cs = connection_status.initialize;
        Thread receive = new Thread(Receive);
        receive.Start();
    }

    void Receive()
    {
        EndPoint remote = (EndPoint)ipep;
        byte[] data = new byte[1024];

        while (true)
        {
            int recv = socket.ReceiveFrom(data, ref remote);
            if (recv > 0)
            {
                string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);

                // Handle different message types
                if (receivedMessage.StartsWith("CHAT:"))
                {
                    // Extract chat message and update the chat UI
                    string chatMessage = receivedMessage.Substring("CHAT:".Length);
                    chatText += $"\n {remote} > {chatMessage}";
                }
                else
                {
                    // General message (e.g., connection acknowledgment)
                    clientText += "\n" + receivedMessage;
                }
            }
        }
    }

    public void SendConnectionRequest(string playerName)
    {
        // Prefix connection requests with "CONNECT:"
        string connectionRequest = "CONNECT:" + playerName;

        byte[] data = Encoding.ASCII.GetBytes(connectionRequest);
        socket.SendTo(data, ipep);
    }


    // ----------- Loby chat ----------- //
    public void SendChatMessage(string message, string sender = null)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        // Prefix chat messages with "CHAT:"
        string s = "CHAT:" + message;

        byte[] data = Encoding.ASCII.GetBytes(s);
        socket.SendTo(data, ipep);
    }

    public void SendMessageToServer()
    {
        SendChatMessage(chat_message.text, nickname.text);
        Debug.Log(this.chat_message.text);
    }

}