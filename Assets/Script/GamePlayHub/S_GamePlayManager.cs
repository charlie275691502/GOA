using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Network;
using GamePlayHub.ClientToServer;
using GamePlayHub.ServerToClient;
using Main;

namespace GamePlayHub
{
    public class S_GamePlayManager : MonoBehaviour
    {
        public ServerController ServerController;

        public int ForceLockCharacterNameCode = -1;
        public int[] ForceGiveCard;
        public List<int> AvailableStrategyCards;

        public List<ServerPlayer> ServerPlayers = new List<ServerPlayer>();
        public List<int> DeckCards = new List<int>();
        public List<int> DrawCards = new List<int>();
        public List<int> GraveCards = new List<int>();
        public bool[] DeckCardsIsReveal;

        public List<int> StrategyDrawCards = new List<int>();
        public Stack<int> RevolutionStack = new Stack<int>();

        bool _IsGameStart = false;
        int _PlayerAmount;
        int _MaxDeckCardAmount;
        int _NowTurn = -1;
        int _NowTurnPlayerSerial;
        int _NowRound = 0;
        int[] _RevealCoveredCardIndexs;

        Queue<int> _CongressMovementSeq;
        int _NowCongressLeaderSerial = -1;
        int _CharacterNameCode1DelayEndTurnPlayerSerial = -1;

        public void Send_GamePlayInfos(GamePlayInfos gamePlayInfos, int playerSerial) {
            if (IsBot(playerSerial)) return;
            int clientSerial = GetClientSerialFromPlayerSerial(playerSerial);
            ServerController.SendData("GamePlayHub", "GamePlayInfos", gamePlayInfos, clientSerial);
        }

        public void Send_ChooseCharacterFromThree(ChooseCharacterFromThree chooseCharacterFromThree, int playerSerial)
        {
            if (IsBot(playerSerial)) {
                AI.ChooseCharacterFromThree(playerSerial, chooseCharacterFromThree, this);
                return;
            }
            int clientSerial = GetClientSerialFromPlayerSerial(playerSerial);
            ServerController.SendData("GamePlayHub", "ChooseCharacterFromThree", chooseCharacterFromThree, clientSerial);
        }

        public void Send_EnemyDecideCharacter_AllClient(EnemyDecideCharacter enemyDecideCharacter)
        {
            ServerController.SendToAllClient("GamePlayHub", "EnemyDecideCharacter", enemyDecideCharacter);
        }

        public void Send_DecideCharacterInfos_AllClient(DecideCharacterInfos decideCharacterInfos)
        {
            ServerController.SendToAllClient("GamePlayHub", "DecideCharacterInfos", decideCharacterInfos);
        }

        public void Send_UpdateDeckCards_AllClient(UpdateDeckCards updateDeckCards)
        {
            ServerController.SendToAllClient("GamePlayHub", "UpdateDeckCards", updateDeckCards);
        }

        public void Send_UpdatePileAmount_AllClient(UpdatePileAmount updatePileAmount)
        {
            ServerController.SendToAllClient("GamePlayHub", "UpdatePileAmount", updatePileAmount);
        }

        public void Send_TurnStart_AllPlayer(TurnStart turnStart)
        {
            ServerController.SendToAllClient("GamePlayHub", "TurnStart", turnStart);
            foreach(ServerPlayer serverPlayer in ServerPlayers) if(_NowTurnPlayerSerial == serverPlayer.PlayerSerial && serverPlayer.IsBot)
                    AI.TurnStart(serverPlayer.PlayerSerial, turnStart, this);
        }

        public void Send_RevealChosenCoveredCards(RevealChosenCoveredCards revealChosenCoveredCards, int playerSerial)
        {
            if (IsBot(playerSerial))
            {
                AI.RevealChosenCoveredCards(playerSerial, revealChosenCoveredCards, this);
                return;
            }
            int clientSerial = GetClientSerialFromPlayerSerial(playerSerial);
            ServerController.SendData("GamePlayHub", "RevealChosenCoveredCards", revealChosenCoveredCards, clientSerial);
        }

        public void Send_DrawCards(DrawCards drawCards, int playerSerial)
        {
            if (IsBot(playerSerial)) return;
            int clientSerial = GetClientSerialFromPlayerSerial(playerSerial);
            ServerController.SendData("GamePlayHub", "DrawCards", drawCards, clientSerial);
        }

        public void Send_DiscardCards(DiscardCards discardCards, int playerSerial)
        {
            if (IsBot(playerSerial)) return;
            int clientSerial = GetClientSerialFromPlayerSerial(playerSerial);
            ServerController.SendData("GamePlayHub", "DiscardCards", discardCards, clientSerial);
        }

        public void Send_HighlightDeckCards(HighlightDeckCards highlightDeckCards, int playerSerial)
        {
            if (IsBot(playerSerial))return;
            int clientSerial = GetClientSerialFromPlayerSerial(playerSerial);
            ServerController.SendData("GamePlayHub", "HighlightDeckCards", highlightDeckCards, clientSerial);
        }

        public void Send_EnemyDecideCardFromRevealedCoveredCards_AllClient(EnemyDecideCardFromRevealedCoveredCards enemyDecideCardFromRevealedCoveredCards)
        {
            ServerController.SendToAllClient("GamePlayHub", "EnemyDecideCardFromRevealedCoveredCards", enemyDecideCardFromRevealedCoveredCards);
        }

        public void Send_EnemyDecideCardFromOpenCard(EnemyDecideCardFromOpenCard enemyDecideCardFromOpenCard, int playerSerial)
        {
            if (IsBot(playerSerial)) return;
            int clientSerial = GetClientSerialFromPlayerSerial(playerSerial);
            ServerController.SendData("GamePlayHub", "EnemyDecideCardFromOpenCard", enemyDecideCardFromOpenCard, clientSerial);
        }

        public void Send_EnemyUseFunctionCard_Mask(EnemyUseFunctionCard_Mask enemyUseFunctionCard_Mask, int playerSerial)
        {
            if (IsBot(playerSerial)) return;
            int clientSerial = GetClientSerialFromPlayerSerial(playerSerial);
            ServerController.SendData("GamePlayHub", "EnemyUseFunctionCard_Mask", enemyUseFunctionCard_Mask, clientSerial);
        }

        public void Send_EnemyUseFunctionCard_Reform(EnemyUseFunctionCard_Reform enemyUseFunctionCard_Reform, int playerSerial)
        {
            if (IsBot(playerSerial)) return;
            int clientSerial = GetClientSerialFromPlayerSerial(playerSerial);
            ServerController.SendData("GamePlayHub", "EnemyUseFunctionCard_Reform", enemyUseFunctionCard_Reform, clientSerial);
        }

        public void Send_EnemyUseFunctionCard_Expand(EnemyUseFunctionCard_Expand enemyUseFunctionCard_Expand, int playerSerial)
        {
            if (IsBot(playerSerial)) return;
            int clientSerial = GetClientSerialFromPlayerSerial(playerSerial);
            ServerController.SendData("GamePlayHub", "EnemyUseFunctionCard_Expand", enemyUseFunctionCard_Expand, clientSerial);
        }

        public void Send_EnemyUseStrategyCard_Requirements(EnemyUseStrategyCard_Requirements enemyUseStrategyCard_Requirements, int playerSerial)
        {
            if (IsBot(playerSerial)) return;
            int clientSerial = GetClientSerialFromPlayerSerial(playerSerial);
            ServerController.SendData("GamePlayHub", "EnemyUseStrategyCard_Requirements", enemyUseStrategyCard_Requirements, clientSerial);
        }

        public void Send_EnemyReleaseCard(EnemyReleaseCard enemyReleaseCard, int playerSerial)
        {
            if (IsBot(playerSerial)) return;
            int clientSerial = GetClientSerialFromPlayerSerial(playerSerial);
            ServerController.SendData("GamePlayHub", "EnemyReleaseCard", enemyReleaseCard, clientSerial);
        }

