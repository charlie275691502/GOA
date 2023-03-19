using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Main;

namespace GamePlayHub.ServerToClient {
    public class GamePlayInfos
    {
        public int PlayerAmount;
        public int SelfPlayerSerial;
        public int[] PlayerSerials;
        public string[] Nicks;
        public bool[] IsBots;

        public GamePlayInfos(List<ServerPlayer> serverPlayers)
        {
            PlayerAmount = serverPlayers.Count;
            SelfPlayerSerial = -1;
            PlayerSerials = new int[serverPlayers.Count];
            Nicks = new string[serverPlayers.Count];
            IsBots = new bool[serverPlayers.Count];
            for(int i = 0; i < serverPlayers.Count; i++) {
                PlayerSerials[i] = serverPlayers[i].PlayerSerial;
                Nicks[i] = serverPlayers[i].Nick;
                IsBots[i] = serverPlayers[i].IsBot;
            }
        }
    }

    public class ChooseCharacterFromThree
    {
        public int[] CharacterNameCodes;

        public ChooseCharacterFromThree(int[] characterNameCodes)
        {
            CharacterNameCodes = characterNameCodes;
        }
    }

    public class EnemyDecideCharacter
    {
        public int PlayerSerial;

        public EnemyDecideCharacter(int playerSerial)
        {
            PlayerSerial = playerSerial;
        }
    }

    public class DecideCharacterInfos
    {
        public int[] PlayerSerials;
        public int[] CharacterNameCodes;

        public DecideCharacterInfos(List<ServerPlayer> serverPlayers)
        {
            PlayerSerials = new int[serverPlayers.Count];
            CharacterNameCodes = new int[serverPlayers.Count];
            for (int i = 0; i < serverPlayers.Count; i++)
            {
                PlayerSerials[i] = serverPlayers[i].PlayerSerial;
                CharacterNameCodes[i] = serverPlayers[i].CharacterNameCode;
            }
        }
    }

    public class UpdateDeckCards
    {
        public int[] DeckCardNameCodes;

        public UpdateDeckCards(List<int> deckCardNameCodes, bool[] deckCardsIsReveal)
        {
            DeckCardNameCodes = new int[deckCardNameCodes.Count];
            for(int i=0;i< deckCardNameCodes.Count;i++) DeckCardNameCodes[i] = (deckCardNameCodes[i] == -2) ? -2 : (deckCardsIsReveal[i]) ? deckCardNameCodes[i] : -1;
        }
    }

    public class UpdatePileAmount
    {
        public int DrawPileAmount;
        public int GravePileAmount;

        public UpdatePileAmount(int drawPileAmount, int gravePileAmount)
        {
            DrawPileAmount = drawPileAmount;
            GravePileAmount = gravePileAmount;
        }
    }

    public class TurnStart
    {
        public int PlayerSerial;

        public TurnStart(int playerSerial)
        {
            PlayerSerial = playerSerial;
        }
    }

    public class RevealChosenCoveredCards
    {
        public int[] ChosenCoveredCardNameCodes;
        public RevealChosenCoveredCards(int[] chosenCoveredCardNameCodes) {
            ChosenCoveredCardNameCodes = chosenCoveredCardNameCodes;
        }
    }

    public class DrawCards
    {
        public int[] DrawnCardNameCode;

        public DrawCards(int drawnCardNameCode)
        {
            DrawnCardNameCode = new int[1] { drawnCardNameCode };
        }

        public DrawCards(int[] drawnCardNameCode)
        {
            DrawnCardNameCode = drawnCardNameCode;
        }
    }

    public class DiscardCards
    {
        public int[] DiscardCardNameCode;

        public DiscardCards(int discardCardNameCode)
        {
            DiscardCardNameCode = new int[1] { discardCardNameCode };
        }

        public DiscardCards(int[] discardCardNameCode)
        {
            DiscardCardNameCode = discardCardNameCode;
        }
    }

    public class HighlightDeckCards
    {
        public int[] HighlightCardIndex;

        public HighlightDeckCards(int highlightCardIndex)
        {
            HighlightCardIndex = new int[1] { highlightCardIndex };
        }

        public HighlightDeckCards(int[] highlightCardIndex)
        {
            HighlightCardIndex = highlightCardIndex;
        }
    }

