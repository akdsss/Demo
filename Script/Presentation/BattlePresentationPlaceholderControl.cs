using Godot;
using System.Threading.Tasks;

public partial class BattlePresentationPlaceholderControl : Control
{
    private PanelContainer card;
    private TextureRect textureRect;
    private Label label;

    public override void _Ready()
    {
        BuildOverlay();
        Visible = false;
    }

    public async Task PlayAsync(CombatEvent combatEvent)
    {
        BuildOverlay();
        textureRect.Texture = BattleAssetCatalog.GetPresentationTexture(combatEvent?.EventType ?? CombatEventType.None);
        label.Text = CombatEventLogFormatter.Format(combatEvent);
        Visible = true;
        await ToSignal(GetTree().CreateTimer(0.25), "timeout");
        Visible = false;
    }

    private void BuildOverlay()
    {
        if (card != null && card.GetParent() != null)
        {
            return;
        }

        SetAnchorsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;

        ColorRect backdrop = new()
        {
            Name = "Backdrop",
            Color = new Color(0, 0, 0, 0.22f),
            MouseFilter = MouseFilterEnum.Ignore
        };
        backdrop.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(backdrop);

        card = new PanelContainer
        {
            Name = "PresentationCard",
            MouseFilter = MouseFilterEnum.Ignore
        };
        card.AnchorLeft = 0.34f;
        card.AnchorRight = 0.66f;
        card.AnchorTop = 0.28f;
        card.AnchorBottom = 0.62f;
        AddChild(card);

        VBoxContainer content = new()
        {
            Name = "Content"
        };
        card.AddChild(content);

        textureRect = new TextureRect
        {
            Name = "PlaceholderTexture",
            CustomMinimumSize = new Vector2(260, 130),
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore
        };
        content.AddChild(textureRect);

        label = new Label
        {
            Name = "PresentationLabel",
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore
        };
        content.AddChild(label);
    }
}
