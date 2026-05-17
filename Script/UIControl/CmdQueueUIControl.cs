using Godot;

public partial class CmdQueueUIControl : Node
{
    [Export] public Texture defaultCharacterHead;
    [Export] public string defaultCmdName;
    [Export] public TextureRect[] allCharacterHeadArray;
    [Export] public Label[] allCmdMatrix;
    public override void _Ready()
    {
        // 注册到场景单例
        Autoloads.sceneSingleton.cmdQueueUIControl = this;
        ResetAll();
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
    public void SetCharacterHeadAndCmd(LevelData levelData)
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

