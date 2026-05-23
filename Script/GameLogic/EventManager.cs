using Godot;
using System;

public class EventManager
{
    public PlayerData currentMainPlayer;
    public PlayerCommandData currentMainPlayerCommand;
    public EnemyData currentMainEnemy;
    public CombatAreaId currentTargetAreaId = CombatAreaId.Unknown;
    public MoveEventInfo moveEventInfo;
    public DamageEventInfo damageEventInfo;
}

public class DamageEventManager
{
    public void DamageEvenManager()
    {
        getDamageEventHandler = dei =>
        {
            dei.damageSourceCharacter.hp -= dei.damageValue;
        };
    }
    // 造成伤害前
    public Action<DamageEventInfo> beforeMakeDamageEventHandler;
    // 造成伤害
    public Action<DamageEventInfo> makeDamageEventHandler;
    // 造成伤害后
    public Action<DamageEventInfo> afterMakeDamageEventHandler;

    // 受到伤害前
    public Action<DamageEventInfo> beforeGetDamageEventHandler;
    // 受到伤害
    public Action<DamageEventInfo> getDamageEventHandler;
    // 受到伤害后
    public Action<DamageEventInfo> afterGetDamageEventHandler;

    public void Execute(DamageEventInfo dei)
    {
        beforeMakeDamageEventHandler(dei);
        makeDamageEventHandler(dei);
        afterMakeDamageEventHandler(dei);
        beforeGetDamageEventHandler(dei);
        getDamageEventHandler(dei);
        afterGetDamageEventHandler(dei);
    }
}
public class DamageEventInfo
{
    public CharacterData damageSourceCharacter;
    public CharacterData damageTargetCharacter;
    public float damageValue;
}
public class MoveEventInfo
{
    public CharacterData moveSourceCharacter;
    public Vector2I moveTargetCoord;
    public CombatAreaId moveTargetAreaId = CombatAreaId.Unknown;
}
