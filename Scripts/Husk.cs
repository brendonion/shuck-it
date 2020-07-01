using Godot;
using System;

public class Husk : RigidBody2D {

    public bool isTopLayer     = false;
    public bool startedPeeling = false;

    public Vector2 screenSize;
    public Vector2 screenCenter;
    public Vector2 startPos;

    public CollisionShape2D collisionShape;

    public AnimatedSprite sprite;

    public override void _Ready() {
        // Randomize seed
        GD.Randomize();

        this.screenSize     = GetViewport().GetVisibleRect().Size;
        this.screenCenter   = this.screenSize / 2;
        this.collisionShape = (CollisionShape2D) FindNode("CollisionShape2D");
        this.sprite         = (AnimatedSprite) FindNode("AnimatedSprite");
    }

    public override void _PhysicsProcess(float delta) {
        // Find parents rigid body children (husks)
        // If this is the last husk, then it is on top
        var siblings = this.GetParent().GetChildren();
        if (siblings[siblings.Count - 1] == this) {
            this.isTopLayer = true;
        }

        // If at the bottom of screen, destroy itself
        if (this.sprite.GlobalPosition.y >= this.screenSize.y) {
            this.QueueFree();
        }
    }

    public override void _Input(InputEvent @event) {
        if (this.isTopLayer && this.startedPeeling) {
            if (this.sprite.Frame == 0) {
                this.sprite.Stop();
                this.startedPeeling = false;
            }

            if (@event.IsActionReleased("ui_touch")) {
                if (this.sprite.Frame < 5) {
                    this.sprite.Play("peel", true);
                } else if (this.sprite.Frame == 5) {
                    this.Mode = ModeEnum.Rigid;
                    this.AngularVelocity = (float) GD.RandRange(-2, 2);
                    this.collisionShape.Disabled = true;
                    this.ZIndex = 100;
                    this.GetParent().MoveChild(this, 0);
                }
            }
        }
    }

    public void _OnHuskInputEvent(Node viewport, InputEvent @event, int shapeIdx) {
        if (this.isTopLayer && @event is InputEventScreenDrag eventDrag && !this.sprite.Playing) {
            if (this.startedPeeling) {
                if (eventDrag.Position.y >= this.sprite.GlobalPosition.y * 1.5f) {
                    this.sprite.Frame = 5;
                } else if (eventDrag.Position.y >= this.sprite.GlobalPosition.y * 1.25f) {
                    this.sprite.Frame = 4;
                } else if (eventDrag.Position.y >= this.sprite.GlobalPosition.y) {
                    this.sprite.Frame = 3;
                } else if (eventDrag.Position.y >= this.sprite.GlobalPosition.y * 0.75f) {
                    this.sprite.Frame = 2;
                }
            // Start peeling from the top
            } else if (eventDrag.Position.y >= this.sprite.GlobalPosition.y * 0.5f) {
                this.sprite.Frame = 1;
                this.startedPeeling = true;
            }
        }
    }
}