        public void Send_EnemyDiscardCards(EnemyDiscardCards enemyDiscardCards, int playerSerial)
        {
            if (IsBot(playerSerial)) return;
            int clientSerial = GetClientSerialFromPlayerSerial(playerSerial);
            ServerController.SendData("GamePlayHub", "EnemyDiscardCards", enemyDiscardCards, clientSerial);
        }

        public void Send_EnemyGainCards(EnemyGainCards enemyGainCards, int playerSerial)
        {
            if (IsBot(playerSerial)) return;
            int clientSerial = GetClientSerialFromPlayerSerial(playerSerial);
            ServerController.SendData("GamePlayHub", "EnemyGainCards", enemyGainCards, clientSerial);
        }

        public void Send_EndStrategy(EndStrategy endStrategy, int playerSerial)
        {
            if (IsBot(playerSerial)) return;
            int clientSerial = GetClientSerialFromPlayerSerial(playerSerial);
            ServerController.SendData("GamePlayHub", "EndStrategy", endStrategy, clientSerial);
        }

        public void Send_RoundStart_AllClient(RoundStart roundStart)
        {
            ServerController.SendToAllClient("GamePlayHub", "RoundStart", roundStart);
        }

        public void Send_CongressTurnStart_AllClient(CongressTurnStart congressTurnStart)
        {
            ServerController.SendToAllClient("GamePlayHub", "CongressTurnStart", congressTurnStart);
        }

        public void Send_Victory_AllClient(Victory victory)
        {
            ServerController.SendToAllClient("GamePlayHub", "Victory", victory);
        }

        public void Send_Revolution_AllClient(Revolution revolution)
        {
            ServerController.SendToAllClient("GamePlayHub", "Revolution", revolution);
        }

        public void Send_UpdatePower_AllClient(UpdatePower updatePower)
        {
            ServerController.SendToAllClient("GamePlayHub", "UpdatePower", updatePower);
        }

        public void Send_UpdateUnmaskedPowerAndVictory_AllClient(UpdateUnmaskedPowerAndVictory updateUnmaskedPowerAndVictory)
        {
            ServerController.SendToAllClient("GamePlayHub", "UpdateUnmaskedPowerAndVictory", updateUnmaskedPowerAndVictory);
        }

        [HideInInspector] public bool Send_UpdatePowerLimit_Lock = false;
        public void Send_UpdatePowerLimit_AllClient(UpdatePowerLimit updatePowerLimit)
        {
            if (Send_UpdatePowerLimit_Lock) return;
            ServerController.SendToAllClient("GamePlayHub", "UpdatePowerLimit", updatePowerLimit);
        }

        public void Send_AddBuffs_AllClient(AddBuffs addBuffs)
        {
            ServerController.SendToAllClient("GamePlayHub", "AddBuffs", addBuffs);
        }
        
        public void Send_CharacterNameCode1AskIfCastSkill(CharacterNameCode1AskIfCastSkill characterNameCode1AskIfCastSkill, int playerSerial)
        {
            if (IsBot(playerSerial))
                return;
            int clientSerial = GetClientSerialFromPlayerSerial(playerSerial);
            ServerController.SendData("GamePlayHub", "CharacterNameCode1AskIfCastSkill", characterNameCode1AskIfCastSkill, clientSerial);
        }

        public void Send_StrategyNameCode20RobGainCardOneMoreTime(StrategyNameCode20RobGainCardOneMoreTime strategyNameCode20RobGainCardOneMoreTime, int playerSerial)
        {
            if (IsBot(playerSerial)) return;
            int clientSerial = GetClientSerialFromPlayerSerial(playerSerial);
            ServerController.SendData("GamePlayHub", "StrategyNameCode20RobGainCardOneMoreTime", strategyNameCode20RobGainCardOneMoreTime, clientSerial);
        }

        public void Send_StrategyNameCode22ChooseDiscardOneHandCard(StrategyNameCode22ChooseDiscardOneHandCard strategyNameCode22ChooseDiscardOneHandCard, int playerSerial)
        {
            if (IsBot(playerSerial)) return;
            int clientSerial = GetClientSerialFromPlayerSerial(playerSerial);
            ServerController.SendData("GamePlayHub", "StrategyNameCode22ChooseDiscardOneHandCard", strategyNameCode22ChooseDiscardOneHandCard, clientSerial);
        }

        public void Send_EnemyCharacterNameCode0RobOneCard_AllClient(EnemyCharacterNameCode0RobOneCard enemyCharacterNameCode0RobOneCard)
        {
            ServerController.SendToAllClient("GamePlayHub", "EnemyCharacterNameCode0RobOneCard", enemyCharacterNameCode0RobOneCard);
        }

        public void Send_EnemyCharacterNameCode1ChooseOneDeckCard_AllClient(EnemyCharacterNameCode1ChooseOneDeckCard enemyCharacterNameCode1ChooseOneDeckCard)
        {
            ServerController.SendToAllClient("GamePlayHub", "EnemyCharacterNameCode1ChooseOneDeckCard", enemyCharacterNameCode1ChooseOneDeckCard);
        }

        public void Send_EnemyCharacterNameCode2DiscardOneCard_AllClient(EnemyCharacterNameCode2DiscardOneCard enemyCharacterNameCode2DiscardOneCard)
        {
            ServerController.SendToAllClient("GamePlayHub", "EnemyCharacterNameCode2DiscardOneCard", enemyCharacterNameCode2DiscardOneCard);
        }

        public void Send_EnemyCharacterNameCode3UseSkill_AllClient(EnemyCharacterNameCode3UseSkill enemyCharacterNameCode3UseSkill)
        {
            ServerController.SendToAllClient("GamePlayHub", "EnemyCharacterNameCode3UseSkill", enemyCharacterNameCode3UseSkill);
        }

        public void Send_EnemyCharacterNameCode6UseSkill_AllClient(EnemyCharacterNameCode6UseSkill enemyCharacterNameCode6UseSkill)
        {
            ServerController.SendToAllClient("GamePlayHub", "EnemyCharacterNameCode6UseSkill", enemyCharacterNameCode6UseSkill);
        }

        public void Send_EnemyCharacterNameCode8DiscardOneCard_AllClient(EnemyCharacterNameCode8DiscardOneCard enemyCharacterNameCode8DiscardOneCard)
        {
            ServerController.SendToAllClient("GamePlayHub", "EnemyCharacterNameCode8DiscardOneCard", enemyCharacterNameCode8DiscardOneCard);
        }

        public void Send_EnemyCharacterNameCode10DiscardOneDeckCard_AllClient(EnemyCharacterNameCode10DiscardOneDeckCard enemyCharacterNameCode10DiscardOneDeckCard)
        {
            ServerController.SendToAllClient("GamePlayHub", "EnemyCharacterNameCode10DiscardOneDeckCard", enemyCharacterNameCode10DiscardOneDeckCard);
        }

        public void Send_EnemyCharacterNameCode13RobOneCard(EnemyCharacterNameCode13RobOneCard enemyCharacterNameCode13RobOneCard, int playerSerial)
        {
            if (IsBot(playerSerial)) return;
            int clientSerial = GetClientSerialFromPlayerSerial(playerSerial);
            ServerController.SendData("GamePlayHub", "EnemyCharacterNameCode13RobOneCard", enemyCharacterNameCode13RobOneCard, clientSerial);
        }

        public void Send_EnemyCharacterNameCode14PlaceBackDeckCard_AllClient(EnemyCharacterNameCode14PlaceBackDeckCard enemyCharacterNameCode14PlaceBackDeckCard)
        {
            ServerController.SendToAllClient("GamePlayHub", "EnemyCharacterNameCode14PlaceBackDeckCard", enemyCharacterNameCode14PlaceBackDeckCard);
        }

        public void Send_EnemyCharacterNameCode18ExpandDeckCardToEnemy_AllClient(EnemyCharacterNameCode18ExpandDeckCardToEnemy enemyCharacterNameCode18ExpandDeckCardToEnemy)
        {
            ServerController.SendToAllClient("GamePlayHub", "EnemyCharacterNameCode18ExpandDeckCardToEnemy", enemyCharacterNameCode18ExpandDeckCardToEnemy);
        }

