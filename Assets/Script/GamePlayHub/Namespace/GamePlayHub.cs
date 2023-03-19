using GamePlayHub.ServerToClient;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Main;
using System.Linq;
using ModestTree;

namespace GamePlayHub
{
    interface IPlayer
    {
        int PlayerSerial { get; set; }
        string Nick { get; set; }
        bool IsBot { get; set; }
        int CharacterNameCode { get; set; }
        int Power { get; set; }
        int PowerLimit { get; set; }
        bool IsDead { get; set; }
        bool IsDecideCharacterReady { get; set; }
        bool CharacterNameCode3HasPassive { get; set; }
    }

    public class ServerPlayer : IPlayer
    {
        public int ClientSerial;
        public int PlayerSerial { get; set; }
        public string Nick { get; set; }
        public bool IsBot { get; set; }
        public int CharacterNameCode { get; set; }
        public int Power { get; set; }
        public int PowerLimit { get; set; }
        public bool IsDead { get; set; }

        public S_GamePlayManager S_GamePlayManager;
        public List<int> HandCards;
        private List<Buff> _Buffs;
        public int StrategyCardAmount
        {
            get
            {
                int amount = 0;
                foreach (int nameCode in HandCards) if (DataMaster.GetInstance().GetCardInfo(nameCode).IsStrategy) amount++;
                return amount;
            }
        }
        public int StrategyCardLimit;
        public bool IsSceneReady;
        public bool IsDecideCharacterReady { get; set; }

        private int _PowerLimitCharacter3NameCodeSkill;
        public int PowerLimitCharacter3NameCodeSkill
        {
            get { return _PowerLimitCharacter3NameCodeSkill; }
            set
            {
                _PowerLimitCharacter3NameCodeSkill = value;
                UpdatePowerLimit();
            }
        }
        private int _PowerLimitCharacter11NameCodeSkill1;
        public int PowerLimitCharacter11NameCodeSkill1
        {
            get { return _PowerLimitCharacter11NameCodeSkill1; }
            set
            {
                _PowerLimitCharacter11NameCodeSkill1 = value;
                UpdatePowerLimit();
            }
        }

        private int _PowerLimitCharacter11NameCodeSkill2;
        public int PowerLimitCharacter11NameCodeSkill2
        {
            get { return _PowerLimitCharacter11NameCodeSkill2; }
            set
            {
                _PowerLimitCharacter11NameCodeSkill2 = value;
                UpdatePowerLimit();
            }
        }

        private int _PowerLimitCharacter12NameCodeSkill;
        public int PowerLimitCharacter12NameCodeSkill
        {
            get { return _PowerLimitCharacter12NameCodeSkill; }
            set
            {
                _PowerLimitCharacter12NameCodeSkill = value;
                UpdatePowerLimit();
            }
        }

        private int _PowerLimitCharacter16NameCodeSkill;
        public int PowerLimitCharacter16NameCodeSkill
        {
            get { return _PowerLimitCharacter16NameCodeSkill; }
            set
            {
                _PowerLimitCharacter16NameCodeSkill = value;
                UpdatePowerLimit();
            }
        }

        private int _PowerLimitCharacterNameCode17Skill;
        public int PowerLimitCharacterNameCode17Skill
        {
            get { return _PowerLimitCharacterNameCode17Skill; }
            set
            {
                _PowerLimitCharacterNameCode17Skill = value;
                UpdatePowerLimit();
            }
        }

        public bool CharacterNameCode1SkillUsed;
        private bool _CharacterNameCode3HasPassive;
        public bool CharacterNameCode3HasPassive
        {
            get { return _CharacterNameCode3HasPassive; }
            set
            {
                _CharacterNameCode3HasPassive = value;
                PowerLimitCharacter3NameCodeSkill = value  ? - 8 : 5;
            }
        }

        public ServerPlayer(int clientSerial, int playerSerial, string nick, bool isBot, S_GamePlayManager s_GamePlayManager)
        {
            ClientSerial = clientSerial;
            PlayerSerial = playerSerial;
            Nick = nick;
            IsBot = isBot;
            PowerLimit = 36;
            IsDead = false;
            S_GamePlayManager = s_GamePlayManager;
            HandCards = new List<int>();
            _Buffs = new List<Buff>();
            StrategyCardLimit = 2;
            IsSceneReady = isBot;
            IsDecideCharacterReady = false;
        }

