using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Network;

namespace RoomHub.ServerToClient
{
    public class UpdatePlayerInfos
    {
        public string[] PlayerNicks;
        public bool[] IsReadys;
        public bool[] IsHosts;
        public bool[] IsBots;

        public UpdatePlayerInfos(List<ServerPlayer> serverPlayers)
        {
            PlayerNicks = new string[serverPlayers.Count];
            IsReadys = new bool[serverPlayers.Count];
            IsHosts = new bool[serverPlayers.Count];
            IsBots = new bool[serverPlayers.Count];
            for (int i = 0; i < serverPlayers.Count; i++)
            {
                PlayerNicks[i] = serverPlayers[i].Nick;
                IsReadys[i] = serverPlayers[i].IsReady;
                IsHosts[i] = serverPlayers[i].IsHost;
                IsBots[i] = serverPlayers[i].IsBot;
            }
        }
    }

    public class StartGame
    {

    }
}