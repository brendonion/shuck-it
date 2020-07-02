using Godot;
using System;

public class Game : Node2D {

    public float timeSec    = 2f;    // How long it takes for the Cob to get to screen center
    public float timePassed = 0f;    // Time passed since initialization
    public bool initialized = false; // Cob initialized flag

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
            this.timePassed += delta;
            this.Cob.Position = this.Cob.Position.LinearInterpolate(this.screenCenter, this.timePassed / this.timeSec);
        } else {
            this.initialized = true;
            this.timePassed  = 0f;
        }
    }
}
