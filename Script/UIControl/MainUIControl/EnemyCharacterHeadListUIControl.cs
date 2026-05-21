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
        enemyHeadButtonList.Clear();
        foreach (EnemyData enemyData in Autoloads.sceneSingleton.battleManager.battleEnemyDataList)
        {
            Node characterHeadButton = enemyHeadButtonPrefab.Instantiate();
            ((CharacterHeadButtonControl)characterHeadButton).characterData = enemyData;
            AddChild(characterHeadButton);
            enemyHeadButtonList.Add((CharacterHeadButtonControl)characterHeadButton);
        }
        UpdateEnemyPrepareDisplays();
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

    public void ShowAllThinking()
    {
        foreach (CharacterHeadButtonControl enemyHeadButton in enemyHeadButtonList)
        {
            enemyHeadButton.SetActionStateText("思考中");
        }
    }

    public void UpdateEnemyPrepareDisplays()
    {
        foreach (CharacterHeadButtonControl enemyHeadButton in enemyHeadButtonList)
        {
            EnemyData enemyData = enemyHeadButton.characterData as EnemyData;
            if (enemyData == null)
            {
                continue;
            }

            if (enemyData.currentRestActionTimes <= 0)
            {
                enemyHeadButton.SetActionStateText("完成");
            }
            else
            {
                enemyHeadButton.SetActionStateText($"还剩 {enemyData.currentRestActionTimes} 次");
            }
            enemyHeadButton.UpdateUIDisplay();
        }
    }
}
