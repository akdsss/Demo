using Godot;
using System.Linq;
using System.Text.RegularExpressions;

public partial class CharacterHeadButtonControl : Node
{
    private static CharacterHeadButtonControl currentSelectedPlayerHead;
    public static CharacterHeadButtonControl CurrentSelectedPlayerHead => currentSelectedPlayerHead;
    Color actPointHexColor = Color.FromHtml("#d8a543");
    [Export] public TextureRect characterHeadTextureRect;
    [Export] public Label actionStateLabel;
    [Export] public ColorRect[] actionTimesArray;
    [Export] public Button button;
    [Export] public TextureRect focusTrangle;
    [Export] public TextureProgressBar hpBar;
    [Export] public Label hpLabel;
    public CharacterData characterData;
    public bool hasAct = false;

    string playerButtonGroupAddress = "res://Data/other/character_head_button_group.tres";
    string enemyButtonGroupAddress = "res://Data/other/enemy_character_head_button_group.tres";

    public override void _Ready()
    {
        if (characterData is PlayerData)
        {
            // 注册按钮组
            button.ButtonGroup = (ButtonGroup)ResourceLoader.Load(playerButtonGroupAddress);
            if (button.ButtonGroup == null)
            {
                GD.PrintErr("未找到按钮组");
            }
        }
        else if (characterData is EnemyData)
        {
            // 注册按钮组
            button.ButtonGroup = (ButtonGroup)ResourceLoader.Load(enemyButtonGroupAddress);
            if (button.ButtonGroup == null)
            {
                GD.PrintErr("未找到按钮组");
            }
            // 关闭交互
            button.MouseFilter = Control.MouseFilterEnum.Ignore;
        }
        else
        {
            GD.PrintErr($"错误：未知角色类型{characterData?.GetType()}");
        }

        // 设置开关类型
        button.ToggleMode = true;
        // 重置按钮状态
        button.ButtonPressed = false;
        focusTrangle.Visible = false;
        actionStateLabel.Text = "待命";

        // 设置头像
        if (characterData == null)
        {
            GD.PrintErr("错误：未设置角色数据，头像按钮初始化失败！");
        }
        else
        {
            if (characterData.characterHeadImage == null)
            {
                GD.PrintErr($"角色ID{characterData.characterId}未设置头像，将使用默认头像!");
                characterHeadTextureRect.Texture = Autoloads.sceneSingleton.defaultCharacterImage;
            }
            else
            {
                characterHeadTextureRect.Texture = characterData.characterHeadImage;
            }
        }
        // 设置血量
        hpBar.Value = characterData.hp / characterData.maxHp * 100;
        hpLabel.Text = characterData.hp.ToString();
        hpLabel.Visible = true;
        hpBar.Visible = true;
    }
    public void UpdateUIDisplay()
    {
        SetActionTimes(characterData.currentRestActionTimes);
        UpdateHPDisplay();
    }
    public void SetActionTimes(int actionTimes)
    {
        if (actionTimesArray.Length < actionTimes)
        {
            GD.PrintErr("行动点数超过最大上限");
        }
        for (int i = 1; i < actionTimesArray.Length + 1; i++)
        {
            if (i <= actionTimes)
            {
                actionTimesArray[^i].Color = actPointHexColor;
            }
            else
            {
                actionTimesArray[^i].Color = Color.Color8(0, 0, 0);
            }
        }
    }
    public void CharacterHeadButtonPressed(bool toggled)
    {
        if (characterData is PlayerData playerData)
        {
            if (toggled == false)
            {
                if (hasAct == true) return;
                //GD.Print("取消点击状态");
                // 取消选中角色
                Autoloads.sceneSingleton.playerActionChoseList.Visible = false;
                actionStateLabel.Text = "待命";
                currentSelectedPlayerHead = null;
                Autoloads.sceneSingleton.battleManager.eventManager.currentMainPlayer = null;
                // if (characterData is PlayerData)
                // {
                //     Autoloads.sceneSingleton.playerActionChoseList.Visible = false;
                //     actionStateLabel.Text = "待命";
                //     currentSelectedPlayerHead = null;
                //     Autoloads.sceneSingleton.battleManager.eventManager.currentMainPlayer = null;
                // }
                // else if (characterData is EnemyData)
                // {
                //     Autoloads.sceneSingleton.battleManager.eventManager.currentMainEnemy = null;
                // }
                focusTrangle.Visible = false;
            }
            else
            {
                //PubTool.instance.PrintToCmdAndTitle("点击了角色头像");
                // 设置当前选中角色
                Autoloads.sceneSingleton.battleManager.eventManager.currentMainPlayer = playerData;
                // GD.Print($"当前选中角色：{Autoloads.sceneSingleton.battleManager.eventManager.currentMainPlayer.characterName}");
                // if (characterData is PlayerData)
                // {
                //     Autoloads.sceneSingleton.battleManager.eventManager.currentMainPlayer = characterData as PlayerData;
                //     GD.Print($"当前选中角色：{Autoloads.sceneSingleton.battleManager.eventManager.currentMainPlayer.characterName}");
                // }
                // else if (characterData is EnemyData)
                // {
                //     Autoloads.sceneSingleton.battleManager.eventManager.currentMainEnemy = characterData as EnemyData;
                // }
                focusTrangle.Visible = true;
                Autoloads.sceneSingleton.playerActionChoseList.Visible = true;
                ((PlayerChoseListPanelControl)Autoloads.sceneSingleton.playerActionChoseList).SetChoseListPanel(playerData.playerCommandDataList.ToList());
                // switch (characterData)
                // {
                //     case PlayerData playerData:
                //         ((PlayerChoseListPanelControl)Autoloads.sceneSingleton.playerActionChoseList).SetChoseListPanel(playerData.playerCommandDataList.ToList());
                //         break;
                //     case EnemyData enemyData:
                //         //((EnemyCharacterHeadListUIControl) Autoloads.sceneSingleton.enemyCharacterHeadListUIControl).SetEnemyCharacterHeadList(characterData);
                //         break;
                //     default:
                //         GD.PrintErr("错误：未知角色类型");
                //         break;
                // }
                actionStateLabel.Text = "行动";

                currentSelectedPlayerHead = this;
            }
        }
        else if (characterData is EnemyData enemyData)
        {
            if (toggled == false)
            {
                // 取消选中角色
                Autoloads.sceneSingleton.battleManager.eventManager.currentMainEnemy = null;
                focusTrangle.Visible = false;
            }
            else
            {
                // 设置当前选中角色
                Autoloads.sceneSingleton.battleManager.eventManager.currentMainEnemy = enemyData;
                // GD.Print($"当前选中角色：{Autoloads.sceneSingleton.battleManager.eventManager.currentMainEnemy.characterName}");
                focusTrangle.Visible = true;

                // 设置指令
                Autoloads.sceneSingleton.battleManager.eventManager.damageEventInfo.damageTargetCharacter = enemyData;
                Autoloads.sceneSingleton.battleManager.eventManager.damageEventInfo.damageTargetCharacterHeadButtonControl = this;
                // Autoloads.sceneSingleton.cmdQueueUIControl.cmdQueueState = CmdQueueState.CMDSET;
                Autoloads.sceneSingleton.cmdQueueUIControl.SwitchOnPlayerCommandSet();
            }
        }
    }
    public void ResetUIDisplay()
    {
        focusTrangle.Visible = false;
        button.ButtonPressed = false;
    }
    public void ChangeToActionOverDisplay()
    {
        actionStateLabel.Text = "已行动";
        button.Disabled = true;
        hasAct = true;
    }
    public void ChangeToActionReadyDisplay()
    {
        focusTrangle.Visible = false;
        Autoloads.sceneSingleton.playerActionChoseList.Visible = false;
        actionStateLabel.Text = "待命";
        hasAct = false;
    }
    public void UpdateHPDisplay()
    {
        hpBar.Value = characterData.hp / characterData.maxHp * 100;
        hpLabel.Text = $"{characterData.hp}";
    }
}
