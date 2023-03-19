using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Main;

namespace GamePlayHub
{
    public class CardDetailHandler : MonoBehaviour
    {
        public GamePlayManager GamePlayManager;

        public CardInfo CardInfo;
        private int _NameCode;
        public int NameCode
        {
            get { return _NameCode; }
            set
            {
                if (_NameCode == value) _NameCode = -1;
                else _NameCode = value;

                CardDetailHandlerUI.PackGMO.SetActive(_NameCode != -1);
                if (_NameCode == -1) return;
                CardInfo = DataMaster.GetInstance().GetCardInfo(value);
                UpdateUDButtons();

                if (CardInfo.IsFunction)
                    CardDetailHandlerUI.IMG.sprite = CardInfo.FunctionInfo.CardDetailSprite;
                else if (CardInfo.IsStrategy)
                    CardDetailHandlerUI.IMG.sprite = CardInfo.StrategyInfo.CardDetailSprite;
                else CardDetailHandlerUI.PackGMO.SetActive(false);
            }
        }

        public CardDetailHandlerUI CardDetailHandlerUI;

        public void OnClickUseButton()
        {
            GamePlayManager.OnClickCardDetailHandlerUseButton(NameCode);
        }

        public void OnClickOkButton()
        {
            GamePlayManager.OnClickCardDetailHandlerOkButton();
        }

        public void OnClickCancelButton()
        {
            GamePlayManager.OnClickCardDetailHandlerCancelButton();
        }

        public void OnClickDiscardButton()
        {
            GamePlayManager.OnClickCardDetailHandlerDiscardButton(NameCode);
        }

        public void UpdateUDButtons() // Use and Discard
        {
            if (NameCode == -1) return;
            CardInfo cardInfo = DataMaster.GetInstance().GetCardInfo(NameCode);


            ClientStage nowStage = GamePlayManager.NowStage;
            CardDetailHandlerUI.DiscardBTN.gameObject.SetActive(cardInfo.IsStrategy);
            CardDetailHandlerUI.DiscardBTN.interactable = (nowStage == ClientStage.MidTurn || nowStage == ClientStage.SelfCongress);

            if (cardInfo.IsPublic)
            {
                CardDetailHandlerUI.UseBTN.interactable = nowStage == ClientStage.MidTurn &&
                    ((cardInfo.IsMask && GamePlayManager.UsedMaskAmount < GamePlayManager.UsedMaskLimit) ||
                     (cardInfo.IsReform && GamePlayManager.UsedReformAmount < GamePlayManager.UsedReformLimit && GamePlayManager.HasCertainPowerTypePowerCardInHand(cardInfo.PublicInfo.PowerType)) ||
                     (cardInfo.IsExpand && GamePlayManager.UsedExpandAmount < GamePlayManager.UsedExpandLimit && GamePlayManager.HasCertainPowerTypePowerCardInDeck(cardInfo.PublicInfo.PowerType)));
            }
            else
            {
                CardDetailHandlerUI.UseBTN.interactable = !GamePlayManager.GetClientPlayerFromPlayerSerial(GamePlayManager.SelfPlayerSerial).BuffHandler.HasBuff(Buff.StrategyRestriction) &&
                                      GamePlayManager.UsedStrategyAmount < GamePlayManager.UsedStrategyLimit &&
                                      ((cardInfo.IsMidTurn && nowStage == ClientStage.MidTurn) ||
                                       (cardInfo.IsCongress && nowStage == ClientStage.SelfCongress));
            }
        }
    }

    [System.Serializable]
    public class CardDetailHandlerUI{
        public GameObject PackGMO;
        public Image IMG;
        public Button UseBTN;
        public Button OkBTN;
        public Button CancelBTN;
        public Button DiscardBTN;
    }
}
