using System.Linq;
using Godot;
using Godot.Collections;
using OpenKh.Godot.Resources;

namespace OpenKh.Godot.Nodes;

[Tool]
public partial class KH2MeshInstance3D : MeshInstance3D
{
    [Export] public Array<KH2TextureAnimation> TextureAnimations = new();
    
    [Export] public Array<int> TextureAnimationsCurrentAnimation = new();
    [Export] public Array<int> TextureAnimationsCurrentAnimationFrame = new();
    [Export] public Array<float> TextureAnimationsAnimationTimer = new();
    [Export] public Array<float> TextureAnimationsRandomAnimationTime = new();

    [Export] public Array<Texture2D> Textures = new();
    [Export] public Array<Texture2D> AnimatedTextures = new();
    public KH2TextureAnimationFrame CurrentFrame(int index)
    {
        var anim = TextureAnimations[index];
        return anim.AnimationList[TextureAnimationsCurrentAnimation[index]].Frames[TextureAnimationsCurrentAnimationFrame[index]];
    }
    public void SetTextureAnimationFrame(int index, int frame)
    {
        if (index > TextureAnimations.Count) return;
        var anim = TextureAnimations[index];
        if (frame >= anim.SpriteFrameCount) return;
        var pos = anim.GetMaterialFrameParameter(frame);
        SetInstanceShaderParameter($"TextureFrame{index}", pos);
    }
    public override void _Ready()
    {
        base._Ready();
        //GD.Print(TextureAnimations.Count);
        for (var index = 0; index < TextureAnimations.Count; index++) SetTextureAnimationFrame(index, 0);
        TextureAnimationsCurrentAnimation = new Array<int>(TextureAnimations.Select(i => i.DefaultAnimationIndex));
        TextureAnimationsCurrentAnimationFrame = new Array<int>(Enumerable.Repeat(0, TextureAnimations.Count));
        TextureAnimationsAnimationTimer = new Array<float>(Enumerable.Repeat(0f, TextureAnimations.Count));
        TextureAnimationsRandomAnimationTime = new Array<float>(Enumerable.Repeat(0f, TextureAnimations.Count));
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        UpdateAnimatedTextures((float)delta);
    }
    private void SetAnimationStateFrame(int index, int frame)
    {
        TextureAnimationsAnimationTimer[index] = 0;
        TextureAnimationsRandomAnimationTime[index] = -1;
        TextureAnimationsCurrentAnimationFrame[index] = frame;
        
        var currentFrame = CurrentFrame(index);
        switch (currentFrame.Operation)
        {
            case KH2TextureAnimationOperation.EnableSprite:
                SetInstanceShaderParameter($"UseSprite{index}", true);
                SetTextureAnimationFrame(index, currentFrame.ImageIndex);
                break;
            case KH2TextureAnimationOperation.DisableSprite:
                SetInstanceShaderParameter($"UseSprite{index}", false);
                break;
            case KH2TextureAnimationOperation.Jump:
                SetAnimationStateFrame(index, TextureAnimationsCurrentAnimationFrame[index] + currentFrame.JumpDelta);
                break;
        }
    }
    private void UpdateAnimatedTextures(float delta)
    {
        for (var index = 0; index < TextureAnimations.Count; index++)
        {
            var currentFrame = CurrentFrame(index);
            if (currentFrame.Operation == KH2TextureAnimationOperation.Stop) continue;
            if (TextureAnimationsRandomAnimationTime[index] == -1) TextureAnimationsRandomAnimationTime[index] = (float)GD.RandRange(currentFrame.MinTime, currentFrame.MaxTime);
            TextureAnimationsAnimationTimer[index] += delta;
            if (TextureAnimationsAnimationTimer[index] >= TextureAnimationsRandomAnimationTime[index]) SetAnimationStateFrame(index, TextureAnimationsCurrentAnimationFrame[index] + 1);
        }
    }
}
