using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePlayHub;
using System;

namespace Main
{
    public class DataMaster
    {
        private static DataMaster _DataMaster;

        private CardInfoMaster _CardInfoMaster;
        private CharacterInfoMaster _CharacterInfoMaster;
        private BuffInfoMaster _BuffInfoMaster;
        public int CharacterCount { get { return _CharacterInfoMaster.CharacterInfos.Length; } }
        public int PublicCardCount { get { return _CardInfoMaster.PublicCardCount; } }
        public int StrategyCardCount { get { return _CardInfoMaster.StrategyCardCount; } }

        private DataMaster()
        {
            LoadAllData();
        }

        public static DataMaster GetInstance()
        {
            if (_DataMaster == null)
                _DataMaster = new DataMaster();
            return _DataMaster;
        }

        public void LoadAllData(params TextAsset[] textAssets)
        {
            TextAsset cardInfosTXA = Resources.Load<TextAsset>("TextAsset/CardInfoTextAsset");
            TextAsset characterInfosTXA = Resources.Load<TextAsset>("TextAsset/CharacterInfoTextAsset");
            _CardInfoMaster = new CardInfoMaster(cardInfosTXA);
            _CharacterInfoMaster = new CharacterInfoMaster(characterInfosTXA);
            _BuffInfoMaster = new BuffInfoMaster();
        }

        public CardInfo GetCardInfo(int nameCode)
        {
            return _CardInfoMaster.CardInfos[nameCode];
        }

        public Sprite GetCoveredCardSprite(bool isChosen)
        {
            return isChosen ? _CardInfoMaster.ChosenCoveredCardSprite : _CardInfoMaster.CoveredCardSprite;
        }

        public Sprite GetBuffSprite(Buff buff)
        {
            return _BuffInfoMaster.BuffSprites[buff];
        }

        public GamePlayHub.CharacterInfo GetCharacterInfo(int nameCode)
        {
            return _CharacterInfoMaster.CharacterInfos[nameCode];
        }

        public int[] GetNameCodesOfPublicCard(PowerType powerType, int power)
        {
            List<int> ret = new List<int>();
            foreach (CardInfo cardInfo in _CardInfoMaster.CardInfos)
                if (cardInfo.IsPower && cardInfo.PublicInfo.PowerType == powerType && cardInfo.PublicInfo.Power == power)
                    ret.Add(cardInfo.NameCode);
            return ret.ToArray();
        }
    }

    public class CharacterInfoMaster
    {
        public GamePlayHub.CharacterInfo[] CharacterInfos;

        public CharacterInfoMaster(TextAsset textAsset) {
            string[] characterInfoStrs = textAsset.ToString().Split('\n');
            CharacterInfos = new GamePlayHub.CharacterInfo[characterInfoStrs.Length];
            for (int i = 0; i < characterInfoStrs.Length; i++)
            {
                string[] characterInfos = characterInfoStrs[i].Split(' ');
                CharacterInfos[i] = new GamePlayHub.CharacterInfo(characterInfoStrs[i]);
            }
        }
    }

    public class CardInfoMaster
    {
        public CardInfo[] CardInfos;
        public Sprite CoveredCardSprite;
        public Sprite ChosenCoveredCardSprite;
        public readonly int PublicCardCount;
        public readonly int StrategyCardCount;

        public CardInfoMaster(TextAsset textAsset)
        {
            CoveredCardSprite = Resources.Load<Sprite>("Images/Cards/PublicCards/Normal/Covered");
            ChosenCoveredCardSprite = Resources.Load<Sprite>("Images/Cards/PublicCards/Chosen/Covered");
            PublicCardCount = 0;
            StrategyCardCount = 0;

            string[] cardInfoStrs = textAsset.ToString().Split('\n');
            CardInfos = new CardInfo[cardInfoStrs.Length];
            for (int i = 0; i < cardInfoStrs.Length; i++)
            {
                string[] cardInfos = cardInfoStrs[i].Split(' ');
                CardInfos[i] = new CardInfo(cardInfos);

                if (CardInfos[i].IsPublic)
                    PublicCardCount++;
                else
                    StrategyCardCount++;
            }
        }
    }

    public class BuffInfoMaster
    {
        public Dictionary<Buff, Sprite> BuffSprites;

        public BuffInfoMaster()
        {
            BuffSprites = new Dictionary<Buff, Sprite>();
            foreach (Buff buff in Enum.GetValues(typeof(Buff)))
                BuffSprites.Add(buff, Resources.Load<Sprite>("Images/Buffs/" + buff.ToString()));
        }
    }
}