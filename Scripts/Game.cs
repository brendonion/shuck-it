using Godot;
using System;

public class Game : Node2D {

    public int speed = 4;
    public bool initialized = false;

    public Vector2 screenSize;
    public Vector2 screenCenter;
    public Vector2 startPos;

    public KinematicBody2D Cob;

    public override void _Ready() {
        // Get screen size
        this.screenSize   = GetViewport().GetVisibleRect().Size;
        this.screenCenter = this.screenSize / 2;

        // Find Cob
        this.Cob      = (KinematicBody2D) FindNode("Cob");
        this.startPos = this.Cob.Position;
    }

    public override void _PhysicsProcess(float delta) {
        if (!this.initialized) {
            this.InitializeCorn(delta);
        } else {

        }
    }

    public void InitializeCorn(float delta) {
        if (this.Cob.Position != this.screenCenter) {
            float t = delta * speed;
            this.Cob.Position = this.Cob.Position.LinearInterpolate(this.screenCenter, t);
        } else {
            this.initialized = true;
        }
    }
}
