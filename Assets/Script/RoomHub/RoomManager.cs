using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RoomHub.ClientToServer;
using RoomHub.ServerToClient;
using Main;

namespace RoomHub{
    public class RoomManager : MonoBehaviour
    {
        public Network.ClientController ClientController;
        public OnNextPhase OnNextPhase;
        public string Hub;

        public GameObject PlayerInfoPRF;
        public List<ClientPlayer> ClientPlayers = new List<ClientPlayer>();
        public InputField NewNickIPF;
        public Button ReadyBTN;
        public Button AddBotBTN;
        public Button RemoveBotBTN;
        public Button StartGameBTN;

        bool _IsReady;
        bool _IsHost;
        int _ReadyPlayerCount {
            get {
                int ret = 0;
                foreach (ClientPlayer clientPlayer in ClientPlayers) if (clientPlayer.IsReady) ret++;
                return ret;
            }
        }

        public void Init(string hub, bool isHost)
        {
            Hub = hub;
            _IsReady = false;
            _IsHost = isHost;
            AddBotBTN.gameObject.SetActive(isHost);
            RemoveBotBTN.gameObject.SetActive(isHost);
            StartGameBTN.gameObject.SetActive(isHost);
            ReadyBTN.transform.Find("Text").GetComponent<Text>().text = "Ready";
            GetNickFromPlayerPrefs();

            for (int i = ClientPlayers.Count - 1; i >= 0; i--)
            {
                Destroy(ClientPlayers[i].NickTXT.transform.parent.gameObject);
                ClientPlayers.RemoveAt(i);
            }
        }

        // UI_Click //

        public void OnClickChangeNick()
        {
            PlayerPrefs.SetString("Nick", NewNickIPF.text);
            Send_SetNick(new SetNick(NewNickIPF.text));
            NewNickIPF.text = "";
        }

        public void OnClickReady()
        {
            _IsReady = !_IsReady;
            ReadyBTN.transform.Find("Text").GetComponent<Text>().text = _IsReady ? "Cancel" : "Ready";
            Send_SetReady(new SetReady(_IsReady));
        }

        public void OnClickAddBot()
        {
            if (!_IsHost) return;
            Send_AddBot(new AddBot());
        }

        public void OnClickRemoveBot()
        {
            if (!_IsHost) return;
            Send_RemoveBot(new RemoveBot()); 
        }

        public void OnClickStartGame()
        {
            if (!_IsHost || !_IsReady) return;
            Send_HostStartGame(new HostStartGame());
        }

        // send //

        public void Send_SetNick(SetNick setNick)
        {
            ClientController.SendPacket(Hub, "SetNick", setNick);
        }

        public void Send_SetReady(SetReady setReady)
        {
            ClientController.SendPacket(Hub, "SetReady", setReady);
        }

        public void Send_AddBot(AddBot addBot)
        {
            ClientController.SendPacket(Hub, "AddBot", addBot);
        }

        public void Send_RemoveBot(RemoveBot removeBot)
        {
            ClientController.SendPacket(Hub, "RemoveBot", removeBot);
        }

        public void Send_HostStartGame(HostStartGame hostStartGame)
        {
            ClientController.SendPacket(Hub, "HostStartGame", hostStartGame);
        }

        // recieve //

        public void UpdatePlayerInfos(UpdatePlayerInfos updatePlayerInfos)
        {
            ClientController.NextPacket();
            // create enough PlayerInfo GameObject
            for (int i = ClientPlayers.Count; i < updatePlayerInfos.PlayerNicks.Length; i++)
            {
                GameObject gmo = Instantiate(PlayerInfoPRF, this.transform);
                gmo.transform.localPosition = new Vector3(0, i * -80, 0);
                ClientPlayers.Add(new ClientPlayer(gmo.transform.Find("Nick").GetComponent<Text>(), gmo.transform.Find("Ready").GetComponent<Text>(), gmo.transform.Find("HostBot").GetComponent<Text>()));
            }

            for (int i= ClientPlayers.Count - 1; i >= 0 ; i--)
            {
                if(i >= updatePlayerInfos.PlayerNicks.Length)
                {
                    Destroy(ClientPlayers[i].NickTXT.transform.parent.gameObject);
                    ClientPlayers.RemoveAt(i);
                    continue;
                }

                ClientPlayers[i].SetValues(updatePlayerInfos.PlayerNicks[i], updatePlayerInfos.IsReadys[i], updatePlayerInfos.IsHosts[i], updatePlayerInfos.IsBots[i]);
            }

            StartGameBTN.interactable = ((3 <= _ReadyPlayerCount && _ReadyPlayerCount <= 5) && _IsReady);
        }

        public void StartGame(StartGame _StartGame)
        {
            ClientController.NextPacket();
            OnNextPhase();
        }

        // process //

        public void GetNickFromPlayerPrefs()
        {
            string nick = PlayerPrefs.GetString("Nick");
            if (nick == "")
            {
                nick = "User" + Random.Range(1000, 10000);
                PlayerPrefs.SetString("Nick", nick);
            }
            Send_SetNick(new SetNick(nick));
        }
    }
}
