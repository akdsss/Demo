using Godot;
using System;

public partial class PubTool : Node
{
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
}
