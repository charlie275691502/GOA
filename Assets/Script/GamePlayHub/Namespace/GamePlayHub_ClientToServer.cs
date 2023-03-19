using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlayHub.ClientToServer{
    public class SceneReady
    {

    }

    public class DecideCharacter
    {
        public int DecideCharacterNameCode;

        public DecideCharacter(int decideCharacterNameCode)
        {
            DecideCharacterNameCode = decideCharacterNameCode;
        }
    }

    public class RevealSeveralCoveredCards
    {
        public int[] RevealCoveredCardIndexs;

        public RevealSeveralCoveredCards(List<int> revealCoveredCardIndexs)
        {
            RevealCoveredCardIndexs = new int[revealCoveredCardIndexs.Count];
            for (int i = 0; i < revealCoveredCardIndexs.Count; i++) RevealCoveredCardIndexs[i] = revealCoveredCardIndexs[i];
        }
    }

    public class DecideCardFromRevealedCoveredCards
    {
        public int DecideCardIndex;
        public int[] RevealCardIndex;
        public int[] CandidateCardIndex;

        public DecideCardFromRevealedCoveredCards(int decideCardIndex, List<int> candidateCardIndex)
        {
            DecideCardIndex = decideCardIndex;
            RevealCardIndex = new int[candidateCardIndex.Count - 1];
            CandidateCardIndex = candidateCardIndex.ToArray();
            int count = 0;
            for (int i = 0; i < candidateCardIndex.Count; i++)
                if (candidateCardIndex[i] != decideCardIndex)
                    RevealCardIndex[count++] = candidateCardIndex[i];
        }

        public DecideCardFromRevealedCoveredCards(int decideCardIndex, int[] candidateCardIndex)
        {
            DecideCardIndex = decideCardIndex;
            RevealCardIndex = new int[candidateCardIndex.Length - 1];
            CandidateCardIndex = candidateCardIndex;
            int count = 0;
            for (int i = 0; i < candidateCardIndex.Length; i++)
                if (candidateCardIndex[i] != decideCardIndex)
                    RevealCardIndex[count++] = candidateCardIndex[i];
        }

        // 約翰專用
        public DecideCardFromRevealedCoveredCards(int decideCardIndex, List<int> candidateCardIndex, CardHandler[] deckCardHandlers)
        {
            DecideCardIndex = decideCardIndex;
            CandidateCardIndex = candidateCardIndex.ToArray();
            int revealCardIndexCount = 0;
            for (int i = 0; i < candidateCardIndex.Count; i++)
                if (candidateCardIndex[i] != decideCardIndex && deckCardHandlers[candidateCardIndex[i]].NameCode != -1)
                    revealCardIndexCount++;

            RevealCardIndex = new int[revealCardIndexCount];
            int count = 0;
            for (int i = 0; i < candidateCardIndex.Count; i++)
                if (candidateCardIndex[i] != decideCardIndex && deckCardHandlers[candidateCardIndex[i]].NameCode != -1)
                    RevealCardIndex[count++] = candidateCardIndex[i];
        }
        
    }

    public class DecideCardFromOpenCard
    {
        public int DecideCardIndex;

        public DecideCardFromOpenCard(int decideCardIndex)
        {
            DecideCardIndex = decideCardIndex;
        }
    }

    public class UseFunctionCard_Mask
    {
        public int FunctionCardNameCode;

        public UseFunctionCard_Mask(int functionCardNameCode)
        {
            FunctionCardNameCode = functionCardNameCode;
        }
    }

    public class UseFunctionCard_Reform
    {
        public int FunctionCardNameCode;
        public int[] DecideCardNameCodes;

        public UseFunctionCard_Reform(int functionCardNameCode, int decideCardNameCode)
        {
            FunctionCardNameCode = functionCardNameCode;
            DecideCardNameCodes = new int[1] { decideCardNameCode };
        }

        public UseFunctionCard_Reform(int functionCardNameCode, int[] decideCardNameCodes)
        {
            FunctionCardNameCode = functionCardNameCode;
            DecideCardNameCodes = decideCardNameCodes;
        }
    }

    public class UseFunctionCard_Expand
    {
        public int FunctionCardNameCode;
        public int DecideDeckCardIndex;

        public UseFunctionCard_Expand(int functionCardNameCode, int decideDeckCardIndex)
        {
            FunctionCardNameCode = functionCardNameCode;
            DecideDeckCardIndex = decideDeckCardIndex;
        }
    }

    public class UseStrategyCard_Requirements
    {
        public int StrategyCardNameCode;
        public int[] RequirementCardNameCodes;

        public UseStrategyCard_Requirements(int strategyCardNameCode, int[] requirementCardNameCodes)
        {
            StrategyCardNameCode = strategyCardNameCode;
            RequirementCardNameCodes = requirementCardNameCodes;
        }
    }

    public class ReleasePower
    {
        public int[] ReleasedCardNameCodes;

        public ReleasePower(int[] releasedCardNameCodes)
        {
            ReleasedCardNameCodes = releasedCardNameCodes;
        }
    }

    public class DiscardStrategyCard
    {
        public int StrategyCardNameCode;

        public DiscardStrategyCard(int strategyCardNameCode)
        {
            StrategyCardNameCode = strategyCardNameCode;
        }
    }

    public class EndTurn
    {

    }

    public class EndCongressTurn
    {

    }

    public class CharacterNameCode0ChooseToRobOneCard
    {
        public int[] ChooseCardNameCodes;

        public CharacterNameCode0ChooseToRobOneCard(int[] chooseCardNameCodes)
        {
            ChooseCardNameCodes = chooseCardNameCodes;
        }
    }

    public class CharacterNameCode1ChooseOneDeckCard
    {
        public int ChooseDeckCardIndex;

        public CharacterNameCode1ChooseOneDeckCard(int chooseDeckCardIndex)
        {
            ChooseDeckCardIndex = chooseDeckCardIndex;
        }
    }

    public class CharacterNameCode2DiscardOneHandCard
    {
        public int DiscardHandCardNameCode;

        public CharacterNameCode2DiscardOneHandCard(int discardHandCardNameCode)
        {
            DiscardHandCardNameCode = discardHandCardNameCode;
        }
    }

    public class CharacterNameCode3UseSkill
    {

    }

    public class CharacterNameCode6UseSkill
    {

    }
    
    public class CharacterNameCode8DiscardOneHandCard
    {
        public int DiscardHandCardNameCode;

        public CharacterNameCode8DiscardOneHandCard(int discardHandCardNameCode)
        {
            DiscardHandCardNameCode = discardHandCardNameCode;
        }
    }

    public class CharacterNameCode10DiscardOneDeckCard
    {
        public int DiscardDeckCardIndex;

        public CharacterNameCode10DiscardOneDeckCard(int discardDeckCardIndex)
        {
            DiscardDeckCardIndex = discardDeckCardIndex;
        }
    }

    public class CharacterNameCode13RobOneHandCard
    {
        public int DecidePlayerSerial;

        public CharacterNameCode13RobOneHandCard(int decidePlayerSerial)
        {
            DecidePlayerSerial = decidePlayerSerial;
        }
    }

    public class CharacterNameCode14PlaceBackDeckCard
    {
        public int PlaceBackCardNameCode;

        public CharacterNameCode14PlaceBackDeckCard(int placeBackCardNameCode)
        {
            PlaceBackCardNameCode = placeBackCardNameCode;
        }
    }

    public class CharacterNameCode18ExpandDeckCardToEnemy
    {
        public int DecideEnemySerial;
        public int ExpandCardNameCode;

        public CharacterNameCode18ExpandDeckCardToEnemy(int decideEnemySerial, int expandCardNameCode)
        {
            DecideEnemySerial = decideEnemySerial;
            ExpandCardNameCode = expandCardNameCode;
        }
    }

    public class StrategyNameCode22DecideDiscardOneHandCard
    {
        public int DiscardHandCardNameCode;

        public StrategyNameCode22DecideDiscardOneHandCard(int discardHandCardNameCode)
        {
            DiscardHandCardNameCode = discardHandCardNameCode;
        }
    }
}