        // recieve //

        public void SceneReady(int playerSerial, SceneReady sceneReady)
        {
            GetServerPlayerFromPlayerSerial(playerSerial).IsSceneReady = true;

            if (AllServerPlayerSceneReady() && !_IsGameStart) GameStart();
        }

        public void DecideCharacter(int playerSerial, DecideCharacter decideCharacter)
        {
            ServerPlayer serverPlayer = GetServerPlayerFromPlayerSerial(playerSerial);
            serverPlayer.CharacterNameCode = decideCharacter.DecideCharacterNameCode;

            // 測試用，綁定角色
            if (ForceLockCharacterNameCode != -1 && serverPlayer.Nick == "Lemonkey") serverPlayer.CharacterNameCode = ForceLockCharacterNameCode;

            // 測試用，給1P一張指定的決策牌 // 不會從決策牌抽牌堆中移除這張牌
            if (ForceGiveCard.Length > 0 && serverPlayer.Nick == "Lemonkey")
            {
                GetServerPlayerFromPlayerSerial(playerSerial).AddHandCard(ForceGiveCard);
                Send_DrawCards(new DrawCards(ForceGiveCard), playerSerial);
            }

            serverPlayer.IsDecideCharacterReady = true;
            Send_EnemyDecideCharacter_AllClient(new EnemyDecideCharacter(playerSerial));
            if (AllServerPlayerDecideCharacterReady()) RevealDecideCharacter();
        }

        public void RevealSeveralCoveredCards(int playerSerial, RevealSeveralCoveredCards revealSeveralCoveredCards)
        {
            // index -> namecode
            int[] coveredCardNameCodes = new int[revealSeveralCoveredCards.RevealCoveredCardIndexs.Length];
            for (int i = 0; i < revealSeveralCoveredCards.RevealCoveredCardIndexs.Length; i++) coveredCardNameCodes[i] = DeckCards[revealSeveralCoveredCards.RevealCoveredCardIndexs[i]];

            // 先send給其他玩家
            HighlightDeckCards highlightDeckCards = new HighlightDeckCards(revealSeveralCoveredCards.RevealCoveredCardIndexs);
            foreach (ServerPlayer serverPlayer in ServerPlayers)
                if (serverPlayer.PlayerSerial != playerSerial)
                    Send_HighlightDeckCards(highlightDeckCards, serverPlayer.PlayerSerial);

            Send_RevealChosenCoveredCards(new RevealChosenCoveredCards(coveredCardNameCodes), playerSerial);
        }

        public void DecideCardFromRevealedCoveredCards(int playerSerial, DecideCardFromRevealedCoveredCards decideCardFromRevealedCoveredCards)
        {
            int[] revealCardNameCodes = new int[decideCardFromRevealedCoveredCards.RevealCardIndex.Length];
            for (int i=0;i< decideCardFromRevealedCoveredCards.RevealCardIndex.Length;i++) {
                DeckCardsIsReveal[decideCardFromRevealedCoveredCards.RevealCardIndex[i]] = true;
                revealCardNameCodes[i] = DeckCards[decideCardFromRevealedCoveredCards.RevealCardIndex[i]];
            }
            GainDeckCard(playerSerial, decideCardFromRevealedCoveredCards.DecideCardIndex);

            EnemyDecideCardFromRevealedCoveredCards enemyDecideCardFromRevealedCoveredCards =
                new EnemyDecideCardFromRevealedCoveredCards(playerSerial,
                                                            decideCardFromRevealedCoveredCards.CandidateCardIndex,
                                                            decideCardFromRevealedCoveredCards.DecideCardIndex,
                                                            decideCardFromRevealedCoveredCards.RevealCardIndex,
                                                            revealCardNameCodes);
            Send_EnemyDecideCardFromRevealedCoveredCards_AllClient(enemyDecideCardFromRevealedCoveredCards);
        }

        public void DecideCardFromOpenCard(int playerSerial, DecideCardFromOpenCard decideCardFromOpenCard)
        {
            // 先send給其他玩家
            EnemyDecideCardFromOpenCard enemyDecideCardFromOpenCard = new EnemyDecideCardFromOpenCard(playerSerial, decideCardFromOpenCard.DecideCardIndex);
            foreach (ServerPlayer sP in ServerPlayers)
                Send_EnemyDecideCardFromOpenCard(enemyDecideCardFromOpenCard, sP.PlayerSerial);
            GainDeckCard(playerSerial, decideCardFromOpenCard.DecideCardIndex);
            Send_UpdateDeckCards_AllClient(new UpdateDeckCards(DeckCards, DeckCardsIsReveal));
        }

        public void UseFunctionCard_Mask(int playerSerial, UseFunctionCard_Mask useFunctionCard_Mask)
        {
            GetServerPlayerFromPlayerSerial(playerSerial).RemoveHandCard(true, useFunctionCard_Mask.FunctionCardNameCode);
            int nameCode = DrawOnePublicCard();
            GetServerPlayerFromPlayerSerial(playerSerial).AddHandCard(nameCode);

            EnemyUseFunctionCard_Mask enemyUseFunctionCard_Mask = new EnemyUseFunctionCard_Mask(playerSerial, useFunctionCard_Mask.FunctionCardNameCode);
            foreach (ServerPlayer serverPlayer in ServerPlayers)
                Send_EnemyUseFunctionCard_Mask(enemyUseFunctionCard_Mask, serverPlayer.PlayerSerial);

            Send_DrawCards(new DrawCards(nameCode), playerSerial);
            Send_UpdatePileAmount_AllClient(new UpdatePileAmount(DrawCards.Count, GraveCards.Count));
        }

        public void UseFunctionCard_Reform(int playerSerial, UseFunctionCard_Reform useFunctionCard_Reform)
        {
            int[] removedCards = new int[useFunctionCard_Reform.DecideCardNameCodes.Length+1];
            removedCards[0] = useFunctionCard_Reform.FunctionCardNameCode;
            for(int i=0;i< useFunctionCard_Reform.DecideCardNameCodes.Length;i++)
                removedCards[i+1] = useFunctionCard_Reform.DecideCardNameCodes[i];

            GetServerPlayerFromPlayerSerial(playerSerial).RemoveHandCard(true, removedCards);

            EnemyUseFunctionCard_Reform enemyUseFunctionCard_Reform = new EnemyUseFunctionCard_Reform(playerSerial, useFunctionCard_Reform.FunctionCardNameCode, useFunctionCard_Reform.DecideCardNameCodes);
            foreach (ServerPlayer serverPlayer in ServerPlayers)
                Send_EnemyUseFunctionCard_Reform(enemyUseFunctionCard_Reform, serverPlayer.PlayerSerial);
        }

        public void UseFunctionCard_Expand(int playerSerial, UseFunctionCard_Expand useFunctionCard_Expand)
        {
            GetServerPlayerFromPlayerSerial(playerSerial).RemoveHandCard(true, useFunctionCard_Expand.FunctionCardNameCode);
            int ExpandedNameCode = DeckCards[useFunctionCard_Expand.DecideDeckCardIndex];

            EnemyUseFunctionCard_Expand enemyUseFunctionCard_Expand = new EnemyUseFunctionCard_Expand(playerSerial, useFunctionCard_Expand.FunctionCardNameCode, ExpandedNameCode, useFunctionCard_Expand.DecideDeckCardIndex);
            foreach (ServerPlayer serverPlayer in ServerPlayers)
                Send_EnemyUseFunctionCard_Expand(enemyUseFunctionCard_Expand, serverPlayer.PlayerSerial);

            GainDeckCard(playerSerial, useFunctionCard_Expand.DecideDeckCardIndex);
            Send_UpdateDeckCards_AllClient(new UpdateDeckCards(DeckCards, DeckCardsIsReveal));
        }

