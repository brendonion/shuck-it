using Godot;
using System;

public class Cob : KinematicBody2D {

    public bool isDraggable = false;
    public bool isReleased  = false;

    public float gravity = 100f;

    public Vector2 dragSpeed;
    public Vector2 velocity;

    public Game Game;
    
    public override void _Ready() {
        this.Game = (Game) this.GetParent();
    }

    public override void _PhysicsProcess(float delta) {
        // Cob has no Husks, can be dragged
        if (!this.isDraggable && this.FindNode("Husks").GetChildCount() == 0) {
            this.isDraggable = true;
        }

        if (this.isReleased) {
            this.MoveAndSlide(new Vector2(this.dragSpeed.x * 500, this.dragSpeed.y * this.gravity));
        }

        if (this.Position.x > this.Game.screenSize.x * 2 || this.Position.x < -this.Game.screenSize.x) {
            this.Position = this.Game.screenCenter;
            this.isReleased = false;
        }
    }

    public void _OnCobInputEvent(Node viewport, InputEvent @event, int shapeIdx) {
        if (this.isDraggable) {
            if (@event.IsActionReleased("ui_touch")) {
                GD.Print("RELEASED!!");
                this.isReleased = true;
            }

            if (@event is InputEventScreenDrag eventDrag) {
                this.GlobalPosition = eventDrag.Position;
                this.dragSpeed      = eventDrag.Speed.Normalized();
                GD.Print("this.dragSpeed: ", this.dragSpeed);
                GD.Print("this.dragSpeed.Normalized(): ", this.dragSpeed.Normalized());
            }
        }
    }
}
