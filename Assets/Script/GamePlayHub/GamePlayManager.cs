using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Main;
using GamePlayHub.ClientToServer;
using GamePlayHub.ServerToClient;
using UnityEngine.UI;
using Network;
using System.Linq;

namespace GamePlayHub
{
    public class GamePlayManager : MonoBehaviour {
        public ClientController ClientController;
        public OnNextPhase OnNextPhase;
        public string Hub;

        public List<ClientPlayer> ClientPlayers = new List<ClientPlayer>();
        int _PlayerAmount;
        public int SelfPlayerSerial;

        public GameObject[] Profiles;
        int[] _ProfileIndexToPlayerSerial;
        public Button EndTurnBTN;
        public Button EndCongressBTN;
        private int _NowRound;
        public int NowRound { get { return _NowRound; } set { _NowRound = value; RoundTXT.text = value.ToString(); } }
        public Text RoundTXT;

        public GameObject CharacterPRF;
        public GameObject[] CharacterCharacterGMO;
        public Transform ChooseCharacterFolder;

        public GameObject CardPRF;
        public HandCardHandler HandCardHandler;
        public CardHandler[] DeckCardHandlers;
        public Transform DeckCardsFolder;
        public Transform ReleaseCardsFolder;
        public Transform HandCardsFolder;

        List<int> _ChooseFromCoveredCardsBuffer = new List<int>();

        public CharacterHandler CharacterDetails;
        public CardDetailHandler CardDetailHandler;

        private ClientStage _NowStage = ClientStage.EnemyTurn;
        public ClientStage NowStage
        {
            get { return _NowStage; }
            set
            {
                _NowStage = value;

                CardDetailHandler.UpdateUDButtons();
                EndTurnBTN.interactable = value == ClientStage.MidTurn;
                EndTurnBTN.gameObject.SetActive(value != ClientStage.SelfCongress);
                EndCongressBTN.gameObject.SetActive(value == ClientStage.SelfCongress);

                if (value == ClientStage.DrawCard) {
                    _ChooseFromCoveredCardsBuffer.Clear();
                    ChooseBTN.interactable = false;
                    UsedMaskAmount = 0;
                    UsedReformAmount = 0;
                    UsedExpandAmount = 0;
                    UsedStrategyAmount = 0;
                    UsedMaskLimit = 1;
                    UsedReformLimit = 1;
                    UsedExpandLimit = 1;
                    UsedStrategyLimit = 1;
                }
                ChooseBTN.gameObject.SetActive(value == ClientStage.DrawCard);

                if (value == ClientStage.SelfCongress || value == ClientStage.EnemyCongress) _IsInCongress = true;

                if(value == ClientStage.EnemyTurn)
                {
                    _CharacterNameCode4HasSecondGain = true;
                    _CharacterNameCode13SkillUsed = false;
                }


                ClientPlayer clientPlayer = GetClientPlayerFromPlayerSerial(SelfPlayerSerial);
                if (value == ClientStage.MidTurn)
                {
                    // 威廉三世，被動技，革命門檻減少８，使用「政治改革」時，效果增加１倍。 必殺技，革命門檻永久增加５，同時自身被動技永久無效。
                    if (clientPlayer.CharacterNameCode == 3)
                        if (clientPlayer.CharacterNameCode3HasPassive)
                            UpdateSkillButtons(false, false, true, true);

                    // 法蘭茲一世，必殺技，行動階段時發動，一輪會議週期內自己不會被革命，其他玩家不得使用決策牌，其他玩家不得宣布勝利。
                    if (clientPlayer.CharacterNameCode == 6)
                        if (!_CharacterNameCode6SkillUsed)
                            UpdateSkillButtons(false, false, true, true);

                    // 繆拉，主動技，指定一名玩家，奪取該玩家的一張手牌，一回合只能發動一次。
                    if (clientPlayer.CharacterNameCode == 13)
                        if (!_CharacterNameCode13SkillUsed)
                        {
                            _CharacterNameCode13DecideCharacterIndex = -1;
                            UpdateSkillButtons(true, false, true, false);
                        }
                }

                //因無法拿牌被強制跳過拿牌回合
                if (value == ClientStage.DrawCard)
                    if (!CanGetCardFromDeck())
                        NowStage = ClientStage.MidTurn;
            }
        }
        private UsingCardStage _NowUsingCardStage = UsingCardStage.None;
        public UsingCardStage NowUsingCardStage
        {
            get { return _NowUsingCardStage; }
            set
            {
                _NowUsingCardStage = value;
                if (value == UsingCardStage.None) _ChooseToReleaseFromHandCardsBuffer.Clear();
                if (value == UsingCardStage.Mask) UsedMaskAmount++;
                if (value == UsingCardStage.Reform) UsedReformAmount++;
                if (value == UsingCardStage.Expand) UsedExpandAmount++;
                if (value == UsingCardStage.Release) ReleaseBTN.interactable = false;
                ReleaseBTN.gameObject.SetActive(value == UsingCardStage.Release);
            }
        }
        public int UsingCardNameCode;
        public int UsedMaskAmount;
        public int UsedReformAmount;
        public int UsedExpandAmount;
        public int UsedStrategyAmount;
        public int UsedMaskLimit;
        public int UsedReformLimit;
        public int UsedExpandLimit;
        public int UsedStrategyLimit;
        private UsingSkillStage _NowUsingSkillStage = UsingSkillStage.None;
        public UsingSkillStage NowUsingSkillStage
        {
            get { return _NowUsingSkillStage; }
            set
            {
                _NowUsingSkillStage = value;

                if (value == UsingSkillStage.CharacterNameCode0PreparingSkill ||
                    value == UsingSkillStage.CharacterNameCode1PreparingSkill ||
                    value == UsingSkillStage.CharacterNameCode2PreparingSkill ||
                    value == UsingSkillStage.CharacterNameCode8PreparingSkill ||
                    value == UsingSkillStage.CharacterNameCode10PreparingSkill ||
                    value == UsingSkillStage.CharacterNameCode15PreparingSkill ||
                    value == UsingSkillStage.CharacterNameCode18PreparingSkill)
                    UpdateSkillButtons(true, false, false, false);

                if (value == UsingSkillStage.CharacterNameCode0ChoosingEffect ||
                    value == UsingSkillStage.CharacterNameCode15ChoosingEffect)
                    UpdateSkillButtons(true, true, false, true);

                if (value == UsingSkillStage.CharacterNameCode1ChoosingEffect ||
                    value == UsingSkillStage.CharacterNameCode2ChoosingEffect ||
                    value == UsingSkillStage.CharacterNameCode8ChoosingEffect ||
                    value == UsingSkillStage.CharacterNameCode10ChoosingEffect ||
                    value == UsingSkillStage.CharacterNameCode13ChoosingEffect ||
                    value == UsingSkillStage.CharacterNameCode18ChoosingEffect)
                    UpdateSkillButtons(true, true, false, false);

                if (value == UsingSkillStage.CharacterNameCode14PreparingSkill)
                    UpdateSkillButtons(false, false, false, true);

                // 拿破崙一世，主動技，「釋出權力」時，可以指定一張權力牌，若這張牌在其他玩家的手牌中，則立即奪取這張牌。
                CardChooserHandler.IsShow = value == UsingSkillStage.CharacterNameCode0ChoosingEffect;
            }
        }

        private UsingStrategyStage _NowUsingStrategyStage = UsingStrategyStage.None;
        public UsingStrategyStage NowUsingStrategyStage
        {
            get { return _NowUsingStrategyStage; }
            set
            {
                _NowUsingStrategyStage = value;
                if (value == UsingStrategyStage.None) _ChooseRequirementFromHandCardsBuffer.Clear();
                if (value == UsingStrategyStage.ChoosingRequirements) CardDetailHandler.CardDetailHandlerUI.OkBTN.interactable = ChosensMatchRequirement();
                CardDetailHandler.CardDetailHandlerUI.UseBTN.gameObject.SetActive(value != UsingStrategyStage.ChoosingRequirements);
                CardDetailHandler.CardDetailHandlerUI.DiscardBTN.gameObject.SetActive(value != UsingStrategyStage.ChoosingRequirements);
                CardDetailHandler.CardDetailHandlerUI.OkBTN.gameObject.SetActive(value == UsingStrategyStage.ChoosingRequirements);
                CardDetailHandler.CardDetailHandlerUI.CancelBTN.gameObject.SetActive(value == UsingStrategyStage.ChoosingRequirements);

            }
        }

        List<int> _ChooseToReleaseFromHandCardsBuffer = new List<int>();
        List<int> _ChooseRequirementFromHandCardsBuffer = new List<int>();
        CardInfo NowUsingStrategyCard = null;
        public Button ReleaseBTN;
        public Button ChooseBTN;

        public Button ChooseEffectBTN;
        public Button CastBTN;
        public Button CancelBTN;
        public Button GiveUpBTN;

        public Sprite HandCardFolder_Normal;
        public Sprite HandCardFolder_Revolution;
        public GameObject RevolutionBTN;
        public GameObject BuffPRF;
        public CardChooserHandler CardChooserHandler;
        public DrawPileHandler DrawPileHandler;
        public GravePileHandler GravePileHandler;
        public RankHandler RankHandler;
        bool _IsInCongress;
        public bool GameEnd;

        int _CharacterNameCode1ChooseOneDeckCardIndex;
        int _CharacterNameCode2ChooseOneHandCardIndex;
        int _CharacterNameCode3PreviousReformHandCardIndex;
        bool _CharacterNameCode4HasSecondGain;
        bool _CharacterNameCode6SkillUsed;
        int _CharacterNameCode8ChooseDiscardOneHandCardIndex;
        int _CharacterNameCode10ChooseDiscardOneDeckCardIndex;
        bool _CharacterNameCode13SkillUsed;
        int _CharacterNameCode13DecideCharacterIndex;
        bool _CharacterNameCode14SkillUsed;
        int _CharacterNameCode14PlaceBackCardNameCode;
        List<int> _CharacterNameCode15TempDeckMemory;
        int _CharacterNameCode15DecideChooseCardIndex;
        int _CharacterNameCode18ExpandCardNameCode;
        int _CharacterNameCode18ChooseExpandProfileIndex;

        private void Start()
        {
            DeckCardsFolder.gameObject.SetActive(false);
            HandCardHandler = new HandCardHandler();
            Send_SceneReady(new SceneReady());
            GameEnd = false;
        }

        public void Init(string hub)
        {
            Hub = hub;
        }

        // UI_Click //

        public void OnClickCast()
        {
            ClientPlayer clientPlayer = GetClientPlayerFromPlayerSerial(SelfPlayerSerial);

            // 拿破崙一世，主動技，「釋出權力」時，可以指定一張權力牌，若這張牌在其他玩家的手牌中，則立即奪取這張牌。
            if (clientPlayer.CharacterNameCode == 0)
            {
                DisactivateAllSkillButtons();
                ChangeStage(ClientStage.MidTurn);
                Send_CharacterNameCode0ChooseToRobOneCard(new CharacterNameCode0ChooseToRobOneCard(DataMaster.GetInstance().GetNameCodesOfPublicCard(
                    CardChooserHandler.PowerType, CardChooserHandler.Power)));
            }

            // 米歇爾·內伊，主動技，當其他玩家回合結束時，可以拿取場上任意一張翻開的牌，此技能一輪會議周期內只能發動一次。
            if (clientPlayer.CharacterNameCode == 1)
            {
                DeckCardHandlers[_CharacterNameCode1ChooseOneDeckCardIndex].IsChosen = false;
                GetClientPlayerFromPlayerSerial(SelfPlayerSerial).AddHandCard(DeckCardHandlers[_CharacterNameCode1ChooseOneDeckCardIndex].NameCode);
                DisactivateAllSkillButtons();
                ChangeStage(ClientStage.EnemyTurn);
                Send_CharacterNameCode1ChooseOneDeckCard(new CharacterNameCode1ChooseOneDeckCard(_CharacterNameCode1ChooseOneDeckCardIndex));
            }

            // 塔列朗，被動技，自身的每張清廉形象遮蔽二張權力牌。主動技，每當失去一張清廉形象時可以丟棄一張手牌。
            if (clientPlayer.CharacterNameCode == 2)
            {
                HandCardHandler.UnChosenAll();
                int nameCode = GetClientPlayerFromPlayerSerial(SelfPlayerSerial).SelfClientPlayerDetails.HandCards[_CharacterNameCode2ChooseOneHandCardIndex];
                GetClientPlayerFromPlayerSerial(SelfPlayerSerial).RemoveHandCard(nameCode);
                DisactivateAllSkillButtons();
                ChangeStage(ClientStage.MidTurn);
                Send_CharacterNameCode2DiscardOneHandCard(new CharacterNameCode2DiscardOneHandCard(nameCode));

                // 塔列朗，被動技，自身的每張清廉形象遮蔽二張權力牌。主動技，每當失去一張清廉形象時可以丟棄一張手牌。
                if (GetClientPlayerFromPlayerSerial(SelfPlayerSerial).CharacterNameCode == 2)
                    if(DataMaster.GetInstance().GetCardInfo(nameCode).IsMask)
                    {
                        _CharacterNameCode2ChooseOneHandCardIndex = -1;
                        ChangeStage(UsingSkillStage.CharacterNameCode2PreparingSkill);
                    }
            }

            // 威廉三世，被動技，革命門檻減少８，使用「政治改革」時，效果增加１倍。 必殺技，革命門檻永久增加５，同時自身被動技永久無效。
            if (clientPlayer.CharacterNameCode == 3)
            {
                clientPlayer.CharacterNameCode3HasPassive = false;
                DisactivateAllSkillButtons();
                ChangeStage(ClientStage.MidTurn);
                Send_CharacterNameCode3UseSkill(new CharacterNameCode3UseSkill());
            }

            // 法蘭茲一世，必殺技，行動階段時發動，一輪會議週期內自己不會被革命，其他玩家不得使用決策牌，其他玩家不得宣布勝利。
            if (clientPlayer.CharacterNameCode == 6)
            {
                _CharacterNameCode6SkillUsed = true;
                DisactivateAllSkillButtons();
                ChangeStage(ClientStage.MidTurn);
                Send_CharacterNameCode6UseSkill(new CharacterNameCode6UseSkill());
            }

            // 卡爾大公，主動技，回合結束後，當自己的點數在「自己革命門檻減４」以上時，可以丟棄一張手牌，一回合只能發動一次。
            if (clientPlayer.CharacterNameCode == 8)
            {
                HandCardHandler.UnChosenAll();
                int nameCode = clientPlayer.SelfClientPlayerDetails.HandCards[_CharacterNameCode8ChooseDiscardOneHandCardIndex];
                clientPlayer.RemoveHandCard(nameCode);
                DisactivateAllSkillButtons();
                ChangeStage(ClientStage.EnemyTurn);
                Send_CharacterNameCode8DiscardOneHandCard(new CharacterNameCode8DiscardOneHandCard(nameCode));
                Send_EndTurn(new EndTurn());
            }

            // 巴克萊·德托利，主動技，回合結束時，可以把一張場牌放入棄牌堆，一回合只能發動一次。
            if (clientPlayer.CharacterNameCode == 10)
            {
                DeckCardHandlers[_CharacterNameCode10ChooseDiscardOneDeckCardIndex].IsChosen = false;
                DisactivateAllSkillButtons();
                ChangeStage(ClientStage.EnemyTurn);
                Send_CharacterNameCode10DiscardOneDeckCard(new CharacterNameCode10DiscardOneDeckCard(_CharacterNameCode10ChooseDiscardOneDeckCardIndex));
                Send_EndTurn(new EndTurn());
            }

            // 繆拉，主動技，指定一名玩家，奪取該玩家的一張手牌，一回合只能發動一次。
            if (clientPlayer.CharacterNameCode == 13)
            {
                _CharacterNameCode13SkillUsed = true;
                if (_CharacterNameCode13DecideCharacterIndex != -1)
                    GetClientPlayerFromPlayerSerial(_ProfileIndexToPlayerSerial[_CharacterNameCode13DecideCharacterIndex]).IsChosen = false;
                DisactivateAllSkillButtons();
                ChangeStage(ClientStage.MidTurn);
                Send_CharacterNameCode13RobOneHandCard(new CharacterNameCode13RobOneHandCard(_ProfileIndexToPlayerSerial[_CharacterNameCode13DecideCharacterIndex]));
            }

            // 路易十八，主動技，抽牌階段只選擇一張牌時，可以將其翻開放回場上。一次會議周期內只能發動一次。
            if (clientPlayer.CharacterNameCode == 14)
            {
                _CharacterNameCode14SkillUsed = true;
                DisactivateAllSkillButtons();
                GetClientPlayerFromPlayerSerial(SelfPlayerSerial).RemoveHandCard(_CharacterNameCode14PlaceBackCardNameCode);
                ChangeStage(ClientStage.MidTurn);
                Send_CharacterNameCode14PlaceBackDeckCard(new CharacterNameCode14PlaceBackDeckCard(_CharacterNameCode14PlaceBackCardNameCode));
            }

            // 卡爾·約翰，被動技，抽牌階段選擇覆蓋的牌時，可以觀看最多三張；主動技，剩餘的牌放回桌面時可以不翻開。
            if (clientPlayer.CharacterNameCode == 15)
            {
                foreach (int index in _ChooseFromCoveredCardsBuffer) DeckCardHandlers[index].IsChosen = false;
                DisactivateAllSkillButtons();
                ChangeStage(ClientStage.MidTurn);
                Send_DecideCardFromRevealedCoveredCards(new DecideCardFromRevealedCoveredCards(_CharacterNameCode15DecideChooseCardIndex, _ChooseFromCoveredCardsBuffer, DeckCardHandlers));
            }


            // 吉爾伯特，主動技，使用「拓殖」時，指定一名玩家，將拓殖得到的牌交給該玩家。
            if (clientPlayer.CharacterNameCode == 18)
            {
                GetClientPlayerFromPlayerSerial(_ProfileIndexToPlayerSerial[_CharacterNameCode18ChooseExpandProfileIndex]).IsChosen = false;
                DisactivateAllSkillButtons();
                ChangeStage(ClientStage.MidTurn);
                Send_CharacterNameCode18ExpandDeckCardToEnemy(
                    new CharacterNameCode18ExpandDeckCardToEnemy(_ProfileIndexToPlayerSerial[_CharacterNameCode18ChooseExpandProfileIndex], _CharacterNameCode18ExpandCardNameCode));
            }
        }

