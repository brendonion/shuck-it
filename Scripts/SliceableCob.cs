using Godot;
using System.Collections.Generic;

public class SliceableCob : Node2D {

    public bool isSliceable = false;
    public bool drawEnabled = false;
    public bool finished    = false;

    public int prevDrawDirection = 0;
    public int drawDirection     = 0;
    public int linesIdx          = -1;

    public Vector2 prevDragPos;
    public Vector2 begin = Vector2.Zero;
    public Vector2 end   = Vector2.Zero;

    public List<Vector2> beginPoints = new List<Vector2>();
    public List<Vector2> endPoints   = new List<Vector2>();

    public PackedScene KernelScene = (PackedScene) ResourceLoader.Load("res://Scenes/Kernel.tscn");

    public Texture goodCob;

    public Node2D husks;

    public AudioStreamPlayer2D purchaseSound;

    public RichTextLabel kernelsEarned;

    public RigidBody2D Cob;

    public Godot.Object Slicer2D;

    public SaveSystem SaveSystem;

    [Signal]
    public delegate void sliced(int point);

    [Signal]
    public delegate void complete(int point);

    public override void _Ready() {
        // Get singletons
        SaveSystem = (SaveSystem) GetParent().FindNode("SaveSystem");

        this.Slicer2D      = (Godot.Object) FindNode("Slicer2D");
        this.Cob           = (RigidBody2D) FindNode("Cob");
        this.husks         = (Node2D) this.Cob.FindNode("Husks");
        this.purchaseSound = (AudioStreamPlayer2D) FindNode("PurchaseSound");
        this.kernelsEarned = (RichTextLabel) FindNode("KernelsEarned");

        this.goodCob = (Texture) ResourceLoader.Load($"res://Art/Unlockables/Good{SaveSystem.currentSkin}");

        this.SetCobTexture();
    }

    public override void _PhysicsProcess(float delta) {
        // Cob has no Husks, can be dragged
        if (!this.isSliceable) {
            int huskCount = GetTree().GetNodesInGroup("husk").Count;
            if (huskCount == 0) {
                this.isSliceable = true;
                // Set z-indices to draw on top of cob
                this.ZIndex = 1;
                this.Cob.ZIndex = -1;
            }
        }
    }

    public override void _Input(InputEvent @event) {
        if (!this.finished && this.isSliceable && @event is InputEventScreenDrag eventDrag) {
            this.drawEnabled = true;

            // Keep track of draw direction and prev drag pos
            this.drawDirection = this.prevDragPos.x - eventDrag.Position.x < 0 ? -1 : 1;
            this.prevDragPos   = eventDrag.Position;

            // Init previous draw direction
            if (this.prevDrawDirection == 0) {
                this.prevDrawDirection = this.drawDirection;  
            }

            // Init begin and beginPoints
            if (this.begin == Vector2.Zero) {
                this.begin = eventDrag.Position;
                this.beginPoints.Add(eventDrag.Position);
            }

            // Keep track of current line
            this.begin = this.beginPoints[this.beginPoints.Count - 1];
            this.end   = eventDrag.Position;

            if (this.drawDirection != this.prevDrawDirection) {
                // Change direction
                this.prevDrawDirection = this.drawDirection;

                // Complete a line
                this.endPoints.Add(this.end);
                this.linesIdx++;

                // Start new line
                this.beginPoints.Add(this.end);
            }

            Update();
        }

        if (!this.finished && this.isSliceable && @event.IsActionReleased("ui_touch")) {
            this.finished    = true;
            this.drawEnabled = false;
            this.isSliceable = false;
            if (this.beginPoints.Count == this.endPoints.Count + 1) {
                this.endPoints.Add(this.end);
                this.linesIdx++;
            }
            this.HandleSlice();
            Update();
        }
    }

    public override void _Draw() {
        if (this.drawEnabled) {
            // Draw current line
            DrawLine(this.begin, this.end, new Color(1f, 0, 0), 2f);

            // Draw complete lines
            if (this.linesIdx > -1) {
                for (int i = 0; i < this.linesIdx + 1; i++) {
                    DrawLine(this.beginPoints[i], this.endPoints[i], new Color(1f, 0, 0), 2f);
                }
            }
        }
    }

    public async void HandleSlice() {
        int bonusKernels = 0;
        this.Cob.GravityScale = 3f;
        this.Cob.ApplyCentralImpulse(new Vector2(0, -50f));

        // Reset z-indices
        this.ZIndex = 0;
        this.Cob.ZIndex = 0;

        // Wait a bit before looping through slices (alleviates weird jittering bug)
        await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

        for (int i = 0; i < this.linesIdx + 1; i++) {
            object sliceData = this.Slicer2D.Call("slice_one", (object) this.Cob, (object) this.beginPoints[i], (object) this.endPoints[i]);
            await ToSignal(GetTree().CreateTimer(0.05f), "timeout");
            bonusKernels += sliceData != null ? 1 : 0;
        }

        this.kernelsEarned.Visible = true;

        if (bonusKernels > 0) {
            EmitSignal(nameof(sliced), 1);
            this.kernelsEarned.BbcodeText = $"[wave amp=15 freq=5][center]+{bonusKernels} kernels[/center][/wave]";
            this.purchaseSound.Play();

            Vector2 screenSize = GetViewport().GetVisibleRect().Size;
            for (int i = 0; i < bonusKernels; i++) {
                Kernel kernel = (Kernel) KernelScene.Instance();
                AddChild(kernel);
                kernel.speed = 0f;
                kernel.body.Position = new Vector2(screenSize.x / 2, screenSize.y / 2);
                await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
                kernel.QueueFree();
            }

            SaveSystem.kernels += bonusKernels;
        } else {
            EmitSignal(nameof(sliced), -1);
            this.kernelsEarned.BbcodeText = $"[shake amp=5 freq=2][center]No bonus kernels![/center][/shake]";
        }

        await ToSignal(GetTree().CreateTimer(1f), "timeout");
        EmitSignal(nameof(complete), 0);
        QueueFree();
    }

    public void SetCobTexture() {
        ((Sprite) this.Cob.FindNode("Sprite")).Texture = goodCob;
    }
}