        public void AddHandCard(params int[] nameCodes)
        {
            foreach (int nameCode in nameCodes) HandCards.Add(nameCode);
            HandCards.Sort();
            UpdatePower();

            // 斐迪南七世，被動技，每持有6點未被遮蔽的海權或軍事權力點，就增加3革命門檻。每失去一張海權或軍事權力牌，革命門檻減少2
            if (CharacterNameCode == 11)
            {
                //passive 1
                int SeaPower_Military_Power = Func.CalculatePower(HandCards, (CharacterNameCode == 2) ? 2 : 1, new List<PowerType>(2) { PowerType.SeaPower, PowerType.Military });
                if (PowerLimitCharacter11NameCodeSkill1 != (SeaPower_Military_Power / 6) * 3)
                    PowerLimitCharacter11NameCodeSkill1 = (SeaPower_Military_Power / 6) * 3;
            }

            // 羅伯特·詹金遜，被動技，每持有三張手牌就使自己革命門檻增加３。
            if (CharacterNameCode == 12)
            {
                int publicCardsCount = HandCards.Count(nameCode => DataMaster.GetInstance().GetCardInfo(nameCode).IsPublic);
                if (PowerLimitCharacter12NameCodeSkill != (publicCardsCount / 3) * 3) // 避免頻繁呼叫send
                    PowerLimitCharacter12NameCodeSkill = (publicCardsCount / 3) * 3;
            }
        }

        public void RemoveHandCard(bool toGrave, params int[] nameCodes)
        {
            foreach (int nameCode in nameCodes)
            {
                HandCards.Remove(nameCode);
                if(toGrave)
                    S_GamePlayManager.GraveCards.Add(nameCode);
            }
            HandCards.Sort();
            UpdatePower();

            // 斐迪南七世，被動技，每持有6點未被遮蔽的海權或軍事權力點，就增加3革命門檻。每失去一張海權或軍事權力牌，革命門檻減少2
            if (CharacterNameCode == 11)
            {
                //passive 1
                int SeaPower_Military_Power = Func.CalculatePower(HandCards, (CharacterNameCode == 2) ? 2 : 1, new List<PowerType>(2) { PowerType.SeaPower, PowerType.Military });
                if (PowerLimitCharacter11NameCodeSkill1 != (SeaPower_Military_Power / 6) * 3)
                    PowerLimitCharacter11NameCodeSkill1 = (SeaPower_Military_Power / 6) * 3;

                // passive 2
                int descrease = 0;
                foreach (int nameCode in nameCodes)
                {
                    CardInfo cardInfo = DataMaster.GetInstance().GetCardInfo(nameCode);
                    if (cardInfo.IsPower && (cardInfo.PublicInfo.PowerType == PowerType.SeaPower || cardInfo.PublicInfo.PowerType == PowerType.Military))
                        descrease += 2;
                }
                if (descrease != 0)
                    PowerLimitCharacter11NameCodeSkill2 += descrease;
            }

            // 羅伯特·詹金遜，被動技，每持有三張手牌就使自己革命門檻增加３。
            if (CharacterNameCode == 12)
            {
                int publicCardsCount = HandCards.Count(nameCode => DataMaster.GetInstance().GetCardInfo(nameCode).IsPublic);
                if (PowerLimitCharacter12NameCodeSkill != (publicCardsCount / 3) * 3) // 避免頻繁呼叫send
                    PowerLimitCharacter12NameCodeSkill = (publicCardsCount / 3) * 3;
            }

            // 希爾維奧·佩利科，被動技，自己每當丟出或丟棄一次手牌時，自己的革命門檻減少１，其他玩家的革命門檻減少２。
            if (CharacterNameCode == 17)
            {
                S_GamePlayManager.Send_UpdatePowerLimit_Lock = true;

                PowerLimitCharacterNameCode17Skill += 1;
                foreach (ServerPlayer serverPlayer in S_GamePlayManager.ServerPlayers)
                    if (serverPlayer.PlayerSerial != PlayerSerial)
                        serverPlayer.PowerLimitCharacterNameCode17Skill += 2;

                S_GamePlayManager.Send_UpdatePowerLimit_Lock = false;
                UpdatePowerLimit();
            }
        }

