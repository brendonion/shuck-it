using Godot;
using System.Collections.Generic;

public class TestSlicing : Node2D {

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

    public Godot.Object Slicer2D;

    public RigidBody2D SliceableCob;

    public override void _Ready() {
        this.Slicer2D = (Godot.Object) FindNode("Slicer2D");
        this.SliceableCob = (RigidBody2D) FindNode("SliceableCob");
    }

    public override void _Input(InputEvent @event) {
        if (!this.finished && @event is InputEventScreenDrag eventDrag) {
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

        if (!this.finished && @event.IsActionReleased("ui_touch")) {
            this.finished    = true;
            this.drawEnabled = false;
            if (this.beginPoints.Count == this.endPoints.Count + 1) {
                this.endPoints.Add(this.end);
                this.linesIdx++;
            }
            this.Slice();
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

    public async void Slice() {
        this.SliceableCob.GravityScale = 3f;
        for (int i = 0; i < this.linesIdx + 1; i++) {
            this.Slicer2D.Call("slice_world", (object) this.beginPoints[i], (object) this.endPoints[i]);
            await ToSignal(GetTree().CreateTimer(0.05f), "timeout");
        }
    }
}
