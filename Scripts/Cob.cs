using Godot;

public class Cob : KinematicBody2D {

    public bool isDraggable = false;
    public bool isReleased  = false;

    public float speed = 500f;

    public Vector2 dragSpeed;
    public Vector2 velocity;
    public Vector2 startPos;

    public Game Game;

    public Sprite sprite;

    public Node2D husks;

    public Texture goodCob = (Texture) ResourceLoader.Load("res://Art/GoodCob.png");
    public Texture badCob  = (Texture) ResourceLoader.Load("res://Art/BadCob.png");

    [Signal]
    public delegate void swiped(int point);

    [Signal]
    public delegate void missed();

    [Signal]
    public delegate void shucked(float duration);

    public override void _Ready() {
        // Randomize seed
        GD.Randomize();

        this.Game     = (Game) this.GetParent();
        this.husks    = (Node2D) FindNode("Husks");
        this.sprite   = (Sprite) FindNode("Sprite");
        this.startPos = this.Position;

        this.SetRandomTexture();
    }

    public override void _PhysicsProcess(float delta) {
        // Cob has no Husks, can be dragged
        if (!this.isDraggable) {
            int huskCount = this.husks.GetChildCount();
            if (huskCount == 0 || (huskCount == 1 && ((Husk) this.husks.GetChild(0)).Mode == RigidBody2D.ModeEnum.Rigid)) {
                this.isDraggable = true;
            }
        }

        // Cob released from drag, can be flung
        if (this.isReleased) {
            this.MoveAndSlide(new Vector2(this.dragSpeed.x * this.speed, 0), Vector2.Down);
            this.CheckSwipe();
        }
    }

    public override void _Input(InputEvent @event) {
        // If "ui_touch" released and dragSpeed.x is not 0, then release
        if (@event.IsActionReleased("ui_touch") && this.dragSpeed.x != 0) {
            this.isReleased = true;
        }
    }

    public void _OnCobInputEvent(Node viewport, InputEvent @event, int shapeIdx) {
        var flies = GetTree().GetNodesInGroup("fly");
        var pigs  = GetTree().GetNodesInGroup("pig");
        // If draggable and there are no flies or pigs
        if (this.isDraggable && flies.Count == 0 && pigs.Count == 0) {
            // If dragging, set Position and dragSpeed
            if (@event is InputEventScreenDrag eventDrag) {
                // Offset the sprite to where initially touched
                if (this.sprite.Position == Vector2.Zero) {
                    this.sprite.Position = new Vector2(
                        this.sprite.GlobalPosition.x - eventDrag.Position.x,
                        this.sprite.GlobalPosition.y - eventDrag.Position.y
                    );
                }
                this.Position  = eventDrag.Position;
                this.dragSpeed = eventDrag.Speed.x != 0
                    ? eventDrag.Speed.Normalized()
                    : (this.Position - this.Game.screenCenter).Normalized();
            }

        }
    }

    public void CheckSwipe() {
        // Swiped Right, it's a match
        if (this.Position.x > this.Game.screenSize.x * 2) {
            int nextPoint = (this.sprite.Texture == badCob) ? -1 : 1;
            EmitSignal(nameof(swiped), nextPoint);
            this.ResetCob();

        // Swiped Left, not a match
        } else if (this.Position.x < -this.Game.screenSize.x) {
            int nextPoint = (this.sprite.Texture == goodCob) ? -1 : 1;
            EmitSignal(nameof(swiped), nextPoint);
            this.ResetCob();
        }
    }

    public void ResetCob() {
        this.sprite.Position = Vector2.Zero;
        this.Position        = this.startPos;
        this.dragSpeed       = new Vector2();
        this.isDraggable     = false;
        this.isReleased      = false;
        this.SetRandomTexture();
    }

    public void SetRandomTexture() {
        Texture[] textures = {goodCob, badCob};
        this.sprite.Texture = textures[(int) GD.RandRange(0, 2)];
    }
}
