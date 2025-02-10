using Godot;
using Godot.Collections;

namespace OpenKh.Godot.Resources;

[Tool]
public partial class KH2TextureAnimations : Resource
{
    [Export] public Array<KH2TextureAnimationFrame> Frames;
}
