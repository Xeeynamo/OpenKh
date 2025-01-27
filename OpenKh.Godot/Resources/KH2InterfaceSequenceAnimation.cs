using System;
using Godot;
using OpenKh.Godot.Nodes;

namespace OpenKh.Godot.Resources;

[Tool]
public partial class KH2InterfaceSequenceAnimation : Resource
{
    [Export] public int SpriteIndex;
    [Export] public InterfaceAnimationFlags AnimationFlags;
    [Export] public float TimeStart;
    [Export] public float TimeEnd;
    [ExportSubgroup("Translate")] 
    [Export] public Vector2 TranslateStart;
    [Export] public Vector2 TranslateEnd;
    [ExportSubgroup("Rotation")] 
    [Export] public Vector3 RotationStart;
    [Export] public Vector3 RotationEnd;
    [ExportSubgroup("Scale")] 
    [Export] public Vector2 ScaleStart;
    [Export] public Vector2 ScaleEnd;
    [Export] public float ScaleUniformStart;
    [Export] public float ScaleUniformEnd;
    [ExportSubgroup("Pivot")] 
    [Export] public Vector2 PivotStart;
    [Export] public Vector2 PivotEnd;
    [ExportSubgroup("Curve")] 
    [Export] public Vector2 CurveStart;
    [Export] public Vector2 CurveEnd;
    [ExportSubgroup("Bounce")] 
    [Export] public Vector2 BounceStart;
    [Export] public Vector2 BounceEnd;
    [Export] public Vector2I BounceCount;
    [ExportSubgroup("Color")] 
    [Export] public int ColorBlend;
    [Export] public Color ColorStart;
    [Export] public Color ColorEnd;

    private static readonly ShaderMaterial MixAlphaMaterial = new(){ Shader = ResourceLoader.Load<Shader>("res://Assets/Shaders/KH2InterfaceMixShader.gdshader") };
    private static readonly ShaderMaterial AddAlphaMaterial = new(){ Shader = ResourceLoader.Load<Shader>("res://Assets/Shaders/KH2InterfaceAddShader.gdshader") };
    private static readonly ShaderMaterial SubAlphaMaterial = new(){ Shader = ResourceLoader.Load<Shader>("res://Assets/Shaders/KH2InterfaceSubShader.gdshader") };

    //TODO
    private static readonly float Epsilon = (1f / 60f) * 0.5f;
    //private static readonly float Epsilon = 0;

    public void Apply(KH2InterfaceSequencePlayer sequence, KH2InterfaceSprite sprite)
    {
        var time = sequence.CurrentTime;
        sprite.Mesh.Visible = true;
        
        if (time < (TimeStart - Epsilon) || time > (TimeEnd + Epsilon))
        {
            sprite.Mesh.Visible = false;
            return;
        }

        var material = ColorBlend switch
        {
            1 => AddAlphaMaterial,
            2 => SubAlphaMaterial,
            _ => MixAlphaMaterial
        };
        if (sprite.Mesh.Material != material) sprite.Mesh.Material = material;

        if (time < TimeStart) time = TimeStart;
        else if (time > TimeEnd) time = TimeEnd;
        
        var delta = Mathf.Remap(time, TimeStart, TimeEnd, 0, 1);

        var t = AnimationFlags.HasFlag(InterfaceAnimationFlags.DisableCurve) ? delta : (Mathf.Sin(delta * Mathf.Pi - Mathf.Pi / 2.0f) + 1.0f) / 2.0f;

        sprite.PositionNode.Position = AnimationFlags.HasFlag(InterfaceAnimationFlags.TranslationInterpolation) ? TranslateStart.Lerp(TranslateEnd, t) : TranslateStart;
        sprite.ScaleNode.Scale = AnimationFlags.HasFlag(InterfaceAnimationFlags.ScalingDisable) ? Vector2.One : Mathf.Lerp(ScaleUniformStart, ScaleUniformEnd, t) * ScaleStart.Lerp(ScaleEnd, t);

        sprite.Mesh.Modulate = !AnimationFlags.HasFlag(InterfaceAnimationFlags.ColorMask)
            ? !AnimationFlags.HasFlag(InterfaceAnimationFlags.ColorInterpolation) ? ColorStart.Lerp(ColorEnd, t) : ColorStart
            : Colors.White;

        if (!AnimationFlags.HasFlag(InterfaceAnimationFlags.PivotDisable))
        {
            sprite.PivotNode.Position = -PivotStart.Lerp(PivotEnd, t);
        }

        if (!AnimationFlags.HasFlag(InterfaceAnimationFlags.RotationDisable))
        {
            var rot = RotationStart.Lerp(RotationEnd, t);
            sprite.RotationNode1.Scale = sprite.RotationNode1.Scale with
            {
                X = Mathf.Cos(rot.Y),
                Y = Mathf.Cos(rot.X)
            };
            sprite.RotationNode2.Rotation = rot.Z;
        }

        if (!AnimationFlags.HasFlag(InterfaceAnimationFlags.BounceDisable))
        {
            var bounceXValue = Mathf.Sin(delta * BounceCount.X * Mathf.Pi);
            var bounceYValue = Mathf.Sin(delta * BounceCount.Y * Mathf.Pi);

            sprite.PositionNode.Position += new Vector2(bounceXValue, bounceYValue) * BounceStart.Lerp(BounceEnd, t);
        }
    }
}

[Flags]
public enum InterfaceAnimationFlags
{
    DisableCurve = 1 << 0,
    IsActive = 1 << 1,
    DisableBilinear = 1 << 2,
    BounceDelay = 1 << 3,
    BounceDisable = 1 << 4,
    RotationDisable = 1 << 5,
    ScalingDisable = 1 << 6,
    ColorInterpolation = 1 << 7,
    RotationInterpolation = 1 << 8,
    ScalingInterpolation = 1 << 9,
    ColorMask = 1 << 10,
    BounceInterpolation = 1 << 11,
    TranslationInterpolation = 1 << 12,
    PivotInterpolation = 1 << 13,
    PivotDisable = 1 << 14,
    LastCut = 1 << 15,
    TranslationDisable = 1 << 16,
    TranslationOffsetDisable = 1 << 17,
    PositionDisable = 1 << 18,
    Tag = 1 << 19,
}
