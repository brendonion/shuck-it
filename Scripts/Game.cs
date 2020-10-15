using Godot;
using System;

public class Game : Node2D {

    public int round = 1; // Round number
    public int husks = 3; // Husk count

    public bool initialized = false; // Cob initialized flag

    public float timeSec    = 1f; // How long it takes for the Cob to get to screen center
    public float timePassed = 0f; // Time passed since initialization

    public Vector2 screenSize;
    public Vector2 screenCenter;

    public Cob Cob;

    public Score Score;

    public PackedScene FlyScene        = (PackedScene) ResourceLoader.Load("res://Scenes/Fly.tscn");
    public PackedScene RightHuskScene  = (PackedScene) ResourceLoader.Load("res://Scenes/RightHusk.tscn");
    public PackedScene LeftHuskScene   = (PackedScene) ResourceLoader.Load("res://Scenes/LeftHusk.tscn");
    public PackedScene MiddleHuskScene = (PackedScene) ResourceLoader.Load("res://Scenes/MiddleHusk.tscn");

    [Signal]
    public delegate void fly_spawned();

    [Signal]
    public delegate void fly_destroyed();

    public override void _Ready() {
        // Randomize seed
        GD.Randomize();

        // Get screen size
        this.screenSize   = GetViewport().GetVisibleRect().Size;
        this.screenCenter = new Vector2(this.screenSize.x / 2, this.screenSize.y / 1.85f);

        // Find scenes
        this.Cob = (Cob) FindNode("Cob");

        // Connect custom signals
        this.Cob.Connect("needs_reinitialization", this, "CreateNextRound");

        this.CreateCorn(this.husks);
    }

    public override void _PhysicsProcess(float delta) {
        if (!this.initialized && this.HasNode("Cob")) {
            this.InitCornPosition(delta);
        }
    }

    public void InitCornPosition(float delta) {
        if (this.Cob.Position != this.screenCenter) {
            this.timePassed  += delta;
            this.Cob.Position = this.Cob.Position.LinearInterpolate(this.screenCenter, this.timePassed / this.timeSec);
        } else {
            this.initialized = true;
            this.timePassed  = 0f;
        }
    }

    public void CreateCorn(int huskCount) {
        PackedScene[] huskArr = {(PackedScene) MiddleHuskScene, (PackedScene) LeftHuskScene, (PackedScene) RightHuskScene};
        for (int i = 1; i <= huskCount; i++) {
            if (i == huskCount) {
                this.Cob.husks.AddChild(huskArr[0].Instance());
            } else if (i == huskCount - 1) {
                this.Cob.husks.AddChild(huskArr[1].Instance());
            } else if (i == huskCount - 2) {
                this.Cob.husks.AddChild(huskArr[2].Instance());
            } else {
                this.Cob.husks.AddChild(huskArr[(int) GD.RandRange(0, 3)].Instance());
            }
        }
    }

    public void CreateFly() {
        int num = (int) GD.RandRange(0, 2);
        if (num > 0) {
            AddChild(FlyScene.Instance());
            EmitSignal(nameof(fly_spawned));
        }
    }

    public void CreateNextRound() {
        // Determine husk count based on round count
        if (this.husks < 5) this.husks += this.round;
        this.round++;

        this.CreateCorn(this.husks);
        this.CreateFly();

        this.initialized = false;
    }
}
