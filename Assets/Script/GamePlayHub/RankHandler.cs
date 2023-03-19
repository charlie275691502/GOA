using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlayHub
{
    public class RankHandler : MonoBehaviour
    {
        public GameObject Pack;
        public Text[] RankTXT;
        public Text[] NickTXT;

        public void Init(int[] Ranks, List<ClientPlayer> clientPlayers)
        {
            Pack.SetActive(true);
            for(int i=0;i< RankTXT.Length; i++)
            {
                RankTXT[i].gameObject.SetActive(Ranks.Length > i);
                NickTXT[i].text = (Ranks.Length > i) ? clientPlayers.Find(cP => cP.PlayerSerial == Ranks[i]).Nick : "";
            }
        }
    }
}
