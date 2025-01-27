using Godot;
using Godot.Collections;

namespace OpenKh.Godot.Resources;

[Tool]
public partial class KH2InterfaceSpriteSequence : Resource
{
    [Export] public Array<Mesh> Sprites = new();
    [Export] public Texture2D Texture;
    [Export] public Array<KH2InterfaceSequenceAnimationGroup> AnimationList = new();
}
