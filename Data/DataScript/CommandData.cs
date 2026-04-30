using Godot;
using System;
using System.Collections.Generic;

public abstract partial class CommandData : Resource
{
    [Export] public int commandId;
    [Export] public string commandName;
    [Export] public string commandDescription;
    [Export] public CommandType commandType;
}

public partial class PlayerCommandData : CommandData
{ 
    public void Execute(CommandExecuteInfo _commandExecuteInfo)
    {
        switch (commandId)
        {
            case 0:// 攻击
                break;
            case 1:// 移动
                break;
            case 2:// 防御
                break;
        }
    }
}

public partial class EnemyCommandData : CommandData
{
}

public class CommandExecuteInfo
{
    public int targetCharacterNum;
    public List<CharacterData> targetCharacterList;
    public 
}

public enum CommandType
{
    ATTACK,
    DEFENT,
    MOVE
}
