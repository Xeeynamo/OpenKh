using System.Linq;
using Godot;
using OpenKh.Godot.Configuration;

namespace OpenKh.Godot.Tools.Launcher;

public partial class GameConfigurator : PanelContainer
{
    [Export] public OptionButton PlatformSelector;
    [Export] public LineEdit ManualPath;
    [Export] public Button PathOpen;
    [Export] public Label Label;
    [Export] public string GameIdentifier;
    [Export] public string LabelText;

    public static readonly PackedScene Packed = ResourceLoader.Load<PackedScene>("res://Scenes/Tools/Launcher/GameConfigurator.tscn");
    public static GameConfigurator Create(string identifier, string labelText)
    {
        var result = Packed.Instantiate<GameConfigurator>();
        result.GameIdentifier = identifier;
        result.LabelText = labelText;
        return result;
    }
    public override void _Ready()
    {
        base._Ready();

        var cfg = Config.Configs.FirstOrDefault(i => i.Identifier == GameIdentifier);
        if (cfg is null) return;

        Label.Text = LabelText;
        
        PlatformSelector.Clear();
        foreach (var pair in PlatformHelpers.Names) PlatformSelector.AddItem(pair.Value);

        PlatformSelector.Selected = (int)cfg.GamePlatform;
        ManualPath.Text = cfg.GamePath;
        
        PathOpen.Pressed += PathOpenOnPressed;
    }
    private void PathOpenOnPressed()
    {
        var window = new FileDialog();
        window.Access = FileDialog.AccessEnum.Filesystem;
        window.FileMode = FileDialog.FileModeEnum.OpenDir;
        window.UseNativeDialog = true;
        AddChild(window);
        window.PopupCentered();
        window.DirSelected += WindowOnDirSelected;
        window.DirSelected += _ =>
        {
            RemoveChild(window);
            window.QueueFree();
        };
        window.Canceled += () =>
        {
            RemoveChild(window);
            window.QueueFree();
        };
    }
    private void WindowOnDirSelected(string dir)
    {
        var steam = dir.Contains("SteamLibrary") || dir.Contains("steamapps");
        //THANKS SQUARE
        if (dir.Contains("KINGDOM HEARTS -HD 1.5 2.5 ReMIX-") && (steam)) dir = dir.Replace("KINGDOM HEARTS -HD 1.5 2.5 ReMIX-", "KINGDOM HEARTS -HD 1.5+2.5 ReMIX-");
        
        var cfg = Config.Configs.FirstOrDefault(i => i.Identifier == GameIdentifier);

        if (cfg is null) return;

        cfg.GamePath = dir;
        ManualPath.Text = dir;
        
        if (steam)
        {
            cfg.GamePlatform = Platform.Steam;
            PlatformSelector.Selected = (int)Platform.Steam;
        }
        
        cfg.Save();
    }
}
