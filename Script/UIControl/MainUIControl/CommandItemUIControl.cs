using Godot;
using System;
using System.Collections.Generic;

public partial class CommandItemUIControl : Control
{
	[Export] public Label label;
	[Export] public ColorRect colorRect;
	static readonly string normalColor = "ffffff";
	static readonly string disableColor = "6d6d6f";
	static readonly string highlightColor = "f1ca8e";
	static readonly string lockedColor = "76a1b1";
	static readonly Dictionary<CommandItemState, string> colorDict = new()
	{
		{CommandItemState.NORMAL, normalColor},
		{CommandItemState.DISABLE, disableColor},
		{CommandItemState.HIGHLIGHT, highlightColor},
		{CommandItemState.LOCKED, lockedColor}
	};
	public CommandItemState commandItemState = CommandItemState.NORMAL;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// HIGHLIGHT模式下闪烁
		if(commandItemState == CommandItemState.HIGHLIGHT)
		{
			// 获取时间
			double time = (double)Time.GetTimeDictFromSystem()["second"];
			time *= 2;
			float t = (float)(Math.Sin(time * Math.PI) * 0.5 + 0.5); // 计算0到1之间的正弦波插值
			
			// 解析高亮颜色和正常颜色
			Color highlight = new(highlightColor);
			Color normal = new(normalColor);
			
			// 颜色插值并赋值给节点
			Color interpolatedColor = normal.Lerp(highlight, t);
			colorRect.Color = interpolatedColor;
		}
		else
		{
			colorRect.Color = new(colorDict[commandItemState]);
		}
	}

	public void MouseEnter()
	{
		commandItemState = CommandItemState.HIGHLIGHT;
		// GD.Print("鼠标进入区域");
	}

	public void MouseExit()
	{
		commandItemState = CommandItemState.NORMAL;
		// GD.Print("鼠标退出区域");
	}

	public void Click()
	{
		commandItemState = CommandItemState.NORMAL;
	}
}

public enum CommandItemState
{
	NORMAL,
	DISABLE,
	HIGHLIGHT,
	LOCKED
}
