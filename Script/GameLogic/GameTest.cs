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
				gameMain.TestLevelLoad(levelData);
				break;
			case 1:
				gameMain.ExitGame();
				break;
			default:
				GD.Print("未知菜单项");
				break;
		}
	}
}
