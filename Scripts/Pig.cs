using Godot;
using System;
using System.Collections.Generic;

public class Pig : Node2D {

    public bool hit               = false;
    public bool dead              = false;
    public bool retreating        = false;
    public bool colorIncrementing = false;

    public float colorDiff    = 1f;
    public float colorAlpha   = 1f;
    public float speed        = 125f;
    public float initialSpeed = 125f;
    public float retreatSpeed = 175f;
    public float deathSpeed   = 50f;

    public int health      = 6;
    public int patrolIndex = 0;

    public SceneTreeTimer modulateTimer;

    public Game Game;

    public KinematicBody2D body;

    public AnimatedSprite animatedSprite;

    public AudioStreamPlayer2D squealSound;
    public AudioStreamPlayer2D deathSound;

    public List<Path2D> paths = new List<Path2D>();

    public Path2D patrolPath;

    public Vector2[] patrolPoints;

    public override void _Ready() {
        GD.Randomize();

        this.paths.Add((Path2D) FindNode("PigPath1"));
        this.paths.Add((Path2D) FindNode("PigPath2"));
        this.paths.Add((Path2D) FindNode("PigPath3"));
        this.paths.Add((Path2D) FindNode("PigPath4"));

        this.initialSpeed   = this.speed;
        this.body           = (KinematicBody2D) FindNode("KinematicBody2D");
        this.animatedSprite = (AnimatedSprite) this.body.FindNode("AnimatedSprite");
        this.squealSound    = (AudioStreamPlayer2D) FindNode("SquealSound");
        this.deathSound    = (AudioStreamPlayer2D) FindNode("DeathSound");

        this.Game = (Game) GetTree().Root.GetChild(0);

        this.SetPath();
        this.SetStartPosition();
    }

    public override void _PhysicsProcess(float delta) {
        Vector2 target   = this.patrolPoints[this.patrolIndex];
        Vector2 velocity = (target - this.body.Position).Normalized() * this.speed;

        this.body.MoveAndSlide(velocity);

        if (this.hit || this.dead) {
            this.ModulateColor();
        }

        // Not retreating and close to target; set next patrolIndex
        if (!this.retreating && this.body.Position.DistanceTo(target) <= 2) {
            if (this.patrolIndex == this.patrolPoints.Length - 1) {
                this.patrolIndex = 0;
            } else {
                this.patrolIndex += 1;
            }
        }

        // Retreating and at end of path; set next patrolPath
        if (this.retreating && !this.dead && this.body.Position.DistanceTo(target) <= 2) {
            this.retreating = false;
            this.speed      = this.initialSpeed;
            this.SetPath();
            this.SetStartPosition();
        }
    }

    public async void _OnBodyInputEvent(Node viewport, InputEvent @event, int shapeIdx) {
        if (@event.IsActionPressed("ui_touch")) {
            // If not hit then set to true and subtract from health
            if (!this.hit) {
                this.hit     = true;
                this.health -= 1;
                this.squealSound.Play();
            }

            // Retreat after 3 hits
            if (this.health % 2 == 0) {
                this.retreating  = true;
                this.speed       = this.retreatSpeed;
                this.patrolIndex = 0;
            }

            // Health reached 0; Pig is dead
            if (this.health == 0) {
                RemoveFromGroup("pig");
                this.dead  = true;
                this.speed = this.deathSpeed;
                this.animatedSprite.Play("death");
                this.deathSound.Play();
                await ToSignal(this.animatedSprite, "animation_finished");
                QueueFree();
            }
        }
    }

    public void SetPath() {
        this.patrolPath   = this.paths[(int) GD.RandRange(0, this.paths.Count)];
        this.patrolPoints = this.patrolPath.Curve.GetBakedPoints();
        this.paths.Remove(this.patrolPath);
    }

    public void SetStartPosition() {
        this.body.Position        = this.patrolPoints[0];
        this.animatedSprite.FlipH = this.body.Position.x < this.Game.screenSize.x / 2;
        this.animatedSprite.FlipV = this.body.Position.y < this.Game.screenSize.y / 2;
    }

    public void ModulateColor() {
        if (this.modulateTimer == null) {
            this.modulateTimer = GetTree().CreateTimer(0.025f, false);
        }

        if (this.modulateTimer != null && this.modulateTimer.TimeLeft <= 0f) {
            if (this.colorIncrementing) {
                this.colorDiff += 0.1f;
                if (this.colorDiff >= 1f) {
                    this.colorIncrementing = false;
                    this.hit = false;
                }
            } else {
                this.colorDiff -= 0.1f;
                if (this.colorDiff <= 0.5f) {
                    this.colorIncrementing = true;
                }
            }

            // Modulate color to indicate pig being hit
            if (!this.dead) {
                this.animatedSprite.Modulate = new Color(1, this.colorDiff, this.colorDiff);
            } else {
                this.colorAlpha -= 0.05f;
                this.animatedSprite.Modulate = new Color(1, this.colorDiff, this.colorDiff, this.colorAlpha);
            }
            this.modulateTimer = null;
        }
    }
}
