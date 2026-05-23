using Godot;

public partial class ChessCellButtonControl : Button
{
    // public ChessCellUIControl chessCellUIControl;
    [Export] public Vector2I coord;
    public void ChessCellButtonClick()
    {
        Autoloads.gd_ChessBoard.chessBoardUIControl.HideAllChessCellButton();
        CombatAreaId areaId = AreaDefinition.GetAreaIdForLegacyCoord(coord);
        EventManager eventManager = Autoloads.sceneSingleton.battleManager.eventManager;
        eventManager.currentTargetAreaId = areaId;
        eventManager.moveEventInfo ??= new MoveEventInfo();
        eventManager.moveEventInfo.moveTargetAreaId = areaId;
        eventManager.moveEventInfo.moveTargetCoord = AreaDefinition.GetLegacyCoordForAreaId(areaId);
        // Autoloads.sceneSingleton.cmdQueueUIControl.cmdQueueState = CmdQueueState.CMDSET;
        Autoloads.sceneSingleton.cmdQueueUIControl.SwitchOnPlayerCommandSet();
    }
}
