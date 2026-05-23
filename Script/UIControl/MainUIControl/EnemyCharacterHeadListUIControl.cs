using System.Collections.Generic;
using Godot;

public partial class EnemyCharacterHeadListUIControl : VBoxContainer
{
    [Export] public PackedScene enemyHeadButtonPrefab;
    public List<CharacterHeadButtonControl> enemyHeadButtonList = new List<CharacterHeadButtonControl>();
    public override void _Ready()
    {
        if (Autoloads.sceneSingleton != null)
        {
            Autoloads.sceneSingleton.enemyCharacterHeadListUIControl = this;
        }
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
        Visible = false;
    }
    public void ResetUIDisplay()
    {
        foreach (CharacterHeadButtonControl enemyHeadButton in enemyHeadButtonList)
        {
            enemyHeadButton.UpdateUIDisplay();
        }
        Autoloads.sceneSingleton.cmdQueueUIControl?.RefreshTimelineUnitInfo();
    }
    public void ChangeToUninteractable()
    {
        foreach (CharacterHeadButtonControl enemyHeadButton in enemyHeadButtonList)
        {
            enemyHeadButton.button.ButtonPressed = false;
            enemyHeadButton.button.MouseFilter = Control.MouseFilterEnum.Ignore;
        }
        Visible = false;
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
        Autoloads.sceneSingleton.cmdQueueUIControl?.RefreshTimelineUnitInfo();
    }
}
