using Godot;

[GlobalClass]
public partial class CommandData : Resource
{
    [Export] public int commandId;
    [Export] public string commandName;
    [Export] public string commandDescription;
    [Export] public float[] allCommandParams;
    [Export] public int priority = 0;
    public CommandData()
    {
        commandId = -1;
        commandName = "默认指令";
        commandDescription = "默认指令描述";
    }

}

public class CommandExecuteInfo
{
    public bool isDefault = false;
    public CharacterData sourceCharacterData;
    public CommandData commandData;
    public Vector2I targetCoord;
    public CombatAreaId targetAreaId = CombatAreaId.Unknown;
    public CharacterData targetCharacterData;
    public CommandExecuteInfo()
    {
        commandData = new CommandData();
        isDefault = true;
    }
}
