using GamePlayHub;
using RoomHub;
using LobbyHub;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Network;
using System;

namespace Main
{
    public enum Phase
    {
        ConnectionSetup,
        Lobby,
        Room,
        GamePlay,
        Summary
    }

    public delegate void OnNextPhase();

    public class GameController : MonoBehaviour
    {
        public Network.ServerController ServerController;
        public Network.ClientController ClientController;

        public Phase NowPhase;
        public Transform CanvasFolder;
        public GameObject ConnectionSetupGMO;
        public GameObject RoomGMO;
        public GameObject LobbyGMO;
        public GameObject SummaryGMO;
        public RoomManager RoomManager;
        public GameObject GamePlayManagerPRF;
        public LobbyManager LobbyManager;
        public S_RoomManager S_RoomManager;
        public S_LobbyManager S_LobbyManager;
        public InputField IP;

        [HideInInspector] public bool isHost;

        private void Awake()
        {
            RoomManager.OnNextPhase += () =>
            {
                ChangePhase(Phase.GamePlay);
            };
            ChangePhase(Phase.ConnectionSetup);
        }

        private void Start()
        {
            LogMaster.GetInstance().View.SetActive(false);
            isHost = false;
        }

        public void StartServer(){
            isHost = true;
            ServerController.gameObject.SetActive(true);
            ServerController.StartServer((IP.text == "") ? "127.0.0.1" : IP.text, 6805);
            ServerController.AddSubscriptor(new Subscriptor("LobbyHub", Type.GetType("LobbyHub.S_LobbyManager"), S_LobbyManager));
            ServerController.AddSubscriptor(new Subscriptor("RoomHub_12", Type.GetType("RoomHub.S_RoomManager"), S_RoomManager));
            ConnectedToServer();
        }

        public void ConnectedToServer()
        {
            ClientController.StartConnection((IP.text == "") ? "127.0.0.1" : IP.text, 6805);
            ClientController.AddSubscriptor(new Subscriptor("LobbyHub", Type.GetType("LobbyHub.LobbyManager"), LobbyManager));
            ClientController.AddSubscriptor(new Subscriptor("RoomHub", Type.GetType("RoomHub.RoomManager"), RoomManager));
            ChangePhase(Phase.Room);
        }

        public void ChangePhase(Phase toPhase)
        {
            NowPhase = toPhase;
            ConnectionSetupGMO.SetActive(NowPhase == Phase.ConnectionSetup);

            LobbyGMO.SetActive(NowPhase == Phase.Lobby);
            if (NowPhase == Phase.Lobby)
                LobbyManager.Init("LobbyHub");

            RoomGMO.SetActive(NowPhase == Phase.Room);
            if (NowPhase == Phase.Room)
                RoomManager.Init("RoomHub_12", isHost);

            if (NowPhase == Phase.GamePlay)
            {
                GameObject gmo = Instantiate(GamePlayManagerPRF, CanvasFolder);
                GamePlayManager gamePlayManager = gmo.GetComponent<GamePlayManager>();
                ClientController.AddSubscriptor(new Subscriptor("GamePlayHub", Type.GetType("GamePlayHub.GamePlayManager"), gamePlayManager));
                gamePlayManager.OnNextPhase += () =>
                {
                    ChangePhase(Phase.Room);
                };
                gamePlayManager.ClientController = ClientController;
                LogMaster.GetInstance().View.SetActive(true);
                LogMaster.GetInstance().ClearLog();
                gamePlayManager.Init("GamePlayHub_16");
            }

            SummaryGMO.SetActive(NowPhase == Phase.Summary);
        }
    }
}
