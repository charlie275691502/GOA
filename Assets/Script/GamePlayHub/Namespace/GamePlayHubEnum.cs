using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlayHub
{
    public enum CardPlace
    {
        Draw,
        Deck,
        Hand,
        Play,
        Grave
    }

    public enum PowerType
    {
        Wealth,
        Industry,
        SeaPower,
        Military
    }

    public enum ClientStage {
        DrawCard,
        ChooseFromRevealedCard,
        MidTurn,
        UsingCard,
        EnemyTurn,
        Revolution,
        Victory,
        SelfCongress,
        EnemyCongress,
        UsingSkill
    }

    public enum UsingCardStage
    {
        None,
        Mask,
        Reform,
        Expand,
        Strategy,
        Release
    }

    public enum UsingSkillStage
    {
        None,
        CharacterNameCode0PreparingSkill,
        CharacterNameCode0ChoosingEffect,
        CharacterNameCode1PreparingSkill,
        CharacterNameCode1ChoosingEffect,
        CharacterNameCode2PreparingSkill,
        CharacterNameCode2ChoosingEffect,
        CharacterNameCode8PreparingSkill,
        CharacterNameCode8ChoosingEffect,
        CharacterNameCode10PreparingSkill,
        CharacterNameCode10ChoosingEffect,
        CharacterNameCode13ChoosingEffect,
        CharacterNameCode14PreparingSkill,
        CharacterNameCode15PreparingSkill,
        CharacterNameCode15ChoosingEffect,
        CharacterNameCode18PreparingSkill,
        CharacterNameCode18ChoosingEffect
    }

    public enum UsingStrategyStage
    {
        None,
        ChoosingRequirements,
        WaitingServer,
        StrategyNameCode22ChooseDiscardOneHandCard
    }

    public enum Buff
    {
        CantWin,
        Invulnerable,
        IncreaseStrategyLimitOne,
        WealthRestriction,
        IndustryRestriction,
        SeaPowerRestriction,
        MilitaryRestriction,
        FunctionRestriction,
        StrategyRestriction
    }

    public enum CardAnimationType
    {
        EnemyToGrave,
        DrawToEnemy,
        OpenDeckToEnemy,
        CoveredDeckToEnemy,
    }
}