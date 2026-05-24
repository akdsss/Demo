using Godot;
using System;

public partial class SceneSingleton : Node
{
	public BattleManager battleManager;
	public MainUIControl mainUIControl;
	[Export] public Label gameStateLable;
	[Export] public Panel playerActionChoseList;
	[Export] public Texture2D defaultCharacterImage;
	public EnemyCharacterHeadListUIControl enemyCharacterHeadListUIControl;
	public CmdQueueUIControl cmdQueueUIControl;
	public TutorialOverlayControl tutorialOverlayControl;
	public EncyclopediaOverlayControl encyclopediaOverlayControl;
	public GrowthRewardOverlayControl growthRewardOverlayControl;
	public AreaTargetMenuControl areaTargetMenuControl;
	public BattleLogOverlayControl battleLogOverlayControl;
	public int gameQueueLength = 6;
	public int gameCharacterNum = 7;
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
