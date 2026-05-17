using Godot;

public partial class EnemyCharacterHeadListUIControl : Node
{
    [Export] public PackedScene enemyHeadButtonPrefab;
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
        }
    }
}
