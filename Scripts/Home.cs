using Godot;

public class Home : Node2D {

    public Texture soundOnTexture  = (Texture) ResourceLoader.Load("res://Art/SoundOn.png");
    public Texture soundOffTexture = (Texture) ResourceLoader.Load("res://Art/SoundOff.png");

    public Button adsButton;
    public Button soundButton;

    public SaveSystem saveSystem;

    public override void _Ready() {
        this.saveSystem = (SaveSystem) GetNode("/root/SaveSystem");

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
        this.saveSystem.enableSound = !saveSystem.enableSound;
        this.saveSystem.Save();
        this.UpdateButtons();
    }

    public void _OnShopPressed() {
        GetTree().ChangeScene("res://Scenes/Shop.tscn");
    }

    public void UpdateButtons() {
        this.adsButton.Disabled = !this.saveSystem.enableAds;
        if (this.saveSystem.enableSound) {
            this.soundButton.Icon = this.soundOnTexture;
            AudioServer.SetBusMute(0, false);
        } else {
            this.soundButton.Icon = this.soundOffTexture;
            AudioServer.SetBusMute(0, true);
        }
    }
}
