using Godot;
using Godot.Collections;

namespace OpenKh.Godot.Resources;

[Tool]
public partial class KH2InterfaceSequenceGroup : Resource
{
    [Export] public Array<KH2InterfaceSpriteSequence> Sequences = new();
    [Export] public Array<int> SequenceIndices = new();
    [Export] public Array<Vector2> SequencePositions = new();
    [Export] public float ShowAtTime;
}
