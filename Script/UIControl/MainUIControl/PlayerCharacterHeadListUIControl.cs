using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerCharacterHeadListUIControl : Node
{
    [Export] public PackedScene playerHeadButtonPrefab;
    public List<CharacterHeadButtonControl> characterHeadButtonControlList = new();
    public override void _Ready()
    {
        // 注册到场景单例
        Autoloads.sceneSingleton.playerCharacterHeadListUIControl = this;
    }
    public void Initialize()
    {
        PubTool.instance.ClearChildren(this);
        foreach (PlayerData playerData in Autoloads.sceneSingleton.battleManager.battlePlayerDataList)
        {
            Node characterHeadButton = playerHeadButtonPrefab.Instantiate();
            ((CharacterHeadButtonControl)characterHeadButton).characterData = playerData;
            AddChild(characterHeadButton);
            characterHeadButtonControlList.Add((CharacterHeadButtonControl)characterHeadButton);
        }
        UpdateAllUIDisplay();
    }
    public void ResetUIDisplay()
    {
        foreach (CharacterHeadButtonControl characterHeadButtonControl in characterHeadButtonControlList)
        {
            characterHeadButtonControl.ResetUIDisplay();
        }
        Autoloads.sceneSingleton.playerActionChoseList.Visible = false;
    }
    public void RecoverForPrepare()
    {
        foreach (CharacterHeadButtonControl characterHeadButtonControl in characterHeadButtonControlList)
        {
            characterHeadButtonControl.ChangeToActionReadyDisplay();
        }
    }
    public void UpdateAllUIDisplay()
    {
        foreach (CharacterHeadButtonControl characterHeadButtonControl in characterHeadButtonControlList)
        {
            characterHeadButtonControl.UpdateUIDisplay();
        }
    }
}
