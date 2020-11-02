using Godot;

public class Score : Control {

    public int points = 0;
    public int misses = 0;

    public RichTextLabel pointCounter;
    public RichTextLabel missCounter;
    public Control gameOver;

    public Cob Cob;
    public TimerBar TimerBar;

    public override void _Ready() {
        this.pointCounter = (RichTextLabel) FindNode("Points");
        this.missCounter  = (RichTextLabel) FindNode("Misses");
        this.gameOver     = (Control) FindNode("Game Over");
        
        this.Cob       = (Cob) this.GetParent().FindNode("Cob");
        this.TimerBar = (TimerBar) this.GetParent().FindNode("TimerBar");

        this.Cob.Connect("swiped", this, "UpdateScore");
        this.Cob.Connect("missed", this, "UpdateMisses");
        this.TimerBar.Connect("timeout", this, "GameOver");
    }

    public void _OnPlayAgainPressed() {
        GetTree().ReloadCurrentScene();
    }

    public async void UpdateScore(int point) {
        this.points += point;

        if (point < 0) {
            this.UpdateMisses();
            this.pointCounter.BbcodeText = $"[shake level=7][center]{this.points}[/center][/shake]";
            await ToSignal(GetTree().CreateTimer(1f), "timeout");
        } else {
            this.pointCounter.BbcodeText = $"[wave amp=20 freq=20][center]{this.points}[/center][/wave]";
            await ToSignal(GetTree().CreateTimer(1f), "timeout");
        }

        this.pointCounter.BbcodeText = $"[wave amp=10 freq=2][center]{this.points}[/center][/wave]";
    }

    public void UpdateMisses() {
        this.misses += 1;
        string missText = "";
        for (int i = 0; i < this.misses; i++) missText += "X ";
        this.missCounter.BbcodeText  = $"[shake level={2 * this.misses}][center]{missText}[/center][/shake]";

        if (this.misses == 3) {
            this.GameOver();
        }
    }

    // TODO :: Put this in Game controller?
    public void GameOver() {
        this.Cob.QueueFree();
        this.TimerBar.QueueFree();
        this.gameOver.Visible = true;
    }
}
