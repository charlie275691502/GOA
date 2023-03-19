using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RoomHub
{
    interface IPlayer
    {
        string Nick { get; set; }
        bool IsReady { get; set; }
        bool IsHost { get; set; }
        bool IsBot { get; set; }
    }

    public class ServerPlayer : IPlayer
    {
        public int ClientSerial;
        public string Nick { get; set; }
        public bool IsReady { get; set; }
        public bool IsHost { get; set; }
        public bool IsBot { get; set; }

        public ServerPlayer(int clientSerial, string nick, bool isReady, bool isHost, bool isBot)
        {
            ClientSerial = clientSerial;
            Nick = nick;
            IsReady = isReady;
            IsHost = isHost;
            IsBot = isBot;
        }
    }

    public class ClientPlayer : IPlayer
    {
        private string _Nick;
        private bool _IsReady;
        private bool _IsHost;
        private bool _IsBot;
        public string Nick { get { return _Nick; }
            set {
                NickTXT.text = value;
                _Nick = value;
            }
        }
        public bool IsReady{ get { return _IsReady; }
            set {
                IsReadyTXT.text = value ? "Ready" : "";
                _IsReady = value;
            }
        }
        public bool IsHost { get { return _IsHost; }
            set {
                HostBotTXT.text = value ? "Host" : HostBotTXT.text;
                _IsHost = value;
            }
        }
        public bool IsBot { get { return _IsBot; }
            set {
                HostBotTXT.text = value ? "Bot" : HostBotTXT.text;
                _IsBot = value;
            }
        }
        public Text NickTXT;
        public Text IsReadyTXT;
        public Text HostBotTXT;

        public ClientPlayer(Text nickTXT, Text isReadyTXT, Text hostBotTXT)
        {
            NickTXT = nickTXT;
            IsReadyTXT = isReadyTXT;
            HostBotTXT = hostBotTXT;
        }

        public void SetValues(string nick, bool isReady, bool isHost, bool isBot)
        {
            Nick = nick;
            IsReady = isReady;
            IsHost = isHost;
            IsBot = isBot;
        }
    }
}