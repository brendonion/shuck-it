using Godot;
using System;

public class RhythmBar : Node2D {

    public float speed = 100f;

    public float timeLeft = 5f;

    public bool moveRight = false;

    public bool onBeat = true; // Default to always on beat

    public SceneTreeTimer timer;

    public Game Game;

    public Cob Cob;

    public KinematicBody2D slider;

    public Area2D arrow;
    public Area2D bar;

    public TextureProgress barProgress;

    public Vector2 barLeft;
    public Vector2 barRight;

    [Signal]
    public delegate void timeout();

    public override void _Ready() {
        // Randomize seed
        GD.Randomize();

        this.Game = (Game) this.GetParent();
        this.Cob  = (Cob) this.Game.FindNode("Cob");

        this.Cob.Connect("shucked", this, "SetTimer");
        this.Cob.Connect("swiped", this, "ClearTimer");
        this.Cob.Connect("swiped", this, "SetRandomArrowPosition");
        this.Game.Connect("fly_spawned", this, "SetTimer");
        this.Game.Connect("fly_destroyed", this, "ClearTimer");

        this.slider      = (KinematicBody2D) FindNode("Slider");
        this.arrow       = (Area2D) FindNode("Arrow");
        this.bar         = (Area2D) FindNode("Bar");
        this.barProgress = (TextureProgress) this.bar.FindNode("TextureProgress");

        var barCollision = ((CollisionShape2D) this.bar.FindNode("CollisionShape2D"));
        var barShape     = (RectangleShape2D) barCollision.Shape;
        var barExtents   = barShape.Extents;
        this.barLeft     = this.bar.Position - barExtents * 2;
        this.barRight    = this.bar.Position + barExtents * 2;
    }

    public override void _PhysicsProcess(float delta) {
        // Move slider back and forth
        if (!moveRight) {
            if (this.slider.Position.x >= this.barLeft.x) {
                this.slider.MoveAndSlide(new Vector2(-this.speed, 0));
            } else {
                this.moveRight = true;
            }
        } else if (moveRight) {
            if (this.slider.Position.x <= this.barRight.x) {
                this.slider.MoveAndSlide(new Vector2(this.speed, 0));
            } else {
                this.moveRight = false;
            }
        }

        if (this.timer != null) {
            // Animate progress bar if timer has started
            if (this.timer.TimeLeft > 0f) {
                this.barProgress.Value = (this.timer.TimeLeft / this.timeLeft) * 100;
            }
            // Emit 'timeout' signal if timer has finished
            if (this.timer.TimeLeft <= 0f) {
                EmitSignal(nameof(timeout));
            }
        }
    }

    public void OnArrowBodyEntered(Node2D body) {
        var sliderSprite = (Sprite) body.FindNode("Sprite");
        if (this.slider.Visible && sliderSprite != null) {
            sliderSprite.Scale = new Vector2(0.9f, 0.9f);
            this.onBeat = true;
        }
    }

    public void OnArrowBodyExited(Node2D body) {
        var sliderSprite = (Sprite) body.FindNode("Sprite");
        if (this.slider.Visible && sliderSprite != null) {
            sliderSprite.Scale = new Vector2(0.5f, 0.5f);
            this.onBeat = false;
        }
    }

    public void SetRandomArrowPosition(int point = 0) {
        // TODO :: Refactor
        if (this.Game.round >= (int) Game.Events.SLIDER) {
            if (this.speed < 200f) this.speed += 10f;
            this.arrow.Position = new Vector2((float) GD.RandRange(this.barLeft.x, this.barRight.x), this.arrow.Position.y);
        }
    }

    public void SetTimer(float duration = 0f) {
        this.timeLeft = duration > 0f ? duration : this.timeLeft;
        this.barProgress.Value = 100;
        this.timer = GetTree().CreateTimer(this.timeLeft);
    }

    // TODO :: Remove point parameter requirement
    public void ClearTimer(int point = 0) {
        this.barProgress.Value = 100;
        this.timer = null;
    }
}
