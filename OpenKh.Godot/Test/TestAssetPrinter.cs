using Godot;
using OpenKh.Godot.Storage;

namespace OpenKh.Godot.Test
{
    public partial class TestAssetPrinter : Node
    {
        public override void _Ready()
        {
            base._Ready();
            foreach (var f in PackFileSystem.GetFiles(Game.Kh2)) GD.Print(f);
        }
    }
}
