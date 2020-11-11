using Godot;
using System;

public class DialogScreen : Control
{
    public SceneTreeTimer timer;

    // public ColorRect background;

    public Dialog text1;
    public Dialog text2;
    public Dialog text3;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        // this.background = (ColorRect) FindNode("ColorRect");
        this.text1 = (Dialog) FindNode("Text1");
        this.text2 = (Dialog) FindNode("Text2");
        this.text3 = (Dialog) FindNode("Text3");
    }

    public override void _PhysicsProcess(float delta) {
        if (this.Visible) {
            if (!this.text3.finished && this.Modulate.a < 1f) {
                this.FadeIn();
            }
            if (this.text3.finished && this.Modulate.a > 0f) {
                this.FadeOut();
            }
            if (this.text3.finished && this.Modulate.a <= 0f) {
                this.Visible = false;
            }

            if (this.Modulate.a >= 1f) {
                if (!this.text1.finished) {
                    this.text1.Visible = true;
                }
                if (this.text1.finished && !this.text2.finished) {
                    this.text1.Visible = false;
                    this.text2.Visible = true;
                }
                if (this.text2.finished && !this.text3.finished) {
                    this.text2.Visible = false;
                    this.text3.Visible = true;
                }
            }
        }
    }

    public void FadeIn() {
        if (this.timer == null || this.timer.TimeLeft <= 0f) {
            this.timer = GetTree().CreateTimer(0.05f);
            this.Modulate = new Color(1, 1, 1, this.Modulate.a + 0.05f);
        }
    }

    public void FadeOut() {
        if (this.timer == null || this.timer.TimeLeft <= 0f) {
            this.timer = GetTree().CreateTimer(0.05f);
            this.Modulate = new Color(1, 1, 1, this.Modulate.a - 0.05f);
        }
    }
}
