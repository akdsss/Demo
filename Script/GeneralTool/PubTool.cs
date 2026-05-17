using Godot;
using System;
using static BattleManager;

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
		// 获取所有子节点的副本（避免遍历时修改集合）
		var children = new Godot.Collections.Array<Node>(parent.GetChildren());

		foreach (Node child in children)
		{
			parent.RemoveChild(child); // 从父节点移除
			child.QueueFree();         // 安全释放内存（推荐）
									   // 或 child.Free();        // 立即释放（不推荐，可能引发错误）
		}
	}
	public void PrintToCmdAndTitle(string content)
	{
		GD.Print(content);
		Autoloads.sceneSingleton.gameStateLable.Text = content;
	}
	public async void PrintToTitleForTime(string content, float time)
	{
		string  old_content = Autoloads.sceneSingleton.gameStateLable.Text;
		Autoloads.sceneSingleton.gameStateLable.Text = content;
        //GD.Print("开始暂停...");
        await ToSignal(GetTree().CreateTimer(time), "timeout");
        //GD.Print("暂停结束，1 秒过去了");
        Autoloads.sceneSingleton.gameStateLable.Text = old_content;
	}
    //public void SetManagerState(BattleManager battleManager, BattleState _battleState)
    //{
    //    battleManager.battleState = _battleState;
    //    EmitSignal(nameof(BattleStateChangedEventHandler));
    //}
    //public void SetManagerState(BattleManager battleManager, MainTurnState _mainTurnState)
    //{
    //    battleManager.mainTurnState = _mainTurnState;
    //    EmitSignal(nameof(MainTurnStateChangedEventHandler));
    //}
    //public void SetManagerState(BattleManager battleManager, PrepareTurnState _prepareTurnState)
    //{
    //    battleManager.prepareTurnState = _prepareTurnState;
    //    EmitSignal(nameof(PrepareTurnStateChangedEventHandler));
    //}
    //public void SetManagerState(BattleManager battleManager, PlayTurnState _playTurnState)
    //{
    //    battleManager.playTurnState = _playTurnState;
    //    EmitSignal(nameof(PlayTurnStateChangedEventHandler));
    //}
}

public enum GameMode
{
	Test,
	Normal
}
