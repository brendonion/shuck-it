using Godot;

public class Finale : Node2D {

    public override void _Ready() {
        AnimationPlayer animPlayer = (AnimationPlayer) FindNode("AnimationPlayer");
        animPlayer.Play("default");
    }
}