        public void OnClickChooseEffect()
        {
            ClientPlayer clientPlayer = GetClientPlayerFromPlayerSerial(SelfPlayerSerial);

            // 拿破崙一世，主動技，「釋出權力」時，可以指定一張權力牌，若這張牌在其他玩家的手牌中，則立即奪取這張牌。
            if (clientPlayer.CharacterNameCode == 0)
                ChangeStage(UsingSkillStage.CharacterNameCode0ChoosingEffect);

            // 米歇爾·內伊，主動技，當其他玩家回合結束時，可以拿取場上任意一張翻開的牌，此技能一輪會議周期內只能發動一次。
            if (clientPlayer.CharacterNameCode == 1)
                ChangeStage(UsingSkillStage.CharacterNameCode1ChoosingEffect);

            // 塔列朗，被動技，自身的每張清廉形象遮蔽二張權力牌。主動技，每當失去一張清廉形象時可以丟棄一張手牌。
            if (clientPlayer.CharacterNameCode == 2)
                ChangeStage(UsingSkillStage.CharacterNameCode2ChoosingEffect);

            // 卡爾大公，主動技，回合結束後，當自己的點數在「自己革命門檻減４」以上時，可以丟棄一張手牌，一回合只能發動一次。
            if (clientPlayer.CharacterNameCode == 8)
                ChangeStage(UsingSkillStage.CharacterNameCode8ChoosingEffect);

            // 巴克萊·德托利，主動技，回合結束時，可以把一張場牌放入棄牌堆，一回合只能發動一次。
            if (clientPlayer.CharacterNameCode == 10)
                ChangeStage(UsingSkillStage.CharacterNameCode10ChoosingEffect);

            // 繆拉，主動技，指定一名玩家，奪取該玩家的一張手牌，一回合只能發動一次。
            if (clientPlayer.CharacterNameCode == 13)
                ChangeStage(UsingSkillStage.CharacterNameCode13ChoosingEffect);

            // 卡爾·約翰，被動技，抽牌階段選擇覆蓋的牌時，可以觀看最多三張；主動技，剩餘的牌放回桌面時可以不翻開。
            if (clientPlayer.CharacterNameCode == 15)
                ChangeStage(UsingSkillStage.CharacterNameCode15ChoosingEffect);

            // 吉爾伯特，主動技，使用「拓殖」時，指定一名玩家，將拓殖得到的牌交給該玩家。
            if (clientPlayer.CharacterNameCode == 18)
                ChangeStage(UsingSkillStage.CharacterNameCode18ChoosingEffect);
        }

        public void OnClickCancel()
        {
            ClientPlayer clientPlayer = GetClientPlayerFromPlayerSerial(SelfPlayerSerial);

            // 拿破崙一世，主動技，「釋出權力」時，可以指定一張權力牌，若這張牌在其他玩家的手牌中，則立即奪取這張牌。
            if (clientPlayer.CharacterNameCode == 0)
                ChangeStage(UsingSkillStage.CharacterNameCode0PreparingSkill);

            // 米歇爾·內伊，主動技，當其他玩家回合結束時，可以拿取場上任意一張翻開的牌，此技能一輪會議周期內只能發動一次。
            if (clientPlayer.CharacterNameCode == 1)
            {
                if (_CharacterNameCode1ChooseOneDeckCardIndex != -1)
                    DeckCardHandlers[_CharacterNameCode1ChooseOneDeckCardIndex].IsChosen = false;
                ChangeStage(UsingSkillStage.CharacterNameCode1PreparingSkill);
            }

            // 塔列朗，被動技，自身的每張清廉形象遮蔽二張權力牌。主動技，每當失去一張清廉形象時可以丟棄一張手牌。
            if (clientPlayer.CharacterNameCode == 2)
            {
                HandCardHandler.UnChosenAll();
                ChangeStage(UsingSkillStage.CharacterNameCode2PreparingSkill);
            }

            // 卡爾大公，主動技，回合結束後，當自己的點數在「自己革命門檻減４」以上時，可以丟棄一張手牌，一回合只能發動一次。
            if (clientPlayer.CharacterNameCode == 8)
            {
                if (_CharacterNameCode8ChooseDiscardOneHandCardIndex != -1)
                    HandCardHandler.ChangeHandCardIsChosen(_CharacterNameCode8ChooseDiscardOneHandCardIndex, false);
                ChangeStage(UsingSkillStage.CharacterNameCode8PreparingSkill);
            }

            // 巴克萊·德托利，主動技，回合結束時，可以把一張場牌放入棄牌堆，一回合只能發動一次。
            if (clientPlayer.CharacterNameCode == 10)
            {
                if (_CharacterNameCode10ChooseDiscardOneDeckCardIndex != -1)
                    DeckCardHandlers[_CharacterNameCode10ChooseDiscardOneDeckCardIndex].IsChosen = false;
                ChangeStage(UsingSkillStage.CharacterNameCode10PreparingSkill);
            }

            // 繆拉，主動技，指定一名玩家，奪取該玩家的一張手牌，一回合只能發動一次。
            if (clientPlayer.CharacterNameCode == 13)
            {
                if (_CharacterNameCode13DecideCharacterIndex != -1)
                    GetClientPlayerFromPlayerSerial(_ProfileIndexToPlayerSerial[_CharacterNameCode13DecideCharacterIndex]).IsChosen = false;
                ChangeStage(ClientStage.MidTurn);
            }

            // 卡爾·約翰，被動技，抽牌階段選擇覆蓋的牌時，可以觀看最多三張；主動技，剩餘的牌放回桌面時可以不翻開。
            if (clientPlayer.CharacterNameCode == 15)
            {
                for (int i = 0; i < DeckCardHandlers.Length; i++) DeckCardHandlers[i].NameCode = _CharacterNameCode15TempDeckMemory[i];
                ChangeStage(UsingSkillStage.CharacterNameCode15PreparingSkill);
            }

            // 吉爾伯特，主動技，使用「拓殖」時，指定一名玩家，將拓殖得到的牌交給該玩家。
            if (clientPlayer.CharacterNameCode == 18)
            {
                if (_CharacterNameCode18ChooseExpandProfileIndex != -1)
                    GetClientPlayerFromPlayerSerial(_ProfileIndexToPlayerSerial[_CharacterNameCode18ChooseExpandProfileIndex]).IsChosen = false;
                ChangeStage(UsingSkillStage.CharacterNameCode18PreparingSkill);
            }
        }

        public void OnClickGiveUp()
        {
            ClientPlayer clientPlayer = GetClientPlayerFromPlayerSerial(SelfPlayerSerial);

            // 拿破崙一世，主動技，「釋出權力」時，可以指定一張權力牌，若這張牌在其他玩家的手牌中，則立即奪取這張牌。
            if (clientPlayer.CharacterNameCode == 0)
            {
                DisactivateAllSkillButtons();
                ChangeStage(ClientStage.MidTurn);
            }

            // 米歇爾·內伊，主動技，當其他玩家回合結束時，可以拿取場上任意一張翻開的牌，此技能一輪會議周期內只能發動一次。
            if (clientPlayer.CharacterNameCode == 1)
            {
                DisactivateAllSkillButtons();
                ChangeStage(ClientStage.EnemyTurn);
                Send_CharacterNameCode1ChooseOneDeckCard(new CharacterNameCode1ChooseOneDeckCard(-1));
            }

            // 塔列朗，被動技，自身的每張清廉形象遮蔽二張權力牌。主動技，每當失去一張清廉形象時可以丟棄一張手牌。
            if (clientPlayer.CharacterNameCode == 2)
            {
                DisactivateAllSkillButtons();
                ChangeStage(ClientStage.MidTurn);
            }

            // 卡爾大公，主動技，回合結束後，當自己的點數在「自己革命門檻減４」以上時，可以丟棄一張手牌，一回合只能發動一次。
            if (clientPlayer.CharacterNameCode == 8)
            {
                DisactivateAllSkillButtons();
                ChangeStage(ClientStage.EnemyTurn);
                Send_EndTurn(new EndTurn());
            }

            // 巴克萊·德托利，主動技，回合結束時，可以把一張場牌放入棄牌堆，一回合只能發動一次。
            if (clientPlayer.CharacterNameCode == 10)
            {
                DisactivateAllSkillButtons();
                ChangeStage(ClientStage.EnemyTurn);
                Send_EndTurn(new EndTurn());
            }

            // 路易十八，主動技，抽牌階段只選擇一張牌時，可以將其翻開放回場上。一次會議周期內只能發動一次。
            if (clientPlayer.CharacterNameCode == 14)
            {
                DisactivateAllSkillButtons();
                ChangeStage(ClientStage.MidTurn);
            }

            // 卡爾·約翰，被動技，抽牌階段選擇覆蓋的牌時，可以觀看最多三張；主動技，剩餘的牌放回桌面時可以不翻開。
            if (clientPlayer.CharacterNameCode == 15)
            {
                foreach (int index in _ChooseFromCoveredCardsBuffer) DeckCardHandlers[index].IsChosen = false;
                DisactivateAllSkillButtons();
                ChangeStage(ClientStage.MidTurn);
                Send_DecideCardFromRevealedCoveredCards(new DecideCardFromRevealedCoveredCards(_CharacterNameCode15DecideChooseCardIndex, _ChooseFromCoveredCardsBuffer, DeckCardHandlers));
            }

            // 吉爾伯特，主動技，使用「拓殖」時，指定一名玩家，將拓殖得到的牌交給該玩家。
            if (clientPlayer.CharacterNameCode == 18)
            {
                DisactivateAllSkillButtons();
                ChangeStage(ClientStage.MidTurn);
            }
        }

        public void OnClickRelease() {
            if (ChosensCanRelease())
            {
                int[] nameCodes = new int[_ChooseToReleaseFromHandCardsBuffer.Count];
                for (int i = 0; i < _ChooseToReleaseFromHandCardsBuffer.Count; i++)
                {
                    HandCardHandler.ChangeHandCardIsChosen(_ChooseToReleaseFromHandCardsBuffer[i], false);
                    nameCodes[i] = GetClientPlayerFromPlayerSerial(SelfPlayerSerial).SelfClientPlayerDetails.HandCards[_ChooseToReleaseFromHandCardsBuffer[i]];
                }
                GetClientPlayerFromPlayerSerial(SelfPlayerSerial).RemoveHandCard(nameCodes);
                Send_ReleasePower(new ReleasePower(nameCodes));


                // 拿破崙一世，主動技，「釋出權力」時，可以指定一張權力牌，若這張牌在其他玩家的手牌中，則立即奪取這張牌。
                if (GetClientPlayerFromPlayerSerial(SelfPlayerSerial).CharacterNameCode == 0)
                {
                    ChangeStage(UsingSkillStage.CharacterNameCode0PreparingSkill);
                    return;
                }

                ChangeStage(ClientStage.MidTurn);
            }
        }

        public void OnClickCardDetailHandlerUseButton(int nameCode)
        {
            CardInfo cardInfo = DataMaster.GetInstance().GetCardInfo(nameCode);
            if (NowStage == ClientStage.MidTurn && cardInfo.IsFunction)
            { // function card
                if (cardInfo.IsMask && UsedMaskAmount < UsedMaskLimit)
                {
                    ChangeStage(UsingCardStage.Mask);
                    ChangeStage(ClientStage.MidTurn);
                    CardDetailHandler.NameCode = -1;
                    GetClientPlayerFromPlayerSerial(SelfPlayerSerial).RemoveHandCard(nameCode);
                    Send_UseFunctionCard_Mask(new UseFunctionCard_Mask(nameCode));

                    // 塔列朗，被動技，自身的每張清廉形象遮蔽二張權力牌。主動技，每當失去一張清廉形象時可以丟棄一張手牌。
                    if (GetClientPlayerFromPlayerSerial(SelfPlayerSerial).CharacterNameCode == 2)
                    {
                        _CharacterNameCode2ChooseOneHandCardIndex = -1;
                        ChangeStage(UsingSkillStage.CharacterNameCode2PreparingSkill);
                    }
                }
                if (cardInfo.IsReform && UsedReformAmount < UsedReformLimit)
                {
                    if (HasCertainPowerTypePowerCardInHand(cardInfo.PublicInfo.PowerType))
                    {
                        UsingCardNameCode = nameCode;
                        ChangeStage(UsingCardStage.Reform);
                    }
                }
                if (cardInfo.IsExpand && UsedExpandAmount < UsedExpandLimit)
                {
                    if (HasCertainPowerTypePowerCardInDeck(cardInfo.PublicInfo.PowerType))
                    {
                        UsingCardNameCode = nameCode;
                        ChangeStage(UsingCardStage.Expand);
                    }
                }
            }
            else if ((NowStage == ClientStage.MidTurn || NowStage == ClientStage.SelfCongress) && !GetClientPlayerFromPlayerSerial(SelfPlayerSerial).BuffHandler.HasBuff(Buff.StrategyRestriction) && cardInfo.IsStrategy && UsedStrategyAmount < UsedStrategyLimit)
            {
                NowUsingStrategyCard = cardInfo;
                ChangeStage(UsingStrategyStage.ChoosingRequirements);
            }
        }

