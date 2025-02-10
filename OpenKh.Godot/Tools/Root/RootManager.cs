using Godot;
using OpenKh.Godot.Tools.Launcher;

namespace OpenKh.Godot.Tools.Root
{
    public partial class RootManager : Node
    {
        [Export] public LauncherRoot Launcher;
        public Node CurrentTool
        {
            get => _currentTool;
            set
            {
                if (_currentTool == value) return;
                RemoveChild(_currentTool);
                _currentTool.QueueFree();
                _currentTool = value;
                if (value == null)
                {
                    Launcher.ProcessMode = ProcessModeEnum.Inherit;
                    Launcher.Visible = true;
                }
                else
                {
                    Launcher.ProcessMode = ProcessModeEnum.Disabled;
                    Launcher.Visible = false;
                    AddChild(_currentTool);
                }
            }
        }
        private Node _currentTool;
    }
}
