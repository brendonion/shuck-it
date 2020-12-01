using Godot;

public class Shop : Control {
    
    public bool allowPress = true;

    public RichTextLabel kernelCount;

    public ScrollContainer scrollContainer;
    
    public AudioStreamPlayer2D purchaseSound;
    public AudioStreamPlayer2D missSound;

    public ConfirmationDialog confirmation;

    public ShopItem selectedItem;

    public Godot.Object admob;

    public SaveSystem SaveSystem;

    public override void _Ready() {
        // Get singletons
        SaveSystem = (SaveSystem) FindNode("SaveSystem");

        this.kernelCount     = (RichTextLabel) FindNode("KernelCount");
        this.scrollContainer = (ScrollContainer) FindNode("ScrollContainer");
        this.purchaseSound   = (AudioStreamPlayer2D) FindNode("PurchaseSound");
        this.missSound   = (AudioStreamPlayer2D) FindNode("MissSound");
        this.confirmation    = (ConfirmationDialog) FindNode("ConfirmationDialog");
        this.confirmation.GetCloseButton().Visible = false;

        this.UpdateKernelCount();
        this.UpdateShopItems();

        if (OS.GetName() == "Android" && SaveSystem.enableAds) {
            this.admob = (Godot.Object) FindNode("AdMob");
            this.admob.Call("load_banner");
        }
    }

    public override void _ExitTree() {
        if (OS.GetName() == "Android" && SaveSystem.enableAds) {
            this.admob.Call("hide_banner");
        }
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

            this.purchaseSound.Play();
            this.confirmation.Hide();

            this.UpdateKernelCount();
            this.UpdateShopItems();
        } else {
            this.confirmation.DialogText = "Not enough kernels!";
            this.missSound.Play();
        }
    }

    public async void _OnScrollStarted() {
        await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
        this.allowPress = false;
    }

    public async void _OnScrollEnded() {
        await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
        this.allowPress = true;
    }

    public void OpenPopup() {
        this.scrollContainer.SetProcessInput(false);
        this.confirmation.PopupCentered();
        this.confirmation.SetGlobalPosition(new Vector2(this.confirmation.RectGlobalPosition.x, this.confirmation.RectGlobalPosition.y + 20));
        this.confirmation.DialogText = $"Are you sure you want to purchase this {this.selectedItem.type} for {this.selectedItem.price} kernels?";
    }

    public void UpdateKernelCount() {
        this.kernelCount.BbcodeText = $"[right]{SaveSystem.kernels}[/right]";
    }

    public void UpdateShopItems() {
        Godot.Collections.Array shopItems = GetTree().GetNodesInGroup("shop_item");
        foreach (var node in shopItems) {
            ShopItem item = (ShopItem) node;

            if (SaveSystem.unlockedBackgrounds.Contains(item.value) || SaveSystem.unlockedSkins.Contains(item.value)) {
                ((RichTextLabel) item.FindNode("Name")).BbcodeText = $"[center]{item.name}[/center]";
                ((RichTextLabel) item.FindNode("Price")).BbcodeText = $"[center]Unequipped[/center]";
                if (item.type == "background") {
                    ((TextureRect) item.FindNode("TextureRect")).Texture = (Texture) ResourceLoader.Load($"res://Art/Unlockables/{item.value}");
                } else {
                    ((TextureRect) item.FindNode("TextureRect")).Texture = (Texture) ResourceLoader.Load($"res://Art/Unlockables/Good{item.value}");
                }
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