        public void OnClickCardDetailHandlerDiscardButton(int nameCode)
        {
            if (!(NowStage == ClientStage.MidTurn || NowStage == ClientStage.SelfCongress)) return;
            CardDetailHandler.NameCode = -1;
            GetClientPlayerFromPlayerSerial(SelfPlayerSerial).RemoveHandCard(nameCode);
            Send_DiscardStrategyCard(new DiscardStrategyCard(nameCode));
        }

        public void OnClickCardDetailHandlerOkButton()
        {
            if (NowStage == ClientStage.UsingCard && NowUsingCardStage == UsingCardStage.Strategy && NowUsingStrategyStage == UsingStrategyStage.ChoosingRequirements)
            {
                if (ChosensMatchRequirement())
                {
                    UsedStrategyAmount++;
                    int[] nameCodes = new int[_ChooseRequirementFromHandCardsBuffer.Count];
                    for (int i = 0; i < _ChooseRequirementFromHandCardsBuffer.Count; i++)
                    {
                        HandCardHandler.ChangeHandCardIsChosen(_ChooseRequirementFromHandCardsBuffer[i], false);
                        nameCodes[i] = GetClientPlayerFromPlayerSerial(SelfPlayerSerial).SelfClientPlayerDetails.HandCards[_ChooseRequirementFromHandCardsBuffer[i]];
                    }
                    GetClientPlayerFromPlayerSerial(SelfPlayerSerial).RemoveHandCard(nameCodes);
                    GetClientPlayerFromPlayerSerial(SelfPlayerSerial).RemoveHandCard(NowUsingStrategyCard.NameCode);

                    Send_UseStrategyCard_Requirements(new UseStrategyCard_Requirements(NowUsingStrategyCard.NameCode, nameCodes));

                    CardDetailHandler.NameCode = -1;
                    CharacterDetails.NameCode = -1;
                    ChangeStage(UsingStrategyStage.WaitingServer);
                }
            }
        }

        public void OnClickCardDetailHandlerCancelButton()
        {
            if (NowStage == ClientStage.UsingCard && NowUsingCardStage == UsingCardStage.Strategy && NowUsingStrategyStage == UsingStrategyStage.ChoosingRequirements)
            {
                for (int i = 0; i < _ChooseRequirementFromHandCardsBuffer.Count; i++)
                    HandCardHandler.ChangeHandCardIsChosen(_ChooseRequirementFromHandCardsBuffer[i], false);
                ChangeStage(_IsInCongress ? ClientStage.SelfCongress : ClientStage.MidTurn);
            }
        }

        public void OnClickChooseButton()
        {
            if (NowStage == ClientStage.DrawCard)
            {
                ChangeStage(ClientStage.ChooseFromRevealedCard);
                Send_RevealSeveralCoveredCard(new RevealSeveralCoveredCards(_ChooseFromCoveredCardsBuffer));
            }
        }

        public void OnClickEndTurnButton()
        {
            // check if using card or skill //
            if (NowStage != ClientStage.MidTurn) return;
            DisactivateAllSkillButtons();

            ClientPlayer clientPlayer = GetClientPlayerFromPlayerSerial(SelfPlayerSerial);

            // 卡爾大公，主動技，回合結束後，當自己的點數在「自己革命門檻減４」以上時，可以丟棄一張手牌，一回合只能發動一次。
            if (clientPlayer.CharacterNameCode == 8)
                if (clientPlayer.PowerLimit - 4 <= clientPlayer.Power && clientPlayer.Power < clientPlayer.PowerLimit)
                {
                    ChangeStage(UsingSkillStage.CharacterNameCode8PreparingSkill);
                    _CharacterNameCode8ChooseDiscardOneHandCardIndex = -1;
                    return;
                }

            // 巴克萊·德托利，主動技，回合結束時，可以把一張場牌放入棄牌堆，一回合只能發動一次。
            if (clientPlayer.CharacterNameCode == 10)
            {
                ChangeStage(UsingSkillStage.CharacterNameCode10PreparingSkill);
                _CharacterNameCode10ChooseDiscardOneDeckCardIndex = -1;
                return;
            }

            ChangeStage(ClientStage.EnemyTurn);
            Send_EndTurn(new EndTurn());
        }

        public void OnClickEndCongressButton()
        {
            // check if using strategy //
            if (NowStage != ClientStage.SelfCongress) return;

            ChangeStage(ClientStage.EnemyCongress);
            Send_EndCongressTurn(new EndCongressTurn());
        }

        public void OnMouseUpCardHandler(CardPlace cardPlace, int cardIndex, int nameCode, CardInfo cardInfo, bool isCovered, bool isChosen)
        {
            if (nameCode == -2) return;

            // deck //
            if (cardPlace == CardPlace.Deck)
            {
                if (isCovered && NowStage == ClientStage.DrawCard) ChooseFromCoveredCards(cardIndex);
                if (!isCovered && NowStage == ClientStage.DrawCard)
                {
                    // check restricts
                    ClientPlayer selfPlayer = GetClientPlayerFromPlayerSerial(SelfPlayerSerial);
                    if (!selfPlayer.CanGainCard(cardInfo))
                        return;

                    // 馮·布呂歇爾，被動技，必須進行二次抽牌階段。
                    if (selfPlayer.CharacterNameCode == 4)
                        if (_CharacterNameCode4HasSecondGain == true)
                        {
                            _CharacterNameCode4HasSecondGain = false;
                            GetClientPlayerFromPlayerSerial(SelfPlayerSerial).AddHandCard(nameCode);
                            Send_DecideCardFromOpenCard(new DecideCardFromOpenCard(cardIndex));
                            ChangeStage(ClientStage.DrawCard);
                            return;
                        }

                    ChangeStage(ClientStage.MidTurn);
                    GetClientPlayerFromPlayerSerial(SelfPlayerSerial).AddHandCard(nameCode);
                    Send_DecideCardFromOpenCard(new DecideCardFromOpenCard(cardIndex));

                    // 路易十八，主動技，抽牌階段只選擇一張牌時，可以將其翻開放回場上。一次會議周期內只能發動一次。
                    if (GetClientPlayerFromPlayerSerial(SelfPlayerSerial).CharacterNameCode == 14)
                        if (!_CharacterNameCode14SkillUsed)
                        {
                            ChangeStage(UsingSkillStage.CharacterNameCode14PreparingSkill);
                            _CharacterNameCode14PlaceBackCardNameCode = nameCode;
                        }
                }
                if (isChosen && NowStage == ClientStage.ChooseFromRevealedCard)
                {
                    // 卡爾·約翰，被動技，抽牌階段選擇覆蓋的牌時，可以觀看最多三張；主動技，剩餘的牌放回桌面時可以不翻開。
                    if (GetClientPlayerFromPlayerSerial(SelfPlayerSerial).CharacterNameCode == 15) {
                        ChangeStage(UsingSkillStage.CharacterNameCode15PreparingSkill);
                        GetClientPlayerFromPlayerSerial(SelfPlayerSerial).AddHandCard(nameCode);
                        DeckCardHandlers[cardIndex].NameCode = -2;
                        _CharacterNameCode15DecideChooseCardIndex = cardIndex;
                        _CharacterNameCode15TempDeckMemory = new List<int>();
                        foreach (CardHandler cardHandler in DeckCardHandlers) _CharacterNameCode15TempDeckMemory.Add(cardHandler.NameCode);
                        return;
                    }


                    // 馮·布呂歇爾，被動技，必須進行二次抽牌階段。
                    if (GetClientPlayerFromPlayerSerial(SelfPlayerSerial).CharacterNameCode == 4)
                        if (_CharacterNameCode4HasSecondGain == true)
                        {
                            _CharacterNameCode4HasSecondGain = false;
                            GetClientPlayerFromPlayerSerial(SelfPlayerSerial).AddHandCard(nameCode);
                            foreach (CardHandler deckCardHandler in DeckCardHandlers) deckCardHandler.IsChosen = false;
                            Send_DecideCardFromRevealedCoveredCards(new DecideCardFromRevealedCoveredCards(cardIndex, _ChooseFromCoveredCardsBuffer));
                            ChangeStage(ClientStage.DrawCard);
                            return;
                        }

                    ChangeStage(ClientStage.MidTurn);
                    GetClientPlayerFromPlayerSerial(SelfPlayerSerial).AddHandCard(nameCode);
                    foreach (CardHandler deckCardHandler in DeckCardHandlers) deckCardHandler.IsChosen = false;
                    Send_DecideCardFromRevealedCoveredCards(new DecideCardFromRevealedCoveredCards(cardIndex, _ChooseFromCoveredCardsBuffer));

                    // 路易十八，主動技，抽牌階段只選擇一張牌時，可以將其翻開放回場上。一次會議周期內只能發動一次。
                    if (GetClientPlayerFromPlayerSerial(SelfPlayerSerial).CharacterNameCode == 14)
                        if(_ChooseFromCoveredCardsBuffer.Count == 1)
                            if (!_CharacterNameCode14SkillUsed)
                            {
                                ChangeStage(UsingSkillStage.CharacterNameCode14PreparingSkill);
                                _CharacterNameCode14PlaceBackCardNameCode = nameCode;
                            }
                }
                if (!isCovered && NowUsingCardStage == UsingCardStage.Expand)
                {   //expand//
                    if (cardInfo.IsPower)
                    {
                        CardInfo usingCardInfo = DataMaster.GetInstance().GetCardInfo(UsingCardNameCode);
                        if (cardInfo.PublicInfo.PowerType != usingCardInfo.PublicInfo.PowerType) return;

                        ChangeStage(ClientStage.MidTurn);
                        CardDetailHandler.NameCode = -1;
                        GetClientPlayerFromPlayerSerial(SelfPlayerSerial).RemoveHandCard(UsingCardNameCode);
                        GetClientPlayerFromPlayerSerial(SelfPlayerSerial).AddHandCard(nameCode);
                        Send_UseFunctionCard_Expand(new UseFunctionCard_Expand(UsingCardNameCode, cardIndex));

                        // 吉爾伯特，主動技，使用「拓殖」時，指定一名玩家，將拓殖得到的牌交給該玩家。
                        if (GetClientPlayerFromPlayerSerial(SelfPlayerSerial).CharacterNameCode == 18)
                        {
                            ChangeStage(UsingSkillStage.CharacterNameCode18PreparingSkill);
                            _CharacterNameCode18ExpandCardNameCode = nameCode;
                            _CharacterNameCode18ChooseExpandProfileIndex = -1;
                        }
                    }
                }

                // 卡爾·約翰，被動技，抽牌階段選擇覆蓋的牌時，可以觀看最多三張；主動技，剩餘的牌放回桌面時可以不翻開。
                if (NowUsingSkillStage == UsingSkillStage.CharacterNameCode15ChoosingEffect) {
                    if (DeckCardHandlers[cardIndex].IsChosen) {
                        if (DeckCardHandlers[cardIndex].NameCode == -1) DeckCardHandlers[cardIndex].NameCode = _CharacterNameCode15TempDeckMemory[cardIndex];
                        else DeckCardHandlers[cardIndex].NameCode = -1;
                    }
                }

                // 米歇爾·內伊，主動技，當其他玩家回合結束時，可以拿取場上任意一張翻開的牌，此技能一輪會議周期內只能發動一次。
                if (GetClientPlayerFromPlayerSerial(SelfPlayerSerial).CharacterNameCode == 1)
                    if (NowUsingSkillStage == UsingSkillStage.CharacterNameCode1ChoosingEffect)
                        if (!isCovered)
                        {
                            if (_CharacterNameCode1ChooseOneDeckCardIndex != -1)
                                DeckCardHandlers[_CharacterNameCode1ChooseOneDeckCardIndex].IsChosen = false;
                            _CharacterNameCode1ChooseOneDeckCardIndex = cardIndex;
                            DeckCardHandlers[cardIndex].IsChosen = true;
                            UpdateSkillButtons(true, true, false, _CharacterNameCode1ChooseOneDeckCardIndex != -1);
                        }

                // 巴克萊·德托利，主動技，回合結束時，可以把一張場牌放入棄牌堆，一回合只能發動一次。
                if (GetClientPlayerFromPlayerSerial(SelfPlayerSerial).CharacterNameCode == 10)
                    if (NowUsingSkillStage == UsingSkillStage.CharacterNameCode10ChoosingEffect)
                    {
                        if (_CharacterNameCode10ChooseDiscardOneDeckCardIndex != -1)
                            DeckCardHandlers[_CharacterNameCode10ChooseDiscardOneDeckCardIndex].IsChosen = false;
                        _CharacterNameCode10ChooseDiscardOneDeckCardIndex = cardIndex;
                        DeckCardHandlers[cardIndex].IsChosen = true;
                        UpdateSkillButtons(true, true, false, _CharacterNameCode10ChooseDiscardOneDeckCardIndex != -1);
                    }
            }

            // hand //
            if (cardPlace == CardPlace.Hand)
            {
                if (NowStage != ClientStage.UsingCard && NowStage != ClientStage.UsingSkill)
                {
                    if ((cardInfo.IsStrategy || cardInfo.IsFunction)) {
                        // display to detail
                        CharacterDetails.NameCode = -1;
                        CardDetailHandler.NameCode = nameCode;
                    } else if (NowStage == ClientStage.MidTurn) {
                        //try to release, 100% power card here
                        if (PossibleToRelease())
                        {
                            ChangeStage(UsingCardStage.Release);
                            ChooseToReleaseFromHandCard(cardIndex);
                        }
                    }
                } else if (NowStage == ClientStage.UsingCard)
                {
                    if (NowUsingCardStage == UsingCardStage.Release && cardInfo.IsPower)
                    {
                        ChooseToReleaseFromHandCard(cardIndex);
                    }
                    else if (NowUsingCardStage == UsingCardStage.Reform)
                    { // reform //

                        if (cardInfo.IsPower)
                        {
                            // 威廉三世，被動技，革命門檻減少８，使用「政治改革」時，效果增加１倍。 必殺技，革命門檻永久增加５，同時自身被動技永久無效。
                            if (GetClientPlayerFromPlayerSerial(SelfPlayerSerial).CharacterNameCode == 3)
                                if (GetClientPlayerFromPlayerSerial(SelfPlayerSerial).CharacterNameCode3HasPassive)
                                {
                                    if (_CharacterNameCode3PreviousReformHandCardIndex == -1)
                                    {
                                        _CharacterNameCode3PreviousReformHandCardIndex = cardIndex;
                                        HandCardHandler.ChangeHandCardIsChosen(cardIndex, true);
                                    }
                                    else
                                    {
                                        if (_CharacterNameCode3PreviousReformHandCardIndex == cardIndex)
                                        {
                                            _CharacterNameCode3PreviousReformHandCardIndex = -1;
                                            HandCardHandler.ChangeHandCardIsChosen(cardIndex, false);
                                        }
                                        else
                                        {
                                            HandCardHandler.ChangeHandCardIsChosen(_CharacterNameCode3PreviousReformHandCardIndex, false);

                                            ChangeStage(ClientStage.MidTurn);
                                            CardDetailHandler.NameCode = -1;
                                            Send_UseFunctionCard_Reform(new UseFunctionCard_Reform(UsingCardNameCode, new int[2] {
                                                GetClientPlayerFromPlayerSerial(SelfPlayerSerial).SelfClientPlayerDetails.HandCards[_CharacterNameCode3PreviousReformHandCardIndex],
                                                GetClientPlayerFromPlayerSerial(SelfPlayerSerial).SelfClientPlayerDetails.HandCards[cardIndex]
                                            }));
                                            GetClientPlayerFromPlayerSerial(SelfPlayerSerial).RemoveHandCard(new int[3] { UsingCardNameCode, GetClientPlayerFromPlayerSerial(SelfPlayerSerial).SelfClientPlayerDetails.HandCards[_CharacterNameCode3PreviousReformHandCardIndex], nameCode });
                                            _CharacterNameCode3PreviousReformHandCardIndex = -1;
                                        }
                                    }
                                    return;
                                }

                            CardInfo usingCardInfo = DataMaster.GetInstance().GetCardInfo(UsingCardNameCode);
                            if (cardInfo.PublicInfo.PowerType != usingCardInfo.PublicInfo.PowerType) return;

                            ChangeStage(ClientStage.MidTurn);
                            CardDetailHandler.NameCode = -1;
                            GetClientPlayerFromPlayerSerial(SelfPlayerSerial).RemoveHandCard(new int[2] { UsingCardNameCode, nameCode });
                            Send_UseFunctionCard_Reform(new UseFunctionCard_Reform(UsingCardNameCode, nameCode));
                        }
                    } else if (NowUsingCardStage == UsingCardStage.Strategy)
                    {
                        // choosing strategy requirements
                        if (NowUsingStrategyStage == UsingStrategyStage.ChoosingRequirements)
                            if ((cardInfo.IsPower))
                                ChooseRequirementFromHandCard(cardIndex);

                        if (NowUsingStrategyStage == UsingStrategyStage.StrategyNameCode22ChooseDiscardOneHandCard)
                        {
                            if (cardInfo.IsStrategy) return;

                            CardDetailHandler.NameCode = -1;
                            GetClientPlayerFromPlayerSerial(SelfPlayerSerial).RemoveHandCard(nameCode);
                            Send_StrategyNameCode22DecideDiscardOneHandCard(new StrategyNameCode22DecideDiscardOneHandCard(nameCode));
                        }
                    }
                }

                // 塔列朗，被動技，自身的每張清廉形象遮蔽二張權力牌。主動技，每當失去一張清廉形象時可以丟棄一張手牌。
                if (GetClientPlayerFromPlayerSerial(SelfPlayerSerial).CharacterNameCode == 2)
                    if (NowUsingSkillStage == UsingSkillStage.CharacterNameCode2ChoosingEffect)
                        if (cardInfo.IsPublic)
                        {
                            if (_CharacterNameCode2ChooseOneHandCardIndex != -1)
                                HandCardHandler.ChangeHandCardIsChosen(_CharacterNameCode2ChooseOneHandCardIndex, false);
                            _CharacterNameCode2ChooseOneHandCardIndex = cardIndex;
                            HandCardHandler.ChangeHandCardIsChosen(cardIndex, true);
                            UpdateSkillButtons(true, true, false, _CharacterNameCode2ChooseOneHandCardIndex != -1);
                        }

                // 卡爾大公，主動技，回合結束後，當自己的點數在「自己革命門檻減４」以上時，可以丟棄一張手牌，一回合只能發動一次。
                if (GetClientPlayerFromPlayerSerial(SelfPlayerSerial).CharacterNameCode == 8)
                    if (NowUsingSkillStage == UsingSkillStage.CharacterNameCode8ChoosingEffect)
                    {
                        if (_CharacterNameCode8ChooseDiscardOneHandCardIndex != -1)
                            HandCardHandler.ChangeHandCardIsChosen(_CharacterNameCode8ChooseDiscardOneHandCardIndex, false);
                        _CharacterNameCode8ChooseDiscardOneHandCardIndex = cardIndex;
                        HandCardHandler.ChangeHandCardIsChosen(cardIndex, true);
                        UpdateSkillButtons(true, true, false, _CharacterNameCode8ChooseDiscardOneHandCardIndex != -1);
                    }
            }
        }

