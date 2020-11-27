using Godot;

public class Shop : Control {
    
    public bool allowPress = true;

    public RichTextLabel kernelCount;

    public ScrollContainer scrollContainer;
    
    public ConfirmationDialog confirmation;

    public ShopItem selectedItem;

    public SaveSystem SaveSystem;

    public override void _Ready() {
        // Get singletons
        SaveSystem = (SaveSystem) FindNode("SaveSystem");

        this.kernelCount     = (RichTextLabel) FindNode("KernelCount");
        this.scrollContainer = (ScrollContainer) FindNode("ScrollContainer");
        this.confirmation    = (ConfirmationDialog) FindNode("ConfirmationDialog");
        this.confirmation.GetCloseButton().Visible = false;

        this.SetKernelCount();
        this.UpdateShopItems();
    }

    public void _OnBackPressed() {
        GetTree().ChangeScene("res://Scenes/Home.tscn");
    }

    public void _OnItemPressed(string nodeName) {
        if (!this.allowPress) return;

        this.selectedItem = (ShopItem) FindNode(nodeName);

        if (this.selectedItem.type == "background") {
            if (SaveSystem.lockedBackgrounds.Contains(this.selectedItem.value)) {
                this.OpenPopup();
            } else if (SaveSystem.unlockedBackgrounds.Contains(this.selectedItem.value) && SaveSystem.currentBackground != this.selectedItem.value) {
                SaveSystem.currentBackground = this.selectedItem.value;
                SaveSystem.Save();
                this.UpdateShopItems();
            }
        } else {
            if (SaveSystem.lockedSkins.Contains(this.selectedItem.value)) {
                this.OpenPopup();
            } else if (SaveSystem.unlockedSkins.Contains(this.selectedItem.value) && SaveSystem.currentSkin != this.selectedItem.value) {
                SaveSystem.currentSkin = this.selectedItem.value;
                SaveSystem.Save();
                this.UpdateShopItems();
            }
        }
    }

    public void _OnPopupHide() {
        this.scrollContainer.SetProcessInput(true);
    }

    public void _OnPopupConfirmed() {
        this.OpenPopup(); // Keep it open

        if (SaveSystem.kernels >= this.selectedItem.price) {
            SaveSystem.kernels -= this.selectedItem.price;
            if (this.selectedItem.type == "background") {
                SaveSystem.lockedBackgrounds.Remove(this.selectedItem.value);
                SaveSystem.unlockedBackgrounds.Add(this.selectedItem.value);
                SaveSystem.currentBackground = this.selectedItem.value;
            } else {
                SaveSystem.lockedSkins.Remove(this.selectedItem.value);
                SaveSystem.unlockedSkins.Add(this.selectedItem.value);
                SaveSystem.currentSkin = this.selectedItem.value;
            }
            SaveSystem.Save();

            this.confirmation.Hide();

            this.SetKernelCount();
            this.UpdateShopItems();
        } else {
            this.confirmation.DialogText = "Not enough kernels!";
        }
    }

    public void _OnScrollStarted() {
        this.allowPress = false;
    }

    public async void _OnScrollEnded() {
        await ToSignal(GetTree().CreateTimer(0.05f), "timeout");
        this.allowPress = true;
    }

    public void OpenPopup() {
        this.scrollContainer.SetProcessInput(false);
        this.confirmation.PopupCentered();
        this.confirmation.SetGlobalPosition(new Vector2(this.confirmation.RectGlobalPosition.x, this.confirmation.RectGlobalPosition.y + 20));
        this.confirmation.DialogText = $"Are you sure you want to purchase this {this.selectedItem.type} for {this.selectedItem.price} kernels?";
    }

    public void SetKernelCount() {
        this.kernelCount.BbcodeText = $"[right]{SaveSystem.kernels}[/right]";
    }

    public void UpdateShopItems() {
        Godot.Collections.Array shopItems = GetTree().GetNodesInGroup("shop_item");
        foreach (var node in shopItems) {
            ShopItem item = (ShopItem) node;

            if (SaveSystem.unlockedBackgrounds.Contains(item.value) || SaveSystem.unlockedSkins.Contains(item.value)) {
                ((RichTextLabel) item.FindNode("Name")).BbcodeText = $"[center]{item.name}[/center]";
                ((RichTextLabel) item.FindNode("Price")).BbcodeText = $"[center]Unequipped[/center]";
                ((TextureRect) item.FindNode("TextureRect")).Texture = ResourceLoader.Load<Texture>($"res://Art/Unlockables/{item.value}");
            } else {
                ((RichTextLabel) item.FindNode("Name")).BbcodeText = $"[center]{item.placeholder}[/center]";
                ((RichTextLabel) item.FindNode("Price")).BbcodeText = $"[center]Price: {item.price}[/center]";
            }

            if (item.value == SaveSystem.currentBackground || item.value == SaveSystem.currentSkin) {
                ((RichTextLabel) item.FindNode("Price")).BbcodeText = "[center]Equipped[/center]";
                item.Disabled = true;
            } else {
                item.Disabled = false;
            }
        }
    }
}
