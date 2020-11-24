using Godot;

public class Home : Node2D {

    public string settingsFile = "user://settings.save";

    public bool enableAds   = true;
    public bool enableSound = true;

    public Texture soundOnTexture  = (Texture) ResourceLoader.Load("res://Art/SoundOn.png");
    public Texture soundOffTexture = (Texture) ResourceLoader.Load("res://Art/SoundOff.png");

    public Button adsButton;
    public Button soundButton;

    public override void _Ready() {
        this.adsButton   = (Button) FindNode("AdsButton");
        this.soundButton = (Button) FindNode("SoundButton");

        this.LoadSettings();
        this.RegisterButtons();
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
        // this.RegisterButtons();
    }

    public void _OnSoundPressed() {
        this.enableSound = !this.enableSound;
        this.SaveSettings();
        this.RegisterButtons();
    }

    public void _OnShopPressed() {
        GetTree().ChangeScene("res://Scenes/Shop.tscn");
    }

    public void RegisterButtons() {
        this.adsButton.Disabled = !this.enableAds;
        if (this.enableSound) {
            this.soundButton.Icon = this.soundOnTexture;
            AudioServer.SetBusMute(0, false);
        } else {
            this.soundButton.Icon = this.soundOffTexture;
            AudioServer.SetBusMute(0, true);
        }
    }

    public void SaveSettings() {
        var settings = new File();
        settings.Open(settingsFile, File.ModeFlags.Write);
        settings.StoreVar(this.enableAds);
        settings.StoreVar(this.enableSound);
        settings.Close();
    }

    public void LoadSettings() {
        var settings = new File();
        if (!settings.FileExists(this.settingsFile))
            return;

        settings.Open(settingsFile, File.ModeFlags.Read);

        Set("enableAds", settings.GetVar());
        Set("enableSound", settings.GetVar());

        settings.Close();
    }
}
