using Godot;
using System.Linq;
using System.Collections.Generic;

[Tool]
public partial class ChessBoardUIControl : Sprite2D
{
	[Export] public ChessCellUIControl[] chessCellUIControlArray;
	[Export] public bool UseResponsiveLayout = false;
	[Export]
	public bool ShowEmptyCharacterAnchorsForDebug
	{
		get => showEmptyCharacterAnchorsForDebug;
		set
		{
			showEmptyCharacterAnchorsForDebug = value;
			if (IsInsideTree())
			{
				if (Engine.IsEditorHint())
				{
					ApplyEditorAnchorPreview(true);
				}
				else
				{
					ApplyEmptyCharacterAnchorsDebugVisibility();
				}
			}
		}
	}
	[Export]
	public float CharacterAnchorDisplayScale
	{
		get => characterAnchorDisplayScale;
		set
		{
			characterAnchorDisplayScale = Mathf.Max(0.01f, value);
			if (IsInsideTree())
			{
				if (Engine.IsEditorHint())
				{
					ApplyEditorAnchorPreview(true);
				}
				else
				{
					ApplyCharacterAnchorDisplayScale();
				}
			}
		}
	}
	[Export] public Vector2 ResponsiveBoardFillRatio = new(0.96f, 0.9f);
	[Export] public Vector2 ResponsiveBoardPositionRatio = new(0.5f, 0.48f);
	private bool showEmptyCharacterAnchorsForDebug = false;
	private float characterAnchorDisplayScale = 2f;
	private Vector2 lastParentSize = Vector2.Zero;
	private Vector2 lastTextureSize = Vector2.Zero;
	private float lastEditorAnchorDisplayScale = -1f;
	private bool lastEditorShowEmptyCharacterAnchorsForDebug;
	private bool hasAppliedEditorAnchorPreview;

	public override void _Ready()
	{
		if (Engine.IsEditorHint())
		{
			SetProcess(true);
			ApplyEditorAnchorPreview(true);
			return;
		}

		// 注册到全局单例
		Autoloads.gd_ChessBoard.chessBoardUIControl = this;
		ApplyCharacterAnchorDisplayScale();
		ApplyEmptyCharacterAnchorsDebugVisibility();
		SetProcess(UseResponsiveLayout);
		if (UseResponsiveLayout)
		{
			UpdateResponsiveLayout();
		}
	}

	public override void _Process(double delta)
	{
		if (Engine.IsEditorHint())
		{
			ApplyEditorAnchorPreview(false);
			return;
		}

		if (!UseResponsiveLayout)
		{
			return;
		}

		UpdateResponsiveLayout();
	}

	private void UpdateResponsiveLayout()
	{
		if (Texture == null || GetParent() is not Control parentControl)
		{
			return;
		}

		Vector2 parentSize = parentControl.Size;
		Vector2 textureSize = Texture.GetSize();
		if (parentSize.X <= 0f || parentSize.Y <= 0f || textureSize.X <= 0f || textureSize.Y <= 0f)
		{
			return;
		}

		if (parentSize == lastParentSize && textureSize == lastTextureSize)
		{
			return;
		}

		lastParentSize = parentSize;
		lastTextureSize = textureSize;
		float scale = Mathf.Min(
			parentSize.X * ResponsiveBoardFillRatio.X / textureSize.X,
			parentSize.Y * ResponsiveBoardFillRatio.Y / textureSize.Y);
		Position = new Vector2(
			parentSize.X * ResponsiveBoardPositionRatio.X,
			parentSize.Y * ResponsiveBoardPositionRatio.Y);
		Scale = new Vector2(scale, scale);
	}

	public void ApplyEmptyCharacterAnchorsDebugVisibility()
	{
		foreach (ChessCellUIControl chessCellUIControl in chessCellUIControlArray ?? System.Array.Empty<ChessCellUIControl>())
		{
			chessCellUIControl?.SetEmptyAnchorsForDebug(ShowEmptyCharacterAnchorsForDebug);
		}
	}

	public void ApplyCharacterAnchorDisplayScale()
	{
		foreach (ChessCellUIControl chessCellUIControl in chessCellUIControlArray ?? System.Array.Empty<ChessCellUIControl>())
		{
			chessCellUIControl?.SetCharacterAnchorDisplayScale(CharacterAnchorDisplayScale);
		}
	}

	private void ApplyEditorAnchorPreview(bool force)
	{
		if (!force &&
			hasAppliedEditorAnchorPreview &&
			Mathf.IsEqualApprox(lastEditorAnchorDisplayScale, CharacterAnchorDisplayScale) &&
			lastEditorShowEmptyCharacterAnchorsForDebug == ShowEmptyCharacterAnchorsForDebug)
		{
			return;
		}

		ApplyCharacterAnchorDisplayScale();
		ApplyEmptyCharacterAnchorsDebugVisibility();
		lastEditorAnchorDisplayScale = CharacterAnchorDisplayScale;
		lastEditorShowEmptyCharacterAnchorsForDebug = ShowEmptyCharacterAnchorsForDebug;
		hasAppliedEditorAnchorPreview = true;
	}

	public void ShowAllChessCellButton()
	{
		foreach (var chessCellUIControl in chessCellUIControlArray)
		{
			if (chessCellUIControl?.chessCellButton == null)
			{
				continue;
			}
			chessCellUIControl.chessCellButton.Disabled = true;
			chessCellUIControl.chessCellButton.Visible = false;
			chessCellUIControl.chessCellButton.MouseFilter = Control.MouseFilterEnum.Ignore;
		}
	}
	public void HideAllChessCellButton()
	{
		foreach (var chessCellUIControl in chessCellUIControlArray)
		{
			if (chessCellUIControl?.chessCellButton == null)
			{
				continue;
			}
			chessCellUIControl.chessCellButton.Disabled = true;
			chessCellUIControl.chessCellButton.Visible = false;
			chessCellUIControl.chessCellButton.MouseFilter = Control.MouseFilterEnum.Ignore;
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
