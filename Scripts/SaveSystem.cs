using Godot;
using Godot.Collections;

public class SaveSystem : Node {

    public const string CONFIG_FILE = "res://config.cfg";

    // Settings sectiom
    public bool enableAds = true;
    public bool enableSound = true;

    // Game section
    public int bestScore = 0;
    public int kernels = 0;

    // Shop section
    public Array<string> unlockedBackgrounds = new Array<string>() {
        "background_1.png"
    };
    public Array<string> unlockedSkins = new Array<string>() {
        "skin_1.png"
    };
    public Array<string> lockedBackgrounds = new Array<string>() {
        "background_2.png",
        "background_3.png",
        "background_4.png",
        "background_5.png",
    };
    public Array<string> lockedSkins = new Array<string>() {
        "skin_2.png",
        "skin_3.png",
    };

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

        Set("unlockedBackgrounds", config.GetValue("shop", "unlocked_backgrounds"));
        Set("unlockedSkins", config.GetValue("shop", "unlocked_skins"));
        Set("unlockedBackgrounds", config.GetValue("shop", "locked_backgrounds"));
        Set("unlockedSkins", config.GetValue("shop", "locked_skins"));
    }

    public void Save() {
        var config = new ConfigFile();

        config.SetValue("settings", "enable_ads", this.enableAds);
        config.SetValue("settings", "enable_sound", this.enableSound);

        config.SetValue("game", "best_score", this.bestScore);
        config.SetValue("game", "kernels", this.kernels);

        config.SetValue("shop", "unlocked_backgrounds", this.unlockedBackgrounds);
        config.SetValue("shop", "unlocked_skins", this.unlockedSkins);
        config.SetValue("shop", "locked_backgrounds", this.lockedBackgrounds);
        config.SetValue("shop", "locked_skins", this.lockedSkins);

        config.Save(CONFIG_FILE);
    }
}
