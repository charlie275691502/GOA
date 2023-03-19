using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlayHub
{
    public class EndGameCardsHandler : MonoBehaviour
    {
        public GamePlayManager GamePlayManager;
        public GameObject Pack;
        public GameObject CardPRF;
        private bool _IsShow;
        public bool IsShow
        {
            get { return _IsShow; }
            set
            {
                _IsShow = value;
                Pack.SetActive(IsShow);
            }
        }

        public void Init(int[] cards)
        {
            for(int i=0;i<cards.Length;i++)
            {
                GameObject gmo = Instantiate(CardPRF, Pack.transform);
                int x = i % 19;
                int y = i / 19;
                gmo.GetComponent<CardHandler>().Init(new Vector3(252 + x * 46, 40 - y * 68, 0), CardPlace.Play, -1, cards[i], GamePlayManager);
                gmo.transform.localScale = Vector3.one * 0.625f;
            }
            IsShow = true;
        }
    }
}