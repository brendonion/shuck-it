using Godot;

public class Dialog : RichTextLabel {

    public bool finished;
    public string presetText;
    public string newText = "";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        this.presetText    = this.Text;
        this.Text          = null;
        this.BbcodeEnabled = true;
    }

    public override void _PhysicsProcess(float delta) {
        if (this.Visible && this.newText == "") {
            this.SetNewText();
        }
    }

    public async void SetNewText() {
        char[] characters = this.presetText.ToCharArray();
        foreach (char character in characters) {
            string temp = this.newText.Replace("[center]", "").Replace("[/center]", "");
            this.newText = $"[center]{temp}{character}[/center]";
            this.BbcodeText = this.newText;
            if (character == '.') {
                await ToSignal(GetTree().CreateTimer(1f), "timeout");
            } else {
                await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
            }
        }
        await ToSignal(GetTree().CreateTimer(2f), "timeout");
        this.finished = true;
    }
}