        public void OnMouseUpClickProfileScript(int profileIndex)
        {

            ClientPlayer clientPlayer = GetClientPlayerFromPlayerSerial(SelfPlayerSerial);

            // 繆拉，主動技，指定一名玩家，奪取該玩家的一張手牌，一回合只能發動一次。
            if (clientPlayer.CharacterNameCode == 13)
                if (NowUsingSkillStage == UsingSkillStage.CharacterNameCode13ChoosingEffect)
                    if (_ProfileIndexToPlayerSerial[profileIndex] != SelfPlayerSerial)
                        if (GetClientPlayerFromPlayerSerial(_ProfileIndexToPlayerSerial[profileIndex]).EnemyClientPlayerDetails.PublicCardAmount > 0)
                        {
                            if (_CharacterNameCode13DecideCharacterIndex != -1)
                                GetClientPlayerFromPlayerSerial(_ProfileIndexToPlayerSerial[_CharacterNameCode13DecideCharacterIndex]).IsChosen = false;
                            _CharacterNameCode13DecideCharacterIndex = profileIndex;
                            GetClientPlayerFromPlayerSerial(_ProfileIndexToPlayerSerial[profileIndex]).IsChosen = true;
                            UpdateSkillButtons(true, true, true, _CharacterNameCode13DecideCharacterIndex != -1);
                            return;
                        }

            // 吉爾伯特，主動技，使用「拓殖」時，指定一名玩家，將拓殖得到的牌交給該玩家。
            if (clientPlayer.CharacterNameCode == 18)
                if (NowUsingSkillStage == UsingSkillStage.CharacterNameCode18ChoosingEffect)
                    if (_ProfileIndexToPlayerSerial[profileIndex] != SelfPlayerSerial)
                    {
                        if (_CharacterNameCode18ChooseExpandProfileIndex != -1)
                            GetClientPlayerFromPlayerSerial(_ProfileIndexToPlayerSerial[_CharacterNameCode18ChooseExpandProfileIndex]).IsChosen = false;
                        _CharacterNameCode18ChooseExpandProfileIndex = profileIndex;
                        GetClientPlayerFromPlayerSerial(_ProfileIndexToPlayerSerial[profileIndex]).IsChosen = true;
                        UpdateSkillButtons(true, true, false, _CharacterNameCode18ChooseExpandProfileIndex != -1);
                        return;
                    }

            CardDetailHandler.NameCode = -1;
            CharacterDetails.NameCode = GetClientPlayerFromPlayerSerial(_ProfileIndexToPlayerSerial[profileIndex]).CharacterNameCode;
        }

        public void OnClickRevolutionNext()
        {
            foreach (ClientPlayer cP in ClientPlayers)
                if (!cP.IsSelf)
                    cP.EnemyClientPlayerDetails.EndGameCardsHandler.IsShow = false;
            DeckCardsFolder.gameObject.SetActive(true);
            RevolutionBTN.SetActive(false);
            ChangeStage(ClientStage.EnemyTurn);
            ClientController.NextPacket();
        }

        public void OnClickBackToRoom()
        {
            ClientController.NextPacket();
            ClientController.RemoveSubscriptor("GamePlayHub");
            LogMaster.GetInstance().View.SetActive(false);
            OnNextPhase();
            Destroy(gameObject);
        }



        // send //

        public void Send_SceneReady(SceneReady sceneReady)
        {
            ClientController.SendPacket(Hub, "SceneReady", sceneReady);
        }

        public void Send_DecideCharacter(DecideCharacter decideCharacter)
        {
            ClientController.SendPacket(Hub, "DecideCharacter", decideCharacter);
        }

        public void Send_RevealSeveralCoveredCard(RevealSeveralCoveredCards revealSeveralCoveredCard)
        {
            ClientController.SendPacket(Hub, "RevealSeveralCoveredCards", revealSeveralCoveredCard);
        }

        public void Send_DecideCardFromRevealedCoveredCards(DecideCardFromRevealedCoveredCards decideCardFromRevealedCoveredCards)
        {
            ClientController.SendPacket(Hub, "DecideCardFromRevealedCoveredCards", decideCardFromRevealedCoveredCards);
        }

        public void Send_DecideCardFromOpenCard(DecideCardFromOpenCard decideCardFromOpenCard)
        {
            ClientController.SendPacket(Hub, "DecideCardFromOpenCard", decideCardFromOpenCard);
        }

        public void Send_UseFunctionCard_Mask(UseFunctionCard_Mask useFunctionCard_Mask)
        {
            ClientController.SendPacket(Hub, "UseFunctionCard_Mask", useFunctionCard_Mask);
        }

        public void Send_UseFunctionCard_Reform(UseFunctionCard_Reform useFunctionCard_Reform)
        {
            ClientController.SendPacket(Hub, "UseFunctionCard_Reform", useFunctionCard_Reform);
        }

        public void Send_UseFunctionCard_Expand(UseFunctionCard_Expand useFunctionCard_Expand)
        {
            ClientController.SendPacket(Hub, "UseFunctionCard_Expand", useFunctionCard_Expand);
        }

        public void Send_UseStrategyCard_Requirements(UseStrategyCard_Requirements useStrategyCard_Requirements)
        {
            ClientController.SendPacket(Hub, "UseStrategyCard_Requirements", useStrategyCard_Requirements);
        }

        public void Send_ReleasePower(ReleasePower releasePower)
        {
            ClientController.SendPacket(Hub, "ReleasePower", releasePower);
        }

        public void Send_DiscardStrategyCard(DiscardStrategyCard discardStrategyCard)
        {
            ClientController.SendPacket(Hub, "DiscardStrategyCard", discardStrategyCard);
        }

        public void Send_EndTurn(EndTurn endTurn)
        {
            ClientController.SendPacket(Hub, "EndTurn", endTurn);
        }

        public void Send_EndCongressTurn(EndCongressTurn endCongressTurn)
        {
            ClientController.SendPacket(Hub, "EndCongressTurn", endCongressTurn);
        }

        public void Send_CharacterNameCode0ChooseToRobOneCard(CharacterNameCode0ChooseToRobOneCard characterNameCode0ChooseToRobOneCard)
        {
            ClientController.SendPacket(Hub, "CharacterNameCode0ChooseToRobOneCard", characterNameCode0ChooseToRobOneCard);
        }

        public void Send_CharacterNameCode1ChooseOneDeckCard(CharacterNameCode1ChooseOneDeckCard characterNameCode1ChooseOneDeckCard)
        {
            ClientController.SendPacket(Hub, "CharacterNameCode1ChooseOneDeckCard", characterNameCode1ChooseOneDeckCard);
        }

        public void Send_CharacterNameCode2DiscardOneHandCard(CharacterNameCode2DiscardOneHandCard characterNameCode2DiscardOneHandCard)
        {
            ClientController.SendPacket(Hub, "CharacterNameCode2DiscardOneHandCard", characterNameCode2DiscardOneHandCard);
        }

        public void Send_CharacterNameCode3UseSkill(CharacterNameCode3UseSkill characterNameCode3UseSkill)
        {
            ClientController.SendPacket(Hub, "CharacterNameCode3UseSkill", characterNameCode3UseSkill);
        }

        public void Send_CharacterNameCode6UseSkill(CharacterNameCode6UseSkill characterNameCode6UseSkill)
        {
            ClientController.SendPacket(Hub, "CharacterNameCode6UseSkill", characterNameCode6UseSkill);
        }

        public void Send_CharacterNameCode8DiscardOneHandCard(CharacterNameCode8DiscardOneHandCard characterNameCode8DiscardOneHandCard)
        {
            ClientController.SendPacket(Hub, "CharacterNameCode8DiscardOneHandCard", characterNameCode8DiscardOneHandCard);
        }

        public void Send_CharacterNameCode10DiscardOneDeckCard(CharacterNameCode10DiscardOneDeckCard characterNameCode10DiscardOneDeckCard)
        {
            ClientController.SendPacket(Hub, "CharacterNameCode10DiscardOneDeckCard", characterNameCode10DiscardOneDeckCard);
        }

        public void Send_CharacterNameCode13RobOneHandCard(CharacterNameCode13RobOneHandCard characterNameCode13RobOneHandCard)
        {
            ClientController.SendPacket(Hub, "CharacterNameCode13RobOneHandCard", characterNameCode13RobOneHandCard);
        }

        public void Send_CharacterNameCode14PlaceBackDeckCard(CharacterNameCode14PlaceBackDeckCard characterNameCode14PlaceBackDeckCard)
        {
            ClientController.SendPacket(Hub, "CharacterNameCode14PlaceBackDeckCard", characterNameCode14PlaceBackDeckCard);
        }

        public void Send_CharacterNameCode18ExpandDeckCardToEnemy(CharacterNameCode18ExpandDeckCardToEnemy characterNameCode18ExpandDeckCardToEnemy)
        {
            ClientController.SendPacket(Hub, "CharacterNameCode18ExpandDeckCardToEnemy", characterNameCode18ExpandDeckCardToEnemy);
        }

        public void Send_StrategyNameCode22DecideDiscardOneHandCard(StrategyNameCode22DecideDiscardOneHandCard strategyNameCode22DecideDiscardOneHandCard)
        {
            ClientController.SendPacket(Hub, "StrategyNameCode22DecideDiscardOneHandCard", strategyNameCode22DecideDiscardOneHandCard);
        }

        // recieve //

        public void GamePlayInfos(GamePlayInfos gamePlayInfos)
        {
            ClientController.NextPacket();
            _PlayerAmount = gamePlayInfos.PlayerAmount;
            SelfPlayerSerial = gamePlayInfos.SelfPlayerSerial;

            _CharacterNameCode6SkillUsed = false;
            _CharacterNameCode4HasSecondGain = true;
            _CharacterNameCode13SkillUsed = false;
            _CharacterNameCode3PreviousReformHandCardIndex = -1;
            int[] profileSerials = GetProfileSerials(gamePlayInfos.PlayerSerials, SelfPlayerSerial, _PlayerAmount);
            _ProfileIndexToPlayerSerial = new int[_PlayerAmount];
            for (int i = 0; i < _PlayerAmount; i++) _ProfileIndexToPlayerSerial[profileSerials[i]] = gamePlayInfos.PlayerSerials[i];
            for (int i = 0; i < _PlayerAmount; i++)
            {
                ClientPlayers.Add(new ClientPlayer(gamePlayInfos.PlayerSerials[i], gamePlayInfos.Nicks[i], gamePlayInfos.IsBots[i], Profiles[profileSerials[i]], gamePlayInfos.PlayerSerials[i] == SelfPlayerSerial, profileSerials[i], this));
            }

            InitDeckCards(_PlayerAmount);
        }

        public void ChooseCharacterFromThree(ChooseCharacterFromThree chooseCharacterFromThree)
        {
            ClientController.NextPacket();
            CharacterCharacterGMO = new GameObject[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject gmo = Instantiate(CharacterPRF, ChooseCharacterFolder);
                gmo.GetComponent<CharacterHandler>().Init(i, this, chooseCharacterFromThree.CharacterNameCodes[i]);
                CharacterCharacterGMO[i] = gmo;
            }
        }

