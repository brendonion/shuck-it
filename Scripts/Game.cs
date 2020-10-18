using Godot;
using System;

public class Game : Node2D {

    // Round numbers that trigger events
    public enum Events {
        START  = 1,
        HUSKS  = 3,
        BAR    = 5,
        SLIDER = 10,
        FLIES  = 20,
        FACES  = 50,
        FINALE = 100,
    }

    public Events currentEvent = Events.START;

    public int round    = 1; // Round number
    public int husks    = 3; // Husk count
    public int maxHusks = 5;
    
    public bool initialized = false; // Cob initialized flag
    public bool spawnFlies  = false;

    public float timeSec    = 1f; // How long it takes for the Cob to get to screen center
    public float timePassed = 0f; // Time passed since initialization

    public Vector2 screenSize;
    public Vector2 screenCenter;

    public Cob Cob;
    public RhythmBar RhythmBar;

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
        this.Cob       = (Cob) FindNode("Cob");
        this.RhythmBar = (RhythmBar) FindNode("RhythmBar");

        // Connect custom signals
        this.Cob.Connect("swiped", this, "CreateNextRound");

        this.CreateCorn();
    }

    public override void _PhysicsProcess(float delta) {
        if (!this.initialized && HasNode("Cob")) {
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

    public void CreateCorn() {
        PackedScene[] huskArr = {(PackedScene) MiddleHuskScene, (PackedScene) LeftHuskScene, (PackedScene) RightHuskScene};
        for (int i = 1; i <= this.husks; i++) {
            // Guarantee middle husk is last if huskCount is >= 5
            if (i == 1 && this.husks == this.maxHusks) {
                this.Cob.husks.AddChild(huskArr[0].Instance());
                continue;
            }
            // Guarantee a middle, left, and right husk to spawn
            if (i == this.husks) {
                this.Cob.husks.AddChild(huskArr[0].Instance());
            } else if (i == this.husks - 1) {
                this.Cob.husks.AddChild(huskArr[1].Instance());
            } else if (i == this.husks - 2) {
                this.Cob.husks.AddChild(huskArr[2].Instance());
            } else {
                // Spawn a random husk
                this.Cob.husks.AddChild(huskArr[(int) GD.RandRange(0, 3)].Instance());
            }
        }
    }

    public async void CreateFlies() {
        int num;
        if (this.round == (int) Events.FLIES) {
            num = 1;
        } else if (this.round <= (int) Events.FLIES + 3) {
            num = (int) GD.RandRange(1, 3); // 1 - 2 flies
        } else {
            num = (int) GD.RandRange(1, 4); // 1 - 3 flies
        }
        for (int i = 0; i < num; i++) {
            // Space out each fly
            await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
            AddChild(FlyScene.Instance());
        }
        EmitSignal(nameof(fly_spawned), 5f); // TODO :: Remove duration param
    }

    public void CreateNextRound(int point = 0) {
        this.round++;

        this.HandleEvent();

        this.CreateCorn();
        if (this.spawnFlies) this.CreateFlies();

        this.initialized = false;
    }

    public void HandleEvent() {
        // Convert Events enum to array of ints
        int[] events = Array.ConvertAll((int[]) Enum.GetValues(typeof(Events)), Convert.ToInt32);

        // Set currentEvent if events array includes round
        if (Array.IndexOf(events, this.round) != -1) {
            this.currentEvent = (Events) this.round;
        };

        switch (this.currentEvent) {
            case Events.START:
                break;
            case Events.HUSKS:
                if (this.husks < this.maxHusks) {
                    this.husks += 1;
                }
                break;
            case Events.BAR:
                this.RhythmBar.Visible = true;
                break;
            case Events.SLIDER:
                this.RhythmBar.slider.Visible = true;
                this.RhythmBar.arrow.Visible  = true;
                break;
            case Events.FLIES:
                this.spawnFlies = true;
                break;
            case Events.FACES:
                GD.Print("ADD FACES");
                break;
            case Events.FINALE:
                GD.Print("FINALE");
                break;

        }
    }
}
