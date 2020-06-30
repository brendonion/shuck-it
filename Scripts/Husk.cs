using Godot;
using System;

public class Husk : RigidBody2D {

    public override void _Ready() {}

    public override void _PhysicsProcess(float delta) {}

    public void _OnHuskInputEvent(Node viewport, InputEvent @event, int shapeIdx) {
        if (@event is InputEventScreenDrag eventDrag) {
            GD.Print("event drag position: ", eventDrag.Position);
        }
    }
}
