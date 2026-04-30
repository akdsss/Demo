using Godot;
using System;

public partial class MainUIControl : CanvasLayer
{
	[Export] public Panel SetPanel;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SetPanelCloseButtonClicked()
	{
		SetPanel.Visible = false;
	}

	public void SetPanelOpenButtonClicked()
	{
		SetPanel.Visible = true;
	}
}
