using Godot;
using System.Linq;
using System.Collections.Generic;

[Tool]
public partial class ChessCellUIControl : Control
{
    [Export] public TextureRect[] allCharacterPointArray;
    [Export] public Button chessCellButton;
    [Export] public bool ShowEmptyAnchorsForDebug = false;
    [Export] public float CharacterAnchorDisplayScale = 1f;
    private Dictionary<CharacterData, TextureRect> characterDisplayMap = new();
    private readonly Dictionary<TextureRect, Texture2D> anchorDefaultTextureMap = new();
    private static ShaderMaterial circularAvatarMaterial;

    public override void _Ready()
    {
        CacheDefaultAnchorTextures();
        ApplyCharacterAnchorDisplayScale();
        if (Engine.IsEditorHint())
        {
            RefreshEmptyAnchorDisplays();
            return;
        }

        RemoveAllCharacterDisplay();
        if (chessCellButton != null)
        {
            chessCellButton.Disabled = true;
            chessCellButton.Visible = false;
            chessCellButton.MouseFilter = Control.MouseFilterEnum.Ignore;
        }
        // ((ChessCellButtonControl)chessCellButton).chessCellUIControl = this;
    }
    //public int currentCharacterNum;

    //public void UpdateChessCellDisplay(List<CharacterData> characterDataList)
    //{
    //    int characterIdx = 0;
    //    foreach(CharacterData characterData in characterDataList)
    //    {
    //        Color color = Color.Color8(0, 0, 0);
    //        switch (characterData)
    //        {
    //            case PlayerData playerData:
    //                color = Color.FromHtml(Autoloads.playerHexColor);
    //                break;
    //            case EnemyData enemyData:
    //                color = Color.FromHtml(Autoloads.enemyHexColor);
    //                break;
    //        }
    //        allCharacterPointArray[characterIdx].Visible = true;
    //        allCharacterPointArray[characterIdx].SelfModulate = color;
    //        characterIdx++;
    //    }
    //    for (int i = characterIdx; i < allCharacterPointArray.Length; i++)
    //    {

    //    }
    //}
    public void RemoveAllCharacterDisplay()
    {
        foreach (CharacterData characterData in characterDisplayMap.Keys)
        {
            if (characterData != null)
            {
                characterData.AreaAnchorIndex = -1;
            }
        }
        characterDisplayMap.Clear();
        foreach (TextureRect textureRect in allCharacterPointArray ?? System.Array.Empty<TextureRect>())
        {
            if (textureRect == null)
            {
                continue;
            }
            ResetEmptyAnchorDisplay(textureRect);
        }
    }

    public void RemoveCharacterDisplay(CharacterData characterData)
    {
        if (characterData == null)
        {
            return;
        }

        if (characterDisplayMap.TryGetValue(characterData, out TextureRect textureRect))
        {
            if (textureRect != null)
            {
                ResetEmptyAnchorDisplay(textureRect);
            }
            characterDisplayMap.Remove(characterData);
            characterData.AreaAnchorIndex = -1;
        }
        else
        {
            GD.PrintErr("RemoveCharacterDisplay Error: 角色未找到");
        }
    }

    public int AddCharacterDisplay(CharacterData characterData)
    {
        if (characterData == null)
        {
            return -1;
        }

        if (characterDisplayMap.ContainsKey(characterData))
        {
            GD.PrintErr("AddCharacterDisplay Error: 角色已存在");
            return characterData.AreaAnchorIndex;
        }

        for (int i = 0; i < allCharacterPointArray.Length; i++)
        {
            TextureRect textureRect = allCharacterPointArray[i];
            if (textureRect == null)
            {
                continue;
            }
            if (!characterDisplayMap.ContainsValue(textureRect))
            {
                textureRect.Visible = true;
                textureRect.Texture = characterData.characterHeadImage;
                textureRect.Material = GetCircularAvatarMaterial();
                characterDisplayMap[characterData] = textureRect;
                characterData.AreaAnchorIndex = i;
                return i;
            }
        }
        GD.PrintErr("AddCharacterDisplay Error: 无可用显示位置");
        return -1;
    }

    public void SetEmptyAnchorsForDebug(bool visible)
    {
        ShowEmptyAnchorsForDebug = visible;
        RefreshEmptyAnchorDisplays();
    }

    public void SetCharacterAnchorDisplayScale(float scale)
    {
        CharacterAnchorDisplayScale = Mathf.Max(0.01f, scale);
        ApplyCharacterAnchorDisplayScale();
    }

    private void ApplyCharacterAnchorDisplayScale()
    {
        foreach (TextureRect textureRect in allCharacterPointArray ?? System.Array.Empty<TextureRect>())
        {
            if (textureRect == null)
            {
                continue;
            }

            textureRect.PivotOffset = textureRect.Size * 0.5f;
            textureRect.Scale = Vector2.One * CharacterAnchorDisplayScale;
        }
    }

    private void RefreshEmptyAnchorDisplays()
    {
        foreach (TextureRect textureRect in allCharacterPointArray ?? System.Array.Empty<TextureRect>())
        {
            if (textureRect == null || characterDisplayMap.ContainsValue(textureRect))
            {
                continue;
            }

            ResetEmptyAnchorDisplay(textureRect);
        }
    }

    private void ResetEmptyAnchorDisplay(TextureRect textureRect)
    {
        if (textureRect == null)
        {
            return;
        }

        textureRect.Visible = ShowEmptyAnchorsForDebug;
        textureRect.Material = null;
        textureRect.Texture = ShowEmptyAnchorsForDebug
            ? GetAnchorDefaultTexture(textureRect)
            : null;
    }

    private void CacheDefaultAnchorTextures()
    {
        anchorDefaultTextureMap.Clear();
        foreach (TextureRect textureRect in allCharacterPointArray ?? System.Array.Empty<TextureRect>())
        {
            if (textureRect != null)
            {
                anchorDefaultTextureMap[textureRect] = textureRect.Texture;
            }
        }
    }

    private Texture2D GetAnchorDefaultTexture(TextureRect textureRect)
    {
        if (textureRect == null)
        {
            return null;
        }

        if (anchorDefaultTextureMap.TryGetValue(textureRect, out Texture2D texture) && texture != null)
        {
            return texture;
        }

        return textureRect.Texture;
    }

    private static ShaderMaterial GetCircularAvatarMaterial()
    {
        if (circularAvatarMaterial != null)
        {
            return circularAvatarMaterial;
        }

        Shader shader = new()
        {
            Code = @"
shader_type canvas_item;

void fragment() {
	vec2 centered_uv = UV - vec2(0.5);
	if (length(centered_uv) > 0.5) {
		discard;
	}
	COLOR = texture(TEXTURE, UV) * COLOR;
}
"
        };
        circularAvatarMaterial = new ShaderMaterial
        {
            Shader = shader
        };
        return circularAvatarMaterial;
    }
}
