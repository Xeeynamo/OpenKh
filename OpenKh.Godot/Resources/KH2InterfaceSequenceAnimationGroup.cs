using Godot;
using Godot.Collections;

namespace OpenKh.Godot.Resources;

[Tool]
public partial class KH2InterfaceSequenceAnimationGroup : Resource
{
    [Export] public Array<KH2InterfaceSequenceAnimation> Animations = new();
    [Export] public bool Loop;
    [Export] public float LoopStart = -1;
    [Export] public float LoopEnd = -1;
}
