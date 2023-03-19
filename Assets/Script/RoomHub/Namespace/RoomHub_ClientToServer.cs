using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoomHub.ClientToServer
{
    public class SetNick
    {
        public string Nick;

        public SetNick(string nick)
        {
            Nick = nick;
        }
    }

    public class SetReady
    {
        public bool IsReady;

        public SetReady(bool isReady)
        {
            IsReady = isReady;
        }
    }

    public class AddBot
    {

    }

    public class RemoveBot
    {

    }

    public class HostStartGame
    {

    }
}