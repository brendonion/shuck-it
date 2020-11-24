using Godot;

public class Shop : Control {

    public override void _Ready() {}

    public void _OnBackPressed() {
        GetTree().ChangeScene("res://Scenes/Home.tscn");
    }
}