        void UpdatePower()
        {
            Power = Func.CalculatePower(HandCards, (CharacterNameCode == 2) ? 2 : 1, new List<PowerType>(4) { PowerType.Wealth, PowerType.Industry, PowerType.SeaPower, PowerType.Military });
        }

        void UpdatePowerLimit()
        {
            PowerLimit = 36 + PowerLimitCharacter3NameCodeSkill + PowerLimitCharacter11NameCodeSkill1 - PowerLimitCharacter11NameCodeSkill2 + PowerLimitCharacter12NameCodeSkill - PowerLimitCharacter16NameCodeSkill - PowerLimitCharacterNameCode17Skill;
            S_GamePlayManager.Send_UpdatePowerLimit_AllClient(new UpdatePowerLimit(S_GamePlayManager.ServerPlayers));
        }

        public void AddBuffs(params Buff[] buffs)
        {
            foreach (Buff buff in buffs)
            {
                if (HasBuff(buff)) continue;
                _Buffs.Add(buff);
            }
        }

        public bool HasBuff(Buff buff)
        {
            return _Buffs.Exists(b => b == buff);
        }

        public void RemoveBuffsAtCongressStart()
        {
            bool hasIncreaseStrategyLimitOne = HasBuff(Buff.IncreaseStrategyLimitOne);
            _Buffs.Clear();
            if (hasIncreaseStrategyLimitOne)
                _Buffs.Add(Buff.IncreaseStrategyLimitOne);
        }
    }

    public class ClientPlayer : IPlayer
    {
        private string _Nick;
        private int _Power;
        private int _PowerLimit;
        public int PlayerSerial { get; set; }
        public string Nick
        {
            get { return _Nick; }
            set
            {
                CommonClientPlayerUI.NickTXT.text = value;
                _Nick = value;
            }
        }
        public bool IsBot { get; set; }
        public int CharacterNameCode { get; set; }
        public int Power
        {
            get { return _Power; }
            set
            {
                _Power = value;
                CommonClientPlayerUI.PowerTXT.text = value.ToString();
                if (SelfClientPlayerDetails != null)
                    SelfClientPlayerDetails.ChangeHandCardFolderSprite(CharacterNameCode, Power, PowerLimit);
            }
        }
        public int PowerLimit
        {
            get { return _PowerLimit; }
            set
            {
                _PowerLimit = value;
                CommonClientPlayerUI.PowerLimitTXT.text = (value - 1).ToString();
                if (SelfClientPlayerDetails != null)
                    SelfClientPlayerDetails.ChangeHandCardFolderSprite(CharacterNameCode, Power, PowerLimit);
            }
        }
        private bool _IsDead;
        public bool IsDead
        {
            get { return _IsDead; }
            set
            {
                _IsDead = value;
                UpdateFrameColor();
            }
        }
        private bool _IsWin;
        public bool IsWin
        {
            get { return _IsWin; }
            set
            {
                _IsWin = value;
                UpdateFrameColor();
            }
        }
        private bool _IsTurn;
        public bool IsTurn
        {
            get { return _IsTurn; }
            set
            {
                _IsTurn = value;
                UpdateFrameColor();
            }
        }
        private bool _IsChosen;
        public bool IsChosen
        {
            get { return _IsChosen; }
            set
            {
                _IsChosen = value;
                UpdateFrameColor();
            }
        }
        private bool _IsDecideCharacterReady;
        public bool IsDecideCharacterReady
        {
            get { return _IsDecideCharacterReady; }
            set
            {
                _IsDecideCharacterReady = value;
                UpdateFrameColor();
            }
        }
        public bool CharacterNameCode3HasPassive { get; set; }

        public int ProfileIndex;
        public SelfClientPlayerDetails SelfClientPlayerDetails;
        public EnemyClientPlayerDetails EnemyClientPlayerDetails;
        public CommonClientPlayerUI CommonClientPlayerUI;

        public BuffHandler BuffHandler;
        public GamePlayManager GamePlayManager;
        public bool IsSelf;