        public void UseStrategyCard_Requirements(int playerSerial, UseStrategyCard_Requirements useStrategyCard_Requirements)
        {
            GetServerPlayerFromPlayerSerial(playerSerial).RemoveHandCard(false, useStrategyCard_Requirements.StrategyCardNameCode);
            GetServerPlayerFromPlayerSerial(playerSerial).RemoveHandCard(true , useStrategyCard_Requirements.RequirementCardNameCodes);

            EnemyUseStrategyCard_Requirements enemyUseStrategyCard_Requirements = new EnemyUseStrategyCard_Requirements(playerSerial, useStrategyCard_Requirements.StrategyCardNameCode, useStrategyCard_Requirements.RequirementCardNameCodes);
            foreach (ServerPlayer serverPlayer in ServerPlayers)
                Send_EnemyUseStrategyCard_Requirements(enemyUseStrategyCard_Requirements, serverPlayer.PlayerSerial);

            GeneralStrategyProcessor(playerSerial, useStrategyCard_Requirements);
        }

        public void ReleasePower(int playerSerial, ReleasePower releasePower)
        {
            GetServerPlayerFromPlayerSerial(playerSerial).RemoveHandCard(true, releasePower.ReleasedCardNameCodes);

            EnemyReleaseCard enemyReleaseCard = new EnemyReleaseCard(playerSerial, releasePower.ReleasedCardNameCodes);
            foreach (ServerPlayer serverPlayer in ServerPlayers)
                Send_EnemyReleaseCard(enemyReleaseCard, serverPlayer.PlayerSerial);

            int nameCode = DrawOneStrategyCard();
            GetServerPlayerFromPlayerSerial(playerSerial).AddHandCard(nameCode);
            Send_DrawCards(new DrawCards(nameCode), playerSerial);
            RefillDeckCards();
            Send_UpdateDeckCards_AllClient(new UpdateDeckCards(DeckCards, DeckCardsIsReveal));
        }

        public void DiscardStrategyCard(int playerSerial, DiscardStrategyCard discardStrategyCard)
        {
            GetServerPlayerFromPlayerSerial(playerSerial).RemoveHandCard(false, discardStrategyCard.StrategyCardNameCode);

            EnemyDiscardCards enemyDiscardCards = new EnemyDiscardCards(playerSerial, discardStrategyCard.StrategyCardNameCode);
            foreach (ServerPlayer serverPlayer in ServerPlayers)
                Send_EnemyDiscardCards(enemyDiscardCards, serverPlayer.PlayerSerial);
        }

        public void EndTurn(int playerSerial, EndTurn endTurn)
        {
            ServerPlayer serverPlayer = GetServerPlayerFromPlayerSerial(playerSerial);
            int victoryType = GetVictoryType(playerSerial);
            if (victoryType != -1)
            {
                Send_Victory_AllClient(new Victory(playerSerial, victoryType, ServerPlayers, RevolutionStack));
                ServerController.RemoveSubscriptor("GamePlayHub_16");
                Destroy(gameObject);
            } else if (IsRevolution(playerSerial))
            {
                RevolutionStack.Push(playerSerial);
                Send_UpdatePower_AllClient(new UpdatePower(ServerPlayers));
                Send_Revolution_AllClient(new Revolution(playerSerial, serverPlayer.HandCards));
                GetServerPlayerFromPlayerSerial(playerSerial).IsDead = true;
            } else if (serverPlayer.StrategyCardAmount > serverPlayer.StrategyCardLimit)
            {
                int strategyCardNameCode = serverPlayer.HandCards.Find(nameCode => DataMaster.GetInstance().GetCardInfo(nameCode).IsStrategy);

                Send_DiscardCards(new DiscardCards(strategyCardNameCode), playerSerial);
                EnemyDiscardCards enemyDiscardCards = new EnemyDiscardCards(playerSerial, strategyCardNameCode);
                foreach (ServerPlayer sP in ServerPlayers)
                    if (sP.PlayerSerial != playerSerial)
                        Send_EnemyDiscardCards(enemyDiscardCards, sP.PlayerSerial);
            }


            // 米歇爾·內伊，主動技，當其他玩家回合結束時，可以拿取場上任意一張翻開的牌，此技能一輪會議周期內只能發動一次。
            foreach (ServerPlayer sP in ServerPlayers)
                if (sP.CharacterNameCode == 1 && !sP.CharacterNameCode1SkillUsed && sP.PlayerSerial != playerSerial && !sP.IsDead) // 自己的回合不能高地衝鋒
                {
                    Send_CharacterNameCode1AskIfCastSkill(new CharacterNameCode1AskIfCastSkill(), sP.PlayerSerial);
                    _CharacterNameCode1DelayEndTurnPlayerSerial = playerSerial;
                    return;
                }

            EndTurn(playerSerial);
        }

        public void EndCongressTurn(int playerSerial, EndCongressTurn endCongressTurn)
        {
            NextCongressStage();
        }

        public void CharacterNameCode0ChooseToRobOneCard(int playerSerial, CharacterNameCode0ChooseToRobOneCard characterNameCode0ChooseToRobOneCard)
        {
            ServerPlayer robberServerPlayer = GetServerPlayerFromPlayerSerial(playerSerial);
            List<int> robbedPlayerSerials = new List<int>();
            List<int> robbedCardNameCodes = new List<int>();
            int robFailNameCode = characterNameCode0ChooseToRobOneCard.ChooseCardNameCodes[0];

            foreach (int nameCode in characterNameCode0ChooseToRobOneCard.ChooseCardNameCodes)
                foreach(ServerPlayer serverPlayer in ServerPlayers)
                    if(serverPlayer.PlayerSerial != playerSerial)
                        if (serverPlayer.HandCards.Exists(nC => nC == nameCode))
                        {
                            ServerPlayer robbedServerPlayer = GetServerPlayerFromPlayerSerial(serverPlayer.PlayerSerial);
                            robbedServerPlayer.RemoveHandCard(false, nameCode);
                            robberServerPlayer.AddHandCard(nameCode);
                            robbedPlayerSerials.Add(serverPlayer.PlayerSerial);
                            robbedCardNameCodes.Add(nameCode);
                            robFailNameCode = -1;
                        }

            EnemyCharacterNameCode0RobOneCard enemyCharacterNameCode0RobOneCard = new EnemyCharacterNameCode0RobOneCard(playerSerial, robbedPlayerSerials, robbedCardNameCodes, robFailNameCode);
            Send_EnemyCharacterNameCode0RobOneCard_AllClient(enemyCharacterNameCode0RobOneCard);
        }

        public void CharacterNameCode1ChooseOneDeckCard(int playerSerial, CharacterNameCode1ChooseOneDeckCard characterNameCode1ChooseOneDeckCard)
        {
            if (characterNameCode1ChooseOneDeckCard.ChooseDeckCardIndex != -1)
            {
                Send_EnemyCharacterNameCode1ChooseOneDeckCard_AllClient(new EnemyCharacterNameCode1ChooseOneDeckCard(playerSerial, characterNameCode1ChooseOneDeckCard.ChooseDeckCardIndex));

                GainDeckCard(playerSerial, characterNameCode1ChooseOneDeckCard.ChooseDeckCardIndex);
                Send_UpdateDeckCards_AllClient(new UpdateDeckCards(DeckCards, DeckCardsIsReveal));
                GetServerPlayerFromPlayerSerial(playerSerial).CharacterNameCode1SkillUsed = true;
            }

            EndTurn(playerSerial);
        }

        public void CharacterNameCode2DiscardOneHandCard(int playerSerial, CharacterNameCode2DiscardOneHandCard characterNameCode2DiscardOneHandCard)
        {
            GetServerPlayerFromPlayerSerial(playerSerial).RemoveHandCard(true, characterNameCode2DiscardOneHandCard.DiscardHandCardNameCode);

            Send_EnemyCharacterNameCode2DiscardOneCard_AllClient(new EnemyCharacterNameCode2DiscardOneCard(playerSerial, characterNameCode2DiscardOneHandCard.DiscardHandCardNameCode));
        }

