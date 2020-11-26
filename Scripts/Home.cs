using Godot;

public class Home : Node2D {

    public Texture soundOnTexture  = (Texture) ResourceLoader.Load("res://Art/SoundOn.png");
    public Texture soundOffTexture = (Texture) ResourceLoader.Load("res://Art/SoundOff.png");

    public Button adsButton;
    public Button soundButton;

    public SaveSystem SaveSystem;

    public override void _Ready() {
        // Get singletons
        SaveSystem = (SaveSystem) FindNode("SaveSystem");

        this.adsButton   = (Button) FindNode("AdsButton");
        this.soundButton = (Button) FindNode("SoundButton");

        this.UpdateButtons();
    }

    public void _OnPlayPressed() {
        GetTree().ChangeScene("res://Scenes/Game.tscn");
    }

    public void _OnTutorialPressed() {
        GetTree().ChangeScene("res://Scenes/Tutorial.tscn");
    }

    public void _OnAdsPressed() {
        // this.enableAds = !this.enableAds;
        // this.SaveSettings();
        // this.UpdateButtons();
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
        this.adsButton.Disabled = !SaveSystem.enableAds;
        if (SaveSystem.enableSound) {
            this.soundButton.Icon = this.soundOnTexture;
            AudioServer.SetBusMute(0, false);
        } else {
            this.soundButton.Icon = this.soundOffTexture;
            AudioServer.SetBusMute(0, true);
        }
    }
}
