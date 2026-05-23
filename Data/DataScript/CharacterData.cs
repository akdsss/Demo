using Godot;
using System.Collections.Generic;

public partial class CharacterData : Resource
{
	[Export] public int characterId;
	[Export] public string characterName;
	[Export] public string characterDescription;
	[Export] public Texture2D characterHeadImage;
    [Export] public float hp;
    [Export] public float maxHp;
    [Export] public int mp;
    [Export] public int maxMp;
    [Export] public float atk;
    public Vector2I coord;
    [Export] public CombatAreaId initialAreaId = CombatAreaId.Unknown;
    public CombatAreaId CurrentAreaId = CombatAreaId.Unknown;
    public int AreaAnchorIndex = -1;
	public CharacterBattleState characterBattleState;
    [Export] public int turnInitialActionTimes;
    public int currentRestActionTimes;
	public List<CommandExecuteInfo> commandQueue;
	public bool hasPrepared;
	public List<string> runtimeStatusIds = new();
	public List<int> runtimeStatusStacks = new();
	public float runtimeShieldValue;

	public virtual void CharacterInitialize()
	{
		ResetCommandQueue();
		hp = maxHp;
		mp = maxMp;
		characterBattleState = CharacterBattleState.ALIVE;
		currentRestActionTimes = turnInitialActionTimes;
		runtimeStatusIds.Clear();
		runtimeStatusStacks.Clear();
		runtimeShieldValue = 0;
		if (CurrentAreaId == CombatAreaId.Unknown)
		{
			CurrentAreaId = ResolveCurrentAreaId();
		}
	}

	public CombatAreaId ResolveCurrentAreaId()
	{
		if (CurrentAreaId != CombatAreaId.Unknown)
		{
			return CurrentAreaId;
		}

		if (initialAreaId != CombatAreaId.Unknown)
		{
			return initialAreaId;
		}

		return AreaDefinition.GetAreaIdForLegacyCoord(coord);
	}

	public void SetCurrentArea(CombatAreaId areaId)
	{
		CurrentAreaId = areaId;
		if (areaId != CombatAreaId.Unknown)
		{
			coord = AreaDefinition.GetLegacyCoordForAreaId(areaId);
		}
	}
	public void ResetCommandQueue()
	{
		commandQueue = new List<CommandExecuteInfo>();
		int targetQueueLength = Autoloads.sceneSingleton.gameQueueLength;
		for (int i = 0; i < targetQueueLength; i++)
		{
			commandQueue.Add(new CommandExecuteInfo());
		}
	}
	public void SetCommand(int actionPointCost, int ccmdQueueIdx, CommandExecuteInfo commandExecuteInfo)
	{
		hasPrepared = true;
		currentRestActionTimes -= actionPointCost;
		GD.Print($"玩家{characterName}设定了指令{commandExecuteInfo.commandData.commandName}，消耗行动次数{actionPointCost}");
		commandQueue[ccmdQueueIdx] = commandExecuteInfo;
		CommandItemUIControl.CurrentSelected.SetCommandToQueue(commandExecuteInfo.commandData);
		Autoloads.sceneSingleton.cmdQueueUIControl?.RefreshTimelineUnitInfo();
	}
	public void SetCommand(int actionPointCost, CharacterHeadButtonControl characterHeadButtonControl, int cmdQueueIdx, CommandExecuteInfo commandExecuteInfo, CommandItemUIControl cmdItemUIControl)
	{
		currentRestActionTimes -= actionPointCost;
		characterHeadButtonControl?.UpdateUIDisplay();
		if (currentRestActionTimes <= 0)
		{
			characterHeadButtonControl?.ChangeToActionOverDisplay();
		}
		commandQueue[cmdQueueIdx] = commandExecuteInfo;
		cmdItemUIControl.SetCommandToQueue(commandExecuteInfo.commandData);
		Autoloads.sceneSingleton.cmdQueueUIControl?.RefreshTimelineUnitInfo();
	}
}

public enum CharacterBattleState
{
	ALIVE,
	DYING,
	DEAD
}