        public void CharacterNameCode3UseSkill(int playerSerial, CharacterNameCode3UseSkill characterNameCode3UseSkill)
        {
            GetServerPlayerFromPlayerSerial(playerSerial).CharacterNameCode3HasPassive = false;
            Send_EnemyCharacterNameCode3UseSkill_AllClient(new EnemyCharacterNameCode3UseSkill(playerSerial));
        }

        public void CharacterNameCode6UseSkill(int playerSerial, CharacterNameCode6UseSkill characterNameCode6UseSkill)
        {
            AddBuffsToPlayers(playerSerial, Buff.Invulnerable);
            AddBuffsToAllPlayersExcept(playerSerial, Buff.CantWin);
            AddBuffsToAllPlayersExcept(playerSerial, Buff.StrategyRestriction);
            Send_EnemyCharacterNameCode6UseSkill_AllClient(new EnemyCharacterNameCode6UseSkill(playerSerial));
        }

        public void CharacterNameCode8DiscardOneHandCard(int playerSerial, CharacterNameCode8DiscardOneHandCard characterNameCode8DiscardOneHandCard)
        {
            GetServerPlayerFromPlayerSerial(playerSerial).RemoveHandCard(true, characterNameCode8DiscardOneHandCard.DiscardHandCardNameCode);
            Send_EnemyCharacterNameCode8DiscardOneCard_AllClient(new EnemyCharacterNameCode8DiscardOneCard(playerSerial, characterNameCode8DiscardOneHandCard.DiscardHandCardNameCode));
        }

        public void CharacterNameCode10DiscardOneDeckCard(int playerSerial, CharacterNameCode10DiscardOneDeckCard characterNameCode10DiscardOneDeckCard)
        {
            DeckCardsIsReveal[characterNameCode10DiscardOneDeckCard.DiscardDeckCardIndex] = false;
            GraveCards.Add(DeckCards[characterNameCode10DiscardOneDeckCard.DiscardDeckCardIndex]);
            DeckCards[characterNameCode10DiscardOneDeckCard.DiscardDeckCardIndex] = -2;
            Send_EnemyCharacterNameCode10DiscardOneDeckCard_AllClient(new EnemyCharacterNameCode10DiscardOneDeckCard(playerSerial, characterNameCode10DiscardOneDeckCard.DiscardDeckCardIndex));
        }

        public void CharacterNameCode13RobOneHandCard(int playerSerial, CharacterNameCode13RobOneHandCard characterNameCode13RobOneHandCard)
        {
            ServerPlayer robbedServerPlayer = GetServerPlayerFromPlayerSerial(characterNameCode13RobOneHandCard.DecidePlayerSerial);
            ServerPlayer robberServerPlayer = GetServerPlayerFromPlayerSerial(playerSerial);
            int randomIndex = Random.Range(0, robbedServerPlayer.HandCards.Count);
            while(DataMaster.GetInstance().GetCardInfo(robbedServerPlayer.HandCards[randomIndex]).IsStrategy)
                randomIndex = Random.Range(0, robbedServerPlayer.HandCards.Count);
            int robbedCardNameCode = robbedServerPlayer.HandCards[randomIndex];
            robbedServerPlayer.RemoveHandCard(false, robbedCardNameCode);
            robberServerPlayer.AddHandCard(robbedCardNameCode);

            EnemyCharacterNameCode13RobOneCard enemyCharacterNameCode13RobOneCard_Effected =
                new EnemyCharacterNameCode13RobOneCard(playerSerial, characterNameCode13RobOneHandCard.DecidePlayerSerial, robbedCardNameCode);
            EnemyCharacterNameCode13RobOneCard enemyCharacterNameCode13RobOneCard_Other =
                new EnemyCharacterNameCode13RobOneCard(playerSerial, characterNameCode13RobOneHandCard.DecidePlayerSerial, -1);

            foreach(ServerPlayer serverPlayer in ServerPlayers)
            {
                if (serverPlayer.PlayerSerial == playerSerial || serverPlayer.PlayerSerial == characterNameCode13RobOneHandCard.DecidePlayerSerial)
                    Send_EnemyCharacterNameCode13RobOneCard(enemyCharacterNameCode13RobOneCard_Effected, serverPlayer.PlayerSerial);
                else
                    Send_EnemyCharacterNameCode13RobOneCard(enemyCharacterNameCode13RobOneCard_Other, serverPlayer.PlayerSerial);
            }
        }

        public void CharacterNameCode14PlaceBackDeckCard(int playerSerial, CharacterNameCode14PlaceBackDeckCard characterNameCode14PlaceBackDeckCard)
        {
            GetServerPlayerFromPlayerSerial(playerSerial).RemoveHandCard(false, characterNameCode14PlaceBackDeckCard.PlaceBackCardNameCode);

            int placeBackDeckIndex = -1;
            for (int i = 0; i < _MaxDeckCardAmount; i++)
                if(DeckCards[i] == -2)
                {
                    placeBackDeckIndex = i;
                    DeckCards[i] = characterNameCode14PlaceBackDeckCard.PlaceBackCardNameCode;
                    DeckCardsIsReveal[i] = true;
                    break;
                }

            Send_EnemyCharacterNameCode14PlaceBackDeckCard_AllClient(new EnemyCharacterNameCode14PlaceBackDeckCard(playerSerial, placeBackDeckIndex, characterNameCode14PlaceBackDeckCard.PlaceBackCardNameCode));
        }

        public void CharacterNameCode18ExpandDeckCardToEnemy(int playerSerial, CharacterNameCode18ExpandDeckCardToEnemy characterNameCode18ExpandDeckCardToEnemy)
        {
            GetServerPlayerFromPlayerSerial(playerSerial).RemoveHandCard(false, characterNameCode18ExpandDeckCardToEnemy.ExpandCardNameCode);
            GetServerPlayerFromPlayerSerial(characterNameCode18ExpandDeckCardToEnemy.DecideEnemySerial).AddHandCard(characterNameCode18ExpandDeckCardToEnemy.ExpandCardNameCode);

            Send_EnemyCharacterNameCode18ExpandDeckCardToEnemy_AllClient(new EnemyCharacterNameCode18ExpandDeckCardToEnemy(playerSerial, characterNameCode18ExpandDeckCardToEnemy.DecideEnemySerial, characterNameCode18ExpandDeckCardToEnemy.ExpandCardNameCode));
        }

        public void StrategyNameCode22DecideDiscardOneHandCard(int playerSerial, StrategyNameCode22DecideDiscardOneHandCard strategyNameCode22DecideDiscardOneHandCard)
        {
            GetServerPlayerFromPlayerSerial(playerSerial).RemoveHandCard(true, strategyNameCode22DecideDiscardOneHandCard.DiscardHandCardNameCode);

            EnemyDiscardCards enemyDiscardCards = new EnemyDiscardCards(playerSerial, strategyNameCode22DecideDiscardOneHandCard.DiscardHandCardNameCode);
            foreach(ServerPlayer serverPlayer in ServerPlayers)
            {
                if (serverPlayer.PlayerSerial == playerSerial)
                    Send_EndStrategy(new EndStrategy(), serverPlayer.PlayerSerial);
                else
                    Send_EnemyDiscardCards(enemyDiscardCards, serverPlayer.PlayerSerial);
            }
        }

        // process //

        void AddBuffsToAllPlayersExcept(int playerSerial, params Buff[] buffs)
        {
            List<int> playerSerials = new List<int>();
            foreach (ServerPlayer serverPlayer in ServerPlayers)
                if (serverPlayer.PlayerSerial != playerSerial)
                    playerSerials.Add(serverPlayer.PlayerSerial);
            AddBuffsToPlayers(playerSerials, buffs);
        }

        void AddBuffsToPlayers(int playerSerial, params Buff[] buffs)
        {
            AddBuffsToPlayers(new List<int>(1) { playerSerial }, buffs);
        }

