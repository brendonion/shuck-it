using Godot;
using System;

public class Game : Node2D {

    public int speed = 4;

    public KinematicBody2D Cob;

    public Vector2 screenSize;
    public Vector2 screenCenter;

    public override void _Ready() {
        // Get screen size
        this.screenSize   = GetViewport().GetVisibleRect().Size;
        this.screenCenter = new Vector2(this.screenSize.x / 2, this.screenSize.y / 2);

        // Find Cob
        this.Cob = (KinematicBody2D) FindNode("Cob");
    }

    public override void _PhysicsProcess(float delta) {
        if (this.Cob.Position.y <= this.screenCenter.y) {
            float t = delta * speed;
            this.Cob.Position = this.Cob.Position.LinearInterpolate(this.screenCenter, t);
        }
    }
}
