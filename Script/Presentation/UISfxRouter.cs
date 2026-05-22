using Godot;
using System.Collections.Generic;

public partial class UISfxRouter : Node
{
    [Export] public AudioStream HoverStream;
    [Export] public AudioStream ClickStream;
    [Export] public AudioStream ConfirmStream;
    [Export] public AudioStream PhaseStream;

    [Signal] public delegate void UiSfxRequestedEventHandler(string cueId);

    private readonly HashSet<ulong> registeredButtonIds = new();
    private AudioStreamPlayer player;

    public override void _Ready()
    {
        player = GetNodeOrNull<AudioStreamPlayer>("AudioStreamPlayer");
        if (player == null)
        {
            player = new AudioStreamPlayer
            {
                Name = "AudioStreamPlayer"
            };
            AddChild(player);
        }
    }

    public void RegisterButton(BaseButton button)
    {
        if (button == null)
        {
            return;
        }

        ulong instanceId = button.GetInstanceId();
        if (!registeredButtonIds.Add(instanceId))
        {
            return;
        }

        button.MouseEntered += PlayHover;
        button.Pressed += PlayClick;
    }

    public void PlayHover()
    {
        Play("hover");
    }

    public void PlayClick()
    {
        Play("click");
    }

    public void PlayConfirm()
    {
        Play("confirm");
    }

    public void PlayPhaseCue()
    {
        Play("phase");
    }

    public void Play(string cueId)
    {
        EmitSignal("UiSfxRequested", cueId);
        AudioStream stream = cueId switch
        {
            "hover" => HoverStream,
            "click" => ClickStream,
            "confirm" => ConfirmStream,
            "phase" => PhaseStream,
            _ => null
        };

        if (stream == null)
        {
            return;
        }

        player ??= GetNodeOrNull<AudioStreamPlayer>("AudioStreamPlayer");
        if (player == null)
        {
            return;
        }

        player.Stream = stream;
        player.Play();
    }
}
