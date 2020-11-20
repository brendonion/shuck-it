using Godot;

public class Cob : KinematicBody2D {

    public bool isDraggable = false;
    public bool isReleased  = false;
    public bool isFinale    = false; // External boolean to be set on final round

    public float speed = 500f;

    public Vector2 dragDirection;
    public Vector2 startPos;

    public Game Game;

    public Sprite sprite;

    public Node2D husks;

    public AnimatedSprite face;

    public Texture goodCob = (Texture) ResourceLoader.Load("res://Art/GoodCob.png");
    public Texture badCob  = (Texture) ResourceLoader.Load("res://Art/BadCob.png");

    [Signal]
    public delegate void swiped(int point);

    [Signal]
    public delegate void shucked(float duration);

    public override void _Ready() {
        // Randomize seed
        GD.Randomize();

        this.Game     = (Game) this.GetParent();
        this.husks    = (Node2D) FindNode("Husks");
        this.sprite   = (Sprite) FindNode("Sprite");
        this.face     = (AnimatedSprite) FindNode("Face");
        this.startPos = this.Position;

        this.SetCobTexture();
    }

    public override void _PhysicsProcess(float delta) {
        // Cob has no Husks, can be dragged
        if (!this.isDraggable) {
            int huskCount = GetTree().GetNodesInGroup("husk").Count;
            if (huskCount == 0) {
                this.isDraggable = !this.isFinale;
                if (this.isFinale) this.HandleFinale();
            }
        }

        // Cob released from drag, can be flung
        if (this.isReleased) {
            this.MoveAndSlide(new Vector2(this.dragDirection.x * this.speed, 0), Vector2.Down);
            this.CheckSwipe();
        }
    }

    public override void _Input(InputEvent @event) {
        // If "ui_touch" released and dragDirection.x is not 0, then release
        if (@event.IsActionReleased("ui_touch") && this.dragDirection.x != 0) {
            this.isReleased = true;
        }
    }

    public void _OnCobInputEvent(Node viewport, InputEvent @event, int shapeIdx) {
        var flies = GetTree().GetNodesInGroup("fly");
        var pigs  = GetTree().GetNodesInGroup("pig");
        // If draggable and there are no flies or pigs
        if (this.isDraggable && flies.Count == 0 && pigs.Count == 0) {
            // If dragging, set Position and dragDirection
            if (@event is InputEventScreenDrag eventDrag) {
                // Offset the sprite to where initially touched
                if (this.sprite.Position == Vector2.Zero) {
                    this.sprite.Position = new Vector2(
                        this.sprite.GlobalPosition.x - eventDrag.Position.x,
                        this.sprite.GlobalPosition.y - eventDrag.Position.y
                    );
                    this.face.Position = this.sprite.Position;
                }
                this.Position      = eventDrag.Position;
                this.dragDirection = eventDrag.Speed.x != 0
                    ? eventDrag.Speed.Normalized()
                    : (this.Position - this.Game.screenCenter).Normalized();
                // Guarantee a consistent speed
                this.dragDirection.x = this.dragDirection.x < 0 ? -1 : 1;
                
                // Guarantee right direction if isFinale
                if (this.isFinale) {
                    this.dragDirection.x = 1;
                }
            }
        }
    }

    public void CheckSwipe() {
        // Swiped Right, it's a match
        if (this.Position.x > this.Game.screenSize.x * 2) {
            int nextPoint;
            if (this.isFinale) {
                GetTree().ChangeScene("res://Scenes/Finale.tscn");
                return;
            } else if (this.face.Visible) {
                nextPoint = this.face.Animation.Contains("bad_face") ? -1 : 1;
            } else {
                nextPoint = (this.sprite.Texture == badCob) ? -1 : 1;
            }
            EmitSignal(nameof(swiped), nextPoint);
            this.ResetCob();

        // Swiped Left, not a match
        } else if (this.Position.x < -this.Game.screenSize.x) {
            int nextPoint;
            if (this.face.Visible) {
                nextPoint = this.face.Animation.Contains("good_face") ? -1 : 1;
            } else {
                nextPoint = (this.sprite.Texture == goodCob) ? -1 : 1;
            }
            EmitSignal(nameof(swiped), nextPoint);
            this.ResetCob();
        }
    }

    public void ResetCob() {
        this.sprite.Position = Vector2.Zero;
        this.face.Position   = Vector2.Zero;
        this.Position        = this.startPos;
        this.dragDirection   = new Vector2();
        this.isDraggable     = false;
        this.isReleased      = false;
        this.SetCobTexture();
    }

    public void SetCobTexture() {
        Texture[] textures = {goodCob, badCob};
        if (!this.face.Visible) {
            this.sprite.Texture = textures[(int) GD.RandRange(0, 2)];
        } else {
            this.sprite.Texture = textures[0];
        }
    }

    public async void HandleFinale() {
        this.face.Play(); // final_face
        await ToSignal(this.face, "animation_finished");
        this.isDraggable = true;
    }
}
