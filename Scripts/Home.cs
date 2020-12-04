using Godot;

public class Home : Node2D {

    public Texture soundOnTexture  = (Texture) ResourceLoader.Load("res://Art/SoundOn.png");
    public Texture soundOffTexture = (Texture) ResourceLoader.Load("res://Art/SoundOff.png");

    public Button adsButton;
    public Button soundButton;

    public Godot.Object androidPayment;

    public SaveSystem SaveSystem;

    public override void _Ready() {
        // Get singletons
        SaveSystem = (SaveSystem) FindNode("SaveSystem");

        this.adsButton   = (Button) FindNode("AdsButton");
        this.soundButton = (Button) FindNode("SoundButton");

        if (OS.GetName() == "Android") {
            this.androidPayment = (Godot.Object) FindNode("AndroidPayment");
            this.androidPayment.Call("check_purchase");
        }

        this.UpdateButtons();
    }

    public void _OnPlayPressed() {
        GetTree().ChangeScene("res://Scenes/Game.tscn");
    }

    public void _OnTutorialPressed() {
        GetTree().ChangeScene("res://Scenes/Tutorial.tscn");
    }

    public void _OnAdsPressed() {
        if (OS.GetName() == "Android") {
            this.androidPayment.Call("purchase_item");
        }
    }

    public void _OnSoundPressed() {
        SaveSystem.enableSound = !SaveSystem.enableSound;
        SaveSystem.Save();
        this.UpdateButtons();
    }

    public void _OnShopPressed() {
        GetTree().ChangeScene("res://Scenes/Shop.tscn");
    }

    public void UpdateButtons() {
        this.adsButton.Visible = SaveSystem.enableAds;
        if (SaveSystem.enableSound) {
            this.soundButton.Icon = this.soundOnTexture;
            AudioServer.SetBusMute(0, false);
        } else {
            this.soundButton.Icon = this.soundOffTexture;
            AudioServer.SetBusMute(0, true);
        }
    }
}
