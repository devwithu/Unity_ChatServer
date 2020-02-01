using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using System.Net;
using System.IO;

public class Server : MonoBehaviour
{
    private List<ServerClient> clients;
    private List<ServerClient> disconnectList;

    public int port = 6321;
    private TcpListener server;
    private bool serverStarted;

    


    // Start is called before the first frame update
    void Start()
    {
        clients = new List<ServerClient>();
        disconnectList = new List<ServerClient>();

        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            StartListening();
            serverStarted = true;
            Debug.Log("Server has been stated on port " + port);
        }
        catch(Exception e)
        {
            Debug.Log("Socket error: " + e.Message);
        }
    }

    private void StartListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient,server);
    }

    private void AcceptTcpClient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener) ar.AsyncState;
        clients.Add(new ServerClient(listener.EndAcceptTcpClient(ar)));
        StartListening();

        // Send a message to everyone , day someone has connected
        //Broadcast(clients[clients.Count-1].clientName + " has connected", clients);
        Broadcast("%NAME", new List<ServerClient>() { clients[clients.Count - 1]});
    }
    // Update is called once per frame
    void Update()
    {
        if(!serverStarted)
            return;

        foreach(ServerClient c in clients)
        {
            // IsInvoking the client still connected:
            if(!isConnected(c.tcp))
            {
                c.tcp.Close();
                disconnectList.Add(c);
                continue;
            }
            else {
                try{
                    NetworkStream s = c.tcp.GetStream();
                
                    if(s.DataAvailable) {
                        StreamReader reader = new StreamReader(s, true);
                        string data = reader.ReadLine();

                        if(data != null) {
                            OnIncomingData(c, data);
                        }
                    }
                }catch (ObjectDisposedException e){
                    c.tcp.Close();
                    disconnectList.Add(c);
                    Debug.Log("c.tcp.GetStream() error : " + e.Message);
                    continue;
                }catch (Exception e) {
                    c.tcp.Close();
                    disconnectList.Add(c);
                    Debug.Log("c.tcp.GetStream() error : " + e.Message);
                    continue;
                }

            }
            // check for maeedage from the client
        }

        for(int i = 0; i < disconnectList.Count -1; i++) {
            Broadcast(disconnectList[i].clientName + " has disconnected" , clients);
            
            clients.Remove(disconnectList[i]);
            disconnectList.RemoveAt(i);
        }
    }

    private void OnIncomingData(ServerClient c, string data) {

        if (data.Contains("&NAME|")) {
            c.clientName = data.Split('|')[1];
            Broadcast(c.clientName + " has connected", clients);
            return;
        }
        //Debug.Log(c.clientName + " has send the following message : " + data);
        Broadcast(c.clientName + " : " + data, clients);
    }
    private void Broadcast(string data, List<ServerClient> cl) {

        foreach(ServerClient c in cl) {
            try {
                StreamWriter writer = new StreamWriter(c.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();

            } catch(Exception e) {
                Debug.Log("Write error : " + e.Message + "to client " + c.clientName);
            }
        }
    }
    private bool isConnected(TcpClient c)
    {
        try
        {
            if(c != null && c.Client != null && c.Client.Connected)
            {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                {
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0); 

                }
                return true;
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}



public class ServerClient{
    public TcpClient tcp;
    public string clientName;
    public ServerClient(TcpClient clientSocket)
    {
        clientName = "Guest";
        tcp = clientSocket;
    }
}