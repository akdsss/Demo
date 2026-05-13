using Godot;

public partial class GameTest : MenuButton
{
	[Export] public GameMain gameMain;
	[Export] public LevelData levelData;
	public override void _Ready()
	{
		var menuButton = GetNode<MenuButton>(".");
		var popup = menuButton.GetPopup();

		// 连接信号
		popup.IdPressed += OnMenuItemPressed;
	}

	private void OnMenuItemPressed(long id)
	{
		switch (id)
		{
			case 0:
				TestStartGame();
                break;
			case 1:
				gameMain.ExitGame();
				break;
			case 2:
				//PubTool.instance.SetManagerState(Autoloads.sceneSingleton.battleManager, PrepareTurnState.ENEMY_PRE_OVER);
                Autoloads.sceneSingleton.battleManager.SetManagerState(PrepareTurnState.ENEMY_PRE_OVER);
                break;
            case 3:
                //PubTool.instance.SetManagerState(Autoloads.sceneSingleton.battleManager, PrepareTurnState.PLAYER_PRE_OVER);
                Autoloads.sceneSingleton.battleManager.SetManagerState(PrepareTurnState.PLAYER_PRE_OVER);
                break;
            default:
				GD.Print("未知菜单项");
				break;
		}
	}
	private void TestStartGame()
	{
		PubTool.instance.gameMode = GameMode.Test;
        levelData.LevelInitialize();
		Autoloads.sceneSingleton.battleManager.BattleStart(levelData);
    }
}
