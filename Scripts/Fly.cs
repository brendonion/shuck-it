using Godot;

public class Fly : Node2D {

    public float speed = 125f;

    public int patrolIndex = 0;

    public Path2D patrolPath;

    public Vector2[] patrolPoints;

    public KinematicBody2D body;

    public AnimatedSprite animatedSprite;

    public AudioStreamPlayer2D audioPlayer;

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

        Vector2 screenSize       = GetViewport().GetVisibleRect().Size;
        Vector2[] startPositions = {
            new Vector2(screenSize.x / 2 + 40, -30),
            new Vector2(screenSize.x / 2 - 40, -30),
            new Vector2(screenSize.x / 2 + 40, screenSize.y),
            new Vector2(screenSize.x / 2 - 40, screenSize.y),
        };

        this.body           = (KinematicBody2D) FindNode("KinematicBody2D");
        this.body.Position  = startPositions[(int) GD.RandRange(0, 4)];
        this.patrolPath     = paths[(int) GD.RandRange(0, 5)];
        this.patrolPoints   = this.patrolPath.Curve.GetBakedPoints();
        this.animatedSprite = (AnimatedSprite) this.body.FindNode("AnimatedSprite");
        this.audioPlayer    = (AudioStreamPlayer2D) FindNode("AudioStreamPlayer2D");
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
                this.timer = GetTree().CreateTimer(0.25f, false);
            } else if (this.timer.TimeLeft <= 0f) {
                this.timer = GetTree().CreateTimer(0.1f, false);
                this.animatedSprite.Modulate = new Color(1, 1, 1, this.animatedSprite.Modulate.a - 0.1f);
            }
        }
    }

    public async void _OnBodyInputEvent(Node viewport, InputEvent @event, int shapeIdx) {
        if ((@event is InputEventScreenTouch || @event is InputEventScreenDrag) && this.speed != 0) {
            RemoveFromGroup("fly");
            this.speed = 0;
            this.animatedSprite.Play("squash");
            this.audioPlayer.Play();
            await ToSignal(this.animatedSprite, "animation_finished");
            QueueFree();
        }
    }
}