        public void EnemyDecideCharacter(EnemyDecideCharacter enemyDecideCharacter)
        {
            ClientController.NextPacket();
            GetClientPlayerFromPlayerSerial(enemyDecideCharacter.PlayerSerial).IsDecideCharacterReady = true;
        }

        public void DecideCharacterInfos(DecideCharacterInfos decideCharacterInfos)
        {
            ClientController.NextPacket();

            for (int i = 0; i < decideCharacterInfos.PlayerSerials.Length; i++)
            {
                GetClientPlayerFromPlayerSerial(decideCharacterInfos.PlayerSerials[i]).IsDecideCharacterReady = false;
                GetClientPlayerFromPlayerSerial(decideCharacterInfos.PlayerSerials[i]).SetCharacterNameCodeValue(DataMaster.GetInstance().GetCharacterInfo(decideCharacterInfos.CharacterNameCodes[i]), decideCharacterInfos.PlayerSerials[i] == SelfPlayerSerial);

                // 威廉三世，被動技，革命門檻減少８，使用「政治改革」時，效果增加１倍。 必殺技，革命門檻永久增加５，同時自身被動技永久無效。
                if (decideCharacterInfos.CharacterNameCodes[i] == 3)
                    GetClientPlayerFromPlayerSerial(decideCharacterInfos.PlayerSerials[i]).CharacterNameCode3HasPassive = true;
            }
        }

        public void UpdateDeckCards(UpdateDeckCards updateDeckCards)
        {
            ClientController.NextPacket();
            for (int i = 0; i < updateDeckCards.DeckCardNameCodes.Length; i++)
            {
                DeckCardHandlers[i].IsChosen = false;
                DeckCardHandlers[i].NameCode = updateDeckCards.DeckCardNameCodes[i];
            }
        }

        public void UpdatePileAmount(UpdatePileAmount updatePileAmount)
        {
            ClientController.NextPacket();
            DrawPileHandler.Amount = updatePileAmount.DrawPileAmount;
            GravePileHandler.Amount = updatePileAmount.GravePileAmount;
        }

        public void TurnStart(TurnStart turnStart)
        {
            ClientController.NextPacket();
            DeckCardsFolder.gameObject.SetActive(true);
            foreach (ClientPlayer clientPlayer in ClientPlayers) clientPlayer.IsTurn = turnStart.PlayerSerial == clientPlayer.PlayerSerial;
            if (turnStart.PlayerSerial == SelfPlayerSerial) NowStage = ClientStage.DrawCard;
            _IsInCongress = false;
        }

        public void RevealChosenCoveredCards(RevealChosenCoveredCards revealChosenCoveredCards)
        {
            ClientController.NextPacket();
            for (int i = 0; i < revealChosenCoveredCards.ChosenCoveredCardNameCodes.Length; i++)
                DeckCardHandlers[_ChooseFromCoveredCardsBuffer[i]].NameCode = revealChosenCoveredCards.ChosenCoveredCardNameCodes[i];
        }

        public void DrawCards(DrawCards drawCards)
        {
            ClientController.NextPacket();
            GetClientPlayerFromPlayerSerial(SelfPlayerSerial).AddHandCard(drawCards.DrawnCardNameCode);
        }

        public void DiscardCards(DiscardCards discardCards)
        {
            ClientController.NextPacket();
            GetClientPlayerFromPlayerSerial(SelfPlayerSerial).RemoveHandCard(discardCards.DiscardCardNameCode);
        }

        public void HighlightDeckCards(HighlightDeckCards highlightDeckCards)
        {
            for (int i = 0; i < highlightDeckCards.HighlightCardIndex.Length; i++) DeckCardHandlers[highlightDeckCards.HighlightCardIndex[i]].IsChosen = true;
            StartCoroutine(ClientController.WaitSecNextPacket(1.0f, null, null));
        }

        public void EnemyDecideCardFromRevealedCoveredCards(EnemyDecideCardFromRevealedCoveredCards enemyDecideCardFromRevealedCoveredCards)
        {
            ClientPlayer enemyPlayer = GetClientPlayerFromPlayerSerial(enemyDecideCardFromRevealedCoveredCards.EnemySerial);
            DeckCardHandlers[enemyDecideCardFromRevealedCoveredCards.DecideCardIndex].NameCode = -2;
            if (!enemyPlayer.IsSelf)
            {
                enemyPlayer.AddReleaseCards(CardAnimationType.CoveredDeckToEnemy, enemyDecideCardFromRevealedCoveredCards.DecideCardIndex);
                for (int i = 0; i < enemyDecideCardFromRevealedCoveredCards.CandidateCardIndex.Length; i++)
                    DeckCardHandlers[enemyDecideCardFromRevealedCoveredCards.CandidateCardIndex[i]].IsChosen = false;
                for (int i = 0; i < enemyDecideCardFromRevealedCoveredCards.RevealCardIndex.Length; i++)
                    DeckCardHandlers[enemyDecideCardFromRevealedCoveredCards.RevealCardIndex[i]].NameCode = enemyDecideCardFromRevealedCoveredCards.RevealCardNameCodes[i];
            }

            LogMaster.GetInstance().AddComponent(enemyPlayer);
            LogMaster.GetInstance().AddComponent("觀看" + enemyDecideCardFromRevealedCoveredCards.CandidateCardIndex.Length + "張牌。拿取1張，");
            if (enemyDecideCardFromRevealedCoveredCards.RevealCardIndex.Length > 0)
            {
                LogMaster.GetInstance().AddComponent("將");
                bool first = true;
                foreach (int index in enemyDecideCardFromRevealedCoveredCards.RevealCardIndex)
                {
                    if (first) first = false;
                    else LogMaster.GetInstance().AddComponent("、");
                    LogMaster.GetInstance().AddComponent(DataMaster.GetInstance().GetCardInfo(DeckCardHandlers[index].NameCode));
                }
                LogMaster.GetInstance().AddComponent("翻開放回桌上");
            }
            else
                LogMaster.GetInstance().AddComponent("不將任何牌翻開");
            LogMaster.GetInstance().Log();

            if (!enemyPlayer.IsSelf)
                StartCoroutine(DestroyEnemyDecideCardFromRevealedCoveredCards(enemyDecideCardFromRevealedCoveredCards));
            else
                ClientController.NextPacket();

        }

        public void EnemyDecideCardFromOpenCard(EnemyDecideCardFromOpenCard enemyDecideCardFromOpenCard)
        {
            ClientPlayer enemyPlayer = GetClientPlayerFromPlayerSerial(enemyDecideCardFromOpenCard.EnemySerial);
            if (!enemyPlayer.IsSelf)
            {
                enemyPlayer.AddReleaseCards(CardAnimationType.OpenDeckToEnemy, DeckCardHandlers[enemyDecideCardFromOpenCard.DecideOpenCardIndex].NameCode);
            }

            LogMaster.GetInstance().AddComponent(enemyPlayer);
            LogMaster.GetInstance().AddComponent("拿取翻開的");
            LogMaster.GetInstance().AddComponent(DataMaster.GetInstance().GetCardInfo(DeckCardHandlers[enemyDecideCardFromOpenCard.DecideOpenCardIndex].NameCode));
            LogMaster.GetInstance().Log();

            DeckCardHandlers[enemyDecideCardFromOpenCard.DecideOpenCardIndex].NameCode = -2;

            if (!enemyPlayer.IsSelf)
                StartCoroutine(DestroyEnemyDecideCardFromOpenCard(enemyDecideCardFromOpenCard));
            else
                ClientController.NextPacket();
        }

        public void EnemyUseFunctionCard_Mask(EnemyUseFunctionCard_Mask enemyUseFunctionCard_Mask)
        {
            ClientPlayer enemyPlayer = GetClientPlayerFromPlayerSerial(enemyUseFunctionCard_Mask.EnemySerial);
            if (!enemyPlayer.IsSelf)
            {
                enemyPlayer.AddReleaseCards(CardAnimationType.EnemyToGrave, enemyUseFunctionCard_Mask.FunctionCardNameCode);
                enemyPlayer.AddReleaseCards(CardAnimationType.DrawToEnemy, -1);
            } else
            {
                GravePileHandler.Amount++;
                DrawPileHandler.Amount--;
            }

            LogMaster.GetInstance().AddComponent(enemyPlayer);
            LogMaster.GetInstance().AddComponent("使用");
            LogMaster.GetInstance().AddComponent(DataMaster.GetInstance().GetCardInfo(enemyUseFunctionCard_Mask.FunctionCardNameCode));
            LogMaster.GetInstance().AddComponent("，抽一張牌");
            LogMaster.GetInstance().Log();

            if (!enemyPlayer.IsSelf)
                StartCoroutine(DestroyEnemyUseFunctionCard_Mask(enemyUseFunctionCard_Mask));
            else
                ClientController.NextPacket();
        }

        public void EnemyUseFunctionCard_Reform(EnemyUseFunctionCard_Reform enemyUseFunctionCard_Reform)
        {
            ClientPlayer enemyPlayer = GetClientPlayerFromPlayerSerial(enemyUseFunctionCard_Reform.EnemySerial);
            if (!enemyPlayer.IsSelf)
            {
                enemyPlayer.AddReleaseCards(CardAnimationType.EnemyToGrave, enemyUseFunctionCard_Reform.FunctionCardNameCode);
                enemyPlayer.AddReleaseCards(CardAnimationType.EnemyToGrave, enemyUseFunctionCard_Reform.DecideCardNameCode);
            }
            else
                GravePileHandler.Amount += 1 + enemyUseFunctionCard_Reform.DecideCardNameCode.Length;

            LogMaster.GetInstance().AddComponent(enemyPlayer);
            LogMaster.GetInstance().AddComponent("使用");
            LogMaster.GetInstance().AddComponent(DataMaster.GetInstance().GetCardInfo(enemyUseFunctionCard_Reform.FunctionCardNameCode));
            LogMaster.GetInstance().AddComponent("，丟棄");
            bool first = true;
            foreach (int nameCode in enemyUseFunctionCard_Reform.DecideCardNameCode)
            {
                if (first) first = false;
                else LogMaster.GetInstance().AddComponent("、");
                LogMaster.GetInstance().AddComponent(DataMaster.GetInstance().GetCardInfo(nameCode));
            }
            LogMaster.GetInstance().Log();

            if (!enemyPlayer.IsSelf)
                StartCoroutine(DestroyEnemyUseFunctionCard_Reform(enemyUseFunctionCard_Reform));
            else
                ClientController.NextPacket();
        }

        public void EnemyUseFunctionCard_Expand(EnemyUseFunctionCard_Expand enemyUseFunctionCard_Expand)
        {
            ClientPlayer enemyPlayer = GetClientPlayerFromPlayerSerial(enemyUseFunctionCard_Expand.EnemySerial);
            if (!enemyPlayer.IsSelf)
            {
                enemyPlayer.AddReleaseCards(CardAnimationType.EnemyToGrave, enemyUseFunctionCard_Expand.FunctionCardNameCode);
                enemyPlayer.AddReleaseCards(CardAnimationType.OpenDeckToEnemy, enemyUseFunctionCard_Expand.DecideCardNameCode);
            }
            else
                GravePileHandler.Amount++;

            DeckCardHandlers[enemyUseFunctionCard_Expand.DecideCardIndex].NameCode = -2;

            LogMaster.GetInstance().AddComponent(enemyPlayer);
            LogMaster.GetInstance().AddComponent("使用");
            LogMaster.GetInstance().AddComponent(DataMaster.GetInstance().GetCardInfo(enemyUseFunctionCard_Expand.FunctionCardNameCode));
            LogMaster.GetInstance().AddComponent("，拿取翻開的");
            LogMaster.GetInstance().AddComponent(DataMaster.GetInstance().GetCardInfo(enemyUseFunctionCard_Expand.DecideCardNameCode));
            LogMaster.GetInstance().Log();

            if (!enemyPlayer.IsSelf)
                StartCoroutine(DestroyEnemyUseFunctionCard_Expand(enemyUseFunctionCard_Expand));
            else
                ClientController.NextPacket();
        }

        public void EnemyUseStrategyCard_Requirements(EnemyUseStrategyCard_Requirements enemyUseStrategyCard_Requirements)
        {
            ClientPlayer enemyPlayer = GetClientPlayerFromPlayerSerial(enemyUseStrategyCard_Requirements.EnemySerial);
            if (!enemyPlayer.IsSelf)
            {
                enemyPlayer.AddReleaseCards(CardAnimationType.EnemyToGrave, enemyUseStrategyCard_Requirements.StrategyCardNameCode);
                enemyPlayer.AddReleaseCards(CardAnimationType.EnemyToGrave, enemyUseStrategyCard_Requirements.RequirementCardNameCodes);
            }
            else
                GravePileHandler.Amount += 1 + enemyUseStrategyCard_Requirements.RequirementCardNameCodes.Length;

            LogMaster.GetInstance().AddComponent(enemyPlayer);
            if (enemyUseStrategyCard_Requirements.RequirementCardNameCodes.Length > 0)
            {
                LogMaster.GetInstance().AddComponent("以");
                bool first = true;
                foreach (int nameCode in enemyUseStrategyCard_Requirements.RequirementCardNameCodes)
                {
                    if (first) first = false;
                    else LogMaster.GetInstance().AddComponent("、");
                    LogMaster.GetInstance().AddComponent(DataMaster.GetInstance().GetCardInfo(nameCode));
                }
                LogMaster.GetInstance().AddComponent("作為條件，");
            }
            LogMaster.GetInstance().AddComponent("發動");
            LogMaster.GetInstance().AddComponent(DataMaster.GetInstance().GetCardInfo(enemyUseStrategyCard_Requirements.StrategyCardNameCode));
            LogMaster.GetInstance().Log();

            if (!enemyPlayer.IsSelf)
                StartCoroutine(DestroyEnemyUseStrategyCard_Requirements(enemyUseStrategyCard_Requirements));
            else
                ClientController.NextPacket();
        }

        public void EnemyReleaseCard(EnemyReleaseCard enemyReleaseCard)
        {
            ClientPlayer enemyPlayer = GetClientPlayerFromPlayerSerial(enemyReleaseCard.EnemySerial);
            if (!enemyPlayer.IsSelf)
                enemyPlayer.AddReleaseCards(CardAnimationType.EnemyToGrave, enemyReleaseCard.ReleaseCardNameCodes);
            else
                GravePileHandler.Amount += enemyReleaseCard.ReleaseCardNameCodes.Length;

            LogMaster.GetInstance().AddComponent(enemyPlayer);
            LogMaster.GetInstance().AddComponent("釋權：");
            bool first = true;
            foreach (int nameCode in enemyReleaseCard.ReleaseCardNameCodes)
            {
                if (first) first = false;
                else LogMaster.GetInstance().AddComponent("、");
                LogMaster.GetInstance().AddComponent(DataMaster.GetInstance().GetCardInfo(nameCode));
            }
            LogMaster.GetInstance().Log();

            if (!enemyPlayer.IsSelf)
                StartCoroutine(DestroyEnemyReleaseCard(enemyReleaseCard));
            else
                ClientController.NextPacket();
        }

