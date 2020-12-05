using Godot;
using System;

public class Game : Node2D {

    // Round numbers that trigger events
    public enum Events {
        START      = 0,
        HUSKS      = 3,
        BAR        = 5,
        FLIES      = 10,
        PIG        = 25,
        SPEED_UP   = 33,
        SPEED_UP_2 = 66,
        SPEED_UP_3 = 99,
        FACES      = 250,
        TINDER     = 251,
        FINALE     = 500,
    }

    public Events currentEvent = Events.START;

    public int round    = 0; // Round number
    public int husks    = 3; // Husk count
    public int maxHusks = 5; // Max husks
    
    public bool ready       = false; // Player readied flag
    public bool initialized = false; // Cob initialized flag
    public bool spawnFlies  = false; // Fly spawn flag
    public bool spawnPigs   = false; // Pig spawn flag
    public bool spawnFaces  = false; // Cob face spawn flag

    public float flySpeed    = 125f; // Fly speed
    public float pigSpeed    = 125f; // Pig speed
    public float kernelSpeed = 150f; // Kernel speed
    public float timeOut     = 10f;  // Timer bar duration

    public float timeSec    = 1f; // How long it takes for the Cob to get to screen center
    public float timePassed = 0f; // Time passed since initialization

    public Vector2 screenSize;
    public Vector2 screenCenter;

    public Cob Cob;
    public TimerBar TimerBar;
    public Score Score;
    public DialogScreen DialogScreen;
    public Control ReadyScreen;

    public PackedScene KernelScene     = (PackedScene) ResourceLoader.Load("res://Scenes/Kernel.tscn");
    public PackedScene FlyScene        = (PackedScene) ResourceLoader.Load("res://Scenes/Fly.tscn");
    public PackedScene PigScene        = (PackedScene) ResourceLoader.Load("res://Scenes/Pig.tscn");
    public PackedScene RightHuskScene  = (PackedScene) ResourceLoader.Load("res://Scenes/RightHusk.tscn");
    public PackedScene LeftHuskScene   = (PackedScene) ResourceLoader.Load("res://Scenes/LeftHusk.tscn");
    public PackedScene MiddleHuskScene = (PackedScene) ResourceLoader.Load("res://Scenes/MiddleHusk.tscn");

    public Godot.Object admob;

    public SaveSystem SaveSystem;

    [Signal]
    public delegate void new_round(float duration);

    public override void _Ready() {
        // Randomize seed
        GD.Randomize();

         // Get singletons
        SaveSystem = (SaveSystem) FindNode("SaveSystem");

        // Set background
        ((TextureRect) FindNode("Background")).Texture = (Texture) ResourceLoader.Load($"res://Art/Unlockables/{SaveSystem.currentBackground}");

        // Get screen size
        this.screenSize   = GetViewport().GetVisibleRect().Size;
        this.screenCenter = new Vector2(this.screenSize.x / 2, this.screenSize.y / 1.85f);

        // Find scenes
        this.Cob          = (Cob) FindNode("Cob");
        this.TimerBar     = (TimerBar) FindNode("TimerBar");
        this.Score        = (Score) FindNode("Score");
        this.DialogScreen = (DialogScreen) FindNode("DialogScreen");
        this.ReadyScreen  = (Control) FindNode("ReadyScreen");

        // Connect custom signals
        this.Cob.Connect("swiped", this, "CreateNextRound");

        this.CreateCorn();

        if (OS.GetName() == "Android" && SaveSystem.enableAds) {
            this.admob = (Godot.Object) FindNode("AdMob");
            this.admob.Call("load_banner");
        }
    }

    public override void _PhysicsProcess(float delta) {
        if (this.ready && !this.initialized && HasNode("Cob") && !this.DialogScreen.Visible) {
            this.InitCornPosition(delta);
        }
    }

    public async void _OnReadyScreenGuiInput(InputEvent @event) {
        if (@event.IsActionPressed("ui_touch")) {
            ((AnimatedSprite) this.ReadyScreen.FindNode("AnimatedSprite")).Visible = false;
            ((RichTextLabel) this.ReadyScreen.FindNode("RichTextLabel")).BbcodeText = "[shake amp=10 freq=2][center]Shuck It![/center][/shake]";
            await ToSignal(GetTree().CreateTimer(1.5f), "timeout");
            this.ReadyScreen.QueueFree();
            this.Score.Visible = true;
            this.ready = true;
            if (OS.GetName() == "Android" && SaveSystem.enableAds) {
                this.admob.Call("hide_banner");
            }
        }
    }

