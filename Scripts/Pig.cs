using Godot;

public class Pig : Node2D {

    public Vector2 startPos;

    public KinematicBody2D body;

    public AnimatedSprite animatedSprite;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        GD.Randomize();

        this.body           = (KinematicBody2D) FindNode("KinematicBody2D");
        this.animatedSprite = (AnimatedSprite) this.body.FindNode("AnimatedSprite");
        this.startPos       = this.body.Position;

        int num = (int) GD.RandRange(0, 2);
        // Spawn bottom left or bottom right
        if (num == 0) {
            // this
        }

    }

    public override void _PhysicsProcess(float delta) {

    }
}
