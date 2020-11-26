using Godot;

public class Shop : Control {
    
    public bool allowPress = true;

    public RichTextLabel kernelCount;

    public ScrollContainer scrollContainer;
    
    public ConfirmationDialog confirmation;

    public SaveSystem SaveSystem;

    public override void _Ready() {
        // Get singletons
        SaveSystem = (SaveSystem) FindNode("SaveSystem");

        this.kernelCount     = (RichTextLabel) FindNode("KernelCount");
        this.scrollContainer = (ScrollContainer) FindNode("ScrollContainer");
        this.confirmation    = (ConfirmationDialog) FindNode("ConfirmationDialog");
        this.confirmation.GetCloseButton().Visible = false;

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

    public void SetKernelCount() {
        this.kernelCount.BbcodeText = $"[right]{SaveSystem.kernels}[/right]";
    }
}
