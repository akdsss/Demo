using Godot;
using System.Linq;
using System.Collections.Generic;

public partial class ChessBoardUIControl : Sprite2D
{
	[Export] public ChessCellUIControl[] chessCellUIControlArray;
	public override void _Ready()
	{
		// 初始化棋盘UI节点树
	}
	public void AddCharacterToChessCell(CharacterData characterData, Vector2I coord)
	{
		int idx = 0;
		switch (coord.X)
		{
			case 1:
				idx += coord.Y - 1;
				break;
			case 2:
				idx += 3 + coord.Y - 1;
				break;
			case 3:
				idx += 7 + coord.Y - 1;
				break;
		}
		chessCellUIControlArray[idx].AddCharacterDisplay(characterData);
	}

	public void RemoveAllCharacterDisplay()
	{
		foreach (var chessCellUIControl in chessCellUIControlArray)
		{
			chessCellUIControl.RemoveAllCharacterDisplay();
		}
	}
}