    public class EnemyDecideCardFromRevealedCoveredCards
    {
        public int EnemySerial;
        public int[] CandidateCardIndex;
        public int DecideCardIndex;
        public int[] RevealCardIndex;
        public int[] RevealCardNameCodes;

        public EnemyDecideCardFromRevealedCoveredCards(int enemySerial, int[] candidateCardIndex, int decideCardIndex, int[] revealCardIndex, int[] revealCardNameCodes)
        {
            EnemySerial = enemySerial;
            CandidateCardIndex = candidateCardIndex;
            DecideCardIndex = decideCardIndex;
            RevealCardIndex = revealCardIndex;
            RevealCardNameCodes = revealCardNameCodes;
        }
    }

    public class EnemyDecideCardFromOpenCard
    {
        public int EnemySerial;
        public int DecideOpenCardIndex;

        public EnemyDecideCardFromOpenCard(int enemySerial, int decideOpenCardIndex)
        {
            EnemySerial = enemySerial;
            DecideOpenCardIndex = decideOpenCardIndex;
        }
    }

    public class EnemyUseFunctionCard_Mask
    {
        public int EnemySerial;
        public int FunctionCardNameCode;

        public EnemyUseFunctionCard_Mask(int enemySerial, int functionCardNameCode)
        {
            EnemySerial = enemySerial;
            FunctionCardNameCode = functionCardNameCode;
        }
    }

    public class EnemyUseFunctionCard_Reform
    {
        public int EnemySerial;
        public int FunctionCardNameCode;
        public int[] DecideCardNameCode;

        public EnemyUseFunctionCard_Reform(int enemySerial, int functionCardNameCode, int decideCardNameCode)
        {
            EnemySerial = enemySerial;
            FunctionCardNameCode = functionCardNameCode;
            DecideCardNameCode = new int[1] { decideCardNameCode };
        }

        public EnemyUseFunctionCard_Reform(int enemySerial, int functionCardNameCode, int[] decideCardNameCode)
        {
            EnemySerial = enemySerial;
            FunctionCardNameCode = functionCardNameCode;
            DecideCardNameCode = decideCardNameCode;
        }
    }

    public class EnemyUseFunctionCard_Expand
    {
        public int EnemySerial;
        public int FunctionCardNameCode;
        public int DecideCardNameCode;
        public int DecideCardIndex;

        public EnemyUseFunctionCard_Expand(int enemySerial, int functionCardNameCode, int decideCardNameCode, int decideCardIndex)
        {
            EnemySerial = enemySerial;
            FunctionCardNameCode = functionCardNameCode;
            DecideCardNameCode = decideCardNameCode;
            DecideCardIndex = decideCardIndex;
        }
    }

    public class EnemyUseStrategyCard_Requirements
    {
        public int EnemySerial;
        public int StrategyCardNameCode;
        public int[] RequirementCardNameCodes;

        public EnemyUseStrategyCard_Requirements(int enemySerial, int strategyCardNameCode, int[] requirementCardNameCodes)
        {
            EnemySerial = enemySerial;
            StrategyCardNameCode = strategyCardNameCode;
            RequirementCardNameCodes = requirementCardNameCodes;
        }
    }

    public class EnemyReleaseCard
    {
        public int EnemySerial;
        public int[] ReleaseCardNameCodes;

        public EnemyReleaseCard(int enemySerial, int[] releaseCardNameCodes)
        {
            EnemySerial = enemySerial;
            ReleaseCardNameCodes = releaseCardNameCodes;
        }
    }

    public class EnemyDiscardCards
    {
        public int EnemySerial;
        public int[] DiscardCardNameCodes;

        public EnemyDiscardCards(int enemySerial, int strategyCardNameCode)
        {
            EnemySerial = enemySerial;
            DiscardCardNameCodes = new int[1] { strategyCardNameCode };
        }

        public EnemyDiscardCards(int enemySerial, int[] strategyCardNameCodes)
        {
            EnemySerial = enemySerial;
            DiscardCardNameCodes = strategyCardNameCodes;
        }
    }

    public class EnemyGainCards
    {
        public int EnemySerial;
        public int[] GainCardNameCodes;

        public EnemyGainCards(int enemySerial, int strategyCardNameCode)
        {
            EnemySerial = enemySerial;
            GainCardNameCodes = new int[1] { strategyCardNameCode };
        }

