using Godot;
using System;

public class Cob : KinematicBody2D {

    public bool isDraggable = false;
    public bool isReleased  = false;

    public float speed = 500f;

    public SceneTreeTimer dragTimer;

    public Vector2 dragSpeed;
    public Vector2 velocity;
    public Vector2 startPos;

    public Game Game;

    public Sprite sprite;

    public Node2D husks;

    public Texture goodCob = (Texture) ResourceLoader.Load("res://Art/GoodCob.png");
    public Texture badCob  = (Texture) ResourceLoader.Load("res://Art/BadCob.png");

    [Signal]
    public delegate void needs_reinitialization();

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
        if (!this.isDraggable && !this.isReleased) {
            int huskCount = this.husks.GetChildCount();
            if (huskCount == 0 || (huskCount == 1 && ((Husk) this.husks.GetChild(0)).Mode == RigidBody2D.ModeEnum.Rigid)) {
                this.isDraggable = true;
            }
        }

        // Release Cob from drag after dragging for a set amount of time
        // Alleviates a bug where _Input isn't called if dragging and releasing too quickly
        if (!this.isReleased && this.dragTimer != null && this.dragTimer.TimeLeft <= 0f) {
            this.isDraggable = false;
            this.isReleased  = true;
        }

        // Cob released from drag, can be flung
        if (this.isReleased) {
            this.MoveAndSlide(new Vector2(this.dragSpeed.x * this.speed, 0), Vector2.Down);
            this.CheckSwipe();
        }
    }

    public override void _Input(InputEvent @event) {
        // If "ui_touch" released and dragSpeed.x is not 0, then release
        if (@event.IsActionReleased("ui_touch") && this.dragSpeed.x != 0) {            
            this.isReleased = true;
        }
    }

    public void _OnCobInputEvent(Node viewport, InputEvent @event, int shapeIdx) {
        if (this.isDraggable) {
            // If dragging, set Position, dragSpeed, and dragTimer
            if (@event is InputEventScreenDrag eventDrag) {
                this.Position  = eventDrag.Position;
                this.dragSpeed = eventDrag.Speed.x != 0
                    ? eventDrag.Speed.Normalized()
                    : (this.Position - this.Game.screenCenter).Normalized();
                this.dragTimer = GetTree().CreateTimer(0.15f);
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
        this.dragTimer   = null;
        this.dragSpeed   = new Vector2();
        this.SetRandomTexture();
    }

    public void SetRandomTexture() {
        Texture[] textures = {goodCob, badCob};
        this.sprite.Texture = textures[(int) GD.RandRange(0, 2)];
    }
}
