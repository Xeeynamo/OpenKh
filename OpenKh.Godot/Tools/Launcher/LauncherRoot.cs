using Godot;

namespace OpenKh.Godot.Tools.Launcher
{
    public partial class LauncherRoot : PanelContainer
    {
        [ExportGroup("Config")] 
        [Export] public GameConfigurator HDRemixConfig;

        public override void _Ready()
        {
            base._Ready();
            
        }
    }
}
