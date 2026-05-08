using Godot;
using System;
using System.Collections.Generic;

public partial class BattleManager 
{
    public BattleState battleState = BattleState.NOT_INITIALIZED;
    private TurnManager turnManager;
    private List<CharacterData> battleCharacterDataList;
    public List<PlayerData> battlePlayerDateList;
    public List<EnemyData> battleEnemyDataList;

	public void BattleStart()
	{
        BattleInitialize();
        battleState = BattleState.PLAYING;
        while(battleState != BattleState.OVER)
        {
            turnManager.TurnStart();
        }
        BattleEnd();
    }
    public void BattleInitialize()
    {
        battleState = BattleState.INITIALIZED;
        turnManager = new();
    }
    public void BattleEnd()
    {
        battleState = BattleState.END;
    }
    private class TurnManager
    {
        int turnNum = 0;
        MainTurnState mainTurnState = MainTurnState.NOT_INITIALIZED;
        public void TurnStart()
        {
            TurnInitialize();
            PrepareTurn();
            PlayTurn();
            TurnEnd();
        }
        public void TurnInitialize()
        {
            turnNum++;
            mainTurnState = MainTurnState.INITIALIZED;
        }
        public void TurnEnd()
        {
            mainTurnState = MainTurnState.END;
        }
        public void PrepareTurn()
        {
            
        }
        public void PlayTurn()
        {
            
        }
    }
    private enum MainTurnState
    {
        NOT_INITIALIZED,
        INITIALIZED,
        PRE_TURN,
        PLY_TURN,
        OVER,
        END
    }
}

public enum BattleState
{
    NOT_INITIALIZED,
    INITIALIZED,
    PLAYING,
    OVER,
    END
}

public enum PrepareTurnState
{
    BEFORE_PRE,
    ENEMY_PRE,
    PLAYER_PRE,
    AFTER_PRE,
}

public enum PlayTurnState
{
    BEFORE_PLAY,
    ENEMY_PLAY,
    PLAYER_PLAY,
    AFTER_PLAY,
}



