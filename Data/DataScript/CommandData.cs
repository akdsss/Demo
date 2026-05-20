using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class CommandData : Resource
{
    [Export] public int commandId;
    [Export] public string commandName;
    [Export] public string commandDescription;
    [Export] public float[] allCommandParams;
    public CommandData()
    {
        commandId = -1;
        commandName = "默认指令";
        commandDescription = "默认指令描述";
    }

}

public class CommandExecuteInfo
{
    public bool isDefault;
    public CharacterData sourceCharacterData;
    public CommandData commandData;
    // public int targetCharacterNum;
    public Vector2I targetCoord;
    // public List<CharacterData> targetCharacterList;
    public CharacterData targetCharacterData;
    public CommandExecuteInfo()
    {
        commandData = new CommandData();
        isDefault = true;
    }
    public virtual void ExecuteInPlay()
    {
        // GD.Print("默认指令执行");
        if (isDefault)
        {
            return;
        }
        
        if (commandData is PlayerCommandData)
        {
            switch (commandData.commandId)
            {
                case 0:// 测试
                    break;
                case 1:// 攻击
                    sourceCharacterData.MakeDamage(targetCharacterData, sourceCharacterData.atk);
                    break;
                case 2:// 移动
                    Autoloads.gd_ChessBoard.MoveCharacter(sourceCharacterData, targetCoord);
                    break;
                case 3:// 防御
                    break;
                default:
                    break;
            }
        }
        else if (commandData is EnemyCommandData)
        {
            switch (commandData.commandId)
            {
                case 0:// 测试
                    break;
                case 1:// 移动
                    Autoloads.gd_ChessBoard.MoveCharacter(sourceCharacterData, targetCoord);
                    break;
                case 2:// 跳过
                    break;
                case 3:// 攻击
                    break;
                default:
                    break;
            }
        }
        else
        {
            GD.PrintErr("错误：未知的命令子类!");
        }
    }
    // public void Execute()
    // {
    // 	if(commandData is PlayerCommandData)
    // 	{
    //         int cmdID = commandData.commandId;
    //         switch (cmdID)
    //         {
    //             case 0:// 攻击
    //                 DamageEventManager dem = new();
    //                 DamageEventInfo dei = new();
    //                 dei.damageSourceCharacter = sourceCharacterData;
    //                 dei.damageValue = sourceCharacterData.atk;
    //                 dei.damageTargetCharacter = targetCharacterList[0];
    //                 dem.Execute(dei);
    //                 break;
    //             case 1:// 移动
    //                 Autoloads.gd_ChessBoard.MoveCharacter(sourceCharacterData, targetCoord);
    //                 break;
    //             case 2:// 防御
    //                 break;
    //             default:
    //                 break;
    //         }
    //     }
    // 	else if(commandData is EnemyCommandData)
    // 	{
    //         int cmdID = commandData.commandId;
    //         switch (cmdID)
    //         {
    //             case 0:// 测试
    //                 GD.Print("怪物测试指令，不执行任何操作");
    //                 break;
    //             default:
    //                 GD.PrintErr("错误：未知的命令ID!");
    //                 break;
    //         }
    //     }
    // 	else
    // 	{
    // 		GD.PrintErr("错误：未知的命令子类!");
    // 	}

    // }
}
