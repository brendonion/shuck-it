using Godot;
using System;

public class RhythmBar : Node2D {

    public bool moveRight = false;

    public KinematicBody2D slider;

    public Area2D arrow;
    public Area2D bar;

    public Vector2 barLeft;
    public Vector2 barRight;

    public override void _Ready() {
        this.slider = (KinematicBody2D) FindNode("Slider");
        this.arrow  = (Area2D) FindNode("Arrow");
        this.bar    = (Area2D) FindNode("Bar");

        var barCollision = ((CollisionShape2D) this.bar.FindNode("CollisionShape2D"));
        var barShape     = (RectangleShape2D) barCollision.Shape;
        var barExtents   = barShape.Extents;
        this.barLeft     = this.bar.Position - barExtents * 2;
        this.barRight    = this.bar.Position + barExtents * 2;
    }

    public override void _PhysicsProcess(float delta) {
        if (!moveRight) {
            if (this.slider.Position.x > this.barLeft.x) {
                this.slider.MoveAndSlide(new Vector2(-100, 0));
            } else {
                this.moveRight = true;
            }
        } else if (moveRight) {
            if (this.slider.Position.x < this.barRight.x) {
                this.slider.MoveAndSlide(new Vector2(100, 0));
            } else {
                this.moveRight = false;
            }
        }
    }

    public void OnArrowBodyEntered(Node2D body) {
        GD.Print("body entered: ", body.Name);
        var sliderSprite = (Sprite) body.FindNode("Sprite");
        sliderSprite.Scale = new Vector2(0.5f, 0.5f);
    }

    public void OnArrowBodyExited(Node2D body) {
        GD.Print("body exited: ", body.Name);
        var sliderSprite = (Sprite) body.FindNode("Sprite");
        sliderSprite.Scale = new Vector2(0.25f, 0.25f);
    }
}
