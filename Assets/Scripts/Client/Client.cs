using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System;
using System.Net;
using System.IO;

public class Client : MonoBehaviour
{
    public GameObject chatContainer;
    public GameObject messagePrefab;

    public string clientName;

    private bool socketReady;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamReader reader;
    private StreamWriter writer;

    public void ConnectToServer() {
        // If aleady connected, ignore this function
        if(socketReady)
            return;
        // Default host/ port values
        string host = "127.0.0.1";
        int port = 6321;

        string h;
        int p;
        h = GameObject.Find("HostInput").GetComponent<InputField>().text;
        if (h != "")
            host = h;
        int.TryParse(GameObject.Find("PortInput").GetComponent<InputField>().text, out p);
        if (p != 0)
            port = p;
        
        // create socket
        try {

            socket = new TcpClient(host,port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);

            socketReady = true;

        }catch(Exception e){
            Debug.Log("Socket error : " + e.Message);
        }

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(socketReady) {
            if(stream.DataAvailable) {
                string data = reader.ReadLine();
                if(data != null)
                    OnIncomingData(data);
            }
        }
    }
    private void OnIncomingData(string data){

        if(data == "%NAME") {
            Send("&NAME|" + clientName);
            return;
        }
        GameObject go = Instantiate(messagePrefab,chatContainer.transform);
        go.GetComponentInChildren<Text>().text = data;
    }

    private void Send(string data) {
        if(!socketReady)
            return;
        
        writer.WriteLine(data);
        writer.Flush();
    }

    public void OnSendButton() {
        string message = GameObject.Find("SendInput").GetComponent<InputField>().text;
        Send(message);
    }

    public void CloseSocket() {
        if(!socketReady)
            return;
        
        writer.Close();
        reader.Close();
        socket.Close();
        socketReady = false; 

    }

    private void OnApplicationQuit() {
        CloseSocket();
    }

    void OnDisable()
    {
        CloseSocket();
    }
}