        public ClientPlayer(int playerSerial, string nick, bool isBot, GameObject profileGMO, bool isSelf, int profileIndex, GamePlayManager gamePlayManager)
        {
            profileGMO.SetActive(true);
            IsSelf = isSelf;
            if (isSelf) SelfClientPlayerDetails = new SelfClientPlayerDetails(profileGMO, gamePlayManager);
            else EnemyClientPlayerDetails = new EnemyClientPlayerDetails(profileGMO);
            CommonClientPlayerUI = new CommonClientPlayerUI(profileGMO);
            BuffHandler = new BuffHandler(gamePlayManager.BuffPRF, profileGMO.transform.Find("Buffs"), isSelf);

            PlayerSerial = playerSerial;
            Nick = nick;
            IsBot = isBot;
            if (!isSelf)
            {
                EnemyClientPlayerDetails.EnemyClientPlayerUI.BotGMO.SetActive(isBot);
                EnemyClientPlayerDetails.EnemyClientPlayerUI.PlayerGMO.SetActive(!isBot);
            }
            SetCharacterNameCodeValue(null, isSelf);
            ProfileIndex = profileIndex;
            GamePlayManager = gamePlayManager;
            IsDead = false;
            IsWin = false;
            IsTurn = false;
            IsChosen = false;
            IsDecideCharacterReady = false;
        }

        public void SetCharacterNameCodeValue(CharacterInfo characterInfo, bool isSelf)
        {
            if (characterInfo == null)
            {
                CharacterNameCode = -1;
                CommonClientPlayerUI.CharacterNameTXT.text = "?";
                CommonClientPlayerUI.AvatarImg.gameObject.SetActive(false);
                if(isSelf) SelfClientPlayerDetails.SelfClientPlayerUI.SkillDescriptionTXT.text = "?";
                CommonClientPlayerUI.PowerTXT.text = "";
                CommonClientPlayerUI.PowerMidTXT.text = "";
                CommonClientPlayerUI.PowerLimitTXT.text = "";
                return;
            }

            CharacterNameCode = characterInfo.NameCode;
            // 數值
            PowerLimit = 36;
            Power = 0;

            CommonClientPlayerUI.CharacterNameTXT.text = characterInfo.CharacterName;
            CommonClientPlayerUI.AvatarImg.gameObject.SetActive(true);
            CommonClientPlayerUI.AvatarImg.sprite = isSelf ? characterInfo.CharacterMidAvatar : characterInfo.CharacterSmallAvatar;
            if (isSelf) SelfClientPlayerDetails.SelfClientPlayerUI.SkillDescriptionTXT.text = characterInfo.SkillDescription;
            CommonClientPlayerUI.PowerMidTXT.text = "/";
        }

        public void AddHandCard(params int[] nameCodes)
        {
            SelfClientPlayerDetails.AddHandCard(nameCodes);
            UpdatePower();
        }

        public void RemoveHandCard(params int[] nameCodes)
        {
            SelfClientPlayerDetails.RemoveHandCard(nameCodes);
            UpdatePower();
        }

        void UpdatePower()
        {
            Power = Func.CalculatePower(SelfClientPlayerDetails.HandCards, (CharacterNameCode == 2) ? 2 : 1, new List<PowerType>(4) { PowerType.Wealth, PowerType.Industry, PowerType.SeaPower, PowerType.Military });
        }

        public void AddReleaseCards(CardAnimationType cardAnimationType, params int[] releaseCards)
        {
            EnemyClientPlayerDetails.AddReleaseCards(cardAnimationType, releaseCards);
        }

        void UpdateFrameColor()
        {
            CommonClientPlayerUI.FrameIMG.color = (IsWin) ? new Color(1, 1, 0) :
                                                  (IsChosen) ? new Color(1, 1, 0) :
                                                  (IsDead) ? new Color(1, 0, 0) :
                                                  (IsTurn) ? new Color(0.6f, 1, 0.6f) :
                                                  (IsDecideCharacterReady) ? new Color(0.6f, 1, 0.6f) : 
                                                  Color.white;
        }

        public bool CanGainCard(CardInfo cardInfo)
        {

            return !((cardInfo.PublicInfo.PowerType == PowerType.Wealth && BuffHandler.HasBuff(Buff.WealthRestriction)) ||
                     (cardInfo.PublicInfo.PowerType == PowerType.Industry && BuffHandler.HasBuff(Buff.IndustryRestriction)) ||
                     (cardInfo.PublicInfo.PowerType == PowerType.SeaPower && BuffHandler.HasBuff(Buff.SeaPowerRestriction)) ||
                     (cardInfo.PublicInfo.PowerType == PowerType.Military && BuffHandler.HasBuff(Buff.MilitaryRestriction)) ||
                     (cardInfo.IsFunction && BuffHandler.HasBuff(Buff.FunctionRestriction)));
        }
    }

