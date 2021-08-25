using Godot;

public class TimerBar : Node2D {

    public float timeLeft = 5f;

    public SceneTreeTimer timer;

    public Game Game;

    public Cob Cob;

    public Area2D bar;

    public TextureProgress barProgress;

    [Signal]
    public delegate void timeout();

    public override void _Ready() {
        // Randomize seed
        GD.Randomize();

        this.bar         = (Area2D) FindNode("Bar");
        this.barProgress = (TextureProgress) this.bar.FindNode("TextureProgress");

        this.Game = (Game) this.GetParent();
        this.Cob  = (Cob) this.Game.FindNode("Cob");

        this.Cob.Connect("swiped", this, "ClearTimer");
        this.Game.Connect("new_round", this, "SetTimer");
    }

    public override void _PhysicsProcess(float delta) {
        if (this.timer != null) {
            // Animate progress bar if timer has started
            if (this.timer.TimeLeft > 0f) {
                this.barProgress.Value = (this.timer.TimeLeft / this.timeLeft) * 100;
            }
            // Emit 'timeout' signal if timer has finished
            if (this.timer.TimeLeft <= 0f) {
                EmitSignal(nameof(timeout));
            }
        }
    }

    public void SetTimer(float duration = 0f) {
        // Set timer only if bar is visible
        if (this.Visible) {
            this.timeLeft = duration > 0f ? duration : this.timeLeft;
            this.barProgress.Value = 100;
            this.timer = GetTree().CreateTimer(this.timeLeft, false);
        }
    }

    public void ClearTimer(int point = 0) {
        if (this.Visible) {
            this.barProgress.Value = 100;
        }
        this.timer = null;
    }
}
