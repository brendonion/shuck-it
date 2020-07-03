using Godot;
using System;

public class Score : Control {

    public int points = 0;

    public RichTextLabel text;

    public Cob Cob;

    public override void _Ready() {
        this.text = (RichTextLabel) FindNode("Text");
        this.Cob  = (Cob) this.GetParent().FindNode("Cob");
        this.Cob.Connect("score_changed", this, "UpdateScore");
    }

    public void UpdateScore(int point) {
        this.points += point;
        this.text.BbcodeText = $"[wave amp=10 freq=2][center]{this.points}[/center][/wave]";
    }

}
