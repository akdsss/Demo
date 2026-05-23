using Godot;
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
					coord = new Vector2I(i, j),
					areaId = AreaDefinition.GetAreaIdForLegacyCoord(new Vector2I(i, j))
				});
			}
			chessCellList.Add(row);
		}
	}

	public override void _Ready()
	{
		Autoloads.gd_ChessBoard = this;
	}

	public void BindAreaAnchors(ChessCellUIControl[] areaAnchors)
	{
		if (areaAnchors == null)
		{
			return;
		}

		int anchorIndex = 0;
		foreach (List<ChessCell> row in chessCellList)
		{
			foreach (ChessCell chessCell in row)
			{
				if (anchorIndex >= areaAnchors.Length)
				{
					GD.PrintErr("区域锚点数量不足，部分区域无法显示角色。");
					return;
				}

				chessCell.chessCellUIControl = areaAnchors[anchorIndex];
				anchorIndex++;
			}
		}
	}

	public void MoveCharacter(CharacterData characterData, Vector2I targetCoord)
	{
		MoveCharacterToArea(characterData, AreaDefinition.GetAreaIdForLegacyCoord(targetCoord));
	}
	public void SetCharacterToChessCell(CharacterData characterData, Vector2I coord)
	{
		SetCharacterToArea(characterData, AreaDefinition.GetAreaIdForLegacyCoord(coord));
	}

	public void RemoveCharacterFromChessCell(CharacterData characterData)
	{
		RemoveCharacterFromArea(characterData);
	}

	public void MoveCharacterToArea(CharacterData characterData, CombatAreaId targetAreaId)
	{
		RemoveCharacterFromArea(characterData);
		SetCharacterToArea(characterData, targetAreaId);
	}

	public void SetCharacterToArea(CharacterData characterData, CombatAreaId areaId)
	{
		if (characterData == null)
		{
			return;
		}

		CombatAreaId resolvedAreaId = areaId == CombatAreaId.Unknown
			? characterData.ResolveCurrentAreaId()
			: areaId;
		if (resolvedAreaId == CombatAreaId.Unknown)
		{
			GD.PrintErr($"SetCharacterToArea Error: {characterData.characterName} 缺少有效区域");
			return;
		}

		characterData.SetCurrentArea(resolvedAreaId);
		ChessCell targetChessCell = GetChessCellByAreaId(resolvedAreaId);
		if (targetChessCell?.chessCellUIControl == null)
		{
			GD.PrintErr($"SetCharacterToArea Error: 找不到区域 {resolvedAreaId} 的显示锚点");
			return;
		}

		targetChessCell.chessCellUIControl.AddCharacterDisplay(characterData);
	}

	public void RemoveCharacterFromArea(CharacterData characterData)
	{
		if (characterData == null)
		{
			return;
		}

		CombatAreaId areaId = characterData.ResolveCurrentAreaId();
		ChessCell targetChessCell = GetChessCellByAreaId(areaId) ?? GetChessCellByLegacyCoord(characterData.coord);
		if (targetChessCell?.chessCellUIControl == null)
		{
			return;
		}

		targetChessCell.chessCellUIControl.RemoveCharacterDisplay(characterData);
	}

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

	private ChessCell GetChessCellByAreaId(CombatAreaId areaId)
	{
		foreach (List<ChessCell> row in chessCellList)
		{
			foreach (ChessCell chessCell in row)
			{
				if (chessCell.areaId == areaId)
				{
					return chessCell;
				}
			}
		}

		return null;
	}

	private ChessCell GetChessCellByLegacyCoord(Vector2I coord)
	{
		if (coord.X < 0 || coord.X >= chessCellList.Count)
		{
			return null;
		}

		if (coord.Y < 0 || coord.Y >= chessCellList[coord.X].Count)
		{
			return null;
		}

		return chessCellList[coord.X][coord.Y];
	}
}

public partial class ChessCell
{
	public Vector2I coord;
	public CombatAreaId areaId;
	public ChessCellUIControl chessCellUIControl;
}