        void AddBuffsToPlayers(List<int> playerSerials, params Buff[] buffs)
        {
            foreach (int playerSerial in playerSerials)
                GetServerPlayerFromPlayerSerial(playerSerial).AddBuffs(buffs);
            Send_AddBuffs_AllClient(new AddBuffs(buffs, playerSerials.ToArray()));
        }

        void RemoveBuffsAtCongressStart()
        {
            foreach (ServerPlayer serverPlayer in ServerPlayers)
                serverPlayer.RemoveBuffsAtCongressStart();
        }

        bool IsRevolution(int playerSerial)
        {
            ServerPlayer serverPlayer = GetServerPlayerFromPlayerSerial(playerSerial);

            if (serverPlayer.HasBuff(Buff.Invulnerable)) return false;

            // 梅特涅，被動技，自身手牌權力點為質數時，不會引發革命。 
            if (serverPlayer.CharacterNameCode == 7)
                if (IsPrime(serverPlayer.Power))
                    return false;
            // end 梅特涅

            return !(serverPlayer.Power < serverPlayer.PowerLimit);
        }

        int GetVictoryType(int playerSerial)
        {
            if (_NowRound <= 4) return -1;

            ServerPlayer serverPlayer = GetServerPlayerFromPlayerSerial(playerSerial);

            // 亞歷山大一世，被動技，會議召開時，「冰封」權力值比自己小的人直到下一次會議召開。被「冰封」的玩家不能宣布勝利。
            if (serverPlayer.HasBuff(Buff.CantWin)) return -1;

            bool noPublicCard = true;
            List<int> handCards = serverPlayer.HandCards;
            bool[][] hasThesePowerCard = new bool[4][];
            for (int i = 0; i < 4; i++)
            {
                hasThesePowerCard[i] = new bool[9];
                for (int j = 0; j < 9; j++) hasThesePowerCard[i][j] = false;
            }
            foreach (int nameCode in handCards)
            {
                CardInfo cardInfo = DataMaster.GetInstance().GetCardInfo(nameCode);
                if (cardInfo.IsPublic) noPublicCard = false;
                if (cardInfo.IsPower)
                    hasThesePowerCard[(int)cardInfo.PublicInfo.PowerType][cardInfo.PublicInfo.PublicCardClass - 1] = true;
            }

            // 卡爾·施泰因，被動技，追加勝利條件「變革者」：不分種類１~８權力牌各持有至少一張。
            if(serverPlayer.CharacterNameCode == 5)
                if (HasReformerVictoryType(hasThesePowerCard))
                    return 3;
            // end 卡爾·施泰因

            if (hasThesePowerCard[0][8] && hasThesePowerCard[1][8] && hasThesePowerCard[2][8] && hasThesePowerCard[3][8])
                return 2;
            if (noPublicCard) return 1;

            for (int i = 0; i < 4; i++) for (int j = 0; j < 5; j++)
                    if (hasThesePowerCard[i][j] && hasThesePowerCard[i][j+1] && hasThesePowerCard[i][j+2] && hasThesePowerCard[i][j+3] && hasThesePowerCard[i][j+4])
                        return 0;
            return -1;
        }

        void GainDeckCard(int playerSerial, int cardIndex)
        {
            GainDeckCard(playerSerial, new List<int>(1) { cardIndex });
        }

        void GainDeckCard(int playerSerial, List<int> cardIndexs)
        {
            ServerPlayer serverPlayer = GetServerPlayerFromPlayerSerial(playerSerial);
            foreach(int cardIndex in cardIndexs)
            {
                DeckCardsIsReveal[cardIndex] = false;
                serverPlayer.AddHandCard(DeckCards[cardIndex]);
                DeckCards[cardIndex] = -2;
            }
        }

        int DrawOneStrategyCard()
        {
            if (StrategyDrawCards.Count == 0) foreach (int nC in AvailableStrategyCards) StrategyDrawCards.Add(nC);
            int nameCode = -1;
            while(!AvailableStrategyCards.Exists(nC => nC == nameCode)) nameCode = StrategyDrawCards[Random.Range(0, StrategyDrawCards.Count)];
            StrategyDrawCards.Remove(nameCode);
            return nameCode;
        }

        bool AllServerPlayerDecideCharacterReady()
        {
            foreach (ServerPlayer serverPlayer in ServerPlayers) if (!serverPlayer.IsDecideCharacterReady) return false;
            return true;
        }

        void RevealDecideCharacter()
        {
            Send_DecideCharacterInfos_AllClient(new DecideCharacterInfos(ServerPlayers));

            // 威廉三世，被動技，革命門檻減少８，使用「政治改革」時，效果增加１倍。 必殺技，革命門檻永久增加５，同時自身被動技永久無效。
            foreach (ServerPlayer serverPlayer in ServerPlayers)
                if (serverPlayer.CharacterNameCode == 3)
                    serverPlayer.CharacterNameCode3HasPassive = true;

            // 米歇爾·內伊，主動技，當其他玩家回合結束時，可以拿取場上任意一張翻開的牌，此技能一輪會議周期內只能發動一次。
            foreach (ServerPlayer serverPlayer in ServerPlayers)
                if (serverPlayer.CharacterNameCode == 1)
                    serverPlayer.CharacterNameCode1SkillUsed = false;

            

            RefillDeckCards();
            Send_UpdateDeckCards_AllClient(new UpdateDeckCards(DeckCards, DeckCardsIsReveal));

            NextTurn();
        }

        void NextRound(int endTurnPlayerSerial)
        {
            _NowRound++;
            if (_NowRound >= 5 && _NowRound % 2 == 1)
            {
                RoundStart roundStart = new RoundStart(_NowRound, ServerPlayers);
                _NowCongressLeaderSerial = roundStart.LeaderSerial;

                RemoveBuffsAtCongressStart();

                // 米歇爾·內伊，主動技，當其他玩家回合結束時，可以拿取場上任意一張翻開的牌，此技能一輪會議周期內只能發動一次。
                foreach (ServerPlayer serverPlayer in ServerPlayers)
                    if (serverPlayer.CharacterNameCode == 1)
                        serverPlayer.CharacterNameCode1SkillUsed = false;

                if (_NowCongressLeaderSerial != -1)
                {
                    // congress leader

                    int nameCode = DrawOneStrategyCard();
                    GetServerPlayerFromPlayerSerial(_NowCongressLeaderSerial).AddHandCard(nameCode);
                    Send_DrawCards(new DrawCards(nameCode), _NowCongressLeaderSerial);

                    // 拜倫，被動技，自己以外的玩家每成為一次會議主導者，該玩家的革命門檻減少３。
                    ServerPlayer characterNameCode16ServerPlayer = ServerPlayers.Find(serverPlayer => serverPlayer.CharacterNameCode == 16);
                    if (characterNameCode16ServerPlayer != null && !characterNameCode16ServerPlayer.IsDead)
                        if(characterNameCode16ServerPlayer.PlayerSerial != _NowCongressLeaderSerial)
                            GetServerPlayerFromPlayerSerial(_NowCongressLeaderSerial).PowerLimitCharacter16NameCodeSkill += 3;

                    // 亞歷山大一世，被動技，會議召開時，「冰封」權力值比自己小的人直到下一次會議召開。被「冰封」的玩家不能宣布勝利。
                    ServerPlayer characterNameCode9ServerPlayer = ServerPlayers.Find(serverPlayer => serverPlayer.CharacterNameCode == 9);
                    List<int> sealedPlayerSerials = new List<int>();
                    if (characterNameCode9ServerPlayer != null && !characterNameCode9ServerPlayer.IsDead)
                        foreach (ServerPlayer serverPlayer in ServerPlayers)
                            if (serverPlayer.Power < characterNameCode9ServerPlayer.Power)
                                sealedPlayerSerials.Add(serverPlayer.PlayerSerial);
                    AddBuffsToPlayers(sealedPlayerSerials, Buff.CantWin);
                }
                Send_RoundStart_AllClient(roundStart);

                InitCongressStage(_NowCongressLeaderSerial, endTurnPlayerSerial);
            } else
            {
                Send_RoundStart_AllClient(new RoundStart(_NowRound));
                RefillDeckCards();
                Send_UpdateDeckCards_AllClient(new UpdateDeckCards(DeckCards, DeckCardsIsReveal));
                NextTurn();
            }
        }

