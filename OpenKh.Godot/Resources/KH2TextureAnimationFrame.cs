using Godot;

namespace OpenKh.Godot.Resources;

public enum KH2TextureAnimationOperation
{
    EnableSprite = 0,
    DisableSprite = 1,
    Jump = 2,
    Stop = 3,
}
[Tool]
public partial class KH2TextureAnimationFrame : Resource
{
    [Export] public int ImageIndex;
    /// <summary>
    /// The operation for the sprite animation
    /// </summary>
    [Export] public KH2TextureAnimationOperation Operation;
    /// <summary>
    /// For jump, what index offset to jump to
    /// </summary>
    [Export] public int JumpDelta;
    [Export] public float MinTime;
    [Export] public float MaxTime;
}
