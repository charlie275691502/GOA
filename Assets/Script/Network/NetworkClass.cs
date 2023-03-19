using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;

namespace Network{
    public class Client{
        public int Serial;
        public Socket Socket;
        public Queue<Packet> SendPacketQueue;
        public bool Sending;
        public float Time;

        public Client(Socket socket, int serial)
        {
            Socket = socket;
            Serial = serial;
            SendPacketQueue = new Queue<Packet>();
            Sending = false;
            Time = 0.0f;
        }
    }

    public class ClientList
    {
        public List<Client> Clients = new List<Client>();

        int serial = 0;
        public Client AddPlayer(Socket socket)
        {
            Client client = new Client(socket, ++serial);
            Clients.Add(client);
            return client;
        }

        public void RemovePlayer(Client client)
        {
            Clients.Remove(client);
            Debug.Log("removed");
        }

        public Client FindPlayer(int serial)
        {
            foreach (Client client in Clients)
            {
                if (client.Serial == serial)
                    return client;
            }

            return null;
        }

        public Client FindPlayer(Socket socket)
        {
            foreach (Client client in Clients)
            {
                if (client.Socket.RemoteEndPoint.ToString() == socket.RemoteEndPoint.ToString())
                    return client;
            }

            return null;
        }
    }

    public class Packet
    {
        public string Hub;
        public string Target;
        public string JsonString;

        public Packet(string hub, string target, string jsonString)
        {
            Hub = hub;
            Target = target;
            JsonString = jsonString;
        }
    }

    // 寄給server
    public class ServerPacket
    {
        public int ClientSerial;
        public Packet Packet;

        public ServerPacket(int clientSerial, Packet packet)
        {
            ClientSerial = clientSerial;
            Packet = packet;
        }
    }

    public class Subscriptor
    {
        public string Hub;
        public Type ManagerType;
        public object Manager;

        public Subscriptor(string hub, Type managerType, object manager)
        {
            Hub = hub;
            ManagerType = managerType;
            Manager = manager;
        }
    }
}