    public class SelfClientPlayerDetails
    {
        public SelfClientPlayerUI SelfClientPlayerUI;
        public GamePlayManager GamePlayManager;
        public List<int> HandCards;

        public SelfClientPlayerDetails(GameObject profileGMO, GamePlayManager gamePlayManager)
        {
            SelfClientPlayerUI = new SelfClientPlayerUI(profileGMO, gamePlayManager.HandCardHandler);
            HandCards = new List<int>();
            GamePlayManager = gamePlayManager;
        }

        public void AddHandCard(params int[] nameCodes)
        {
            foreach (int nameCode in nameCodes)
                HandCards.Add(nameCode);
            HandCards.Sort();
            UpdateHandCardsUI();
        }

        public void RemoveHandCard(params int[] nameCodes)
        {
            foreach (int nameCode in nameCodes)
                HandCards.Remove(nameCode);
            HandCards.Sort();
            UpdateHandCardsUI();
        }

        void UpdateHandCardsUI()
        {
            SelfClientPlayerUI.HandCardHandler.UpdateHandCardsUI(HandCards, GamePlayManager);
        }

        public void ChangeHandCardFolderSprite(int characterNameCode, int power, int powerLimit)
        {
            bool isRevolution = (power >= powerLimit) && !GamePlayManager.GameEnd;

            // 梅特涅，被動技，自身手牌權力點為質數時，不會引發革命。 
            if (characterNameCode == 7)
                if (IsPrime(power))
                    isRevolution = false;

            SelfClientPlayerUI.HandCardFolder.sprite = isRevolution ? GamePlayManager.HandCardFolder_Revolution : GamePlayManager.HandCardFolder_Normal;
        }

        bool IsPrime(int power)
        {
            int sqrtPower = Mathf.FloorToInt(Mathf.Sqrt(power)) + 1;
            for (int i = 2; i <= sqrtPower; i++)
                if (power % sqrtPower == 0) return false;
            return true;
        }
    }

    public class EnemyClientPlayerDetails
    {
        private int _PublicCardAmount;
        private int _StrategyCardAmount;
        public EnemyClientPlayerUI EnemyClientPlayerUI { get; set; }
        public ReleaseCardsHandler ReleaseCardsHandler;
        public EndGameCardsHandler EndGameCardsHandler;
        public int PublicCardAmount
        {
            get { return _PublicCardAmount; }
            set
            {
                _PublicCardAmount = value;
                EnemyClientPlayerUI.PublicCardAmountTXT.text = value.ToString();
            }
        }
        public int StrategyCardAmount
        {
            get { return _StrategyCardAmount; }
            set
            {
                _StrategyCardAmount = value;
                EnemyClientPlayerUI.StrategyCardAmountTXT.text = value.ToString();
            }
        }

        public EnemyClientPlayerDetails(GameObject profileGMO)
        {
            EnemyClientPlayerUI = new EnemyClientPlayerUI(profileGMO);
            ReleaseCardsHandler = profileGMO.transform.Find("ReleaseCards").GetComponent<ReleaseCardsHandler>();
            EndGameCardsHandler = profileGMO.transform.Find("EndGameCards").GetComponent<EndGameCardsHandler>();
            PublicCardAmount = 0;
            StrategyCardAmount = 0;
        }

        public void AddReleaseCards(CardAnimationType cardAnimationType, params int[] releaseCards)
        {
            ReleaseCardsHandler.AddReleaseCards(cardAnimationType, releaseCards);
        }
    }

    public class CommonClientPlayerUI
    {
        public GameObject ProfileGMO { get; set; }
        public Image AvatarImg { get; set; }
        public Text NickTXT { get; set; }
        public Text CharacterNameTXT { get; set; }
        public Text PowerTXT { get; set; }
        public Text PowerMidTXT { get; set; }
        public Text PowerLimitTXT { get; set; }
        public Image FrameIMG { get; set; }

