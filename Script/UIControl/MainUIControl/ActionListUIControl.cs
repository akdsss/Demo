using Godot;
using System;
using System.Collections.Generic;

public partial class ActionListUIControl : ColorRect
{
	[Export] public PackedScene actionItemPrefab;
	[Export] public VBoxContainer VBoxActionItemContent;
	public int ExpectedItemCount { get; set; }
	public List<CommandItemUIControl> actionItemUIControlList = new();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BuildActionItems();
	}

	public void BuildActionItems()
	{
		if (VBoxActionItemContent == null || actionItemPrefab == null)
		{
			return;
		}

		PubTool.instance.ClearChildren(VBoxActionItemContent);
		actionItemUIControlList.Clear();
		int itemCount = ExpectedItemCount > 0
			? ExpectedItemCount
			: Autoloads.sceneSingleton?.gameCharacterNum ?? 0;
		if (itemCount <= 0)
		{
			return;
		}

		for(int i = 0; i < itemCount; i++)
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
