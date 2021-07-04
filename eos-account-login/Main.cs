using Godot;
using System;

public class Main : VBoxContainer
{
    public override void _Ready()
    {
        GetNode("LoginButton").Connect("pressed", this, nameof(_on_Login_pressed));
    }

    private void _on_Login_pressed()
    {
        GD.Print("Login pressed!");
    }
}
