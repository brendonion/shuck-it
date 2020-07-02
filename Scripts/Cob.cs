using Godot;
using System;

public class Cob : KinematicBody2D {

    public bool isDraggable = false;
    public bool isReleased  = false;
    public float timeSec    = 1f; // How long it takes for the Cob to get to screen center
    public float timePassed = 0f; // Time passed since initialization

    public Game Game;
    
    public override void _Ready() {
        this.Game = (Game) this.GetParent();
    }

    public override void _PhysicsProcess(float delta) {
        // Cob has no Husks, can be dragged
        if (!this.isDraggable && this.FindNode("Husks").GetChildCount() == 0) {
            this.isDraggable = true;
        }

        // TODO: Refactor to be more DRY
        if (this.isReleased) {
            GD.Print("INTERPOLATING");
            this.timePassed += delta;
            this.Position = this.Position.LinearInterpolate(this.Game.screenCenter, this.timePassed / this.timeSec);
        }

        if (this.isReleased && this.Position == this.Game.screenCenter) {
            GD.Print("IS NOT RELEASED");
            this.isReleased = false;
            this.timePassed = 0;
        }
    }

    // public override void _Input(InputEvent @event) {
        
    // }

    public void _OnCobInputEvent(Node viewport, InputEvent @event, int shapeIdx) {
        if (this.isDraggable) {
            if (@event.IsActionReleased("ui_touch")) {
                GD.Print("RELEASED!!");
                this.isReleased = true;
            }

            if (@event is InputEventScreenDrag eventDrag) {
                this.GlobalPosition = eventDrag.Position;
            }
        }
    }
}
