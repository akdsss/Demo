using Godot;
using System.Linq;
using System.Collections.Generic;

public partial class ChessCellUIControl : Control
{
    [Export] public TextureRect[] allCharacterPointArray;

    public override void _Ready()
    {
        RemoveAllCharacterDisplay();
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
        foreach (TextureRect textureRect in allCharacterPointArray)
        {
            textureRect.Visible = false;
        }
        //currentCharacterNum = 0;
    }
    public void RemoveCharacterDisplay(CharacterData characterData)
    {
        bool removed = false;
        Color color = Color.Color8(0, 0, 0);
        switch (characterData)
        {
            case PlayerData playerData:
                color = Color.FromHtml(Autoloads.playerHexColor);
                break;
            case EnemyData enemyData:
                color = Color.FromHtml(Autoloads.enemyHexColor);
                break;
        }
        foreach (TextureRect textureRect in allCharacterPointArray)
        {
            if(textureRect.Visible == true && textureRect.SelfModulate == color)
            {
                textureRect.Visible = false;
                removed = true;
                break;
            }
        }
        if (removed == false)
        {
            GD.PrintErr("RemoveCharacterDisplay Error");
        }
        //allCharacterPointArray[characterIdx].Visible = false;
        //currentCharacterNum--;
    }
    public void AddCharacterDisplay(CharacterData characterData)
    {
        Color color = Color.Color8(0,0,0);
        bool added = false;
        if (characterData is PlayerData)
        {
            color = Color.FromHtml(Autoloads.playerHexColor);
        }
        else if (characterData is EnemyData)
        {
            color = Color.FromHtml(Autoloads.enemyHexColor);
        }
        foreach (TextureRect textureRect in allCharacterPointArray)
        {
            if (textureRect.Visible == false)
            {
                textureRect.Visible = true;

                textureRect.SelfModulate = color;
                //currentCharacterNum++;
                added = true;
                break;
            }
        }
        if (added == false)
        {
            GD.PrintErr("AddCharacterDisplay Error");
        }
    }
}
