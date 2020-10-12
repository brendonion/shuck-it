using Godot;
using System;

public class Cob : KinematicBody2D {

    public bool isDraggable = false;
    public bool isReleased  = false;
    public bool isFlickable = false;

    public float gravity = 200f;
    public float speed   = 500f;

    public Vector2 dragSpeed;
    public Vector2 velocity;
    public Vector2 startPos;

    public Game Game;

    public Sprite sprite;

    public Node2D pivot;
    public Node2D husks;

    public Texture goodCob = (Texture) ResourceLoader.Load("res://Art/GoodCob.png");
    public Texture badCob  = (Texture) ResourceLoader.Load("res://Art/BadCob.png");

    [Signal]
    public delegate void needs_reinitialization();

    [Signal]
    public delegate void needs_centering();

    [Signal]
    public delegate void score_changed(int point);

    [Signal]
    public delegate void missed();

    public override void _Ready() {
        // Randomize seed
        GD.Randomize();

        this.Game     = (Game) this.GetParent();
        this.husks    = (Node2D) FindNode("Husks");
        this.sprite   = (Sprite) FindNode("Sprite");
        this.startPos = this.Position;

        this.SetRandomTexture();
    }

    public override void _PhysicsProcess(float delta) {
        // Cob has no Husks, can be dragged
        if (!this.isDraggable) {
            int huskCount = this.husks.GetChildCount();
            if (huskCount == 0 || (huskCount == 1 && ((Husk) this.husks.GetChild(0)).Mode == RigidBody2D.ModeEnum.Rigid)) {
                this.isDraggable = true;
            }
        }

        // Cob released from drag, can be flung
        if (this.isReleased) {
            this.MoveAndSlide(new Vector2(this.dragSpeed.x * this.speed, 0), Vector2.Down);
            this.CheckSwipe();
        }
    }

    public override void _Input(InputEvent @event) {
        if (this.isFlickable) {
            if (@event.IsActionReleased("ui_touch")) {
                this.isReleased = true;
            }
        }
    }

    public void _OnCobInputEvent(Node viewport, InputEvent @event, int shapeIdx) {
        if (this.isDraggable) {
            // Go back to center if released untimely
            if (@event.IsActionReleased("ui_touch") && this.dragSpeed != Vector2.Zero) {
                this.dragSpeed   = Vector2.Zero;
                this.isFlickable = false;
                this.isReleased  = false;
                EmitSignal(nameof(needs_centering));
            } else if (@event is InputEventScreenDrag eventDrag) {
                this.GlobalPosition = eventDrag.Position;
                this.dragSpeed      = eventDrag.Speed.Normalized();
                this.isFlickable    = true;
            }
        }
    }

    public void CheckSwipe() {
        // Swiped Right, it's a match
        if (this.Position.x > this.Game.screenSize.x * 2) {
            int nextPoint = (this.sprite.Texture == badCob) ? -1 : 1;
            EmitSignal(nameof(score_changed), nextPoint);
            EmitSignal(nameof(needs_reinitialization));
            this.ResetCob();

        // Swiped Left, not a match
        } else if (this.Position.x < -this.Game.screenSize.x) {
            int nextPoint = (this.sprite.Texture == goodCob) ? -1 : 1;
            EmitSignal(nameof(score_changed), nextPoint);
            EmitSignal(nameof(needs_reinitialization));
            this.ResetCob();
        }
    }

    public void ResetCob() {
        this.Position    = this.startPos;
        this.isDraggable = false;
        this.isReleased  = false;
        this.isFlickable = false;
        this.SetRandomTexture();
    }

    public void SetRandomTexture() {
        Texture[] textures = {goodCob, badCob};
        this.sprite.Texture = textures[(int) GD.RandRange(0, 2)];
    }
}
