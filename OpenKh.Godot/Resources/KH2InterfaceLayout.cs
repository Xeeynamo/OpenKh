using Godot;
using Godot.Collections;

namespace OpenKh.Godot.Resources;

[Tool]
public partial class KH2InterfaceLayout : Resource
{
    [Export] public Array<KH2InterfaceSequenceGroup> Groups = new();
}
