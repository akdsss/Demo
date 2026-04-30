using Godot;
using System;

public partial class StartMenuContol : CanvasLayer
{
	[Export] public Panel SystemSetPanel;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void StartGameButtonClicked()
	{
		GD.Print("开始游戏");
		GetTree().ChangeSceneToFile("res://Scene/MainScene.tscn");
	}

	public void SetButtonClicked()
	{
		SystemSetPanel.Visible = true;

    }

	public void ExitButtonClicked()
	{
		GetTree().Quit();
	}

	public void SetPanelCloseButtonClicked()
	{
		SystemSetPanel.Visible = false;
	}
}
