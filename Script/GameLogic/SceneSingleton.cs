using Godot;
using System;

public partial class SceneSingleton : Node
{
	public BattleManager battleManager;
	[Export] public Label gameStateLable;
	[Export] public Panel playerActionChoseList;
	[Export] public Texture2D defaultCharacterImage;
	public PlayerCharacterHeadListUIControl playerCharacterHeadListUIControl;
	public EnemyCharacterHeadListUIControl enemyCharacterHeadListUIControl;
	public CmdQueueUIControl cmdQueueUIControl;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		battleManager = new();
		// 注册到AutoLoads
		Autoloads.sceneSingleton = this;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
