using Godot;
using System;
using System.Collections.Generic;

public partial class CmdQueueUIControl : Node
{
    [Export] public Texture defaultCharacterHead;
    [Export] public string defaultCmdName;
    [Export] public TextureRect[] allCharacterHeadArray;
    [Export] public Label[] allCmdMatrix;
    [Export] public HBoxContainer CommandQueueMatrix;
    [Export] public PackedScene commandListPrefab;
    List<ActionListUIControl> actionListUIControlList = new();
    public List<List<CommandItemUIControl>> commandItemUIControlMatrix;
    public override void _Ready()
    {
        // 注册到场景单例
        Autoloads.sceneSingleton.cmdQueueUIControl = this;
        // ResetAll();

        for (int i = 0; i < allCharacterHeadArray.Length; i++)
        {
            allCharacterHeadArray[i].Texture = (Texture2D)defaultCharacterHead;
        }

        PubTool.instance.ClearChildren(CommandQueueMatrix);
        for(int i = 0; i < 7; i++)
        {
            Node commandItemList = commandListPrefab.Instantiate();
            actionListUIControlList.Add(commandItemList as ActionListUIControl);
            CommandQueueMatrix.AddChild(commandItemList);
        }

        commandItemUIControlMatrix = new List<List<CommandItemUIControl>>();
        foreach(ActionListUIControl actionListUIControl in actionListUIControlList)
        {
            commandItemUIControlMatrix.Add(actionListUIControl.actionItemUIControlList);
        }

        foreach (var commandItemUIControlList in commandItemUIControlMatrix)
        {
            foreach (var commandItemUIControl in commandItemUIControlList)
            {
                commandItemUIControl.commandItemState = CommandItemState.NORMAL;
                commandItemUIControl.label.Text = defaultCmdName;
            }
        }
    }
    public void ResetAll()
    {
        for (int i = 0; i < allCharacterHeadArray.Length; i++)
        {
            allCharacterHeadArray[i].Texture = (Texture2D)defaultCharacterHead;
        }
        for (int i = 0; i < allCmdMatrix.Length; i++)
        {
            allCmdMatrix[i].Text = defaultCmdName;
        }
    }
    public void SetCharacterHead(LevelData levelData)
    {
        int idx = 0;
        for (int i = 0; i < levelData.playerInfoInLevelArray.Length; i++)
        {
            PlayerInfoInLevel playerInfoInLevel = levelData.playerInfoInLevelArray[i];
            if (idx >= allCharacterHeadArray.Length)
            {
                GD.PrintErr("头像列表赋值错误，超出最大上限");
            }
            allCharacterHeadArray[idx].Texture = playerInfoInLevel.playerData.characterHeadImage;
            idx++;
        }
        for (int i = 0; i < levelData.enemyInfoInLevelArray.Length; i++)
        {
            EnemyInfoInLevel enemyInfoInLevel = levelData.enemyInfoInLevelArray[i];
            if (idx >= allCharacterHeadArray.Length)
            {
                GD.PrintErr("头像列表赋值错误，超出最大上限");
            }
            allCharacterHeadArray[idx].Texture = enemyInfoInLevel.enemyData.characterHeadImage;
            idx++;
        }
    }
    
}

