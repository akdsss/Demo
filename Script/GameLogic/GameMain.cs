using Godot;

public partial class GameMain : Node
{
	private static readonly string[] LevelResourcePaths =
	{
		"res://Data/LevelData/Level_data0.tres",
		"res://Data/LevelData/Level_data1.tres",
		"res://Data/LevelData/Level_data2.tres"
	};

	[Export] public LevelData levelData;

	private int currentLevelIndex = -1;
	private bool isStartingLevel;

	public override void _Ready()
	{
		Autoloads.gd_ChessBoard.BindAreaAnchors(
			Autoloads.gd_ChessBoard.chessBoardUIControl.chessCellUIControlArray);

		if (Autoloads.sceneSingleton.playerActionChoseList != null)
		{
			Autoloads.sceneSingleton.playerActionChoseList.Visible = false;
		}

		currentLevelIndex = ResolveLevelIndex(levelData);
		StartGame();
	}

	public void StartGame()
	{
		PubTool.instance.gameMode = GameMode.Normal;
		StartLevel(levelData);
	}

	public void ContinueAfterVictory()
	{
		int nextLevelIndex = currentLevelIndex + 1;
		if (nextLevelIndex < 0 || nextLevelIndex >= LevelResourcePaths.Length)
		{
			PubTool.instance.PrintToCmdAndTitle("All configured demo levels are complete.");
			return;
		}

		LevelData nextLevelData = ResourceLoader.Load<LevelData>(LevelResourcePaths[nextLevelIndex]);
		if (nextLevelData == null)
		{
			GD.PrintErr($"Failed to load next level: {LevelResourcePaths[nextLevelIndex]}");
			return;
		}

		currentLevelIndex = nextLevelIndex;
		levelData = nextLevelData;
		StartLevel(levelData);
	}

	public void ExitGame()
	{
		GD.Print("Exit game.");
		GetTree().Quit();
	}

	private void StartLevel(LevelData nextLevelData)
	{
		if (nextLevelData == null || isStartingLevel)
		{
			return;
		}

		isStartingLevel = true;
		nextLevelData.LevelInitialize();
		Autoloads.sceneSingleton.battleManager.BattleStart(nextLevelData);
		isStartingLevel = false;
	}

	private static int ResolveLevelIndex(LevelData data)
	{
		if (data == null)
		{
			return 0;
		}

		for (int i = 0; i < LevelResourcePaths.Length; i++)
		{
			if (data.ResourcePath == LevelResourcePaths[i] || data.levelId == i + 1)
			{
				return i;
			}
		}

		return 0;
	}
}
