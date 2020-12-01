using Godot;

public class Kernel : Node2D {

    public float speed      = 150f;
    public float timeSec    = 1f;
    public float timePassed = 0f;

    public int patrolIndex = 0;

    public SaveSystem SaveSystem;

    public Path2D patrolPath;

    public Vector2[] patrolPoints;

    public Vector2 startPos;

    public KinematicBody2D body;

    public AnimatedSprite animatedSprite;

    public AudioStreamPlayer2D audioPlayer;

    public SceneTreeTimer timer;

    public override void _Ready() {
        // Randomize seed
        GD.Randomize();

        // Get singletons
        SaveSystem = (SaveSystem) GetParent().FindNode("SaveSystem");

        Path2D[] paths = {
            (Path2D) FindNode("KernelPath1"),
            (Path2D) FindNode("KernelPath2"),
        };

        this.patrolPath     = paths[(int) GD.RandRange(0, 2)];
        this.patrolPoints   = this.patrolPath.Curve.GetBakedPoints();
        this.body           = (KinematicBody2D) FindNode("KinematicBody2D");
        this.animatedSprite = (AnimatedSprite) this.body.FindNode("AnimatedSprite");
        this.audioPlayer    = (AudioStreamPlayer2D) FindNode("AudioStreamPlayer2D");
        this.body.Position  = this.patrolPoints[0];
    }

    public override void _PhysicsProcess(float delta) {
        Vector2 target   = this.patrolPoints[this.patrolIndex];
        Vector2 velocity = (target - this.body.Position).Normalized() * this.speed;
        this.body.MoveAndSlide(velocity);
        if (this.body.Position.DistanceTo(target) <= 2) {
            if (this.patrolIndex == this.patrolPoints.Length - 1) {
                QueueFree();
            } else {
                this.patrolIndex += 1;
            }
        }
        
        if (this.speed == 0) {
            // Move kernel upward slightly
            Vector2 upwardPos  = new Vector2(this.patrolPoints[this.patrolIndex].x, this.patrolPoints[this.patrolIndex].y - 50);
            this.timePassed   += delta;
            this.body.Position = this.body.Position.LinearInterpolate(upwardPos, this.timePassed / this.timeSec);

            // Fade out the Coin when collected
            if (this.timer == null) {
                this.timer = GetTree().CreateTimer(0.25f, false);
            } else if (this.timer.TimeLeft <= 0f) {
                this.timer = GetTree().CreateTimer(0.1f, false);
                this.animatedSprite.Modulate = new Color(1, 1, 1, this.animatedSprite.Modulate.a - 0.1f);
            }
        }
    }

    public async void _OnBodyInputEvent(Node viewport, InputEvent @event, int shapeIdx) {
        if (@event.IsActionPressed("ui_touch") && this.speed != 0) {
            // If SaveSystem.kernels is less than 9999, add 1 to them but do not save until game over
            if (SaveSystem.kernels < 9999) SaveSystem.kernels += 1;

            this.speed = 0;
            this.animatedSprite.Play("collected");
            this.audioPlayer.Play();
            await ToSignal(this.animatedSprite, "animation_finished");
            QueueFree();
        }
    }
}
