using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePlayHub.ClientToServer;
using GamePlayHub.ServerToClient;
using Main;

namespace GamePlayHub{
    public static class AI
    {
        // Receive //

        public static void ChooseCharacterFromThree(int playerSerial, ChooseCharacterFromThree chooseCharacterFromThree, S_GamePlayManager s_GamePlayManager)
        {
            s_GamePlayManager.DecideCharacter(playerSerial, new DecideCharacter(12));
        }

        public static void TurnStart(int playerSerial, TurnStart turnStart, S_GamePlayManager s_GamePlayManager)
        {
            List<int> coveredCardIndexList = new List<int>();
            for (int i = 0; i < s_GamePlayManager.DeckCards.Count; i++) if (s_GamePlayManager.DeckCards[i] != -2 && !s_GamePlayManager.DeckCardsIsReveal[i]) coveredCardIndexList.Add(i);
            List<int> openCardNameCodeList = new List<int>();
            for (int i = 0; i < s_GamePlayManager.DeckCards.Count; i++) if (s_GamePlayManager.DeckCards[i] != -2 && s_GamePlayManager.DeckCardsIsReveal[i]) openCardNameCodeList.Add(s_GamePlayManager.DeckCards[i]);

            int nameCode = ChoosePreferCard(openCardNameCodeList);
            if (nameCode != -1 && DataMaster.GetInstance().GetCardInfo(nameCode).PublicInfo.Power <= 3 || coveredCardIndexList.Count < 2)
            {
                int cardIndex = s_GamePlayManager.DeckCards.FindIndex(x => x == nameCode);
                s_GamePlayManager.DecideCardFromOpenCard(playerSerial, new DecideCardFromOpenCard(cardIndex));
                InTurnMovement(playerSerial, s_GamePlayManager);
            }
            else
            {
                // draw from covered
                int random1 = Random.Range(0, coveredCardIndexList.Count);
                int random2 = Random.Range(0, coveredCardIndexList.Count);
                while (random1 == random2) random2 = Random.Range(0, coveredCardIndexList.Count);
                s_GamePlayManager.RevealSeveralCoveredCards(playerSerial,
                    new RevealSeveralCoveredCards(new List<int>(2) { coveredCardIndexList[random1], coveredCardIndexList[random2] }));
            }
        }

        public static void CongressTurnStart(int playerSerial, CongressTurnStart congressTurnStart, S_GamePlayManager s_GamePlayManager)
        {
            s_GamePlayManager.EndCongressTurn(playerSerial, new EndCongressTurn());
        }

        public static void RevealChosenCoveredCards(int playerSerial, RevealChosenCoveredCards revealChosenCoveredCards, S_GamePlayManager s_GamePlayManager)
        {
            int nameCode = ChoosePreferCard(revealChosenCoveredCards.ChosenCoveredCardNameCodes);
            int[] revealIndexList = new int[revealChosenCoveredCards.ChosenCoveredCardNameCodes.Length];
            for(int i=0;i< revealChosenCoveredCards.ChosenCoveredCardNameCodes.Length;i++)
                revealIndexList[i] = s_GamePlayManager.DeckCards.FindIndex(x => x == revealChosenCoveredCards.ChosenCoveredCardNameCodes[i]);

            int cardIndex = s_GamePlayManager.DeckCards.FindIndex(x => x == nameCode);
            s_GamePlayManager.DecideCardFromRevealedCoveredCards(playerSerial, new DecideCardFromRevealedCoveredCards(cardIndex, revealIndexList));
            InTurnMovement(playerSerial, s_GamePlayManager);
        }

        // process //

        static void InTurnMovement(int playerSerial, S_GamePlayManager s_GamePlayManager)
        {
            //馮·布呂歇爾，被動技，必須進行二次抽牌階段。 // 先不要endturn
            if (s_GamePlayManager.GetServerPlayerFromPlayerSerial(playerSerial).CharacterNameCode == 4) return;
            s_GamePlayManager.EndTurn(playerSerial, new EndTurn());
        }

        static int ChoosePreferCard(int[] candidates)
        {
            List<int> ret = new List<int>();
            foreach (int candidate in candidates) ret.Add(candidate);
            return ChoosePreferCard(ret);
        }

        static int ChoosePreferCard(List<int> candidates)
        {
            int minPower = 99;
            int minPowerNameCode = -1;
            foreach(int nameCode in candidates)
            {
                CardInfo cardInfo = DataMaster.GetInstance().GetCardInfo(nameCode);
                int power = cardInfo.PublicInfo.Power;
                if(power < minPower)
                {
                    minPower = power;
                    minPowerNameCode = nameCode;
                }
            }
            return minPowerNameCode;
        }
    }
}
