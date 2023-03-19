using GamePlayHub.ServerToClient;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Main;

namespace GamePlayHub
{
    public interface IStrategyInfo
    {
        int StrategyCardClass { get; } // 每張牌一個class
        string StrategyName { get; }
        string StrategyDescription { get; }
        List<Requirement> Requirements { get; }
        Sprite CardDetailSprite { get; }
    }

    public class CongressStrategyInfo : IStrategyInfo
    {
        public int StrategyCardClass { get; } // 每張牌一個class
        public string StrategyName { get; }
        public string StrategyDescription { get; }
        public List<Requirement> Requirements { get; }
        public Sprite CardDetailSprite { get; }

        public CongressStrategyInfo(string[] cardInfoStrs)
        {
            StrategyCardClass = int.Parse(cardInfoStrs[3]);
            StrategyName = cardInfoStrs[4];
            StrategyDescription = cardInfoStrs[5];
            CardDetailSprite = Resources.Load<Sprite>("Images/Cards/StrategyCards/Details/" + StrategyName);

            Requirements = new List<Requirement>();
            for (int i = 6; i < cardInfoStrs.Length; i += 2) Requirements.Add(new Requirement(int.Parse(cardInfoStrs[i]), int.Parse(cardInfoStrs[i + 1])));
        }
    }

    public class MidTurnStrategyInfo : IStrategyInfo
    {
        public int StrategyCardClass { get; } // 每張牌一個class
        public string StrategyName { get; }
        public string StrategyDescription { get; }
        public List<Requirement> Requirements { get; }
        public Sprite CardDetailSprite { get; }

        public MidTurnStrategyInfo(string[] cardInfoStrs)
        {
            StrategyCardClass = int.Parse(cardInfoStrs[3]);
            StrategyName = cardInfoStrs[4];
            StrategyDescription = cardInfoStrs[5];
            CardDetailSprite = Resources.Load<Sprite>("Images/Cards/StrategyCards/Details/" + StrategyName);

            Requirements = new List<Requirement>();
            for (int i = 6; i < cardInfoStrs.Length; i += 2) Requirements.Add(new Requirement(int.Parse(cardInfoStrs[i]), int.Parse(cardInfoStrs[i + 1])));
        }
    }

    public class EmptyStrategyInfo : IStrategyInfo
    {
        public int StrategyCardClass { get; } // 每張牌一個class
        public string StrategyName { get; }
        public string StrategyDescription { get; }
        public List<Requirement> Requirements { get; }
        public Sprite CardDetailSprite { get; }
    }

    public interface IPublicInfo
    {
        int PublicCardClass { get; }
        PowerType PowerType { get; }
        int Power { get; }
    }

    public class PublicInfo : IPublicInfo
    {
        public int PublicCardClass { get; }
        public PowerType PowerType { get; }
        public int Power { get; }

        public PublicInfo(string[] cardInfoStrs)
        {
            PublicCardClass = int.Parse(cardInfoStrs[3]);
            PowerType = (PowerType)int.Parse(cardInfoStrs[4]);
            Power = int.Parse(cardInfoStrs[5]);

        }
    }

    public class EmptyPublicInfo : IPublicInfo
    {
        public int PublicCardClass { get; }
        public PowerType PowerType { get; }
        public int Power { get; }
    }

    public interface IFunctionInfo
    {
        Sprite CardDetailSprite { get; }
    }

    public class MaskFunctionInfo : IFunctionInfo
    {
        public Sprite CardDetailSprite { get; }

        public MaskFunctionInfo(PowerType powerType)
        {
            CardDetailSprite = Resources.Load<Sprite>("Images/Cards/PublicCards/Details/" + powerType.ToString() + "_Mask");
        }
    }

    public class ReformFunctionInfo : IFunctionInfo
    {
        public Sprite CardDetailSprite { get; }

        public ReformFunctionInfo(PowerType powerType)
        {
            CardDetailSprite = Resources.Load<Sprite>("Images/Cards/PublicCards/Details/" + powerType.ToString() + "_Reform");
        }
    }

    public class ExpandFunctionInfo : IFunctionInfo
    {
        public Sprite CardDetailSprite { get; }

        public ExpandFunctionInfo(PowerType powerType)
        {
            CardDetailSprite = Resources.Load<Sprite>("Images/Cards/PublicCards/Details/" + powerType.ToString() + "_Expand");
        }
    }

    public class EmptyFunctionInfo : IFunctionInfo
    {
        public Sprite CardDetailSprite { get; }
    }

    public class CardInfo
    {
        public int NameCode { get; }
        public bool IsPublic { get; }
        public bool IsStrategy { get; }
        public bool IsPower { get; }
        public bool IsFunction { get; }
        public bool IsCongress { get; }
        public bool IsMidTurn { get; }
        public bool IsMask { get; }
        public bool IsReform { get; }
        public bool IsExpand { get; }

        public Sprite CardImage;
        public Sprite ChosenCardImage;
        public IStrategyInfo StrategyInfo { get; }
        public IPublicInfo PublicInfo { get; }
        public IFunctionInfo FunctionInfo { get; }

