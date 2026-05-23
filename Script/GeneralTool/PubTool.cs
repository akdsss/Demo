using Godot;

public partial class PubTool : Node
{
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
		if (Autoloads.sceneSingleton?.gameStateLable != null)
		{
			Autoloads.sceneSingleton.gameStateLable.Text = content;
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
