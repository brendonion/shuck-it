using Godot;

public class Shop : Control {

    public ScrollContainer scrollContainer;
    
    public ConfirmationDialog confirmation;

    public override void _Ready() {
        this.scrollContainer = (ScrollContainer) FindNode("ScrollContainer");
        this.confirmation    = (ConfirmationDialog) FindNode("ConfirmationDialog");
        this.confirmation.GetCloseButton().Visible = false;
    }

    public void _OnBackPressed() {
        GetTree().ChangeScene("res://Scenes/Home.tscn");
    }

    public void _OnItemPressed() {
        this.scrollContainer.SetProcessInput(false);
        this.confirmation.PopupCentered();
    }

    public void _OnPopupHide() {
        this.scrollContainer.SetProcessInput(true);
    }

    public void _OnPopupConfirmed() {
        GD.Print("PURCHASED!");
    }
}
