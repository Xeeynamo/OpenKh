using Godot;
using Godot.Collections;

namespace OpenKh.Godot.Resources;

public partial class KH2InterfaceSequence : Resource
{
    [Export] public Dictionary<string, KH2InterfaceSpriteSequence> Sequences = new();
}
