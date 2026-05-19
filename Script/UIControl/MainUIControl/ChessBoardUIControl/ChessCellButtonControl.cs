using Godot;

public partial class ChessCellButtonControl : Button
{
    // public ChessCellUIControl chessCellUIControl;
    [Export] public Vector2I coord;
    public void ChessCellButtonClick()
    {
        Autoloads.gd_ChessBoard.chessBoardUIControl.HideAllChessCellButton();
        Autoloads.sceneSingleton.battleManager.eventManager.moveEventInfo.moveTargetCoord = coord;
    }
}