        public void EnemyDiscardCards(EnemyDiscardCards enemyDiscardCards)
        {
            ClientPlayer enemyPlayer = GetClientPlayerFromPlayerSerial(enemyDiscardCards.EnemySerial);
            if (!enemyPlayer.IsSelf)
                enemyPlayer.AddReleaseCards(CardAnimationType.EnemyToGrave, enemyDiscardCards.DiscardCardNameCodes);
            else
                GravePileHandler.Amount += enemyDiscardCards.DiscardCardNameCodes.Length;

            LogMaster.GetInstance().AddComponent(enemyPlayer);
            LogMaster.GetInstance().AddComponent("丟棄");
            bool first = true;
            foreach (int nameCode in enemyDiscardCards.DiscardCardNameCodes)
            {
                if (first) first = false;
                else LogMaster.GetInstance().AddComponent("、");
                LogMaster.GetInstance().AddComponent(DataMaster.GetInstance().GetCardInfo(nameCode));
            }
            LogMaster.GetInstance().Log();

            if (!enemyPlayer.IsSelf)
                StartCoroutine(DestroyEnemyDiscardCards(enemyDiscardCards));
            else
                ClientController.NextPacket();
        }

        public void EnemyGainCards(EnemyGainCards enemyGainCards)
        {
            ClientPlayer enemyPlayer = GetClientPlayerFromPlayerSerial(enemyGainCards.EnemySerial);
            if (!enemyPlayer.IsSelf)
                enemyPlayer.AddReleaseCards(CardAnimationType.EnemyToGrave, enemyGainCards.GainCardNameCodes);

            LogMaster.GetInstance().AddComponent(enemyPlayer);
            LogMaster.GetInstance().AddComponent("獲得");
            bool first = true;
            foreach (int nameCode in enemyGainCards.GainCardNameCodes)
            {
                if (first) first = false;
                else LogMaster.GetInstance().AddComponent("、");
                LogMaster.GetInstance().AddComponent(DataMaster.GetInstance().GetCardInfo(nameCode));
            }
            LogMaster.GetInstance().Log();

            if (!enemyPlayer.IsSelf)
                StartCoroutine(DestroyEnemiesGainCards(enemyGainCards));
            else
                ClientController.NextPacket();
        }

        public void EndStrategy(EndStrategy endStrategy)
        {
            ClientController.NextPacket();
            ChangeStage(_IsInCongress ? ClientStage.SelfCongress : ClientStage.MidTurn);
        }

        public void RoundStart(RoundStart roundStart)
        {
            NowRound = roundStart.NowRound;
            // implement //
            if (roundStart.HasCongress)
            {
                _CharacterNameCode14SkillUsed = false;
                for (int i = 0; i < roundStart.PlayerSerials.Length; i++)
                    GetClientPlayerFromPlayerSerial(roundStart.PlayerSerials[i]).Power = roundStart.Powers[i];

                if (roundStart.LeaderSerial == -1)
                    LogMaster.GetInstance().AddComponent("列強會議開始\n沒有會議主導者");
                else
                {
                    if (!GetClientPlayerFromPlayerSerial(roundStart.LeaderSerial).IsSelf)
                        GetClientPlayerFromPlayerSerial(roundStart.LeaderSerial).EnemyClientPlayerDetails.StrategyCardAmount++;
                    LogMaster.GetInstance().AddComponent("列強會議開始\n");
                    LogMaster.GetInstance().AddComponent(GetClientPlayerFromPlayerSerial(roundStart.LeaderSerial));
                    LogMaster.GetInstance().AddComponent("成為會議主導者");
                }
                LogMaster.GetInstance().Log();

                foreach(ClientPlayer clientPlayer in ClientPlayers)
                    clientPlayer.BuffHandler.CongressStart();
                StartCoroutine(ClientController.WaitSecNextPacket(1.5f, null, null));
            } else ClientController.NextPacket();
        }

        public void CongressTurnStart(CongressTurnStart congressTurnStart)
        {
            ClientController.NextPacket();
            if (congressTurnStart.PlayerSerial == SelfPlayerSerial) NowStage = ClientStage.SelfCongress;
            else NowStage = ClientStage.EnemyCongress;
            foreach (ClientPlayer clientPlayer in ClientPlayers) clientPlayer.IsTurn = congressTurnStart.PlayerSerial == clientPlayer.PlayerSerial;
        }

        public void Victory(Victory victory)
        {
            ClientPlayer clientPlayer = GetClientPlayerFromPlayerSerial(victory.VictorySerial);
            clientPlayer.IsWin = true;
            ChangeStage(ClientStage.Victory);
            LogMaster.GetInstance().AddComponent(clientPlayer);
            LogMaster.GetInstance().AddComponent("勝利了!");
            LogMaster.GetInstance().Log();
            foreach (ClientPlayer cP in ClientPlayers)
                cP.IsTurn = false;

            CardDetailHandler.NameCode = -1;
            CharacterDetails.NameCode = -1;
            DeckCardsFolder.gameObject.SetActive(false);
            if (_PlayerAmount > 0)
                if (!GetClientPlayerFromPlayerSerial(0).IsSelf)
                    if (!GetClientPlayerFromPlayerSerial(0).IsDead)
                        GetClientPlayerFromPlayerSerial(0).EnemyClientPlayerDetails.EndGameCardsHandler.Init(victory.PlayerHandCards0);
            if (_PlayerAmount > 1)
                if (!GetClientPlayerFromPlayerSerial(1).IsSelf)
                    if (!GetClientPlayerFromPlayerSerial(1).IsDead)
                        GetClientPlayerFromPlayerSerial(1).EnemyClientPlayerDetails.EndGameCardsHandler.Init(victory.PlayerHandCards1);
            if (_PlayerAmount > 2)
                if (!GetClientPlayerFromPlayerSerial(2).IsSelf)
                    if (!GetClientPlayerFromPlayerSerial(2).IsDead)
                        GetClientPlayerFromPlayerSerial(2).EnemyClientPlayerDetails.EndGameCardsHandler.Init(victory.PlayerHandCards2);
            if (_PlayerAmount > 3)
                if (!GetClientPlayerFromPlayerSerial(3).IsSelf)
                    if (!GetClientPlayerFromPlayerSerial(3).IsDead)
                        GetClientPlayerFromPlayerSerial(3).EnemyClientPlayerDetails.EndGameCardsHandler.Init(victory.PlayerHandCards3);
            if (_PlayerAmount > 4)
                if (!GetClientPlayerFromPlayerSerial(4).IsSelf)
                    if (!GetClientPlayerFromPlayerSerial(4).IsDead)
                        GetClientPlayerFromPlayerSerial(4).EnemyClientPlayerDetails.EndGameCardsHandler.Init(victory.PlayerHandCards4);

            RankHandler.Init(victory.Ranks, ClientPlayers);
        }

        public void Revolution(Revolution revolution)
        {
            ClientPlayer clientPlayer = GetClientPlayerFromPlayerSerial(revolution.RevolutionSerial);
            clientPlayer.IsDead = true;
            ChangeStage(ClientStage.Revolution);
            LogMaster.GetInstance().AddComponent(clientPlayer);
            LogMaster.GetInstance().AddComponent("的政權已被革命推翻");
            LogMaster.GetInstance().Log();
            DeckCardsFolder.gameObject.SetActive(false);
            RevolutionBTN.SetActive(true);
            if (!clientPlayer.IsSelf)
                clientPlayer.EnemyClientPlayerDetails.EndGameCardsHandler.Init(revolution.HandCards);
        }

        public void UpdatePower(UpdatePower updatePower)
        {
            ClientController.NextPacket();
            for (int i = 0; i < updatePower.PlayerSerials.Length; i++)
                GetClientPlayerFromPlayerSerial(updatePower.PlayerSerials[i]).Power = updatePower.Powers[i];
        }

        public void UpdateUnmaskedPowerAndVictory(UpdateUnmaskedPowerAndVictory updateUnmaskedPowerAndVictory)
        {
            GameEnd = true;
            for (int i = 0; i < updateUnmaskedPowerAndVictory.PlayerSerials.Length; i++)
                GetClientPlayerFromPlayerSerial(updateUnmaskedPowerAndVictory.PlayerSerials[i]).Power = updateUnmaskedPowerAndVictory.UnmaskedPowers[i];
            ChangeStage(ClientStage.Victory);

            foreach (ClientPlayer cP in ClientPlayers)
                cP.IsTurn = false;

            if (updateUnmaskedPowerAndVictory.VictorySerial != -1)
            {
                ClientPlayer clientPlayer = GetClientPlayerFromPlayerSerial(updateUnmaskedPowerAndVictory.VictorySerial);
                clientPlayer.IsWin = true;
                LogMaster.GetInstance().AddComponent(clientPlayer);
                LogMaster.GetInstance().AddComponent("在歐洲的影響力無人能及，勝利!");
                LogMaster.GetInstance().Log();
            } else
            {
                LogMaster.GetInstance().AddComponent("全歐洲陷入革命狂潮當中，再也沒有人能夠控制局面\n...\n...你們到底怎麼玩到這個結局的...");
                LogMaster.GetInstance().Log();
            }

            CardDetailHandler.NameCode = -1;
            CharacterDetails.NameCode = -1;
            DeckCardsFolder.gameObject.SetActive(false);
            if (_PlayerAmount > 0)
                if (!GetClientPlayerFromPlayerSerial(0).IsSelf)
                    if (!GetClientPlayerFromPlayerSerial(0).IsDead)
                        GetClientPlayerFromPlayerSerial(0).EnemyClientPlayerDetails.EndGameCardsHandler.Init(updateUnmaskedPowerAndVictory.PlayerHandCards0);
            if (_PlayerAmount > 1)
                if (!GetClientPlayerFromPlayerSerial(1).IsSelf)
                    if (!GetClientPlayerFromPlayerSerial(1).IsDead)
                        GetClientPlayerFromPlayerSerial(1).EnemyClientPlayerDetails.EndGameCardsHandler.Init(updateUnmaskedPowerAndVictory.PlayerHandCards1);
            if (_PlayerAmount > 2)
                if (!GetClientPlayerFromPlayerSerial(2).IsSelf)
                    if (!GetClientPlayerFromPlayerSerial(2).IsDead)
                        GetClientPlayerFromPlayerSerial(2).EnemyClientPlayerDetails.EndGameCardsHandler.Init(updateUnmaskedPowerAndVictory.PlayerHandCards2);
            if (_PlayerAmount > 3)
                if (!GetClientPlayerFromPlayerSerial(3).IsSelf)
                    if (!GetClientPlayerFromPlayerSerial(3).IsDead)
                        GetClientPlayerFromPlayerSerial(3).EnemyClientPlayerDetails.EndGameCardsHandler.Init(updateUnmaskedPowerAndVictory.PlayerHandCards3);
            if (_PlayerAmount > 4)
                if (!GetClientPlayerFromPlayerSerial(4).IsSelf)
                    if (!GetClientPlayerFromPlayerSerial(4).IsDead)
                        GetClientPlayerFromPlayerSerial(4).EnemyClientPlayerDetails.EndGameCardsHandler.Init(updateUnmaskedPowerAndVictory.PlayerHandCards4);

            RankHandler.Init(updateUnmaskedPowerAndVictory.Ranks, ClientPlayers);
        }

        public void UpdatePowerLimit(UpdatePowerLimit updatePowerLimit)
        {
            ClientController.NextPacket();
            for (int i = 0; i < updatePowerLimit.PlayerSerials.Length; i++)
                GetClientPlayerFromPlayerSerial(updatePowerLimit.PlayerSerials[i]).PowerLimit = updatePowerLimit.PowerLimits[i];
        }

        public void AddBuffs(AddBuffs addBuffs)
        {
            ClientController.NextPacket();
            foreach (int playerSerial in addBuffs.PlayerSerials)
                GetClientPlayerFromPlayerSerial(playerSerial).BuffHandler.AddBuffs(addBuffs.Buffs);
        }
        
        public void CharacterNameCode1AskIfCastSkill(CharacterNameCode1AskIfCastSkill characterNameCode1AskIfCastSkill)
        {
            // 米歇爾·內伊，主動技，當其他玩家回合結束時，可以拿取場上任意一張翻開的牌，此技能一輪會議周期內只能發動一次。
            ClientController.NextPacket();
            _CharacterNameCode1ChooseOneDeckCardIndex = -1;
            ChangeStage(UsingSkillStage.CharacterNameCode1PreparingSkill);
        }

        public void StrategyNameCode20RobGainCardOneMoreTime(StrategyNameCode20RobGainCardOneMoreTime strategyNameCode20RobGainCardOneMoreTime)
        {
            ClientController.NextPacket();
            ChangeStage(ClientStage.DrawCard);
            UsedMaskLimit = 99;
            UsedReformLimit = 99;
            UsedExpandLimit = 99;
        }

        public void StrategyNameCode22ChooseDiscardOneHandCard(StrategyNameCode22ChooseDiscardOneHandCard strategyNameCode22ChooseDiscardOneHandCard)
        {
            ClientController.NextPacket();
            ChangeStage(UsingStrategyStage.StrategyNameCode22ChooseDiscardOneHandCard);
        }

        public void EnemyCharacterNameCode0RobOneCard(EnemyCharacterNameCode0RobOneCard enemyCharacterNameCode0RobOneCard)
        {
            ClientController.NextPacket();
            ClientPlayer robberClientPlayer = GetClientPlayerFromPlayerSerial(enemyCharacterNameCode0RobOneCard.RobberPlayerSerial);
            if (robberClientPlayer.IsSelf)
                robberClientPlayer.AddHandCard(enemyCharacterNameCode0RobOneCard.RobbedCardNameCodes);
            else
                robberClientPlayer.EnemyClientPlayerDetails.PublicCardAmount += enemyCharacterNameCode0RobOneCard.RobbedCardNameCodes.Length;


            for (int i = 0; i < enemyCharacterNameCode0RobOneCard.RobbedPlayerSerials.Length; i++)
            {
                int robbedPlayerSerial = enemyCharacterNameCode0RobOneCard.RobbedPlayerSerials[i];
                ClientPlayer robbedClientPlayer = GetClientPlayerFromPlayerSerial(robbedPlayerSerial);
                if (robbedClientPlayer.IsSelf)
                    robbedClientPlayer.RemoveHandCard(enemyCharacterNameCode0RobOneCard.RobbedCardNameCodes[i]);
                else
                    robbedClientPlayer.EnemyClientPlayerDetails.PublicCardAmount--;
            }

            LogMaster.GetInstance().AddComponent_CastSkill(robberClientPlayer, 0);
            LogMaster.GetInstance().Log();

            for (int i = 0; i < enemyCharacterNameCode0RobOneCard.RobbedPlayerSerials.Length; i++)
            {
                LogMaster.GetInstance().AddComponent(robberClientPlayer);
                LogMaster.GetInstance().AddComponent("奪取");
                int robbedPlayerSerial = enemyCharacterNameCode0RobOneCard.RobbedPlayerSerials[i];
                ClientPlayer robbedClientPlayer = GetClientPlayerFromPlayerSerial(robbedPlayerSerial);
                LogMaster.GetInstance().AddComponent(robbedClientPlayer);
                LogMaster.GetInstance().AddComponent("的");
                LogMaster.GetInstance().AddComponent(DataMaster.GetInstance().GetCardInfo(enemyCharacterNameCode0RobOneCard.RobbedCardNameCodes[i]));
                LogMaster.GetInstance().Log();
            }

            if(enemyCharacterNameCode0RobOneCard.RobFailNameCode != -1)
            {
                LogMaster.GetInstance().AddComponent(robberClientPlayer);
                LogMaster.GetInstance().AddComponent("未能成功奪取");
                LogMaster.GetInstance().AddComponent(DataMaster.GetInstance().GetCardInfo(enemyCharacterNameCode0RobOneCard.RobFailNameCode));
                LogMaster.GetInstance().Log();
            }
        }

