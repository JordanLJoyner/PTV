using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;

//https://forum.unity.com/threads/listen-to-port-for-another-app.348967/
public class MyNetworkClass {
    public class PacketMessage : MessageBase {
        public string messageType;
        public string payload;
    }

    /// <summary>
    /// Point this to your own handler to process messages
    /// </summary>
    public Action<PacketMessage> HandleMessage;
    
    private Thread ListenerThread = null;
    private bool KeepListening = true;
    int port = 8888;
    public MyNetworkClass() {
        HandleMessage = (p) =>
        {
            Debug.Log("Did not handle message: " + p.messageType);
        };

        ListenerThread = new Thread(ListenWorker);
        ListenerThread.Start();
    }

    private void ListenWorker() {
        KeepListening = true;
        var dataBuffer = new StringBuilder();
        var receiveBuffer = new byte[1024];

        // Set up a local socket for listening
        using (var localSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
            // Set up an endpoint and start listening
            var localEndpoint = new IPEndPoint(IPAddress.Any, port);
            localSocket.Bind(localEndpoint);
            localSocket.Listen(10);
            Debug.Log("Socket Standby....");

            while (KeepListening) {
                try {
                    // Clear input buffer (Assumption: messages are always string data)
                    dataBuffer.Remove(0, dataBuffer.Length);

                    // This call will block until we get a message. Using Async methods
                    // will have better performance, but this is simpler
                    var remoteSocket = localSocket.Accept();
                    Debug.Log("Socket Connected.");

                    // Connect to the remote client and receive the message as text
                    var remoteEndpoint = (IPEndPoint)remoteSocket.RemoteEndPoint;
                    var receiveStream = new NetworkStream(remoteSocket);
                    while (receiveStream.DataAvailable) {
                        receiveStream.Read(receiveBuffer, 0, receiveBuffer.Length);
                        var data = Encoding.UTF8.GetString(receiveBuffer);
                        dataBuffer.Append(data);
                    }
                    
                    var test = dataBuffer.ToString();
                    Debug.Log(test);
                    // Here we assume the remote client is sending us JSON data that describes
                    // a PacketMessage object.  Deserialize the Json and call our custom handler
                    var message = JsonUtility.FromJson<PacketMessage>(dataBuffer.ToString());
                    HandleMessage(message);
                } catch (Exception e) {
                    // report errors and keep listening.
                    Debug.Log("Network Error: " + e.Message);

                    // Sleep 5 seconds so that we don't flood the output with errors
                    Thread.Sleep(5000);
                }
            }
        }
    }
}