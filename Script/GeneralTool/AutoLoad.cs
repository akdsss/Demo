using Godot;

public static class Autoloads
{
	public static string playerHexColor = "#c1cfeb";
	public static string enemyHexColor = "#ebc1c1";
	private static ChessBoard _gd_ChessBoard;
	public static ChessBoard gd_ChessBoard
	{
		get
		{
			if (_gd_ChessBoard != null)
			{
				return _gd_ChessBoard;
			}

			if (Engine.GetMainLoop() is SceneTree st)
			{
				_gd_ChessBoard = st.Root.GetNodeOrNull<ChessBoard>("/root/gd_ChessBoard");
			}
			return _gd_ChessBoard;
		}
		set => _gd_ChessBoard = value;
	}
}
