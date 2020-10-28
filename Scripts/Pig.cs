using Godot;
using System;

public class Pig : Node2D {

    public float speed = 125f;

    public int patrolIndex = 0;

    public Game Game;

    public KinematicBody2D body;

    public AnimatedSprite animatedSprite;

    public Path2D[] paths;

    public Path2D patrolPath;

    public Vector2[] patrolPoints;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        GD.Randomize();

        Path2D[] paths = {
            (Path2D) FindNode("PigPath1"), // Bottom Left
            (Path2D) FindNode("PigPath2"), // Bottom Right
            (Path2D) FindNode("PigPath3"), // Top Left
            (Path2D) FindNode("PigPath4"), // Top Right
        };

        this.paths          = paths;
        this.patrolPath     = this.paths[(int) GD.RandRange(0, 4)];
        this.patrolPoints   = this.patrolPath.Curve.GetBakedPoints();
        this.body           = (KinematicBody2D) FindNode("KinematicBody2D");
        this.animatedSprite = (AnimatedSprite) this.body.FindNode("AnimatedSprite");

        // this.Game = (Game) GetTree().Root.GetChild(0);

        this.DetermineStartPosition();
    }

    public override void _PhysicsProcess(float delta) {
        Vector2 target   = this.patrolPoints[this.patrolIndex];
        Vector2 velocity = (target - this.body.Position).Normalized() * this.speed;
        this.body.MoveAndSlide(velocity);
        if (this.body.Position.DistanceTo(target) <= 2) {
            if (this.patrolIndex == this.patrolPoints.Length - 1) {
                this.patrolIndex = 0;
            } else {
                this.patrolIndex += 1;
            }
        }
    }

    public void DetermineStartPosition() {
        int index = Array.IndexOf(this.paths, this.patrolPath);
        this.body.Position = this.patrolPoints[0];
        this.animatedSprite.FlipH = (index == 0 || index == 2);
        this.animatedSprite.FlipV = (index == 2 || index == 3);
    }
}
