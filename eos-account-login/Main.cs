using Godot;
using System;

public class Main : VBoxContainer
{
    private GameState _state;
    private Label _userLabel;

    public override void _Ready()
    {
        _state = GetNode<GameState>("/root/GameState");
        GetNode("LoginButton").Connect("pressed", this, nameof(_on_Login_pressed));
        _userLabel = GetNode<Label>("UserLabel");
    }

    private void _on_Login_pressed()
    {
        GD.Print("Login pressed!");
        _userLabel.Text = "Logging in...";
        _state.Eos.Login(OnAccountLogin);
    }

    private void OnAccountLogin(string loginMessage)
    {
        _userLabel.Text = loginMessage;
    }
}
