using Godot;

public class Score : Control {

    public int points  = 0;
    public int misses  = 0;

    public RichTextLabel pointCounter;
    public RichTextLabel missCounter;
    public RichTextLabel finalScore;
    public RichTextLabel bestScore;
    public Button pauseButton;
    public Button playAgainButton;
    public Button exitButton;
    public Control gameOver;
    public Sprite trophy;
    public AudioStreamPlayer2D pointSound;
    public AudioStreamPlayer2D missSound;
    public AudioStreamPlayer2D gameOverSound;

    public Cob Cob;
    public TimerBar TimerBar;
    public Control PauseScreen;
    public DialogScreen DialogScreen;

    public Texture trophyTexture       = (Texture) ResourceLoader.Load("res://Art/Trophy.png");
    public Texture silverTrophyTexture = (Texture) ResourceLoader.Load("res://Art/SilverTrophy.png");
    public Texture emptyTrophyTexture  = (Texture) ResourceLoader.Load("res://Art/EmptyTrophy.png");

    public Godot.Object admob;

    public SaveSystem SaveSystem;

    public override void _Ready() {
        GD.Randomize();

        // Get singletons
        SaveSystem = (SaveSystem) GetParent().FindNode("SaveSystem");

        // Score tracking nodes
        this.pointCounter = (RichTextLabel) FindNode("Points");
        this.missCounter  = (RichTextLabel) FindNode("Misses");
        this.pauseButton  = (Button) FindNode("PauseButton");
        this.pointSound   = (AudioStreamPlayer2D) FindNode("PointSound");
        this.missSound    = (AudioStreamPlayer2D) FindNode("MissSound");
        
        // Game over nodes
        this.gameOver        = (Control) FindNode("Game Over");
        this.gameOverSound   = (AudioStreamPlayer2D) this.gameOver.FindNode("GameOverSound");
        this.trophy          = (Sprite) this.gameOver.FindNode("Trophy");
        this.finalScore      = (RichTextLabel) this.gameOver.FindNode("FinalScore");
        this.bestScore       = (RichTextLabel) this.gameOver.FindNode("BestScore");
        this.playAgainButton = (Button) this.gameOver.FindNode("PlayAgainButton");
        this.exitButton      = (Button) this.gameOver.FindNode("ExitButton");
        
        this.Cob          = (Cob) this.GetParent().FindNode("Cob");
        this.TimerBar     = (TimerBar) this.GetParent().FindNode("TimerBar");
        this.PauseScreen  = (Control) this.GetParent().FindNode("PauseScreen");
        this.DialogScreen = (DialogScreen) this.GetParent().FindNode("DialogScreen");

        this.Cob.Connect("swiped", this, "UpdateScore");
        this.TimerBar.Connect("timeout", this, "GameOver");

        if (OS.GetName() == "Android" && SaveSystem.enableAds) {
            this.admob = (Godot.Object) this.GetParent().FindNode("AdMob");
            this.admob.Call("load_interstitial");
        }
    }

    public void _OnPlayAgainPressed() {
        if (OS.GetName() == "Android" && SaveSystem.enableAds) this.admob.Call("hide_banner");
        GetTree().ReloadCurrentScene();
    }

    public void _OnExitPressed() {
        if (OS.GetName() == "Android" && SaveSystem.enableAds) this.admob.Call("hide_banner");
        GetTree().ChangeScene("res://Scenes/Home.tscn");
    }

    public void _OnPausePressed() {
        if (OS.GetName() == "Android" && SaveSystem.enableAds) this.admob.Call("load_banner");
        GetTree().Paused         = true;
        this.Cob.Visible         = false;
        this.PauseScreen.Visible = true;
        this.PauseScreen.SetAsToplevel(true);
    }

    public void _OnResumePressed() {
        if (OS.GetName() == "Android" && SaveSystem.enableAds) this.admob.Call("hide_banner");
        GetTree().Paused         = false;
        this.Cob.Visible         = true;
        this.PauseScreen.Visible = false;
    }

    public async void UpdateScore(int point) {
        this.points += point;

        if (point < 0) {
            this.UpdateMisses();
            this.pointCounter.BbcodeText = $"[shake level=7][center]{this.points}[/center][/shake]";
            await ToSignal(GetTree().CreateTimer(1f, false), "timeout");
        } else {
            this.pointCounter.BbcodeText = $"[wave amp=20 freq=20][center]{this.points}[/center][/wave]";
            this.pointSound.Play();
            await ToSignal(GetTree().CreateTimer(1f, false), "timeout");
        }

        this.pointCounter.BbcodeText = $"[wave amp=10 freq=2][center]{this.points}[/center][/wave]";
    }

    public void UpdateMisses() {
        this.misses += 1;
        if (this.misses < 3) this.missSound.Play();
        string missText = "";
        for (int i = 0; i < this.misses; i++) missText += "X ";
        this.missCounter.BbcodeText  = $"[shake level={2 * this.misses}][center]{missText}[/center][/shake]";

        if (this.misses == 3) {
            this.GameOver();
        }
    }

    // TODO :: Put this in Game controller?
    public async void GameOver() {
        SaveSystem.timesPlayed += 1;

        if (OS.GetName() == "Android" && SaveSystem.enableAds) {
            this.admob.Call("load_banner");
            if (SaveSystem.timesPlayed % 3 == 0) {
                this.admob.Call("show_interstitial");
            }
        }

        this.gameOverSound.Play();

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
        
        // Determine trophy and save high score
        if (this.points > SaveSystem.bestScore) {
            this.bestScore.BbcodeText = $"[center]New Best![/center]";
            this.trophy.Texture = this.trophyTexture;
            SaveSystem.bestScore = this.points;
        } else if (this.points > SaveSystem.bestScore / 2 && this.points <= SaveSystem.bestScore) {
            this.bestScore.BbcodeText = $"[center]Best: {SaveSystem.bestScore}[/center]";
            this.trophy.Texture = this.silverTrophyTexture;
        } else {
            this.bestScore.BbcodeText = $"[center]Best: {SaveSystem.bestScore}[/center]";
            this.trophy.Texture = this.emptyTrophyTexture;
        }

        SaveSystem.Save();

        // Wait a bit to show action buttons (ad retention purposes)
        await ToSignal(GetTree().CreateTimer(2.5f), "timeout");

        this.playAgainButton.Visible = true;
        this.exitButton.Visible      = true;
    }
}