        void InitCongressStage(int leaderSerial, int endTurnPlayerSerial)
        {
            _NowTurnPlayerSerial = (leaderSerial == -1) ? endTurnPlayerSerial : leaderSerial;
            _CongressMovementSeq.Clear();

            int congressStartPlayerSerial = _NowTurnPlayerSerial;
            for(int i = 0; i < _PlayerAmount; i++)
            {
                ServerPlayer serverPlayer = GetServerPlayerFromPlayerSerial(congressStartPlayerSerial);
                if (serverPlayer.StrategyCardAmount > 0) {
                    _CongressMovementSeq.Enqueue(serverPlayer.PlayerSerial);
                    Debug.Log("PS = " + serverPlayer.PlayerSerial);
                }
                   
                int serverPlayerIndex = ServerPlayers.IndexOf(serverPlayer);
                serverPlayerIndex = (serverPlayerIndex + 1) % _PlayerAmount;
                congressStartPlayerSerial = ServerPlayers[serverPlayerIndex].PlayerSerial;
            }           
            NextCongressStage();
        }

        void NextCongressStage()
        {
            Debug.Log(_CongressMovementSeq.Count);
            if (_CongressMovementSeq.Count == 0)
            {
                EndCongressStage();
                return;
            }
            int nowCongressTurnPlayerSerial = _CongressMovementSeq.Dequeue();
            CongressTurnStart congressTurnStart = new CongressTurnStart(nowCongressTurnPlayerSerial);
            Send_CongressTurnStart_AllClient(congressTurnStart);
            if (IsBot(nowCongressTurnPlayerSerial)) AI.CongressTurnStart(nowCongressTurnPlayerSerial, congressTurnStart, this);
        }

        void EndCongressStage()
        {
            Debug.Log("End");
            RefillDeckCards();
            Send_UpdateDeckCards_AllClient(new UpdateDeckCards(DeckCards, DeckCardsIsReveal));
            NextTurn();
        }

        void NextTurn()
        {
            _NowTurn++;
            int serverPlayerIndex = ServerPlayers.IndexOf(GetServerPlayerFromPlayerSerial(_NowTurnPlayerSerial));
            serverPlayerIndex = (serverPlayerIndex + 1) % _PlayerAmount;
            _NowTurnPlayerSerial = ServerPlayers[serverPlayerIndex].PlayerSerial;

            ServerPlayer nowTurnServerPlayer = GetServerPlayerFromPlayerSerial(_NowTurnPlayerSerial);

            if (nowTurnServerPlayer.IsDead)
            {// game end
                Send_UpdateUnmaskedPowerAndVictory_AllClient(new UpdateUnmaskedPowerAndVictory(ServerPlayers, RevolutionStack));
                ServerController.RemoveSubscriptor("GamePlayHub_16");
                Destroy(gameObject);
            }
            else
                Send_TurnStart_AllPlayer(new TurnStart(_NowTurnPlayerSerial));
        }

        void EndTurn(int endTurnPlayerSerial)
        {
            if (DeckRemainCardAmount() <= _PlayerAmount) NextRound(endTurnPlayerSerial);
            else NextTurn();
        }

        bool AllServerPlayerSceneReady()
        {
            foreach (ServerPlayer serverPlayer in ServerPlayers) if (!serverPlayer.IsSceneReady) return false;
            return true;
        }

        void GameStart()
        {
            Debug.Log("GameStart");
            _IsGameStart = true;

            // init game setting
            _NowRound = 1;
            Send_RoundStart_AllClient(new RoundStart(_NowRound));
            _CongressMovementSeq = new Queue<int>();
            _NowCongressLeaderSerial = -1;
            _PlayerAmount = ServerPlayers.Count;
            _NowTurnPlayerSerial = ServerPlayers[_PlayerAmount - 1].PlayerSerial;
            _MaxDeckCardAmount = _PlayerAmount * 3;
            DeckCardsIsReveal = new bool[_MaxDeckCardAmount];
            for (int i = 0; i < DataMaster.GetInstance().PublicCardCount; i++) DrawCards.Add(i);
            RefillDeckCards();

            GamePlayInfos gamePlayInfos = new GamePlayInfos(ServerPlayers);
            int maxCharacter = DataMaster.GetInstance().CharacterCount;
            int [][] ChararcterCodes = RandomThreeCharacter(maxCharacter, _PlayerAmount);
            for(int i = 0; i < _PlayerAmount; i++) {
                gamePlayInfos.SelfPlayerSerial = ServerPlayers[i].PlayerSerial;
                Send_GamePlayInfos(gamePlayInfos, ServerPlayers[i].PlayerSerial);
            }
            for (int i = 0; i < _PlayerAmount; i++)
                Send_ChooseCharacterFromThree(new ChooseCharacterFromThree(ChararcterCodes[i]), ServerPlayers[i].PlayerSerial);
        }

        int [][] RandomThreeCharacter(int maxCharacter, int playerAmount)
        {
            int[][] ret = new int[playerAmount][];
            bool[] characterInRet = new bool[maxCharacter];
            for (int i = 0; i < maxCharacter; i++) characterInRet[i] = false;

            for(int i = 0; i < playerAmount; i++)
            {
                ret[i] = new int[3];
                for(int j = 0; j < 3; j++)
                {
                    int random = Random.Range(0, maxCharacter);
                    while (characterInRet[random]) random = Random.Range(0, maxCharacter);
                    characterInRet[random] = true;
                    ret[i][j] = random;
                }
            }

            return ret;
        }

        int DeckRemainCardAmount()
        {
            int ret = 0;
            for (int i = 0; i < _MaxDeckCardAmount; i++) if (DeckCards[i] != -2) ret++;
            return ret;
        }

        void RefillDeckCards()
        {
            for(int i=0;i< _MaxDeckCardAmount; i++)
            {
                if (DeckCards.Count <= i)
                {
                    DeckCardsIsReveal[DeckCards.Count] = false;
                    DeckCards.Add(DrawOnePublicCard());
                } else if (DeckCards[i] == -2)
                {
                    DeckCardsIsReveal[i] = false;
                    DeckCards[i] = DrawOnePublicCard();
                }
            }
            while (DeckCards.Count < _MaxDeckCardAmount)
            {
                DeckCardsIsReveal[DeckCards.Count] = false;
                DeckCards.Add(DrawOnePublicCard());
            }
            foreach (int nameCode in GraveCards) if(DataMaster.GetInstance().GetCardInfo(nameCode).IsPublic) DrawCards.Add(nameCode);
            GraveCards.Clear();
            Send_UpdatePileAmount_AllClient(new UpdatePileAmount(DrawCards.Count, GraveCards.Count));
        }

        void RemoveDeckCards(List<int> removeIndex)
        {
            foreach(int cardIndex in removeIndex)
            {
                DeckCardsIsReveal[cardIndex] = false;
                GraveCards.Add(DeckCards[cardIndex]);
                DeckCards[cardIndex] = -2;
            }
        }

        int DrawOnePublicCard()
        {
            int randomIndex = Random.Range(0, DrawCards.Count);
            int ret = DrawCards[randomIndex];
            DrawCards.RemoveAt(randomIndex);
            return ret;
        }

        public int GetPlayerSerialFromClientSerial(int clientSerial) {
            return ServerPlayers.Find(serverPlayer => serverPlayer.ClientSerial == clientSerial).PlayerSerial;
        }

        int GetClientSerialFromPlayerSerial(int playerSerial)
        {
            return ServerPlayers.Find(serverPlayer => serverPlayer.PlayerSerial == playerSerial).ClientSerial;
        }

