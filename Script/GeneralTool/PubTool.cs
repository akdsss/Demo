using Godot;
using System.Collections.Generic;

public partial class PubTool : Node
{
	private const int MaxPrintHistoryLines = 800;
	private readonly List<string> printHistory = new();
	public GameMode gameMode = GameMode.Normal;
	private static PubTool _instance;
	public static PubTool instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new PubTool();
			}
			return _instance;
		}
	}
	private PubTool() { }
	public void ClearChildren(Node parent)
	{
		var children = new Godot.Collections.Array<Node>(parent.GetChildren());

		foreach (Node child in children)
		{
			parent.RemoveChild(child);
			child.QueueFree();
		}
	}
	public void PrintToCmdAndTitle(string content)
	{
		GD.Print(content);
		AppendPrintHistory(content);
		Autoloads.sceneSingleton?.battleLogOverlayControl?.RefreshLogText();
		if (Autoloads.sceneSingleton?.gameStateLable != null)
		{
			Autoloads.sceneSingleton.gameStateLable.Text = content;
		}
	}
	public string GetPrintHistoryText()
	{
		return string.Join("\n", printHistory);
	}
	private void AppendPrintHistory(string content)
	{
		printHistory.Add(content ?? string.Empty);
		if (printHistory.Count > MaxPrintHistoryLines)
		{
			printHistory.RemoveRange(0, printHistory.Count - MaxPrintHistoryLines);
		}
	}
	public async void PrintToTitleForTime(string content, float time)
	{
		if (Autoloads.sceneSingleton?.gameStateLable == null)
		{
			return;
		}

		string old_content = Autoloads.sceneSingleton.gameStateLable.Text;
		Autoloads.sceneSingleton.gameStateLable.Text = content;
        await ToSignal(GetTree().CreateTimer(time), "timeout");
        Autoloads.sceneSingleton.gameStateLable.Text = old_content;
	}
}

public enum GameMode
{
	Test,
	Normal
}
