using Godot;

public class Fly : Node2D {

    public float speed = 125f;

    public int patrolIndex = 0;

    public Vector2[] patrolPoints;

    public Vector2 startPos;

    public KinematicBody2D body;

    public override void _Ready() {
        this.patrolPoints = ((Path2D) FindNode("Path2D")).Curve.GetBakedPoints();
        this.body         = (KinematicBody2D) FindNode("KinematicBody2D");
        this.startPos     = this.body.Position;
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
    }

    public void _OnBodyInputEvent(Node viewport, InputEvent @event, int shapeIdx) {
        if (@event.IsActionReleased("ui_touch")) {
            // TODO :: Play fly squashed animation
            this.QueueFree();
        }
    }
}