        ServerPlayer GetServerPlayerFromClientSerial(int clientSerial)
        {
            return ServerPlayers.Find(serverPlayer => serverPlayer.ClientSerial == clientSerial);
        }

        public ServerPlayer GetServerPlayerFromPlayerSerial(int playerSerial)
        {
            return ServerPlayers.Find(serverPlayer => serverPlayer.PlayerSerial == playerSerial);
        }

        bool IsBot(int playerSerial)
        {
            return ServerPlayers.Find(serverPlayer => serverPlayer.PlayerSerial == playerSerial).IsBot;
        }

        // Characters Process //

        bool IsPrime(int power)
        {
            int sqrtPower = Mathf.FloorToInt(Mathf.Sqrt(power))+1;
            for (int i = 2; i <= sqrtPower; i++)
                if (power % sqrtPower == 0) return false;
            return true;
        }

        bool HasReformerVictoryType(bool[][] hasThesePowerCard)
        {
            for (int i = 0; i < 8; i++)
                if (!hasThesePowerCard[0][i] && !hasThesePowerCard[1][i] && !hasThesePowerCard[2][i] && !hasThesePowerCard[3][i])
                    return false;
            return true;
        }

        // Strategy Process //
        void GeneralStrategyProcessor(int playerSerial, UseStrategyCard_Requirements useStrategyCard_Requirements)
        {
            ServerPlayer serverPlayer = GetServerPlayerFromPlayerSerial(playerSerial);
            IStrategyInfo strategyCardInfo = DataMaster.GetInstance().GetCardInfo(useStrategyCard_Requirements.StrategyCardNameCode).StrategyInfo;

            switch (strategyCardInfo.StrategyCardClass)
            {
                case 2:
                    foreach (ServerPlayer sP in ServerPlayers)
                    {
                        int[] removeCards = sP.HandCards.FindAll(card => DataMaster.GetInstance().GetCardInfo(card).IsMask).ToArray();
                        sP.RemoveHandCard(true, removeCards);
                        Send_DiscardCards(new DiscardCards(removeCards), sP.PlayerSerial);

                        if (removeCards.Length == 0) continue;

                        EnemyDiscardCards enemyDiscardCards = new EnemyDiscardCards(sP.PlayerSerial, removeCards);
                        foreach (ServerPlayer sP2 in ServerPlayers)
                            if(sP.PlayerSerial != sP2.PlayerSerial)
                                Send_EnemyDiscardCards(enemyDiscardCards, sP2.PlayerSerial);
                    }

                    Send_EndStrategy(new EndStrategy(), playerSerial);
                    break;
                case 7:
                    List<int> industryCardIndexs = new List<int>();
                    List<int> industryCardNameCodes = new List<int>();
                    for (int i = 0; i < DeckCards.Count; i++)
                    {
                        if (DeckCards[i] == -2 || !DeckCardsIsReveal[i]) continue;
                        CardInfo cardInfo = DataMaster.GetInstance().GetCardInfo(DeckCards[i]);
                        if (cardInfo.IsPublic && cardInfo.PublicInfo.PowerType == PowerType.Industry)
                        {
                            industryCardIndexs.Add(i);
                            industryCardNameCodes.Add(DeckCards[i]);
                        }
                    }
                    GainDeckCard(playerSerial, industryCardIndexs);
                    Send_DrawCards(new DrawCards(industryCardNameCodes.ToArray()), playerSerial);
                    Send_EndStrategy(new EndStrategy(), playerSerial);
                    break;
                case 10:
                    AddBuffsToAllPlayersExcept(playerSerial, Buff.SeaPowerRestriction);
                    Send_EndStrategy(new EndStrategy(), playerSerial);
                    break;
                case 11:
                    foreach (ServerPlayer sP in ServerPlayers)
                    {
                        int[] removeCards = sP.HandCards.FindAll(card => DataMaster.GetInstance().GetCardInfo(card).IsExpand).ToArray();
                        sP.RemoveHandCard(true, removeCards);
                        Send_DiscardCards(new DiscardCards(removeCards), sP.PlayerSerial);

                        EnemyDiscardCards enemyDiscardCards = new EnemyDiscardCards(sP.PlayerSerial, removeCards);
                        foreach (ServerPlayer sP2 in ServerPlayers)
                            if (sP.PlayerSerial != sP2.PlayerSerial)
                                Send_EnemyDiscardCards(enemyDiscardCards, sP2.PlayerSerial);
                    }

                    List<int> removeExpandCardIndexs = new List<int>();
                    for (int i = 0; i < DeckCards.Count; i++)
                    {
                        if (DeckCards[i] == -2 || !DeckCardsIsReveal[i]) continue;
                        CardInfo cardInfo = DataMaster.GetInstance().GetCardInfo(DeckCards[i]);
                        if (cardInfo.IsExpand)
                            removeExpandCardIndexs.Add(i);
                    }

                    RemoveDeckCards(removeExpandCardIndexs);
                    Send_UpdateDeckCards_AllClient(new UpdateDeckCards(DeckCards, DeckCardsIsReveal));
                    Send_EndStrategy(new EndStrategy(), playerSerial);
                    break;
                case 13:
                    AddBuffsToAllPlayersExcept(playerSerial, Buff.MilitaryRestriction);
                    Send_EndStrategy(new EndStrategy(), playerSerial);
                    break;
                case 14:
                    AddBuffsToAllPlayersExcept(playerSerial, Buff.StrategyRestriction);
                    Send_EndStrategy(new EndStrategy(), playerSerial);
                    break;
                case 15:
                    AddBuffsToAllPlayersExcept(playerSerial, Buff.WealthRestriction, Buff.IndustryRestriction);
                    Send_EndStrategy(new EndStrategy(), playerSerial);
                    break;
                case 19:
                    serverPlayer.StrategyCardLimit++;
                    Send_EndStrategy(new EndStrategy(), playerSerial);
                    break;
                case 20:
                    Send_EndStrategy(new EndStrategy(), playerSerial);
                    Send_StrategyNameCode20RobGainCardOneMoreTime(new StrategyNameCode20RobGainCardOneMoreTime(), playerSerial);
                    break;
                case 21:
                    foreach (ServerPlayer sP in ServerPlayers)
                    {
                        int[] removeCards = sP.HandCards.FindAll(card => DataMaster.GetInstance().GetCardInfo(card).IsReform).ToArray();
                        sP.RemoveHandCard(true, removeCards);
                        Send_DiscardCards(new DiscardCards(removeCards), sP.PlayerSerial);

                        EnemyDiscardCards enemyDiscardCards = new EnemyDiscardCards(sP.PlayerSerial, removeCards);
                        foreach (ServerPlayer sP2 in ServerPlayers)
                            if (sP.PlayerSerial != sP2.PlayerSerial)
                                Send_EnemyDiscardCards(enemyDiscardCards, sP2.PlayerSerial);
                    }

                    List<int> removeReformCardIndexs = new List<int>();
                    for (int i = 0; i < DeckCards.Count; i++)
                    {
                        if (DeckCards[i] == -2 || !DeckCardsIsReveal[i]) continue;
                        CardInfo cardInfo = DataMaster.GetInstance().GetCardInfo(DeckCards[i]);
                        if (cardInfo.IsReform)
                            removeReformCardIndexs.Add(i);
                    }

                    RemoveDeckCards(removeReformCardIndexs);
                    Send_UpdateDeckCards_AllClient(new UpdateDeckCards(DeckCards, DeckCardsIsReveal));
                    Send_EndStrategy(new EndStrategy(), playerSerial);
                    break;
                case 22:
                    Send_StrategyNameCode22ChooseDiscardOneHandCard(new StrategyNameCode22ChooseDiscardOneHandCard(), playerSerial);
                    break;
                case 23:
                    if (_NowCongressLeaderSerial != -1)
                        AddBuffsToPlayers(_NowCongressLeaderSerial, Buff.FunctionRestriction);
                    Send_EndStrategy(new EndStrategy(), playerSerial);
                    break;
                default: break;
            }
        }
    }

}