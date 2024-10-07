using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
using System;
using UnityEngine.tvOS;

public class ServerUDP : MonoBehaviour
{
    Socket socket;

    public GameObject UItextObj;
    TextMeshProUGUI UItext;
    string serverText;

    void Start()
    {
        UItext = UItextObj.GetComponent<TextMeshProUGUI>();

    }
    public void startServer()
    {
        serverText = "Starting UDP Server...";

        //TO DO 1
        //UDP doesn't keep track of our connections like TCP
        //This means that we "can only" reply to other endpoints,
        //since we don't know where or who they are
        //We want any UDP connection that wants to communicate with 9050 port to send it to our socket.
        //So as with TCP, we create a socket and bind it to the 9050 port. 

        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        //TO DO 3
        //Our client is sending a handshake, the server has to be able to recieve it
        //It's time to call the Receive thread
        Thread newConnection = new Thread(Receive);
        newConnection.Start();
    }

    void Update()
    {
        UItext.text = serverText;

    }

 
    void Receive()
    {
        int recv;
        byte[] data = new byte[1024];
        
        serverText = serverText + "\n" + "Waiting for new Client...";

        //TO DO 3
        //We don't know who may be comunicating with this server, so we have to create an
        //endpoint with any address and an IpEndpoint from it to reply to it later.
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint Remote = (EndPoint)(sender);

        //Loop the whole process, and start receiveing messages directed to our socket
        //(the one we binded to a port before)
        //When using socket.ReceiveFrom, be sure send our remote as a reference so we can keep
        //this adress (the client) and reply to it on TO DO 4
        
        while (true)
        {

            //serverText = serverText + "\n" + "Message received from {0}:" + Remote.ToString();
            //serverText = serverText + "\n" + Encoding.ASCII.GetString(data, 0, recv);

            //TO DO 4
            //When our UDP server receives a message from a random remote, it has to send a ping,
            //Call a send thread


            try
            {
                // Receive data from the socket
                recv = socket.ReceiveFrom(data, ref Remote);
                string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);

                // Log the received message
                serverText += $"\nMessage received from {Remote.ToString()}: {receivedMessage}";

                // TO DO 4: Send a response (ping) back to the client
                string responseMessage = "Ping received!";
                byte[] responseData = Encoding.ASCII.GetBytes(responseMessage);

                // Send the response back to the sender
                socket.SendTo(responseData, Remote);
                serverText += $"\nResponse sent to {Remote.ToString()}: {responseMessage}";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while receiving data: " + ex.Message);
                break; // Exit the loop on error
            }
        }

    }

    void Send(EndPoint Remote)
    {
        //TO DO 4
        //Use socket.SendTo to send a ping using the remote we stored earlier.
        byte[] data = new byte[1024];
        string welcome = "Ping";

    }

   
}
