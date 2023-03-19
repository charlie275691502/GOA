using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using Main;
using GamePlayHub;

namespace Network{
    public delegate void BeforeNextStageDelegate(object o);

    public class ClientController : MonoBehaviour{

        private Socket _ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private byte[] _RecieveBuffer = new byte[65536];

        public Queue<Packet> ReceivePacketQueue = new Queue<Packet>();
        public Queue<Packet> SendPacketQueue = new Queue<Packet>();
        List<Subscriptor> _Subscriptors;


        bool _Receiving = false;
        bool _Sending = false;
        float _Time = 0.0f;

        private void Start()
        {
            _Subscriptors = new List<Subscriptor>();
        }

        private void Update()
        {
            // calculate ReceivePacketQueue
            while (ReceivePacketQueue.Count > 0 && !_Receiving)
            {
                _Receiving = true;
                Packet packet = ReceivePacketQueue.Dequeue();
                string hubHeadName = packet.Hub.Split('_')[0];
                object[] obj = new object[1] { JsonUtility.FromJson(packet.JsonString, System.Type.GetType(hubHeadName + ".ServerToClient." + packet.Target)) };

                Subscriptor subscriptor = _Subscriptors.Find(sub => sub.Hub == packet.Hub);
                subscriptor.ManagerType.GetMethod(packet.Target).Invoke(subscriptor.Manager, obj);
            }

            while (SendPacketQueue.Count > 0 && !_Sending)
            {
                _Sending = true;
                _Time = 0.0f;
                Packet packet = SendPacketQueue.Dequeue();
                SendData(JsonUtility.ToJson(packet), true);
            }

            if(_Sending) _Time += Time.deltaTime;
            if (_Time >= 5.0f)
            {
                LogMaster.GetInstance().AddComponent("與伺服器連線中斷");
                LogMaster.GetInstance().Log();
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

        public void NextPacket()
        {
            _Receiving = false;
        }

        public IEnumerator WaitSecNextPacket(float waitTime, BeforeNextStageDelegate beforeNextStageDelegate, object delegatePara)
        {
            yield return new WaitForSeconds(waitTime);
            if(beforeNextStageDelegate != null) beforeNextStageDelegate(delegatePara);
            NextPacket();
        }

        void OnApplicationQuit()
        {
            CloseConnection();
        }

        public void StartConnection(string ip, int port)
        {
            try
            {
                _ClientSocket.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), new AsyncCallback(OnConnect), _ClientSocket);
            }
            catch (SocketException ex)
            {
                Debug.Log(ex.Message);
            }
        }
        private void SendData(byte[] data)
        {
            SocketAsyncEventArgs socketAsyncData = new SocketAsyncEventArgs();
            socketAsyncData.SetBuffer(data, 0, data.Length);
            _ClientSocket.SendAsync(socketAsyncData);
        }

        private void OnConnect(IAsyncResult iar)
        {
            Debug.Log("On Server Connected");
            _ClientSocket.BeginReceive(_RecieveBuffer, 0, _RecieveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
        }

        public void SendPacket(string hub, string target, object obj) {
            Packet packet = new Packet(hub, target, JsonUtility.ToJson(obj));
            SendPacketQueue.Enqueue(packet);
        }

        private void SendData(string jsonString, bool log){
            if(log) Debug.Log("client send data : " + jsonString);
            byte[] data = System.Text.Encoding.ASCII.GetBytes(jsonString);
            try
            {
                SendData(data);
            }
            catch (SocketException ex)
            {
                Debug.LogWarning(ex.Message);
            }
        }

        /// 接收封包.
        private void ReceiveCallback(IAsyncResult AR)
        {
            int recieved = _ClientSocket.EndReceive(AR);
            if (recieved <= 0)
                return;
            byte[] recData = new byte[recieved];
            Buffer.BlockCopy(_RecieveBuffer, 0, recData, 0, recieved);

            _ClientSocket.BeginReceive(_RecieveBuffer, 0, _RecieveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);

            // decode
            string jsonString = System.Text.Encoding.ASCII.GetString(recData);
            Packet packet = JsonUtility.FromJson<Packet>(jsonString);

            if (packet.Hub == "Network")
            {
                _Sending = false;
                return;
            }

            if (packet.Hub == "Disconnect")
            {
                LogMaster.GetInstance().AddComponent("Client " + packet.JsonString + " 與伺服器連線中斷");
                LogMaster.GetInstance().Log();
                return;
            }

            Debug.Log("client received data : " + packet.JsonString);
            SendData(JsonUtility.ToJson(new Packet("Network", "", "")), false);


            // save to packet queue
            ReceivePacketQueue.Enqueue(packet);
        }

        /// 關閉 Socket 連線.
        public void CloseConnection()
        {
            _ClientSocket.Shutdown(SocketShutdown.Both);
            _ClientSocket.Close();
        }

    }
}
    