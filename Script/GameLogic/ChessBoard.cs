using Godot;
using System.Linq;
using System.Collections.Generic;

public partial class ChessBoard : Node
{
	public List<List<ChessCell>> chessCellList;
	public ChessBoardUIControl chessBoardUIControl;

	public ChessBoard()
	{
		chessCellList = new List<List<ChessCell>>();
		for (int i = 0; i < 3; i++)
		{
			List<ChessCell> row = new List<ChessCell>();
			for (int j = 0; j < (i == 1 ? 4 : 3); j++)
			{
				row.Add(new ChessCell()
				{
					coord = new Vector2I(i, j)
				});
			}
			chessCellList.Add(row);
		}
	}

	public override void _Ready()
	{
		// 在场景树准备好时把实例注册到静态访问器
		Autoloads.gd_ChessBoard = this;
	}

	public void SetCharacterToChessCell(CharacterData characterData, Vector2I coord)
	{
		// 设置角色坐标
		characterData.coord = coord;
		// 更新UI显示
		int row_idx = coord.X;
		int col_idx = coord.Y;
		ChessCell targetChessCell = chessCellList[row_idx][col_idx];		
		targetChessCell.chessCellUIControl.AddCharacterDisplay(characterData);
		//foreach (var chessCell in chessCellList)
		//{
		//	if (chessCell.coord == coord)
		//	{
		//		chessCell.characterDataList.Add(characterData);
		//		ResetChessBoard();
		//		break;
		//	}
		//}
	}

	//public void RemoveCharacterFromChessCell(CharacterData characterData)
	//{
	//	var chessBoardUIControl = GetTree().CurrentScene.GetNode<ChessBoardUIControl>("ChessBoard");
	//	foreach (var chessCellRow in chessCellList)
	//	{
	//		foreach(var chessCell in chessCellRow)
	//		{
	//			chessCell.chessCellUIControl.RemoveAllCharacterDisplay();
	//		}
	//	}
	//}

	public void ResetChessBoard()
	{
		foreach(var chessCellRow in chessCellList)
		{
			foreach(var chessCell in chessCellRow)
			{
				chessCell.chessCellUIControl.RemoveAllCharacterDisplay();
			}
		}
	}
}

public partial class ChessCell
{
	public Vector2I coord;
	//public List<CharacterData> characterDataList;
	public ChessCellUIControl chessCellUIControl;
	public ChessCell()
	{
		//characterDataList = new List<CharacterData>();
	}
	//public void SetCharacterToChessCell(CharacterData characterData)
	//{
	//	characterDataList.Add(characterData);
	//	characterData.coord = coord;

	//}
	//public void RemoveCharacterFromChessCell(CharacterData characterData)
	//{
	//	characterDataList.Remove(characterData);
	//	characterData.coord = Vector2I.Zero;
	//}
}
