using System.Collections.Generic;
using Godot;

public partial class EnemyCharacterHeadListUIControl : Node
{
    [Export] public PackedScene enemyHeadButtonPrefab;
    public List<CharacterHeadButtonControl> enemyHeadButtonList = new List<CharacterHeadButtonControl>();
    public override void _Ready()
    {
        // 注册到场景单例
        Autoloads.sceneSingleton.enemyCharacterHeadListUIControl = this;
    }
    public void Initialize()
    {
        PubTool.instance.ClearChildren(this);
        foreach (EnemyData enemyData in Autoloads.sceneSingleton.battleManager.battleEnemyDataList)
        {
            Node characterHeadButton = enemyHeadButtonPrefab.Instantiate();
            ((CharacterHeadButtonControl)characterHeadButton).characterData = enemyData;
            AddChild(characterHeadButton);
            enemyHeadButtonList.Add((CharacterHeadButtonControl)characterHeadButton);
        }
    }
    public void ResetUIDisplay()
    {
        foreach (CharacterHeadButtonControl enemyHeadButton in enemyHeadButtonList)
        {
            enemyHeadButton.UpdateUIDisplay();
        }
    }
    public void ChangeToInteractable()
    {
        foreach (CharacterHeadButtonControl enemyHeadButton in enemyHeadButtonList)
        {
            enemyHeadButton.button.Disabled = false;
            enemyHeadButton.button.MouseFilter = Control.MouseFilterEnum.Stop;
        }
    }
    public void ChangeToUninteractable()
    {
        foreach (CharacterHeadButtonControl enemyHeadButton in enemyHeadButtonList)
        {
            enemyHeadButton.button.ButtonPressed = false;
            enemyHeadButton.button.MouseFilter = Control.MouseFilterEnum.Ignore;
        }
    }
    public void UpdateAllUIDisplay()
    {
        foreach (CharacterHeadButtonControl enemyHeadButton in enemyHeadButtonList)
        {
            enemyHeadButton.UpdateUIDisplay();
        }
    }
}
