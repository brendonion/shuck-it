using Godot;

public class Fly : Node2D {

    public float speed = 125f;

    public int patrolIndex = 0;

    public Game Game;

    public Path2D patrolPath;

    public Vector2[] patrolPoints;

    public Vector2 startPos;

    public KinematicBody2D body;

    public AnimatedSprite animatedSprite;

    public SceneTreeTimer timer;

    public override void _Ready() {
        // Randomize seed
        GD.Randomize();

        Path2D[] paths = {
            (Path2D) FindNode("FlyPath1"),
            (Path2D) FindNode("FlyPath2"),
            (Path2D) FindNode("FlyPath3"),
            (Path2D) FindNode("FlyPath4"),
            (Path2D) FindNode("FlyPath5"),
        };

        this.patrolPath     = paths[(int) GD.RandRange(0, 5)];
        this.patrolPoints   = this.patrolPath.Curve.GetBakedPoints();
        this.body           = (KinematicBody2D) FindNode("KinematicBody2D");
        this.animatedSprite = (AnimatedSprite) this.body.FindNode("AnimatedSprite");
        this.startPos       = this.body.Position;

        this.Game = (Game) GetTree().Root.GetChild(0);
    }

    public override void _PhysicsProcess(float delta) {
        Vector2 target   = this.patrolPoints[this.patrolIndex];
        Vector2 velocity = (target - this.body.Position).Normalized() * this.speed;
        this.body.MoveAndSlide(velocity);
        if (this.body.Position.DistanceTo(target) <= 2) {
            if (this.patrolIndex == this.patrolPoints.Length - 1) {
                this.patrolIndex = 0;
            } else {
                this.patrolIndex += 1;
            }
        }
        // Fade out the Fly when squashed
        if (this.speed == 0) {
            if (this.timer == null) {
                this.timer = GetTree().CreateTimer(0.25f);
            } else if (this.timer.TimeLeft <= 0f) {
                this.timer = GetTree().CreateTimer(0.1f);
                this.animatedSprite.Modulate = new Color(1, 1, 1, this.animatedSprite.Modulate.a - 0.1f);
            }
        }
    }

    public async void _OnBodyInputEvent(Node viewport, InputEvent @event, int shapeIdx) {
        if (@event.IsActionPressed("ui_touch")) {
            RemoveFromGroup("fly"); // Causing weird bug (remove_from_group: Condition "!data.grouped.has(p_identifier)" is true)
            var flies = GetTree().GetNodesInGroup("fly");
            if (flies.Count == 0) {
                this.Game.EmitSignal("fly_destroyed", 0);
            }

            this.speed = 0;
            this.animatedSprite.Play("squash");
            await ToSignal(this.animatedSprite, "animation_finished");
            QueueFree();
        }
    }
}
