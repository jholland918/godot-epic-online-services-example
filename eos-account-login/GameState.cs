using Godot;

// NOTE: This class is autoloaded (should be set in Project > Project Settings... > AutoLoad)
public class GameState : Godot.Node
{
    public Eos Eos { get; set; }

    private Timer _eosTimer;

    public override void _Ready()
    {
        GD.Print("_Ready...");

        _eosTimer = GetNode<Timer>("/root/VBoxContainer/EosTimer");
        _eosTimer.Connect("timeout", this, nameof(EosTick));

        Eos = new Eos();
        Eos.Initialize();
        _eosTimer.Start();
    }

    private void EosTick()
    {
        Eos.Tick();
    }

    public override void _Notification(int what)
    {
        if (what == MainLoop.NotificationWmQuitRequest)
        {
            _eosTimer.Stop();
            Eos.Shutdown();
        }
    }
}