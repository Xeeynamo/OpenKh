using Godot;
using Godot.Collections;

namespace OpenKh.Godot.Nodes
{
    public partial class KH2Map : Node3D
    {
        [Export] public Dictionary<int, Array<Node3D>> Groups = new();
    }
}
