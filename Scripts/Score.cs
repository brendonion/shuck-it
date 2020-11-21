using Godot;

public class Score : Control {

    public int points = 0;
    public int misses = 0;

    public RichTextLabel pointCounter;
    public RichTextLabel missCounter;
    public RichTextLabel finalScore;
    public RichTextLabel bestScore;
    public Button pauseButton;
    public Control gameOver;
    public AudioStreamPlayer2D pointSound;
    public AudioStreamPlayer2D missSound;

    public Cob Cob;
    public TimerBar TimerBar;
    public Control PauseScreen;
    public DialogScreen DialogScreen;

    public override void _Ready() {
        // Score tracking nodes
        this.pointCounter = (RichTextLabel) FindNode("Points");
        this.missCounter  = (RichTextLabel) FindNode("Misses");
        this.pauseButton  = (Button) FindNode("PauseButton");
        this.pointSound   = (AudioStreamPlayer2D) FindNode("PointSound");
        this.missSound    = (AudioStreamPlayer2D) FindNode("MissSound");
        
        // Game over nodes
        this.gameOver     = (Control) FindNode("Game Over");
        this.finalScore   = (RichTextLabel) this.gameOver.FindNode("FinalScore");
        this.bestScore    = (RichTextLabel) this.gameOver.FindNode("BestScore");
        
        this.Cob          = (Cob) this.GetParent().FindNode("Cob");
        this.TimerBar     = (TimerBar) this.GetParent().FindNode("TimerBar");
        this.PauseScreen  = (Control) this.GetParent().FindNode("PauseScreen");
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

    public void _OnPausePressed() {
        GetTree().Paused         = true;
        this.PauseScreen.Visible = true;
        this.PauseScreen.SetAsToplevel(true);
    }

    public void _OnResumePressed() {
        GetTree().Paused         = false;
        this.PauseScreen.Visible = false;
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
        // Remove gameplay nodes
        this.DialogScreen.QueueFree();
        this.Cob.QueueFree();
        this.TimerBar.QueueFree();

        // Hide score tracking nodes
        this.missCounter.Visible = false;
        this.pointCounter.Visible = false;
        this.pauseButton.Visible = false;

        // Display game over content
        this.Visible = true;
        this.gameOver.Visible = true;
        this.finalScore.BbcodeText = $"[center]Score: {this.points}[/center]";
        this.bestScore.BbcodeText  = $"[center]Best: {this.points}[/center]";
    }
}
