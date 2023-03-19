using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LobbyHub
{
    public class LobbyManager : MonoBehaviour
    {
        public Network.ClientController ClientController;
        public string Hub;

        public void Init(string hub)
        {
            Hub = hub;
        }
    }
}