        public CommonClientPlayerUI(GameObject profileGMO)
        {
            ProfileGMO = profileGMO;
            AvatarImg = profileGMO.transform.Find("Avatar").GetComponent<Image>();
            NickTXT = profileGMO.transform.Find("Nick").GetComponent<Text>();
            CharacterNameTXT = profileGMO.transform.Find("CharacterName").GetComponent<Text>();
            PowerTXT = profileGMO.transform.Find("Power").Find("Power").GetComponent<Text>();
            PowerMidTXT = profileGMO.transform.Find("Power").Find("<").GetComponent<Text>();
            PowerLimitTXT = profileGMO.transform.Find("Power").Find("PowerLimit").GetComponent<Text>();
            FrameIMG = profileGMO.transform.Find("Frame").GetComponent<Image>();
        }
    }

    public class SelfClientPlayerUI
    {
        public HandCardHandler HandCardHandler;
        public Image HandCardFolder;
        public Text SkillDescriptionTXT;

        public SelfClientPlayerUI(GameObject profileGMO, HandCardHandler handCardHandler)
        {
            SkillDescriptionTXT = profileGMO.transform.Find("SkillDescription").GetComponent<Text>();
            HandCardFolder = profileGMO.transform.Find("HandCardFolder").GetComponent<Image>();

            HandCardHandler = handCardHandler;
        }
    }

    public class EnemyClientPlayerUI
    {
        public Text PublicCardAmountTXT;
        public Text StrategyCardAmountTXT;
        public GameObject BotGMO;
        public GameObject PlayerGMO;

        public EnemyClientPlayerUI(GameObject profileGMO)
        {
            PublicCardAmountTXT = profileGMO.transform.Find("PublicCardInfos").Find("Amount").GetComponent<Text>();
            StrategyCardAmountTXT = profileGMO.transform.Find("StrategyCardInfos").Find("Amount").GetComponent<Text>();
            BotGMO = profileGMO.transform.Find("Bot").gameObject;
            PlayerGMO = profileGMO.transform.Find("Player").gameObject;
        }
    }

    public class BuffHandler
    {
        List<Buff> _Buffs;
        List<Image> _BuffIMG;
        GameObject _BuffPRF;
        Transform _BuffFolder;
        bool _IsSelf;

        public BuffHandler(GameObject buffPRF, Transform buffFolder, bool isSelf)
        {
            _Buffs = new List<Buff>();
            _BuffIMG = new List<Image>();
            _BuffPRF = buffPRF;
            _BuffFolder = buffFolder;
            _IsSelf = isSelf;
        }

        public void AddBuffs(params Buff[] buffs)
        {
            foreach(Buff buff in buffs)
            {
                if (HasBuff(buff)) continue;
                _Buffs.Add(buff);
            }
            UpdateUI();
        }

        public void CongressStart()
        {
            bool hasIncreaseStrategyLimitOne = HasBuff(Buff.IncreaseStrategyLimitOne);
            _Buffs.Clear();
            if (hasIncreaseStrategyLimitOne)
                _Buffs.Add(Buff.IncreaseStrategyLimitOne);
            UpdateUI();
        }

        public bool HasBuff(Buff buff)
        {
            return _Buffs.Exists(b => b == buff);
        }

        void UpdateUI()
        {
            for(int i=0;i< _Buffs.Count; i++)
            {
                if(_BuffIMG.Count == i)
                {
                    GameObject gmo = MonoBehaviour.Instantiate(_BuffPRF, _BuffFolder);
                    gmo.transform.localPosition = GetLocalPosition(i);
                    Image img = gmo.GetComponent<Image>();
                    img.sprite = DataMaster.GetInstance().GetBuffSprite(_Buffs[i]);
                    _BuffIMG.Add(img);
                } else
                {
                    _BuffIMG[i].transform.localPosition = GetLocalPosition(i);
                    _BuffIMG[i].sprite = DataMaster.GetInstance().GetBuffSprite(_Buffs[i]);
                }
            }


            while (_Buffs.Count < _BuffIMG.Count)
            {
                MonoBehaviour.Destroy(_BuffIMG[_BuffIMG.Count - 1].gameObject);
                _BuffIMG.RemoveAt(_BuffIMG.Count - 1);
            }
        }

        public Vector3 GetLocalPosition(int index)
        {
            if (_IsSelf)
                return new Vector3(-315 + index * 45, 90, 0);
            int x = index % 4;
            int y = index / 4;
            return new Vector3(0 - x * 45, 90 - y * 65, 0);
        }
    }

