using Godot;
using System.Linq;
using System.Collections.Generic;

public partial class ChessCellUIControl : Control
{
    [Export] public TextureRect[] allCharacterPointArray;
    [Export] public Button chessCellButton;
    private Dictionary<CharacterData, TextureRect> characterDisplayMap = new();

    public override void _Ready()
    {
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
        foreach (TextureRect textureRect in allCharacterPointArray)
        {
            if (textureRect == null)
            {
                continue;
            }
            textureRect.Visible = false;
            textureRect.Texture = null;
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
                textureRect.Visible = false;
                textureRect.Texture = null;
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
            if (textureRect.Visible == false)
            {
                textureRect.Visible = true;
                textureRect.Texture = characterData.characterHeadImage;
                characterDisplayMap[characterData] = textureRect;
                characterData.AreaAnchorIndex = i;
                return i;
            }
        }
        GD.PrintErr("AddCharacterDisplay Error: 无可用显示位置");
        return -1;
    }
}
