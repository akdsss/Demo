using Godot;

public partial class PlayerCharacterHeadListUIControl : Node
{
    [Export] public PackedScene playerHeadButtonPrefab;
    public override void _Ready()
    {
        // 注册到场景单例
        Autoloads.sceneSingleton.playerCharacterHeadListUIControl = this;
    }
    public void Initialize()
    {
        PubTool.instance.ClearChildren(this);
        foreach(PlayerData playerData in Autoloads.sceneSingleton.battleManager.battlePlayerDataList)
        {
            Node characterHeadButton = playerHeadButtonPrefab.Instantiate();
            ((CharacterHeadButtonControl)characterHeadButton).characterData = playerData;
            AddChild(characterHeadButton);
        }
    }
}
