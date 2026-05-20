using Godot;
using System.Linq;
using System.Text.RegularExpressions;

public partial class CharacterHeadButtonControl : Node
{
    Color actPointHexColor = Color.FromHtml("#d8a543");
    [Export] public TextureRect characterHeadTextureRect;
    [Export] public Label actionStateLabel;
    [Export] public ColorRect[] actionTimesArray;
    [Export] public Button button;
    [Export] public TextureRect focusTrangle;
    public CharacterData characterData;

    string buttonGroupAddress = "res://Data/other/character_head_button_group.tres";

    public override void _Ready()
    {
        if (characterData is PlayerData)
        {
            // 注册按钮组
            button.ButtonGroup = (ButtonGroup)ResourceLoader.Load(buttonGroupAddress);
            if (button.ButtonGroup == null)
            {
                GD.PrintErr("未找到按钮组");
            }
        }
        else if (characterData is EnemyData)
        {

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
    }

    public void SetActionTimes(int actionTimes)
    {
        if (actionTimesArray.Length > actionTimes)
        {
            GD.PrintErr("行动点数超过最大上限");
        }
        for (int i = 0; i < actionTimesArray.Length; i++)
        {
            if (i < actionTimes)
            {
                actionTimesArray[i].Color = actPointHexColor;
            }
            else
            {
                actionTimesArray[i].Color = Color.Color8(0, 0, 0);
            }
        }
    }
    public void CharacterHeadButtonPressed(bool toggled)
    {
        if (toggled == false)
        {
            //GD.Print("取消点击状态");
            // 取消选中角色
            if (characterData is PlayerData)
            {
                Autoloads.sceneSingleton.battleManager.eventManager.currentMainPlayer = null;
            }
            else if (characterData is EnemyData)
            {
                Autoloads.sceneSingleton.battleManager.eventManager.currentMainEnemy = null;
            }
            focusTrangle.Visible = false;
            Autoloads.sceneSingleton.playerActionChoseList.Visible = false;
            actionStateLabel.Text = "待命";
        }
        else
        {
            //PubTool.instance.PrintToCmdAndTitle("点击了角色头像");
            // 设置当前选中角色
            if (characterData is PlayerData)
            {
                Autoloads.sceneSingleton.battleManager.eventManager.currentMainPlayer = characterData as PlayerData;
            }
            else if (characterData is EnemyData)
            {
                Autoloads.sceneSingleton.battleManager.eventManager.currentMainEnemy = characterData as EnemyData;
            }
            focusTrangle.Visible = true;
            Autoloads.sceneSingleton.playerActionChoseList.Visible = true;
            switch (characterData)
            {
                case PlayerData playerData:
                    ((PlayerChoseListPanelControl)Autoloads.sceneSingleton.playerActionChoseList).SetChoseListPanel(playerData.playerCommandDataList.ToList());
                    break;
                case EnemyData enemyData:
                    //((EnemyCharacterHeadListUIControl) Autoloads.sceneSingleton.enemyCharacterHeadListUIControl).SetEnemyCharacterHeadList(characterData);
                    break;
                default:
                    GD.PrintErr("错误：未知角色类型");
                    break;
            }
            actionStateLabel.Text = "行动";
        }
    }
    public void ResetUIDisplay()
    {
        focusTrangle.Visible = false;
        button.ButtonPressed = false;
    }
}
