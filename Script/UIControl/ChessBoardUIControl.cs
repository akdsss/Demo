using Godot;
using System.Linq;
using System.Collections.Generic;

public partial class ChessBoardUIControl : Sprite2D
{
	[Export] public ChessCellUIControl[] chessCellUIControlArray;

	public override void _Ready()
	{
		// 注册到全局单例
		Autoloads.gd_ChessBoard.chessBoardUIControl = this;
	}
	public void ShowAllChessCellButton()
	{
		foreach (var chessCellUIControl in chessCellUIControlArray)
		{
			chessCellUIControl.chessCellButton.Disabled = false;
		}
	}
	public void HideAllChessCellButton()
	{
		foreach (var chessCellUIControl in chessCellUIControlArray)
		{
			chessCellUIControl.chessCellButton.Disabled = true;
		}
	}
	//public void AddCharacterToChessCell(CharacterData characterData, Vector2I coord)
	//{
	//	Initialize();

 //       int idx = 0;
	//	switch (coord.X)
	//	{
	//		case 1:
	//			idx += coord.Y - 1;
	//			break;
	//		case 2:
	//			idx += 3 + coord.Y - 1;
	//			break;
	//		case 3:
	//			idx += 7 + coord.Y - 1;
	//			break;
	//	}
	//	chessCellUIControlArray[idx].AddCharacterDisplay(characterData);
	//}

	//public void RemoveAllCharacterDisplay()
	//{
 //       Initialize();
 //       foreach (var chessCellUIControl in chessCellUIControlArray)
	//	{
	//		chessCellUIControl.RemoveAllCharacterDisplay();
	//	}
	//}
}