        public EnemyGainCards(int enemySerial, int[] strategyCardNameCodes)
        {
            EnemySerial = enemySerial;
            GainCardNameCodes = strategyCardNameCodes;
        }
    }

    public class EndStrategy
    {

    }


    public class RoundStart
    {
        public int NowRound;
        public bool HasCongress;
        public int[] PlayerSerials;
        public int[] Powers;
        public int LeaderSerial;

        public RoundStart(int nowRound)
        {
            NowRound = nowRound;
            HasCongress = false;
            PlayerSerials = new int[0];
            Powers = new int[0];
            LeaderSerial = -1;
        }

        public RoundStart(int nowRound, List<ServerPlayer> serverPlayers)
        {
            NowRound = nowRound;
            HasCongress = true;

            PlayerSerials = new int[serverPlayers.Count];
            for (int i = 0; i < serverPlayers.Count; i++) PlayerSerials[i] = serverPlayers[i].PlayerSerial;

            int maxPower = -1;
            int maxPowerPlayerIndex = -1;
            Powers = new int[serverPlayers.Count];
            for (int i = 0; i < serverPlayers.Count; i++) {
                if (serverPlayers[i].IsDead) continue;
                Powers[i] = serverPlayers[i].Power;
                if(maxPower < Powers[i]) {
                    maxPower = Powers[i];
                    maxPowerPlayerIndex = i;
                } else if(maxPower == Powers[i]) maxPowerPlayerIndex = -1;
            }
            if (maxPowerPlayerIndex == -1) LeaderSerial = -1;
            else LeaderSerial = serverPlayers[maxPowerPlayerIndex].PlayerSerial;
        }
    }

    public class CongressTurnStart
    {
        public int PlayerSerial;

        public CongressTurnStart(int playerSerial)
        {
            PlayerSerial = playerSerial;
        }
    }

    public class Victory
    {
        public int VictorySerial;
        public int VictoryType;

        public int[] PlayerHandCards0;
        public int[] PlayerHandCards1;
        public int[] PlayerHandCards2;
        public int[] PlayerHandCards3;
        public int[] PlayerHandCards4;

        public int[] Ranks;

        public Victory(int victorySerial, int vicotryType, List<ServerPlayer> serverPlayers, Stack<int> revolutionStack)
        {
            VictorySerial = victorySerial;
            VictoryType = vicotryType;

            PlayerHandCards0 = (serverPlayers.Count > 0) ? serverPlayers.Find(sP => sP.PlayerSerial == 0).HandCards.ToArray() : new int[0];
            PlayerHandCards1 = (serverPlayers.Count > 1) ? serverPlayers.Find(sP => sP.PlayerSerial == 1).HandCards.ToArray() : new int[0];
            PlayerHandCards2 = (serverPlayers.Count > 2) ? serverPlayers.Find(sP => sP.PlayerSerial == 2).HandCards.ToArray() : new int[0];
            PlayerHandCards3 = (serverPlayers.Count > 3) ? serverPlayers.Find(sP => sP.PlayerSerial == 3).HandCards.ToArray() : new int[0];
            PlayerHandCards4 = (serverPlayers.Count > 4) ? serverPlayers.Find(sP => sP.PlayerSerial == 4).HandCards.ToArray() : new int[0];

            Ranks = Func.GetRanks(revolutionStack, VictorySerial, serverPlayers);
        }
    }

    public class Revolution
    {
        public int RevolutionSerial;
        public int[] HandCards;

        public Revolution(int revolutionSerial, List<int> handCards)
        {
            RevolutionSerial = revolutionSerial;

            Debug.Log("handCards.Count = " + handCards.Count);
            HandCards = new int[handCards.Count];
            for (int i = 0; i < handCards.Count; i++) HandCards[i] = handCards[i];
        }
    }

    public class UpdatePower
    {
        public int[] PlayerSerials;
        public int[] Powers;

        public UpdatePower(List<ServerPlayer> serverPlayers)
        {
            PlayerSerials = new int[serverPlayers.Count];
            Powers = new int[serverPlayers.Count];
            for (int i = 0; i < serverPlayers.Count; i++)
            {
                PlayerSerials[i] = serverPlayers[i].PlayerSerial;
                Powers[i] = serverPlayers[i].Power;
            }
        }
    }

