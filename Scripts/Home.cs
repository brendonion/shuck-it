using Godot;

public class Home : Node2D {

    public void _OnPlayPressed() {
        GetTree().ChangeScene("res://Scenes/Game.tscn");
    }

    public void _OnFinalePressed() {
        GetTree().ChangeScene("res://Scenes/Finale.tscn");
    }
}