    public static class Func
    {
        public static int CalculatePower(List<int> nameCodes, int maskMultiplier, List<PowerType> calculatedPowerTypes)
        {
            List<CardInfo> cardInfos = new List<CardInfo>();
            for (int i = 0; i < nameCodes.Count; i++)
                cardInfos.Add(DataMaster.GetInstance().GetCardInfo(nameCodes[i]));

            int totalPower = 0;
            int[] numberOfMask = new int[4] { 0, 0, 0, 0 };

            foreach (CardInfo cardInfo in cardInfos)
            {
                if (cardInfo.IsStrategy) continue;
                if (!calculatedPowerTypes.Exists(powerType => powerType == cardInfo.PublicInfo.PowerType)) continue;
                if (cardInfo.IsMask) numberOfMask[(int)cardInfo.PublicInfo.PowerType] += 1 * maskMultiplier;
                totalPower += cardInfo.PublicInfo.Power;
            }

            int max_numberOfMask = Mathf.Max(numberOfMask);

            bool[] masked = new bool[cardInfos.Count];
            for (int i = 0; i < cardInfos.Count; i++) masked[i] = false;

            for (int i = 0; i < max_numberOfMask; i++)
            {
                int[] maxPowerIndex = new int[4] { -1, -1, -1, -1 };
                for (int j = 0; j < cardInfos.Count; j++)
                {

                    if (cardInfos[j].IsPower)
                        if (!masked[j])
                            if (maxPowerIndex[(int)cardInfos[j].PublicInfo.PowerType] == -1 ||
                                cardInfos[maxPowerIndex[(int)cardInfos[j].PublicInfo.PowerType]].PublicInfo.Power < cardInfos[j].PublicInfo.Power)
                                    maxPowerIndex[(int)cardInfos[j].PublicInfo.PowerType] = j;
                }
                for (int j = 0; j < 4; j++)
                {
                    if (i >= numberOfMask[j] || maxPowerIndex[j] == -1) continue;
                    if (!calculatedPowerTypes.Exists(powerType => powerType == (PowerType)j)) continue;
                    masked[maxPowerIndex[j]] = true;
                    totalPower -= cardInfos[maxPowerIndex[j]].PublicInfo.Power;
                }
            }
            return totalPower;
        }

        public static string GetChineseNameOfPowerType(PowerType powerType)
        {
            if (powerType == PowerType.Wealth) return "財富";
            if (powerType == PowerType.Industry) return "工業";
            if (powerType == PowerType.SeaPower) return "海權";
            return "軍事";
        }

        public static string GetColorStringOfPowerType(PowerType powerType)
        {
            if (powerType == PowerType.Wealth) return "418F06";
            if (powerType == PowerType.Industry) return "FBB666";
            if (powerType == PowerType.SeaPower) return "176BB8";
            return "F85757";
        }

        public static string GetChineseNameOfPublicCardClass(int publicCardClass)
        {
            if (1 <= publicCardClass && publicCardClass <= 9) return publicCardClass.ToString();
            if (publicCardClass == 11) return "清廉形象";
            if (publicCardClass == 12) return "政治改革";
            return "拓殖";
        }

        public static string GetEnglishNameOfPublicCardClass(int publicCardClass)
        {
            if (1 <= publicCardClass && publicCardClass <= 9) return publicCardClass.ToString();
            if (publicCardClass == 11) return "Mask";
            if (publicCardClass == 12) return "Reform";
            return "Expand";
        }

        public static string GetFunctionCardDecriptionString(PowerType powerType, int publicCardClass)
        {
            string powerTypeName = GetChineseNameOfPowerType(powerType);
            if (publicCardClass == 11)
                return "在革命判定及選出會議主導者時，遮蔽手牌中點數最大的「" + powerTypeName + "」權力牌的點數，持有多張清廉形象時遮蔽多張權力牌。使用時，從牌堆中抽一張牌。";
            if (publicCardClass == 12)
                return "使用時，從手牌中丟棄一張「" + powerTypeName + "」權力牌。只有在手牌中有「" + powerTypeName + "」權力牌時才能使用。";
            return "使用時，從翻開的場牌中選取一張「" + powerTypeName + "」權力牌。只有在翻開的場牌中有「" + powerTypeName + "」權力牌時才能使用。";
        }