        public CardInfo(string[] cardInfoStrs)
        {
            IsPublic = false;
            IsStrategy = false;
            IsMidTurn = false;
            IsCongress = false;
            IsPower = false;
            IsFunction = false;
            IsMask = false;
            IsReform = false;
            IsExpand = false;

            int cardType = int.Parse(cardInfoStrs[1]);
            if (cardType == 0)
            { // public card

                NameCode = int.Parse(cardInfoStrs[0]);
                IsPublic = true;

                StrategyInfo = new EmptyStrategyInfo();
                PublicInfo = new PublicInfo(cardInfoStrs);

                int publicType = int.Parse(cardInfoStrs[2]);
                if (publicType == 0)
                { // Power Card
                    IsPower = true;
                    FunctionInfo = new EmptyFunctionInfo();
                }
                else
                { // Function Card
                    IsFunction = true;
                    int functionType = int.Parse(cardInfoStrs[3]);
                    if (functionType == 11)
                    { // Mask Card
                        IsMask = true;
                        FunctionInfo = new MaskFunctionInfo(PublicInfo.PowerType);
                    } else if(functionType == 12)
                    { // Reform Card
                        IsReform = true;
                        FunctionInfo = new ReformFunctionInfo(PublicInfo.PowerType);
                    }
                    else
                    { // Expand Card
                        IsExpand = true;
                        FunctionInfo = new ExpandFunctionInfo(PublicInfo.PowerType);
                    }
                }


            } else
            { // strategy card
                NameCode = int.Parse(cardInfoStrs[0]);
                IsStrategy = true;
                PublicInfo = new EmptyPublicInfo();
                FunctionInfo = new EmptyFunctionInfo();

                int strategyType = int.Parse(cardInfoStrs[2]);
                if (strategyType == 0)
                { // MidTurn Strategy
                    IsMidTurn = true;
                    StrategyInfo = new MidTurnStrategyInfo(cardInfoStrs);
                }
                else
                {
                    IsCongress = true;
                    StrategyInfo = new CongressStrategyInfo(cardInfoStrs);
                }
            }
            CardImage = IsPublic ? Resources.Load<Sprite>("Images/Cards/PublicCards/Normal/" + PublicInfo.PowerType.ToString() + "_" + Func.GetEnglishNameOfPublicCardClass(PublicInfo.PublicCardClass)) :
                                   Resources.Load<Sprite>("Images/Cards/StrategyCards/CardImages/" + StrategyInfo.StrategyName + "s");
            if(IsPublic) ChosenCardImage = Resources.Load<Sprite>("Images/Cards/PublicCards/Chosen/" + PublicInfo.PowerType.ToString() + "_" + Func.GetEnglishNameOfPublicCardClass(PublicInfo.PublicCardClass));
        }
    }

    public class HandCardHandler
    {
        List<CardHandler> CardHandlers;

        public HandCardHandler()
        {
            CardHandlers = new List<CardHandler>();
        }
        public void UpdateHandCardsUI(List<int> handCards, GamePlayManager gamePlayManager)
        {
            int max = Mathf.Max(CardHandlers.Count, handCards.Count);
            for(int i = 0; i < handCards.Count; i++)
            {
                if (CardHandlers.Count == i)
                {
                    GameObject gmo = MonoBehaviour.Instantiate(gamePlayManager.CardPRF, gamePlayManager.HandCardsFolder);
                    CardHandler cardHandler = gmo.GetComponent<CardHandler>();
                    cardHandler.Init(GetLocalPosition(handCards.Count, i), CardPlace.Hand, i, handCards[i], gamePlayManager);
                    cardHandler.transform.localScale = Vector3.one * (handCards.Count <= 18 ? 1.25f : 0.85f);
                    CardHandlers.Add(cardHandler);
                }
                else
                {
                    CardHandlers[i].NameCode = handCards[i];
                    CardHandlers[i].transform.localPosition = GetLocalPosition(handCards.Count, i);
                    CardHandlers[i].transform.localScale = Vector3.one * (handCards.Count <= 18 ? 1.25f : 0.85f);
                }
            }
            while (handCards.Count < CardHandlers.Count)
            {
                MonoBehaviour.Destroy(CardHandlers[CardHandlers.Count - 1].gameObject);
                CardHandlers.RemoveAt(CardHandlers.Count - 1);
            }
        }

        public Vector3 GetLocalPosition(int handCardCount, int index)
        {
            int x;
            int y;
            if (handCardCount <= 18)
            {
                x = index % 9;
                y = index / 9;
                return new Vector3(-378 + 95 * x, -280 + y * -136, 0);
            }
            x = index % 13;
            y = index / 13;
            return new Vector3(-394 + 66 * x, -255 + y * -93, 0);
        }

        public void ChangeHandCardIsChosen(int index, bool isChosen)
        {
            CardHandlers[index].IsChosen = isChosen;
        }

        public void UnChosenAll()
        {
            foreach(CardHandler cardHandler in CardHandlers)
                cardHandler.IsChosen = false;
        }
    }

    public class Requirement
    {
        public PowerType PowerType;
        public int Power;

        public Requirement(int powerType, int power)
        {
            PowerType = (PowerType)powerType;
            Power = power;
        }
    }
}