using Godot;
using System.Collections.Generic;

public partial class PlayerChoseListPanelControl : Panel
{
    [Export] public PackedScene choseButtonPrefab;
    [Export] public VBoxContainer allChoseContent;
    public void SetChoseListPanel(List<PlayerCommandData> playerCommandDataList)
    {
        PubTool.instance.ClearChildren(allChoseContent);
        foreach(PlayerCommandData playerCommandData in playerCommandDataList)
        {
            Button choseButton = (Button)choseButtonPrefab.Instantiate();
            allChoseContent.AddChild(choseButton);
            choseButton.Text = playerCommandData.commandName;
        }
    }
}
