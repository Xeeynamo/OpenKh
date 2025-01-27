using Godot;
using Godot.Collections;

namespace OpenKh.Godot.Resources;

[Tool]
public partial class KH2TextureAnimation : Resource
{
    [Export] public int TextureIndex;
    [Export] public int SpriteFrameCount;
    [Export] public int DefaultAnimationIndex;
    [Export] public Array<KH2TextureAnimations> AnimationList;
    
    /*
    [Export] public int CurrentAnimation;
    [Export] public int CurrentAnimationFrame;
    [Export] public float AnimationTimer;
    [Export] public float RandomAnimationTime = -1;
    */

    /*
    [Export]
    public int SetCurrentAnimation
    {
        get => CurrentAnimation;
        set
        {
            AnimationTimer = 0;
            RandomAnimationTime = -1;
            CurrentAnimationFrame = 0;
            var index = Mathf.Clamp(value, 0, AnimationList.Count - 1);
            CurrentAnimation = index;
        }
    }
    */
    public Vector2 GetMaterialFrameParameter(int frame) => new(frame / (float)SpriteFrameCount, (frame+1) / (float)SpriteFrameCount);
}