        public static int[] GetRanks(Stack<int> revolutionStack, int victorySerial, List<ServerPlayer> serverPlayers)
        {
            int[] ret = new int[serverPlayers.Count];
            List<int> rankerPlayerSerial = new List<int>();
            List<int> rankerPower = new List<int>();
            List<int> rankerCardCount = new List<int>();
            int nowRank = 0;
            if (victorySerial != -1)
                ret[nowRank++] = victorySerial;
            for(int i=0;i< serverPlayers.Count; i++)
            {
                if (serverPlayers[i].PlayerSerial == victorySerial || serverPlayers[i].IsDead) continue;
                rankerPlayerSerial.Add(serverPlayers[i].PlayerSerial);
                rankerPower.Add(Func.CalculatePower(serverPlayers[i].HandCards, 0, new List<PowerType>(4) { PowerType.Wealth, PowerType.Industry, PowerType.SeaPower, PowerType.Military }));
                rankerCardCount.Add(serverPlayers[i].HandCards.Count);
            }

            for(int i=0;i< rankerPlayerSerial.Count;i++)
                for(int j=i+1;j< rankerPlayerSerial.Count; j++)
                    if(rankerPower[i] < rankerPower[j] || (rankerPower[i] == rankerPower[j] && rankerCardCount[i] < rankerCardCount[j]))
                    {
                        int tmp = rankerPlayerSerial[i];
                        rankerPlayerSerial[i] = rankerPlayerSerial[j];
                        rankerPlayerSerial[j] = tmp;
                        tmp = rankerPower[i];
                        rankerPower[i] = rankerPower[j];
                        rankerPower[j] = tmp;
                        tmp = rankerCardCount[i];
                        rankerCardCount[i] = rankerCardCount[j];
                        rankerCardCount[j] = tmp;
                    }

            foreach(int playerSerial in rankerPlayerSerial)
                ret[nowRank++] = playerSerial;

            while(revolutionStack.Count > 0)
                ret[nowRank++] = revolutionStack.Pop();

            return ret;
        }
    }

    public class CharacterInfo
    {
        public int NameCode;
        public string CharacterName;
        public string SkillName;
        public string SkillDescription;
        public Sprite CharacterSmallAvatar;
        public Sprite CharacterMidAvatar;

        public CharacterInfo(string characterInfoStr)
        {
            string[] characterInfos = characterInfoStr.Split(' ');
            SetValues(int.Parse(characterInfos[0]), characterInfos[1], characterInfos[2], characterInfos[3]);
        }

        public void SetValues(int nameCode, string characterName, string skillName, string skillDescription)
        {
            NameCode = nameCode;
            CharacterName = characterName;
            SkillName = skillName;
            SkillDescription = skillDescription;
            CharacterSmallAvatar = Resources.Load<Sprite>("Images/Characters/Small/" + nameCode.ToString());
            CharacterMidAvatar =   Resources.Load<Sprite>("Images/Characters/Mid/"   + nameCode.ToString());
        }
    }

    [System.Serializable]
    public class DrawPileHandler
    {
        public GameObject Pack;
        public GameObject ZeroCardGMO;
        public GameObject OneCardGMO;
        public GameObject MultipleCardsGMO;
        public Text OneAmountTXT;
        public Text MultipleAmountTXT;
        private int _Amount;
        [HideInInspector]
        public int Amount
        {
            get { return _Amount; }
            set
            {
                _Amount = value;
                OneAmountTXT.text = value.ToString();
                MultipleAmountTXT.text = value.ToString();
                ZeroCardGMO.SetActive(value == 0);
                OneCardGMO.SetActive(value == 1);
                MultipleCardsGMO.SetActive(value >= 2);
            }
        }
    }

    [System.Serializable]
    public class GravePileHandler
    {
        public GameObject Pack;
        public GameObject ZeroCardGMO;
        public GameObject OneCardGMO;
        public GameObject MultipleCardsGMO;
        public Text OneAmountTXT;
        public Text MultipleAmountTXT;
        private int _Amount;
        [HideInInspector] public int Amount
        {
            get { return _Amount; }
            set
            {
                _Amount = value;
                OneAmountTXT.text = value.ToString();
                MultipleAmountTXT.text = value.ToString();
                ZeroCardGMO.SetActive(value == 0);
                OneCardGMO.SetActive(value == 1);
                MultipleCardsGMO.SetActive(value >= 2);
            }
        }
    }
}