using Godot;

namespace OpenKh.Godot.Resources;

[Tool]
public partial class KH2MovesetEntry : Resource
{
    [Export] public InterpolatedMotionResource Motion;
}
