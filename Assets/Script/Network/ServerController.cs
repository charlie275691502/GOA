using Main;
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using GamePlayHub;

namespace Network{
    public class ServerController : MonoBehaviour
    {
        private Socket _ServerSocket;//伺服器本身的Socket
        private Thread _ThreadConnect;//連線的Thread
        private Thread _ThreadReceive;//接收資料的Thread
        private byte[] _RecieveBuffer = new byte[65536];

        public Queue<ServerPacket> ServerPacketQueue = new Queue<ServerPacket>();

        public GameController GameController;
        List<Subscriptor> _Subscriptors = new List<Subscriptor>();

        ClientList _ClientList = new ClientList();

        private void Update()
        {
            // calculate ServerPacketQueue
            while (ServerPacketQueue.Count > 0)
            {
                ServerPacket serverPacket = ServerPacketQueue.Dequeue();
                string hubHeadName = serverPacket.Packet.Hub.Split('_')[0];
                object[] obj = new object[0];
                if (hubHeadName == "RoomHub")
                    obj = new object[2] { serverPacket.ClientSerial, JsonUtility.FromJson(serverPacket.Packet.JsonString, System.Type.GetType(hubHeadName + ".ClientToServer." + serverPacket.Packet.Target)) };
                if (hubHeadName == "GamePlayHub")
                    obj = new object[2] { ((S_GamePlayManager)_Subscriptors.Find(sub => sub.Hub == serverPacket.Packet.Hub).Manager).GetPlayerSerialFromClientSerial(serverPacket.ClientSerial), JsonUtility.FromJson(serverPacket.Packet.JsonString, System.Type.GetType(hubHeadName + ".ClientToServer." + serverPacket.Packet.Target)) };

                Subscriptor subscriptor = _Subscriptors.Find(sub => sub.Hub == serverPacket.Packet.Hub);
                subscriptor.ManagerType.GetMethod(serverPacket.Packet.Target).Invoke(subscriptor.Manager, obj);

            }

            foreach (Client client in _ClientList.Clients)
            {
                while (client.SendPacketQueue.Count > 0 && !client.Sending)
                {
                    client.Sending = true;
                    client.Time = 0.0f;
                    Packet packet = client.SendPacketQueue.Dequeue();
                    SendData(JsonUtility.ToJson(packet), client.Socket, true);
                }
            }
            foreach (Client client in _ClientList.Clients)
            {
                if (client.Sending) client.Time += Time.deltaTime;
                if (client.Time >= 5.0f)
                    foreach(Client client2 in _ClientList.Clients)
                        SendData(JsonUtility.ToJson(new Packet("Disconnect", "", client.Serial.ToString())), client2.Socket, false);
            }
        }

        public void AddSubscriptor(Subscriptor subscriptor)
        {
            _Subscriptors.Add(subscriptor);
        }

        public void RemoveSubscriptor(string hub)
        {
            _Subscriptors.Remove(_Subscriptors.Find(subscriptor => subscriptor.Hub == hub));
        }

        void OnApplicationQuit()
        {
            Debug.Log("Application ending after " + Time.time + " seconds");
            StopServer();
        }

        public void StartServer(string ip, int port)
        {
            _ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//new server socket object
            _ServerSocket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
            _ServerSocket.Listen(16);//最多一次接受多少人連線
            _ThreadConnect = new Thread(Accept);
            _ThreadConnect.IsBackground = true;//設定為背景執行續，當程式關閉時會自動結束
            _ThreadConnect.Start();
            Debug.Log("Server Start");


        }

        public void StopServer()
        {
            foreach (Client client in _ClientList.Clients)
            {
                try
                {
                    client.Socket.Close();
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.ToString());
                }
            }

            _ServerSocket.Close();
        }

        private void Accept()
        {
            try
            {
                Socket clientSocket = _ServerSocket.Accept(); // blocking
                Client client = _ClientList.AddPlayer(clientSocket);
                Debug.Log("Accept: " + clientSocket.RemoteEndPoint.ToString());
                clientSocket.BeginReceive(_RecieveBuffer, 0, _RecieveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), client);

                _ThreadConnect = new Thread(Accept);
                _ThreadConnect.IsBackground = true;
                _ThreadConnect.Start();

            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
            }

        }
        public void SendToAllClient(string hub, string target, object obj)
        {
            Packet packet = new Packet(hub, target, JsonUtility.ToJson(obj));
            foreach (Client client in _ClientList.Clients)
                client.SendPacketQueue.Enqueue(packet);
        }

        public void SendData(string hub, string target, object obj, int clientSerial)
        {
            Packet packet = new Packet(hub, target, JsonUtility.ToJson(obj));
            _ClientList.FindPlayer(clientSerial).SendPacketQueue.Enqueue(packet);
        }

        private void SendData(string jsonString, Socket clientSocket, bool log)
        {
            byte[] data = System.Text.Encoding.ASCII.GetBytes(jsonString);
            if(log) Debug.Log("server send data : " + jsonString);
            SocketAsyncEventArgs socketAsyncData = new SocketAsyncEventArgs();
            socketAsyncData.SetBuffer(data, 0, data.Length);
            clientSocket.SendAsync(socketAsyncData);
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            Client client = (Client)AR.AsyncState;
            Socket clientSocket = client.Socket;
            int recieved = clientSocket.EndReceive(AR);
            if (recieved <= 0)
                return;

            byte[] recData = new byte[recieved];
            Buffer.BlockCopy(_RecieveBuffer, 0, recData, 0, recieved);

            clientSocket.BeginReceive(_RecieveBuffer, 0, _RecieveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), client);

            string jsonString = System.Text.Encoding.ASCII.GetString(recData);
            Packet packet = JsonUtility.FromJson<Packet>(jsonString);

            if (packet.Hub == "Network")
            {
                _ClientList.FindPlayer(clientSocket).Sending = false;
                return;
            }

            Debug.Log("server received data : " + jsonString);
            SendData(JsonUtility.ToJson(new Packet("Network", "", "")), clientSocket, false);

            // save to serverPacket queue
            ServerPacketQueue.Enqueue(new ServerPacket(_ClientList.FindPlayer(clientSocket).Serial, packet));
        }
    }
}