        public void EnemyCharacterNameCode1ChooseOneDeckCard(EnemyCharacterNameCode1ChooseOneDeckCard enemyCharacterNameCode1ChooseOneDeckCard)
        {
            ClientController.NextPacket();
            ClientPlayer enemyPlayer = GetClientPlayerFromPlayerSerial(enemyCharacterNameCode1ChooseOneDeckCard.SkillPlayerSerial);
            if (!enemyPlayer.IsSelf)
                enemyPlayer.EnemyClientPlayerDetails.PublicCardAmount ++;

            LogMaster.GetInstance().AddComponent_CastSkill(enemyPlayer, 1);
            LogMaster.GetInstance().AddComponent("\n");
            LogMaster.GetInstance().AddComponent(enemyPlayer);
            LogMaster.GetInstance().AddComponent("拿取");
            LogMaster.GetInstance().AddComponent(DataMaster.GetInstance().GetCardInfo(DeckCardHandlers[enemyCharacterNameCode1ChooseOneDeckCard.ChooseDeckCardIndex].NameCode));
            LogMaster.GetInstance().Log();
        }

        public void EnemyCharacterNameCode2DiscardOneCard(EnemyCharacterNameCode2DiscardOneCard enemyCharacterNameCode2DiscardOneCard)
        {
            ClientPlayer enemyPlayer = GetClientPlayerFromPlayerSerial(enemyCharacterNameCode2DiscardOneCard.SkillPlayerSerial);
            if (!enemyPlayer.IsSelf)
                enemyPlayer.AddReleaseCards(CardAnimationType.EnemyToGrave, enemyCharacterNameCode2DiscardOneCard.DiscardCardNameCode);
            else
                GravePileHandler.Amount++;

            LogMaster.GetInstance().AddComponent_CastSkill(enemyPlayer, 2);
            LogMaster.GetInstance().AddComponent("\n");
            LogMaster.GetInstance().AddComponent(enemyPlayer);
            LogMaster.GetInstance().AddComponent("丟棄");
            LogMaster.GetInstance().AddComponent(DataMaster.GetInstance().GetCardInfo(enemyCharacterNameCode2DiscardOneCard.DiscardCardNameCode));
            LogMaster.GetInstance().Log();

            if (!enemyPlayer.IsSelf)
                StartCoroutine(DestroyEnemyDiscardCards(new EnemyDiscardCards(enemyCharacterNameCode2DiscardOneCard.SkillPlayerSerial, enemyCharacterNameCode2DiscardOneCard.DiscardCardNameCode)));
            else
                ClientController.NextPacket();
        }

        public void EnemyCharacterNameCode3UseSkill(EnemyCharacterNameCode3UseSkill enemyCharacterNameCode3UseSkill)
        {
            ClientController.NextPacket();
            ClientPlayer enemyPlayer = GetClientPlayerFromPlayerSerial(enemyCharacterNameCode3UseSkill.SkillPlayerSerial);
            LogMaster.GetInstance().AddComponent_CastSkill(enemyPlayer, 3);
            LogMaster.GetInstance().Log();
        }

        public void EnemyCharacterNameCode6UseSkill(EnemyCharacterNameCode6UseSkill enemyCharacterNameCode6UseSkill)
        {
            ClientController.NextPacket();
            ClientPlayer enemyPlayer = GetClientPlayerFromPlayerSerial(enemyCharacterNameCode6UseSkill.SkillPlayerSerial);
            LogMaster.GetInstance().AddComponent_CastSkill(enemyPlayer, 6);
            LogMaster.GetInstance().Log();
        }

        public void EnemyCharacterNameCode8DiscardOneCard(EnemyCharacterNameCode8DiscardOneCard enemyCharacterNameCode8DiscardOneCard)
        {
            ClientPlayer enemyPlayer = GetClientPlayerFromPlayerSerial(enemyCharacterNameCode8DiscardOneCard.SkillPlayerSerial);
            if (!enemyPlayer.IsSelf)
                enemyPlayer.AddReleaseCards(CardAnimationType.EnemyToGrave, enemyCharacterNameCode8DiscardOneCard.DiscardCardNameCode);
            else
                GravePileHandler.Amount++;

            LogMaster.GetInstance().AddComponent_CastSkill(enemyPlayer, 8);
            LogMaster.GetInstance().AddComponent("\n");
            LogMaster.GetInstance().AddComponent(enemyPlayer);
            LogMaster.GetInstance().AddComponent("丟棄");
            LogMaster.GetInstance().AddComponent(DataMaster.GetInstance().GetCardInfo(enemyCharacterNameCode8DiscardOneCard.DiscardCardNameCode));
            LogMaster.GetInstance().Log();

            if (!enemyPlayer.IsSelf)
                StartCoroutine(DestroyEnemyDiscardCards(new EnemyDiscardCards(enemyCharacterNameCode8DiscardOneCard.SkillPlayerSerial, enemyCharacterNameCode8DiscardOneCard.DiscardCardNameCode)));
            else
                ClientController.NextPacket();
        }

        public void EnemyCharacterNameCode10DiscardOneDeckCard(EnemyCharacterNameCode10DiscardOneDeckCard enemyCharacterNameCode10DiscardOneDeckCard)
        {
            ClientController.NextPacket();
            ClientPlayer enemyPlayer = GetClientPlayerFromPlayerSerial(enemyCharacterNameCode10DiscardOneDeckCard.SkillPlayerSerial);

            int nameCode = DeckCardHandlers[enemyCharacterNameCode10DiscardOneDeckCard.DiscardDeckCardIndex].NameCode;
            DeckCardHandlers[enemyCharacterNameCode10DiscardOneDeckCard.DiscardDeckCardIndex].NameCode = -2;

            GravePileHandler.Amount++;

            LogMaster.GetInstance().AddComponent_CastSkill(enemyPlayer, 10);
            LogMaster.GetInstance().AddComponent("\n");
            LogMaster.GetInstance().AddComponent(enemyPlayer);
            LogMaster.GetInstance().AddComponent("將");
            if(nameCode != -1)
                LogMaster.GetInstance().AddComponent(DataMaster.GetInstance().GetCardInfo(nameCode));
            else
                LogMaster.GetInstance().AddComponent("1張蓋牌");
            LogMaster.GetInstance().AddComponent("放入棄牌堆");
            LogMaster.GetInstance().Log();
        }

        public void EnemyCharacterNameCode13RobOneCard(EnemyCharacterNameCode13RobOneCard enemyCharacterNameCode13RobOneCard)
        {
            ClientController.NextPacket();
            ClientPlayer robberClientPlayer = GetClientPlayerFromPlayerSerial(enemyCharacterNameCode13RobOneCard.RobberPlayerSerial);
            ClientPlayer robbedClientPlayer = GetClientPlayerFromPlayerSerial(enemyCharacterNameCode13RobOneCard.RobbedPlayerSerial);
            if (robberClientPlayer.IsSelf)
                robberClientPlayer.AddHandCard(enemyCharacterNameCode13RobOneCard.RobbedCardNameCode);
            else
                robberClientPlayer.EnemyClientPlayerDetails.PublicCardAmount++;
            if (robbedClientPlayer.IsSelf)
                robbedClientPlayer.RemoveHandCard(enemyCharacterNameCode13RobOneCard.RobbedCardNameCode);
            else
                robbedClientPlayer.EnemyClientPlayerDetails.PublicCardAmount--;


            LogMaster.GetInstance().AddComponent_CastSkill(robberClientPlayer, 13);
            LogMaster.GetInstance().AddComponent("\n");
            LogMaster.GetInstance().AddComponent(robberClientPlayer);
            LogMaster.GetInstance().AddComponent("奪取");
            LogMaster.GetInstance().AddComponent(robbedClientPlayer);
            LogMaster.GetInstance().AddComponent("的");
            if (enemyCharacterNameCode13RobOneCard.RobbedCardNameCode != -1)
                LogMaster.GetInstance().AddComponent(DataMaster.GetInstance().GetCardInfo(enemyCharacterNameCode13RobOneCard.RobbedCardNameCode));
            else
                LogMaster.GetInstance().AddComponent("1張牌");
            LogMaster.GetInstance().Log();
        }

        public void EnemyCharacterNameCode14PlaceBackDeckCard(EnemyCharacterNameCode14PlaceBackDeckCard enemyCharacterNameCode14PlaceBackDeckCard)
        {
            ClientController.NextPacket();
            ClientPlayer enemyPlayer = GetClientPlayerFromPlayerSerial(enemyCharacterNameCode14PlaceBackDeckCard.SkillPlayerSerial);

            DeckCardHandlers[enemyCharacterNameCode14PlaceBackDeckCard.PlaceBackDeckIndex].NameCode = enemyCharacterNameCode14PlaceBackDeckCard.PlaceBackCardNameCode;

            if(!enemyPlayer.IsSelf)
                enemyPlayer.EnemyClientPlayerDetails.PublicCardAmount--;

            LogMaster.GetInstance().AddComponent_CastSkill(enemyPlayer, 14);
            LogMaster.GetInstance().AddComponent("\n");
            LogMaster.GetInstance().AddComponent(enemyPlayer);
            LogMaster.GetInstance().AddComponent("將");
            LogMaster.GetInstance().AddComponent(DataMaster.GetInstance().GetCardInfo(enemyCharacterNameCode14PlaceBackDeckCard.PlaceBackCardNameCode));
            LogMaster.GetInstance().AddComponent("放回場上");
            LogMaster.GetInstance().Log();
        }

        public void EnemyCharacterNameCode18ExpandDeckCardToEnemy(EnemyCharacterNameCode18ExpandDeckCardToEnemy enemyCharacterNameCode18ExpandDeckCardToEnemy)
        {
            ClientController.NextPacket();
            ClientPlayer skillPlayer = GetClientPlayerFromPlayerSerial(enemyCharacterNameCode18ExpandDeckCardToEnemy.SkillPlayerSerial);
            ClientPlayer expandedPlayer = GetClientPlayerFromPlayerSerial(enemyCharacterNameCode18ExpandDeckCardToEnemy.DecideEnemySerial);
            if (skillPlayer.IsSelf)
                skillPlayer.RemoveHandCard(enemyCharacterNameCode18ExpandDeckCardToEnemy.ExpandCardNameCode);
            else
                skillPlayer.EnemyClientPlayerDetails.PublicCardAmount--;
            if (expandedPlayer.IsSelf)
                expandedPlayer.AddHandCard(enemyCharacterNameCode18ExpandDeckCardToEnemy.ExpandCardNameCode);
            else
                expandedPlayer.EnemyClientPlayerDetails.PublicCardAmount++;


            LogMaster.GetInstance().AddComponent_CastSkill(skillPlayer, 18);
            LogMaster.GetInstance().AddComponent("\n");
            LogMaster.GetInstance().AddComponent(skillPlayer);
            LogMaster.GetInstance().AddComponent("將");
            LogMaster.GetInstance().AddComponent(DataMaster.GetInstance().GetCardInfo(enemyCharacterNameCode18ExpandDeckCardToEnemy.ExpandCardNameCode));
            LogMaster.GetInstance().AddComponent("拓殖給");
            LogMaster.GetInstance().AddComponent(expandedPlayer);
            LogMaster.GetInstance().Log();
        }


        // process //

        void UpdateSkillButtons(bool needToChooseEffect, bool isChoosingEffect, bool isMidTurnSkill, bool isLegalEffect)
        {
            if (needToChooseEffect)
            {
                if (isChoosingEffect)
                {
                    CastBTN.interactable = isLegalEffect;
                    ChooseEffectBTN.gameObject.SetActive(false);
                    CastBTN.gameObject.SetActive(true);
                    CancelBTN.gameObject.SetActive(true);
                    GiveUpBTN.gameObject.SetActive(false);
                } else
                {
                    ChooseEffectBTN.gameObject.SetActive(true);
                    CastBTN.gameObject.SetActive(false);
                    CancelBTN.gameObject.SetActive(false);
                    GiveUpBTN.gameObject.SetActive(!isMidTurnSkill);
                }
            } else
            {
                ChooseEffectBTN.gameObject.SetActive(false);
                CastBTN.gameObject.SetActive(true);
                CancelBTN.gameObject.SetActive(false);
                GiveUpBTN.gameObject.SetActive(!isMidTurnSkill);
            }
        }

        void DisactivateAllSkillButtons()
        {
            ChooseEffectBTN.gameObject.SetActive(false);
            CastBTN.gameObject.SetActive(false);
            CancelBTN.gameObject.SetActive(false);
            GiveUpBTN.gameObject.SetActive(false);
        }

        void ChooseToReleaseFromHandCard(int cardIndex) // to release
        {
            HandCardHandler.ChangeHandCardIsChosen(cardIndex, !_ChooseToReleaseFromHandCardsBuffer.Contains(cardIndex));
            if (_ChooseToReleaseFromHandCardsBuffer.Contains(cardIndex)) _ChooseToReleaseFromHandCardsBuffer.Remove(cardIndex);
            else _ChooseToReleaseFromHandCardsBuffer.Add(cardIndex);

            ReleaseBTN.interactable = ChosensCanRelease();

            if (_ChooseToReleaseFromHandCardsBuffer.Count == 0)
                ChangeStage(ClientStage.MidTurn);
        }

        void ChooseRequirementFromHandCard(int cardIndex) // to release
        {
            HandCardHandler.ChangeHandCardIsChosen(cardIndex, !_ChooseRequirementFromHandCardsBuffer.Contains(cardIndex));
            if (_ChooseRequirementFromHandCardsBuffer.Contains(cardIndex)) _ChooseRequirementFromHandCardsBuffer.Remove(cardIndex);
            else _ChooseRequirementFromHandCardsBuffer.Add(cardIndex);


            CardDetailHandler.CardDetailHandlerUI.OkBTN.interactable = ChosensMatchRequirement();
        }

        void ChooseFromCoveredCards(int cardIndex) {
            DeckCardHandlers[cardIndex].IsChosen = !DeckCardHandlers[cardIndex].IsChosen;
            if(DeckCardHandlers[cardIndex].IsChosen) _ChooseFromCoveredCardsBuffer.Add(cardIndex);
            else _ChooseFromCoveredCardsBuffer.Remove(cardIndex);

            ChooseBTN.interactable = _ChooseFromCoveredCardsBuffer.Count > 0;

            int maxBufferCount = 2;
            // 卡爾·約翰，被動技，抽牌階段選擇覆蓋的牌時，可以觀看最多三張；主動技，剩餘的牌放回桌面時可以不翻開。
            if (GetClientPlayerFromPlayerSerial(SelfPlayerSerial).CharacterNameCode == 15)
                maxBufferCount = 3;

            if (_ChooseFromCoveredCardsBuffer.Count == maxBufferCount || _ChooseFromCoveredCardsBuffer.Count ==  DeckCoveredCardsCount()) {
                ChangeStage(ClientStage.ChooseFromRevealedCard);
                Send_RevealSeveralCoveredCard(new RevealSeveralCoveredCards(_ChooseFromCoveredCardsBuffer));
            } else if (_ChooseFromCoveredCardsBuffer.Count == 0) {
                ChangeStage(ClientStage.DrawCard);
            }
        }