    public class UpdateUnmaskedPowerAndVictory
    {
        public int VictorySerial;
        public int[] PlayerSerials;
        public int[] UnmaskedPowers;

        public int[] PlayerHandCards0;
        public int[] PlayerHandCards1;
        public int[] PlayerHandCards2;
        public int[] PlayerHandCards3;
        public int[] PlayerHandCards4;

        public int[] Ranks;

        public UpdateUnmaskedPowerAndVictory(List<ServerPlayer> serverPlayers, Stack<int> revolutionStack)
        {
            PlayerSerials = new int[serverPlayers.Count];
            UnmaskedPowers = new int[serverPlayers.Count];

            bool allDied = true;
            foreach (ServerPlayer serverPlayer in serverPlayers)
                if (!serverPlayer.IsDead)
                    allDied = false;

            for (int i = 0; i < serverPlayers.Count; i++)
            {
                PlayerSerials[i] = serverPlayers[i].PlayerSerial;
                if (serverPlayers[i].IsDead)
                    UnmaskedPowers[i] = 0;
                else
                    UnmaskedPowers[i] = Func.CalculatePower(serverPlayers[i].HandCards, 0, new List<PowerType>(4) { PowerType.Wealth, PowerType.Industry, PowerType.SeaPower, PowerType.Military });
            }

            if (allDied)
                VictorySerial = -1;
            else
            {
                int maxIndex = 0;
                int maxCard = 0;
                for (int i = 0; i < serverPlayers.Count; i++)
                    if (UnmaskedPowers[maxIndex] < UnmaskedPowers[i] || (UnmaskedPowers[maxIndex] == UnmaskedPowers[i] && maxCard <= serverPlayers[i].HandCards.Count))
                    {
                        maxIndex = i;
                        maxCard = serverPlayers[i].HandCards.Count;
                    }
                VictorySerial = PlayerSerials[maxIndex];
            }

            PlayerHandCards0 = (serverPlayers.Count > 0) ? serverPlayers.Find(sP => sP.PlayerSerial == 0).HandCards.ToArray() : new int[0];
            PlayerHandCards1 = (serverPlayers.Count > 1) ? serverPlayers.Find(sP => sP.PlayerSerial == 1).HandCards.ToArray() : new int[0];
            PlayerHandCards2 = (serverPlayers.Count > 2) ? serverPlayers.Find(sP => sP.PlayerSerial == 2).HandCards.ToArray() : new int[0];
            PlayerHandCards3 = (serverPlayers.Count > 3) ? serverPlayers.Find(sP => sP.PlayerSerial == 3).HandCards.ToArray() : new int[0];
            PlayerHandCards4 = (serverPlayers.Count > 4) ? serverPlayers.Find(sP => sP.PlayerSerial == 4).HandCards.ToArray() : new int[0];

            Ranks = Func.GetRanks(revolutionStack, VictorySerial, serverPlayers);
        }
    }

    public class UpdatePowerLimit
    {
        public int[] PlayerSerials;
        public int[] PowerLimits;

        public UpdatePowerLimit(List<ServerPlayer> serverPlayers)
        {
            PlayerSerials = new int[serverPlayers.Count];
            PowerLimits = new int[serverPlayers.Count];
            for (int i = 0; i < serverPlayers.Count; i++)
            {
                PlayerSerials[i] = serverPlayers[i].PlayerSerial;
                PowerLimits[i] = serverPlayers[i].PowerLimit;
            }
        }
    }

    public class AddBuffs
    {
        public Buff[] Buffs;
        public int[] PlayerSerials;

        public AddBuffs(Buff[] buffs, int[] playerSerials)
        {
            Buffs = buffs;
            PlayerSerials = playerSerials;
        }
    }

    public class CharacterNameCode1AskIfCastSkill
    {

    }

    public class StrategyNameCode20RobGainCardOneMoreTime
    {

    }

    public class StrategyNameCode22ChooseDiscardOneHandCard
    {

    }

    public class EnemyCharacterNameCode0RobOneCard
    {
        public int RobberPlayerSerial;
        public int[] RobbedPlayerSerials;
        public int[] RobbedCardNameCodes;
        public int RobFailNameCode;

