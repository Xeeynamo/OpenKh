using Godot;
using Godot.Collections;

namespace OpenKh.Godot.Resources;

[Tool]
public partial class SoundContainer : Resource
{
    [Export] public Array<SoundResource> Sounds = new();
}
