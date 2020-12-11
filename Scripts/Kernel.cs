using Godot;

public class Kernel : Node2D {

    public bool isBomb = false;

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

    public AudioStreamPlayer2D coinSound;
    public AudioStreamPlayer2D explosionSound;

    public SceneTreeTimer timer;

    [Signal]
    public delegate void detonated(int point);

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
        this.coinSound      = (AudioStreamPlayer2D) FindNode("CoinSound");
        this.explosionSound = (AudioStreamPlayer2D) FindNode("ExplosionSound");
        this.body.Position  = this.patrolPoints[0];

        if (this.isBomb) {
            this.animatedSprite.Play("bomb");
            this.animatedSprite.FlipH = this.patrolPath == paths[1];
        }
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
        
        if (this.speed == 0 && !this.isBomb) {
            // Move kernel upward slightly
            Vector2 upwardPos  = new Vector2(this.patrolPoints[this.patrolIndex].x, this.patrolPoints[this.patrolIndex].y - 50);
            this.timePassed   += delta;
            this.body.Position = this.body.Position.LinearInterpolate(upwardPos, this.timePassed / this.timeSec);

            // Fade out the kernel when tapped
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
            this.speed = 0;

            if (this.isBomb) {
                this.animatedSprite.Play("explosion");
                this.explosionSound.Play();
                EmitSignal(nameof(detonated), -1);
            } else {
                // TODO :: If SaveSystem.kernels is less than 9999, add 1 to them but do not save until game over
                SaveSystem.kernels += 1;
                this.animatedSprite.Play("collected");
                this.coinSound.Play();
            }

            await ToSignal(this.animatedSprite, "animation_finished");
            QueueFree();
        }
    }
}
