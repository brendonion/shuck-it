using Godot;
using Godot.Collections;

public class SaveSystem : Node {

    public const string CONFIG_FILE = "res://config.cfg";

    // Settings sectiom
    public bool enableAds   = true;
    public bool enableSound = true;

    // Game section
    public int bestScore   = 0;
    public int kernels     = 0;
    public int timesPlayed = 0;

    // Shop section
    public Array<string> unlockedBackgrounds = new Array<string>() {
        "Background1.png"
    };
    public Array<string> unlockedSkins = new Array<string>() {
        "Skin1.png"
    };
    public Array<string> lockedBackgrounds = new Array<string>() {
        "Background2.png",
        "Background3.png",
        "Background4.png",
        "Background5.png",
    };
    public Array<string> lockedSkins = new Array<string>() {
        "Skin2.png",
        "Skin3.png",
    };

    // Customization section
    public string currentBackground = "Background1.png";
    public string currentSkin       = "Skin1.png";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        Load();
    }

    public void Load() {
        var config = new ConfigFile();
        var err    = config.Load(CONFIG_FILE);

        // If error not OK then return
        if (err != Error.Ok) return;

        // Load data into member variables
        Set("enableAds", config.GetValue("settings", "enable_ads"));
        Set("enableSound", config.GetValue("settings", "enable_sound"));

        Set("bestScore", config.GetValue("game", "best_score"));
        Set("kernels", config.GetValue("game", "kernels"));
        Set("timesPlayed", config.GetValue("game", "times_played"));

        Set("unlockedBackgrounds", config.GetValue("shop", "unlocked_backgrounds"));
        Set("unlockedSkins", config.GetValue("shop", "unlocked_skins"));
        Set("lockedBackgrounds", config.GetValue("shop", "locked_backgrounds"));
        Set("lockedSkins", config.GetValue("shop", "locked_skins"));

        Set("currentBackground", config.GetValue("customization", "current_background"));
        Set("currentSkin", config.GetValue("customization", "current_skin"));
    }

    public void Save() {
        var config = new ConfigFile();

        config.SetValue("settings", "enable_ads", this.enableAds);
        config.SetValue("settings", "enable_sound", this.enableSound);

        config.SetValue("game", "best_score", this.bestScore);
        config.SetValue("game", "kernels", this.kernels);
        config.SetValue("game", "times_played", this.timesPlayed);

        config.SetValue("shop", "unlocked_backgrounds", this.unlockedBackgrounds);
        config.SetValue("shop", "unlocked_skins", this.unlockedSkins);
        config.SetValue("shop", "locked_backgrounds", this.lockedBackgrounds);
        config.SetValue("shop", "locked_skins", this.lockedSkins);

        config.SetValue("customization", "current_background", this.currentBackground);
        config.SetValue("customization", "current_skin", this.currentSkin);

        config.Save(CONFIG_FILE);
    }
}
