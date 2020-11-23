using Godot;

public class Score : Control {

    public int points = 0;
    public int misses = 0;
    public int best   = 0;

    public string saveFile = "user://savegame.save";

    public RichTextLabel pointCounter;
    public RichTextLabel missCounter;
    public RichTextLabel finalScore;
    public RichTextLabel bestScore;
    public Button pauseButton;
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

    public override void _Ready() {
        // Score tracking nodes
        this.pointCounter = (RichTextLabel) FindNode("Points");
        this.missCounter  = (RichTextLabel) FindNode("Misses");
        this.pauseButton  = (Button) FindNode("PauseButton");
        this.pointSound   = (AudioStreamPlayer2D) FindNode("PointSound");
        this.missSound    = (AudioStreamPlayer2D) FindNode("MissSound");
        
        // Game over nodes
        this.gameOver      = (Control) FindNode("Game Over");
        this.gameOverSound = (AudioStreamPlayer2D) this.gameOver.FindNode("GameOverSound");
        this.trophy        = (Sprite) this.gameOver.FindNode("Trophy");
        this.finalScore    = (RichTextLabel) this.gameOver.FindNode("FinalScore");
        this.bestScore     = (RichTextLabel) this.gameOver.FindNode("BestScore");
        
        this.Cob          = (Cob) this.GetParent().FindNode("Cob");
        this.TimerBar     = (TimerBar) this.GetParent().FindNode("TimerBar");
        this.PauseScreen  = (Control) this.GetParent().FindNode("PauseScreen");
        this.DialogScreen = (DialogScreen) this.GetParent().FindNode("DialogScreen");

        this.Cob.Connect("swiped", this, "UpdateScore");
        this.TimerBar.Connect("timeout", this, "GameOver");

        if (OS.GetName() == "Android") {
            this.admob = (Godot.Object) this.GetParent().FindNode("AdMob");
            this.admob.Call("load_banner");
            this.admob.Call("load_interstitial");
        }

        this.Load();
    }

    public void _OnPlayAgainPressed() {
        GetTree().ReloadCurrentScene();
    }

    public void _OnExitPressed() {
        GetTree().ChangeScene("res://Scenes/Home.tscn");
    }

    public void _OnPausePressed() {
        GetTree().Paused         = true;
        this.Cob.Visible         = false;
        this.PauseScreen.Visible = true;
        this.PauseScreen.SetAsToplevel(true);
    }

    public void _OnResumePressed() {
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
    public void GameOver() {
        if (OS.GetName() == "Android") {
            this.admob.Call("show_banner");
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
        if (this.points > this.best) {
            this.bestScore.BbcodeText = $"[center]New Best![/center]";
            this.trophy.Texture = this.trophyTexture;
            this.Save();
        } else if (this.points > this.best / 2 && this.points <= this.best) {
            this.bestScore.BbcodeText = $"[center]Best: {this.best}[/center]";
            this.trophy.Texture = this.silverTrophyTexture;
        } else {
            this.bestScore.BbcodeText = $"[center]Best: {this.best}[/center]";
            this.trophy.Texture = this.emptyTrophyTexture;
        }
    }

    public void Load() {
        var saveGame = new File();
        if (!saveGame.FileExists(this.saveFile))
            return; // Error!  We don't have a save to load.
        
        saveGame.Open(this.saveFile, File.ModeFlags.Read);

        while (saveGame.GetPosition() < saveGame.GetLen()) {
            var saveData = new Godot.Collections.Dictionary<string, object>((Godot.Collections.Dictionary) JSON.Parse(saveGame.GetLine()).Result);
            Set("best", saveData["BestScore"]);
        }

        saveGame.Close();
    }

    public void Save() {
        var saveGame = new File();
        saveGame.Open(this.saveFile, File.ModeFlags.Write);

        var dict = new Godot.Collections.Dictionary<string, object>() {
            { "BestScore", this.points },
        };

        saveGame.StoreLine(JSON.Print(dict));
        saveGame.Close();
    }
}
