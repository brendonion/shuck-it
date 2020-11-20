using Godot;

public class Score : Control {

    public int points = 0;
    public int misses = 0;

    public RichTextLabel pointCounter;
    public RichTextLabel missCounter;
    public Control gameOver;
    public AudioStreamPlayer2D pointSound;
    public AudioStreamPlayer2D missSound;

    public Cob Cob;
    public TimerBar TimerBar;
    public DialogScreen DialogScreen;

    public override void _Ready() {
        this.pointCounter = (RichTextLabel) FindNode("Points");
        this.missCounter  = (RichTextLabel) FindNode("Misses");
        this.gameOver     = (Control) FindNode("Game Over");
        this.pointSound   = (AudioStreamPlayer2D) FindNode("PointSound");
        this.missSound    = (AudioStreamPlayer2D) FindNode("MissSound");
        
        this.Cob          = (Cob) this.GetParent().FindNode("Cob");
        this.TimerBar     = (TimerBar) this.GetParent().FindNode("TimerBar");
        this.DialogScreen = (DialogScreen) this.GetParent().FindNode("DialogScreen");

        this.Cob.Connect("swiped", this, "UpdateScore");
        this.TimerBar.Connect("timeout", this, "GameOver");
    }

    public void _OnPlayAgainPressed() {
        GetTree().ReloadCurrentScene();
    }

    public void _OnExitPressed() {
        GetTree().ChangeScene("res://Scenes/Home.tscn");
    }

    public async void UpdateScore(int point) {
        this.points += point;

        if (point < 0) {
            this.UpdateMisses();
            this.pointCounter.BbcodeText = $"[shake level=7][center]{this.points}[/center][/shake]";
            await ToSignal(GetTree().CreateTimer(1f), "timeout");
        } else {
            this.pointCounter.BbcodeText = $"[wave amp=20 freq=20][center]{this.points}[/center][/wave]";
            this.pointSound.Play();
            await ToSignal(GetTree().CreateTimer(1f), "timeout");
        }

        this.pointCounter.BbcodeText = $"[wave amp=10 freq=2][center]{this.points}[/center][/wave]";
    }

    public void UpdateMisses() {
        this.misses += 1;
        this.missSound.Play();
        string missText = "";
        for (int i = 0; i < this.misses; i++) missText += "X ";
        this.missCounter.BbcodeText  = $"[shake level={2 * this.misses}][center]{missText}[/center][/shake]";

        if (this.misses == 3) {
            this.GameOver();
        }
    }

    // TODO :: Put this in Game controller?
    public void GameOver() {
        this.DialogScreen.QueueFree();
        this.Cob.QueueFree();
        this.TimerBar.QueueFree();
        this.Visible = true;
        this.gameOver.Visible = true;
    }
}
