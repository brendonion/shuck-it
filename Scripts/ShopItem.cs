using Godot;

public class ShopItem : TextureButton {
    [Export]
    public string type = "background";

    [Export]
    public string value = "Background1.png";

    [Export]
    public string name = "Default";

    [Export]
    public string placeholder = $"??? background";

    [Export]
    public int price = 10;

    public override void _Ready() {}
}