        int DeckCoveredCardsCount() {
            int ret = 0;
            foreach (CardHandler cardHandler in DeckCardHandlers) if (cardHandler.NameCode == -1) ret++;
            return ret;
        }

        int[] GetProfileSerials(int[] playerSerials, int selfPlayerSerial, int playerAmount)
        {
            int[] ret = new int[playerAmount];
            int selfIndex = -1;
            for (int i = 0; i < playerAmount; i++) if (playerSerials[i] == selfPlayerSerial) selfIndex = i;
            for (int i=0;i< playerAmount; i++)
            {
                int availableIndex = (i - selfIndex >= 0) ? i - selfIndex : i - selfIndex + playerAmount;
                ret[i] = availableIndex;
            }
            return ret;
        }

        void InitDeckCards(int playerAmount)
        {
            DeckCardHandlers = new CardHandler[playerAmount * 3];
            for (int i = 0; i < playerAmount * 3; i++) DeckCardHandlers[i] = Instantiate(CardPRF, DeckCardsFolder).GetComponent<CardHandler>();


            for (int i = 0; i < 3; i++)
                for (int j = 0; j < playerAmount; j++)
                    DeckCardHandlers[i * playerAmount + j].Init(new Vector3(j * 95 - 95 - 47.5f * (playerAmount - 3), i * -136 + 136, 0), CardPlace.Deck, i * playerAmount + j, -2, this);

        }

        bool CanGetCardFromDeck()
        {
            int coverCardCount = 0;
            int validOpenCardCount = 0;
            foreach(CardHandler cardHandler in DeckCardHandlers)
            {
                if (cardHandler.NameCode == -1)
                    coverCardCount++;
                if (cardHandler.NameCode >= 0)
                    if (GetClientPlayerFromPlayerSerial(SelfPlayerSerial).CanGainCard(cardHandler.CardInfo))
                        validOpenCardCount++;
            }

            return !(coverCardCount == 0 && validOpenCardCount == 0);
        }

        public bool HasCertainPowerTypePowerCardInDeck(PowerType powerType)
        {
            foreach(CardHandler deckCardHandler in DeckCardHandlers)
            {
                if (deckCardHandler.NameCode == -2 || deckCardHandler.NameCode == -1) continue;
                CardInfo cardInfo = deckCardHandler.CardInfo;
                if (cardInfo.IsPower && cardInfo.PublicInfo.PowerType == powerType) return true;
            }
            return false;
        }

        public bool HasCertainPowerTypePowerCardInHand(PowerType powerType)
        {
            foreach (int nameCode in GetClientPlayerFromPlayerSerial(SelfPlayerSerial).SelfClientPlayerDetails.HandCards)
            {
                CardInfo cardInfo = DataMaster.GetInstance().GetCardInfo(nameCode);
                if (cardInfo.IsPower && cardInfo.PublicInfo.PowerType == powerType) return true;
            }
            return false;
        }

        bool PossibleToRelease()
        {
            bool[][] hasThesePowerCard = new bool[4][];
            for (int i = 0; i < 4; i++)
            {
                hasThesePowerCard[i] = new bool[9];
                for (int j = 0; j < 9; j++) hasThesePowerCard[i][j] = false;
            }
            foreach (int nameCode in GetClientPlayerFromPlayerSerial(SelfPlayerSerial).SelfClientPlayerDetails.HandCards)
            {
                CardInfo cardInfo = DataMaster.GetInstance().GetCardInfo(nameCode);
                if (cardInfo.IsPower)
                    hasThesePowerCard[(int)cardInfo.PublicInfo.PowerType][cardInfo.PublicInfo.PublicCardClass-1] = true;
            }

            for (int i = 0; i < 4; i++) for (int j = 0; j < 7; j++) if(hasThesePowerCard[i][j] && hasThesePowerCard[i][j+1] && hasThesePowerCard[i][j+2]) return true;
            return false;
        }

        bool ChosensCanRelease()
        {
            if (_ChooseToReleaseFromHandCardsBuffer.Count < 3) return false;
            bool[][] hasThesePowerCard = new bool[4][];
            for (int i = 0; i < 4; i++)
            {
                hasThesePowerCard[i] = new bool[9];
                for (int j = 0; j < 9; j++) hasThesePowerCard[i][j] = false;
            }
            foreach (int cardIndex in _ChooseToReleaseFromHandCardsBuffer)
            {
                int nameCode = GetClientPlayerFromPlayerSerial(SelfPlayerSerial).SelfClientPlayerDetails.HandCards[cardIndex];

                CardInfo cardInfo = DataMaster.GetInstance().GetCardInfo(nameCode);
                if (cardInfo.IsPower)
                    hasThesePowerCard[(int)cardInfo.PublicInfo.PowerType][cardInfo.PublicInfo.PublicCardClass - 1] = true;
            }

            for (int i = 0; i < 4; i++) for (int j = 0; j < 9; j++) if (hasThesePowerCard[i][j])
                    {
                        int seq_num = 0;
                        while (j < 9 && hasThesePowerCard[i][j++]) seq_num++;
                        return seq_num == _ChooseToReleaseFromHandCardsBuffer.Count;
                    }
            return false;
        }

        bool ChosensMatchRequirement()
        {
            if (NowUsingStrategyCard.StrategyInfo.Requirements.Count != _ChooseRequirementFromHandCardsBuffer.Count) return false;

            foreach(Requirement requirement in NowUsingStrategyCard.StrategyInfo.Requirements)
            {
                bool canFindThisRequriement = false;
                foreach(int cardIndex in _ChooseRequirementFromHandCardsBuffer)
                {
                    int nameCode = GetClientPlayerFromPlayerSerial(SelfPlayerSerial).SelfClientPlayerDetails.HandCards[cardIndex];
                    CardInfo cardInfo = DataMaster.GetInstance().GetCardInfo(nameCode);
                    if (cardInfo.IsPower)
                        if (cardInfo.PublicInfo.PowerType == requirement.PowerType && cardInfo.PublicInfo.Power >= requirement.Power)
                            canFindThisRequriement = true;
                }
                if (!canFindThisRequriement) return false;
            }

            return true;
        }

        public void DecideCharacter(int serial)
        {
            foreach (GameObject gameObject in CharacterCharacterGMO) Destroy(gameObject);
            Send_DecideCharacter(new DecideCharacter(serial));
        }

        public ClientPlayer GetClientPlayerFromPlayerSerial(int playerSerial)
        {
            return ClientPlayers.Find(clientPlayer => clientPlayer.PlayerSerial == playerSerial);
        }

        // delegate //

        IEnumerator DestroyEnemyDecideCardFromRevealedCoveredCards(object o)
        {
            EnemyDecideCardFromRevealedCoveredCards enemyDecideCardFromRevealedCoveredCards = (EnemyDecideCardFromRevealedCoveredCards)o;
            ClientPlayer emenyClientPlayer = GetClientPlayerFromPlayerSerial(enemyDecideCardFromRevealedCoveredCards.EnemySerial);

            yield return emenyClientPlayer.EnemyClientPlayerDetails.ReleaseCardsHandler.PlayAnimation();
            emenyClientPlayer.EnemyClientPlayerDetails.PublicCardAmount++;
            StartCoroutine(ClientController.WaitSecNextPacket(0.5f, null, null));
        }

        IEnumerator DestroyEnemyDecideCardFromOpenCard(object o)
        {
            EnemyDecideCardFromOpenCard enemyDecideCardFromOpenCard = (EnemyDecideCardFromOpenCard)o;
            ClientPlayer emenyClientPlayer = GetClientPlayerFromPlayerSerial(enemyDecideCardFromOpenCard.EnemySerial);

            yield return new WaitForSeconds(0.5f);
            yield return emenyClientPlayer.EnemyClientPlayerDetails.ReleaseCardsHandler.PlayAnimation();
            emenyClientPlayer.EnemyClientPlayerDetails.PublicCardAmount++;
            StartCoroutine(ClientController.WaitSecNextPacket(0.5f, null, null));
        }

        IEnumerator DestroyEnemyUseFunctionCard_Mask(object o)
        {
            EnemyUseFunctionCard_Mask enemyUseFunctionCard_Mask = (EnemyUseFunctionCard_Mask)o;
            ClientPlayer emenyClientPlayer = GetClientPlayerFromPlayerSerial(enemyUseFunctionCard_Mask.EnemySerial);

            yield return new WaitForSeconds(0.5f);
            yield return emenyClientPlayer.EnemyClientPlayerDetails.ReleaseCardsHandler.PlayAnimation();
            StartCoroutine(ClientController.WaitSecNextPacket(0.5f, null, null));
        }

        IEnumerator DestroyEnemyUseFunctionCard_Reform(object o)
        {
            EnemyUseFunctionCard_Reform enemyUseFunctionCard_Reform = (EnemyUseFunctionCard_Reform)o;
            ClientPlayer emenyClientPlayer = GetClientPlayerFromPlayerSerial(enemyUseFunctionCard_Reform.EnemySerial);

            yield return new WaitForSeconds(0.5f);
            GetClientPlayerFromPlayerSerial(enemyUseFunctionCard_Reform.EnemySerial).EnemyClientPlayerDetails.PublicCardAmount -= (enemyUseFunctionCard_Reform.DecideCardNameCode.Length + 1);
            yield return emenyClientPlayer.EnemyClientPlayerDetails.ReleaseCardsHandler.PlayAnimation();
            StartCoroutine(ClientController.WaitSecNextPacket(0.5f, null, null));
        }

        IEnumerator DestroyEnemyUseFunctionCard_Expand(object o)
        {
            EnemyUseFunctionCard_Expand enemyUseFunctionCard_Expand = (EnemyUseFunctionCard_Expand)o;
            ClientPlayer emenyClientPlayer = GetClientPlayerFromPlayerSerial(enemyUseFunctionCard_Expand.EnemySerial);

            yield return new WaitForSeconds(0.5f);
            yield return emenyClientPlayer.EnemyClientPlayerDetails.ReleaseCardsHandler.PlayAnimation();
            StartCoroutine(ClientController.WaitSecNextPacket(0.5f, null, null));
        }

        IEnumerator DestroyEnemyUseStrategyCard_Requirements(object o)
        {
            EnemyUseStrategyCard_Requirements enemyUseStrategyCard_Requirements = (EnemyUseStrategyCard_Requirements)o;
            ClientPlayer emenyClientPlayer = GetClientPlayerFromPlayerSerial(enemyUseStrategyCard_Requirements.EnemySerial);

            GetClientPlayerFromPlayerSerial(enemyUseStrategyCard_Requirements.EnemySerial).EnemyClientPlayerDetails.PublicCardAmount -= enemyUseStrategyCard_Requirements.RequirementCardNameCodes.Length;
            yield return new WaitForSeconds(0.5f);
            yield return emenyClientPlayer.EnemyClientPlayerDetails.ReleaseCardsHandler.PlayAnimation();
            GetClientPlayerFromPlayerSerial(enemyUseStrategyCard_Requirements.EnemySerial).EnemyClientPlayerDetails.StrategyCardAmount--;
            StartCoroutine(ClientController.WaitSecNextPacket(0.5f, null, null));
        }

        IEnumerator DestroyEnemyReleaseCard(object o)
        {
            EnemyReleaseCard enemyReleaseCard = (EnemyReleaseCard) o;
            ClientPlayer emenyClientPlayer = GetClientPlayerFromPlayerSerial(enemyReleaseCard.EnemySerial);

            GetClientPlayerFromPlayerSerial(enemyReleaseCard.EnemySerial).EnemyClientPlayerDetails.PublicCardAmount -= enemyReleaseCard.ReleaseCardNameCodes.Length;
            yield return new WaitForSeconds(0.5f);
            yield return emenyClientPlayer.EnemyClientPlayerDetails.ReleaseCardsHandler.PlayAnimation();
            GetClientPlayerFromPlayerSerial(enemyReleaseCard.EnemySerial).EnemyClientPlayerDetails.StrategyCardAmount++;
            StartCoroutine(ClientController.WaitSecNextPacket(0.5f, null, null));
        }

        IEnumerator DestroyEnemyDiscardCards(object o)
        {
            EnemyDiscardCards enemyDiscardCards = (EnemyDiscardCards)o;
            ClientPlayer emenyClientPlayer = GetClientPlayerFromPlayerSerial(enemyDiscardCards.EnemySerial);

            foreach (int nameCode in enemyDiscardCards.DiscardCardNameCodes)
            {
                if (DataMaster.GetInstance().GetCardInfo(nameCode).IsPublic)
                    GetClientPlayerFromPlayerSerial(enemyDiscardCards.EnemySerial).EnemyClientPlayerDetails.PublicCardAmount--;
                else
                    GetClientPlayerFromPlayerSerial(enemyDiscardCards.EnemySerial).EnemyClientPlayerDetails.StrategyCardAmount--;
            }

            yield return new WaitForSeconds(0.5f);
            yield return emenyClientPlayer.EnemyClientPlayerDetails.ReleaseCardsHandler.PlayAnimation();
            StartCoroutine(ClientController.WaitSecNextPacket(0.5f, null, null));
        }

        IEnumerator DestroyEnemiesGainCards(object o)
        {
            EnemyGainCards enemyGainCards = (EnemyGainCards)o;
            ClientPlayer emenyClientPlayer = GetClientPlayerFromPlayerSerial(enemyGainCards.EnemySerial);

            yield return new WaitForSeconds(0.5f);
            yield return emenyClientPlayer.EnemyClientPlayerDetails.ReleaseCardsHandler.PlayAnimation();
            StartCoroutine(ClientController.WaitSecNextPacket(0.5f, null, null));

            foreach (int nameCode in enemyGainCards.GainCardNameCodes)
            {
                if (DataMaster.GetInstance().GetCardInfo(nameCode).IsPublic)
                    GetClientPlayerFromPlayerSerial(enemyGainCards.EnemySerial).EnemyClientPlayerDetails.PublicCardAmount++;
                else
                    GetClientPlayerFromPlayerSerial(enemyGainCards.EnemySerial).EnemyClientPlayerDetails.StrategyCardAmount++;
            }
        }

        void ChangeStage(ClientStage toStage)
        {
            NowStage = toStage;
            if (toStage != ClientStage.UsingCard)
            {
                NowUsingCardStage = UsingCardStage.None;
                NowUsingStrategyStage = UsingStrategyStage.None;
            }
            if (toStage != ClientStage.UsingSkill)
                NowUsingSkillStage = UsingSkillStage.None;
        }

        void ChangeStage(UsingCardStage toStage)
        {
            NowStage = ClientStage.UsingCard;
            NowUsingCardStage = toStage;
            NowUsingSkillStage = UsingSkillStage.None;
            if (toStage != UsingCardStage.Strategy)
                NowUsingStrategyStage = UsingStrategyStage.None;
        }

        void ChangeStage(UsingSkillStage toStage)
        {
            NowStage = ClientStage.UsingSkill;
            NowUsingSkillStage = toStage;
            NowUsingCardStage = UsingCardStage.None;
            NowUsingStrategyStage = UsingStrategyStage.None;
        }



        void ChangeStage(UsingStrategyStage toStage)
        {
            NowStage = ClientStage.UsingCard;
            NowUsingSkillStage = UsingSkillStage.None;
            NowUsingCardStage = UsingCardStage.Strategy;
            NowUsingStrategyStage = toStage;
        }
    }
}
