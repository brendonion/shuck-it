using Godot;

public class Home : Node2D {

    public void _OnPlayPressed() {
        GetTree().ChangeScene("res://Scenes/Game.tscn");
    }
}
