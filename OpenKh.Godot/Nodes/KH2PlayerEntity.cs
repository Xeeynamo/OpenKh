using Godot;

namespace OpenKh.Godot.Nodes
{
    public partial class KH2PlayerEntity : KH2Entity
    {
        [Export] public Camera3D Camera { get; private set; }

        public override void _Ready()
        {
            base._Ready();
            if (Camera is null)
            {
                Camera = new Camera3D();
                AddChild(Camera);
            }
        }
    }
}
