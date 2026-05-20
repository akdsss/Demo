using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class PlayerCommandData : CommandData
{
    //public void Execute(CommandExecuteInfo _commandExecuteInfo)
    //{
    //    switch (commandId)
    //    {
    //        case 0:// 攻击
    //            break;
    //        case 1:// 移动
    //            break;
    //        case 2:// 防御
    //            break;
    //    }
    //}
    public void UIButtonClick()
    {
        switch (commandId)
        {
            case 0:
                GD.Print("测试指令，不执行任何操作");
                break;
            case 1:// 攻击
                Autoloads.sceneSingleton.battleManager.eventManager.damageEventInfo = new()
                {
                    damageSourceCharacter = Autoloads.sceneSingleton.battleManager.eventManager.currentMainPlayer
                };
                Autoloads.sceneSingleton.enemyCharacterHeadListUIControl.ChangeToInteractable();
                break;
            case 2:// 移动
                Autoloads.sceneSingleton.battleManager.eventManager.moveEventInfo = new()
                {
                    moveSourceCharacter = Autoloads.sceneSingleton.battleManager.eventManager.currentMainPlayer
                };
                Autoloads.gd_ChessBoard.chessBoardUIControl.ShowAllChessCellButton();
                break;
            case 3:// 防御
                break;
        }
    }
}
