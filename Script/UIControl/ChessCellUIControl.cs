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
        chessCellButton.Disabled = true;
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
        characterDisplayMap.Clear();
        foreach (TextureRect textureRect in allCharacterPointArray)
        {
            textureRect.Visible = false;
            textureRect.Texture = null;
        }
    }

    public void RemoveCharacterDisplay(CharacterData characterData)
    {
        if (characterDisplayMap.TryGetValue(characterData, out TextureRect textureRect))
        {
            textureRect.Visible = false;
            textureRect.Texture = null;
            characterDisplayMap.Remove(characterData);
        }
        else
        {
            GD.PrintErr("RemoveCharacterDisplay Error: 角色未找到");
        }
    }

    public void AddCharacterDisplay(CharacterData characterData)
    {
        if (characterDisplayMap.ContainsKey(characterData))
        {
            GD.PrintErr("AddCharacterDisplay Error: 角色已存在");
            return;
        }

        foreach (TextureRect textureRect in allCharacterPointArray)
        {
            if (textureRect.Visible == false)
            {
                textureRect.Visible = true;
                textureRect.Texture = characterData.characterHeadImage;
                characterDisplayMap[characterData] = textureRect;
                return;
            }
        }
        GD.PrintErr("AddCharacterDisplay Error: 无可用显示位置");
    }
}