        public EnemyCharacterNameCode0RobOneCard(int robberPlayerSerial, List<int> robbedPlayerSerials, List<int> robbedCardNameCodes, int robFailNameCode)
        {
            RobberPlayerSerial = robberPlayerSerial;
            RobbedPlayerSerials = robbedPlayerSerials.ToArray();
            RobbedCardNameCodes = robbedCardNameCodes.ToArray();
            RobFailNameCode = robFailNameCode;
        }
    }

    public class EnemyCharacterNameCode1ChooseOneDeckCard
    {
        public int SkillPlayerSerial;
        public int ChooseDeckCardIndex;

        public EnemyCharacterNameCode1ChooseOneDeckCard(int skillPlayerSerial, int chooseDeckCardIndex)
        {
            SkillPlayerSerial = skillPlayerSerial;
            ChooseDeckCardIndex = chooseDeckCardIndex;
        }
    }

    public class EnemyCharacterNameCode2DiscardOneCard
    {
        public int SkillPlayerSerial;
        public int DiscardCardNameCode;

        public EnemyCharacterNameCode2DiscardOneCard(int skillPlayerSerial, int discardCardNameCode)
        {
            SkillPlayerSerial = skillPlayerSerial;
            DiscardCardNameCode = discardCardNameCode;
        }
    }

    public class EnemyCharacterNameCode3UseSkill
    {
        public int SkillPlayerSerial;

        public EnemyCharacterNameCode3UseSkill(int skillPlayerSerial)
        {
            SkillPlayerSerial = skillPlayerSerial;
        }
    }

    public class EnemyCharacterNameCode6UseSkill
    {
        public int SkillPlayerSerial;

        public EnemyCharacterNameCode6UseSkill(int skillPlayerSerial)
        {
            SkillPlayerSerial = skillPlayerSerial;
        }
    }

    public class EnemyCharacterNameCode8DiscardOneCard
    {
        public int SkillPlayerSerial;
        public int DiscardCardNameCode;

        public EnemyCharacterNameCode8DiscardOneCard(int skillPlayerSerial, int discardCardNameCode)
        {
            SkillPlayerSerial = skillPlayerSerial;
            DiscardCardNameCode = discardCardNameCode;
        }
    }

    public class EnemyCharacterNameCode10DiscardOneDeckCard
    {
        public int SkillPlayerSerial;
        public int DiscardDeckCardIndex;

        public EnemyCharacterNameCode10DiscardOneDeckCard(int skillPlayerSerial, int discardDeckCardIndex)
        {
            SkillPlayerSerial = skillPlayerSerial;
            DiscardDeckCardIndex = discardDeckCardIndex;
        }
    }

    public class EnemyCharacterNameCode13RobOneCard
    {
        public int RobberPlayerSerial;
        public int RobbedPlayerSerial;
        public int RobbedCardNameCode;

        public EnemyCharacterNameCode13RobOneCard(int robberPlayerSerial, int robbedPlayerSerial, int robbedCardNameCode)
        {
            RobberPlayerSerial = robberPlayerSerial;
            RobbedPlayerSerial = robbedPlayerSerial;
            RobbedCardNameCode = robbedCardNameCode;
        }
    }

    public class EnemyCharacterNameCode14PlaceBackDeckCard
    {
        public int SkillPlayerSerial;
        public int PlaceBackCardNameCode;
        public int PlaceBackDeckIndex;

        public EnemyCharacterNameCode14PlaceBackDeckCard(int skillPlayerSerial, int placeBackDeckIndex, int placeBackCardNameCode)
        {
            SkillPlayerSerial = skillPlayerSerial;
            PlaceBackCardNameCode = placeBackCardNameCode;
            PlaceBackDeckIndex = placeBackDeckIndex;
        }
    }

    public class EnemyCharacterNameCode18ExpandDeckCardToEnemy
    {
        public int SkillPlayerSerial;
        public int DecideEnemySerial;
        public int ExpandCardNameCode;

        public EnemyCharacterNameCode18ExpandDeckCardToEnemy(int skillPlayerSerial, int decideEnemySerial, int expandCardNameCode)
        {
            SkillPlayerSerial = skillPlayerSerial;
            DecideEnemySerial = decideEnemySerial;
            ExpandCardNameCode = expandCardNameCode;
        }
    }
}