    public void InitCornPosition(float delta) {
        if (this.Cob.Position != this.screenCenter) {
            this.timePassed  += delta;
            this.Cob.Position = this.Cob.Position.LinearInterpolate(this.screenCenter, this.timePassed / this.timeSec);
        } else {
            this.initialized = true;
            this.timePassed  = 0f;
            EmitSignal(nameof(new_round), this.timeOut);
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

    public async void CreateKernel() {
        if (this.round == (int) Events.FINALE) return;

        int num;
        if (this.round > (int) Events.SPEED_UP_3) {
            num = (int) GD.RandRange(1, 6); // 1 in 5
        } else {
            num = (int) GD.RandRange(1, 11); // 1 in 10
        }
        // Spawn a kernel if num equals 1
        if (num == 1) {
            // Wait 1 - 3 seconds before spawning
            float waitTime = (float) GD.RandRange(1, 4);
            await ToSignal(GetTree().CreateTimer(waitTime, false), "timeout");
            // Spawn kernel and set it's speed
            Kernel kernel = (Kernel) KernelScene.Instance();
            kernel.speed  = this.kernelSpeed;
            AddChild(kernel);
        }
    }

    public async void CreateFlies() {
        int num;
        if (this.round == (int) Events.FLIES) {
            num = 1;
        } else if (this.round <= (int) Events.FLIES + 5) {
            num = (int) GD.RandRange(1, 3); // 1 - 2 flies
        } else if (this.round <= (int) Events.FLIES + 10) {
            num = (int) GD.RandRange(2, 5); // 2 - 4 flies
        } else if (this.round <= (int) Events.FLIES + 25) {
            num = (int) GD.RandRange(3, 5); // 3 - 4 flies
        } else if (this.round <= (int) Events.FLIES + 40) {
            num = (int) GD.RandRange(3, 6); // 3 - 5 flies
        } else if (this.round <= (int) Events.FLIES + 60) {
            num = (int) GD.RandRange(4, 7); // 4 - 6 flies
        } else if (this.round <= (int) Events.FLIES + 80) {
            num = (int) GD.RandRange(4, 8); // 4 - 7 flies
        } else if (this.round <= (int) Events.FLIES + 100) {
            num = (int) GD.RandRange(5, 9); // 5 - 8 flies
        } else {
            num = (int) GD.RandRange(7, 11); // 7 - 10 flies
        }

        for (int i = 0; i < num; i++) {
            // Spawn fly, set it's speed, and stop their process
            Fly fly   = (Fly) FlyScene.Instance();
            fly.speed = this.flySpeed;
            fly.SetPhysicsProcess(false);
            AddChild(fly);

            // Space them out between 0.25 - 0.5 seconds
            await ToSignal(GetTree().CreateTimer((float) GD.RandRange(0.25f, 0.5f), false), "timeout");

            // Start the fly's process again
            fly.SetPhysicsProcess(true);
        }
    }

    public void CreatePigs() {
        int num;
        if (this.round == (int) Events.PIG) {
            num = 1; // Guarantee a pig to spawn
        } else if (this.round < (int) Events.SPEED_UP_3) {
            num = (int) GD.RandRange(1, 5); // 1 in 4
        } else {
            num = (int) GD.RandRange(1, 4); // 1 in 3
        }
        // Spawn pig and set it's speed if num equals 1
        if (num == 1) {
            Pig pig   = (Pig) PigScene.Instance();
            pig.speed = this.pigSpeed;
            AddChild(pig);
        }
    }

    public void CreateFaces() {
        if (this.round == (int) Events.FINALE) {
            this.Cob.face.Play("finale_face");
            this.Cob.face.Stop();
            this.Cob.face.Frame = 0;
            this.Cob.isFinale   = true;
        } else {
            int rand    = (int) GD.RandRange(0, 2); // 0 - 1
            int num     = (int) GD.RandRange(1, 6); // 1 - 5
            string anim = (rand == 1) ? "good_face_" : "bad_face_";
            this.Cob.face.Play(anim + num);
        }
    }

    public void CreateNextRound(int point = 0) {
        this.round++;

        this.HandleEvent();

        this.CreateCorn();
        this.CreateKernel();
        if (this.spawnFlies) this.CreateFlies();
        if (this.spawnPigs)  this.CreatePigs();
        if (this.spawnFaces) this.CreateFaces();

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
                this.TimerBar.Visible = true;
                break;
            case Events.FLIES:
                this.spawnFlies = true;
                break;
            case Events.PIG:
                this.spawnPigs = true;
                break;
            case Events.SPEED_UP:
                this.flySpeed    = 150f;
                // this.timeOut  = 9f;
                break;
            case Events.SPEED_UP_2:
                this.pigSpeed    = 150f;
                this.flySpeed    = 175f;
                this.kernelSpeed = 175f;
                // this.timeOut  = 8f;
                break;
            case Events.SPEED_UP_3:
                this.pigSpeed    = 175f;
                this.flySpeed    = 200f;
                this.kernelSpeed = 200f;
                // this.timeOut  = 7f;
                break;
            case Events.FACES:
                // Hide everything else for dramatic effect
                this.maxHusks         = 10;
                this.husks            = 10;
                this.spawnFlies       = false;
                this.spawnPigs        = false;
                this.TimerBar.Visible = false;
                // Show faces
                this.Cob.face.Visible = true;
                this.spawnFaces       = true;
                break;
            case Events.TINDER:
                // Faces still visible, revert back everything else, but now it's tinder
                this.maxHusks         = 5;
                this.husks            = 5;
                this.spawnFlies       = true;
                this.spawnPigs        = true;
                this.TimerBar.Visible = true;
                break;
            case Events.FINALE:
                // Saves bestScore and kernels
                if (this.Score.points > SaveSystem.bestScore) SaveSystem.bestScore = this.Score.points;
                SaveSystem.Save();
                this.maxHusks             = 10;
                this.husks                = 10;
                this.spawnFlies           = false;
                this.spawnPigs            = false;
                this.TimerBar.Visible     = false;
                this.Score.Visible        = false;
                this.DialogScreen.Visible = true;
                break;
            default:
                break;
        }
    }
}
