using Godot;

public class Husk : RigidBody2D {

    public bool isTopLayer        = false;
    public bool startedPeeling    = false;
    public bool colorIncrementing = false;
    public float colorDiff        = 1f;

    public SceneTreeTimer modulateTimer;

    public CollisionShape2D collisionShape;

    public AnimatedSprite sprite;

    public Particles2D particles;

    public Game Game;

    public Cob Cob;

    public RhythmBar RhythmBar;

    public override void _Ready() {
        // Randomize seed
        GD.Randomize();

        this.collisionShape = (CollisionShape2D) FindNode("CollisionShape2D");
        this.sprite         = (AnimatedSprite) FindNode("AnimatedSprite");
        this.particles      = (Particles2D) FindNode("Particles2D");
        this.Game           = (Game) GetTree().Root.GetChild(0);
        this.Cob            = (Cob) this.Game.FindNode("Cob");
        this.RhythmBar      = (RhythmBar) this.Game.FindNode("RhythmBar");
    }

    public override void _PhysicsProcess(float delta) {
        // Find parents rigid body children (husks)
        // If this is the last husk, then it is on top
        var siblings = this.GetParent().GetChildren();
        if (siblings[siblings.Count - 1] == this) {
            this.isTopLayer = true;
            this.ModulateColor();
        }

        // If at the bottom of screen, destroy itself
        if (this.sprite.GlobalPosition.y >= this.Game.screenSize.y) {
            this.QueueFree();
        }
    }

    public override void _Input(InputEvent @event) {
        if (this.isTopLayer && this.startedPeeling && this.Mode != ModeEnum.Rigid) {
            if (this.sprite.Frame == 0) {
                this.sprite.Stop();
                this.startedPeeling = false;
            }

            if (@event.IsActionReleased("ui_touch")) {
                if (this.sprite.Frame < 5) {
                    this.sprite.Play("peel", true);
                } else if (this.sprite.Frame == 5) {
                    // Mark as a miss if not on beat
                    if (!this.RhythmBar.onBeat) {
                        this.Cob.EmitSignal(nameof(Cob.missed));
                        this.particles.Modulate = new Color(1f, 0.25f, 0.25f, 1f);
                    }

                    // Drop Husk
                    this.Mode = ModeEnum.Rigid;
                    this.AngularVelocity = (float) GD.RandRange(-2, 2);
                    this.collisionShape.Disabled = true;
                    this.ZIndex = 100;
                    this.GetParent().MoveChild(this, 0);
                    
                    // Display particles
                    this.particles.SetAsToplevel(true);
                    this.particles.ZIndex = 100;
                    this.particles.Position = new Vector2(this.sprite.GlobalPosition.x, this.sprite.GlobalPosition.y * 1.5f);
                    this.particles.Emitting = true;
                }
            }
        }
    }

    public void _OnHuskInputEvent(Node viewport, InputEvent @event, int shapeIdx) {
        if (this.isTopLayer && @event is InputEventScreenDrag eventDrag && !this.sprite.Playing) {
            int frame        = this.sprite.Frame;
            float spritePosY = this.sprite.GlobalPosition.y;
            float dragPosY   = eventDrag.Position.y;
            if (frame == 4 && dragPosY >= spritePosY * 1.5f) {
                this.sprite.Frame = 5;
            } else if ((frame == 3 || frame == 5) && dragPosY >= spritePosY * 1.25f && dragPosY < spritePosY * 1.5f) {
                this.sprite.Frame = 4;
            } else if ((frame == 2 || frame == 4) && dragPosY >= spritePosY  && dragPosY < spritePosY * 1.25f) {
                this.sprite.Frame = 3;
            } else if ((frame == 1 || frame == 3) && dragPosY >= spritePosY * 0.75f && dragPosY < spritePosY) {
                this.sprite.Frame = 2;
            } else if (dragPosY >= spritePosY * 0.25f && dragPosY < spritePosY * 0.75f) {
                this.sprite.Frame = 1;
                this.startedPeeling = true;
            }
        }
    }

    public void ModulateColor() {
        if (this.modulateTimer == null) {
            this.modulateTimer = GetTree().CreateTimer(0.1f);
        }

        if (this.modulateTimer != null && this.modulateTimer.TimeLeft <= 0f) {
            if (this.colorIncrementing) {
                this.colorDiff += 0.05f;
                if (this.colorDiff >= 1f) {
                    this.colorIncrementing = false;
                }
            } else {
                this.colorDiff -= 0.05f;
                if (this.colorDiff <= 0.75f) {
                    this.colorIncrementing = true;
                }
            }

            // Modulate color to indicate husk is on top
            this.sprite.Modulate = new Color(this.colorDiff, this.colorDiff, this.colorDiff);
            this.modulateTimer = null;
        }
    }
}
