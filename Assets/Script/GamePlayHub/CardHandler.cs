using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Main;

namespace GamePlayHub
{
    public class CardHandler : MonoBehaviour
    {
        public GamePlayManager GamePlayManager;
        public CardPlace CardPlace;
        public int Index;

        public CardInfo CardInfo;
        private int _NameCode;
        public int NameCode { get { return _NameCode; }
            set{
                _NameCode = value;
                CardHandelrUI.Pack.SetActive(value != -2); // no card here
                if (value == -2) return;
                else if (value == -1) // ? Card
                { // unreveal
                    UpdateCardSprite();
                } else
                {
                    CardInfo = DataMaster.GetInstance().GetCardInfo(value);
                    UpdateCardSprite();
                }
        }}
        private bool _IsChosen;
        public bool IsChosen { get { return _IsChosen; }
            set {
                _IsChosen = value;
                UpdateCardSprite();
        }}

        public CardHandelrUI CardHandelrUI;

        private void OnMouseUp()
        {
            GamePlayManager.OnMouseUpCardHandler(CardPlace, Index, NameCode, CardInfo, NameCode == -1, IsChosen);
        }

        public void Init(Vector3 localPosition, CardPlace cardPlace, int index, int nameCode, GamePlayManager gamePlayManager)
        {
            GamePlayManager = gamePlayManager;
            CardPlace = cardPlace;
            Index = index;
            NameCode = nameCode;
            IsChosen = false;
            transform.localPosition = localPosition;
        }

        void UpdateCardSprite()
        {
            if (_NameCode == -2) return;
            if (_NameCode == -1)
                CardHandelrUI.CardIMG.sprite = DataMaster.GetInstance().GetCoveredCardSprite(IsChosen);
            else
                CardHandelrUI.CardIMG.sprite = IsChosen ? CardInfo.ChosenCardImage : CardInfo.CardImage;
        }
    }

    [System.Serializable]
    public class CardHandelrUI
    {
        public Image CardIMG;
        public GameObject Pack;
    }
}
