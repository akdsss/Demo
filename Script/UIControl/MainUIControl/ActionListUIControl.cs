using Godot;
using System;
using System.Collections.Generic;

public partial class ActionListUIControl : ColorRect
{
	[Export] public PackedScene actionItemPrefab;
	[Export] public VBoxContainer VBoxActionItemContent;
	public List<CommandItemUIControl> actionItemUIControlList = new();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PubTool.instance.ClearChildren(VBoxActionItemContent);
		for(int i = 0; i < Autoloads.sceneSingleton.gameCharacterNum; i++)
		{
			Node actionItem = actionItemPrefab.Instantiate();
			actionItemUIControlList.Add(actionItem as CommandItemUIControl);
			VBoxActionItemContent.AddChild(actionItem);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
