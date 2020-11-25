using Godot;

public class Shop : Control {

    public string saveFile = "user://savegame.save";
    
    public bool allowPress = true;

    public int best; // ignored
    public int kernels;

    public RichTextLabel kernelCount;

    public ScrollContainer scrollContainer;
    
    public ConfirmationDialog confirmation;

    public override void _Ready() {
        this.kernelCount     = (RichTextLabel) FindNode("KernelCount");
        this.scrollContainer = (ScrollContainer) FindNode("ScrollContainer");
        this.confirmation    = (ConfirmationDialog) FindNode("ConfirmationDialog");
        this.confirmation.GetCloseButton().Visible = false;

        this.Load();
        this.SetKernelCount();
    }

    public void _OnBackPressed() {
        GetTree().ChangeScene("res://Scenes/Home.tscn");
    }

    public void _OnItemPressed() {
        if (this.allowPress) {
            this.scrollContainer.SetProcessInput(false);
            this.confirmation.PopupCentered();
            this.confirmation.SetGlobalPosition(
                new Vector2(this.confirmation.RectGlobalPosition.x, this.confirmation.RectGlobalPosition.y + 20)
            );
        }
    }

    public void _OnPopupHide() {
        this.scrollContainer.SetProcessInput(true);
    }

    public void _OnPopupConfirmed() {
        GD.Print("PURCHASED!");
    }

    public void _OnScrollStarted() {
        this.allowPress = false;
    }

    public async void _OnScrollEnded() {
        await ToSignal(GetTree().CreateTimer(0.05f), "timeout");
        this.allowPress = true;
    }

    public void Load() {
        var saveGame = new File();
        if (!saveGame.FileExists(this.saveFile))
            return; // Error!  We don't have a save to load.
        
        saveGame.Open(this.saveFile, File.ModeFlags.Read);

        Set("best", saveGame.GetVar());
        Set("kernels", saveGame.GetVar());

        saveGame.Close();
    }

    public void Save() {
        var saveGame = new File();
        saveGame.Open(this.saveFile, File.ModeFlags.Write);

        saveGame.StoreVar(this.best);    // index 0
        saveGame.StoreVar(this.kernels); // index 1

        saveGame.Close();
    }

    public void SetKernelCount() {
        this.kernelCount.BbcodeText = $"[right]{this.kernels}[/right]";
    }
}
