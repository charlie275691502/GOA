using Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoomHub.ClientToServer;
using RoomHub.ServerToClient;
using System;

namespace RoomHub
{
    public class S_RoomManager : MonoBehaviour{
        public ServerController ServerController;
        public GameObject S_GamePlayManagerPRF;

        public List<ServerPlayer> ServerPlayers = new List<ServerPlayer>();

        // send //

        public void Send_UpdatePlayerInfos_All(){
            ServerController.SendToAllClient("RoomHub", "UpdatePlayerInfos", new UpdatePlayerInfos(ServerPlayers));
        }

        public void Send_StartGame_All()
        {
            ServerController.SendToAllClient("RoomHub", "StartGame", new StartGame());
        }

        // recieve //

        public void SetNick(int clientSerial, SetNick setNick){
            // known player
            if (ServerPlayers.Exists(serverPlayer => serverPlayer.ClientSerial == clientSerial))
            {
                ServerPlayers.Find(serverPlayer => serverPlayer.ClientSerial == clientSerial).Nick = setNick.Nick;
            }
            else
            // unknown player
            {
                ServerPlayers.Add(new ServerPlayer(clientSerial, setNick.Nick, false, ServerPlayers.Count == 0, false));
            }
            Send_UpdatePlayerInfos_All();
        }

        public void SetReady(int clientSerial, SetReady setReady)
        {
            ServerPlayers.Find(serverPlayer => serverPlayer.ClientSerial == clientSerial).IsReady = setReady.IsReady;
            Send_UpdatePlayerInfos_All();
        }

        public void AddBot(int clientSerial, AddBot addBot)
        {
            ServerPlayers.Add(new ServerPlayer(-1, "Bot" + UnityEngine.Random.Range(1000, 10000), true, false, true));
            Send_UpdatePlayerInfos_All();
        }

        public void RemoveBot(int clientSerial, RemoveBot removeBot)
        {
            ServerPlayers.Remove(ServerPlayers.FindLast(serverPlayer => serverPlayer.IsBot));
            Send_UpdatePlayerInfos_All();
        }

        public void HostStartGame(int clientSerial, HostStartGame hostStartGame)
        {
            int playerSerial = 0;
            //foreach(ServerPlayer serverPlayer in ServerPlayers)
            //    S_GamePlayManager.ServerPlayers.Add(new GamePlayHub.ServerPlayer(serverPlayer.ClientSerial, playerSerial++, serverPlayer.Nick, serverPlayer.IsBot, S_GamePlayManager));

            GameObject gmo = Instantiate(S_GamePlayManagerPRF, transform);
            GamePlayHub.S_GamePlayManager s_GamePlayManager = gmo.GetComponent<GamePlayHub.S_GamePlayManager>();
            s_GamePlayManager.ServerPlayers = new List<GamePlayHub.ServerPlayer>();
            s_GamePlayManager.ServerController = ServerController;
            ServerController.AddSubscriptor(new Subscriptor("GamePlayHub_16", Type.GetType("GamePlayHub.S_GamePlayManager"), s_GamePlayManager));
            while (ServerPlayers.Count > 0)
            {
                int random = UnityEngine.Random.Range(0, ServerPlayers.Count);
                s_GamePlayManager.ServerPlayers.Add(new GamePlayHub.ServerPlayer(ServerPlayers[random].ClientSerial, playerSerial++, ServerPlayers[random].Nick, ServerPlayers[random].IsBot, s_GamePlayManager));
                ServerPlayers.RemoveAt(random);
            }

            ServerPlayers.Clear();
            Send_StartGame_All();
        }

        // process //
    }
}