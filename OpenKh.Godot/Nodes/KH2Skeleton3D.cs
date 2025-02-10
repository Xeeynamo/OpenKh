using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using OpenKh.Godot.Animation;
using OpenKh.Godot.Helpers;
using OpenKh.Godot.Resources;
using OpenKh.Kh2;

namespace OpenKh.Godot.Nodes;

[Tool]
public partial class KH2Skeleton3D : Skeleton3D
{
    [Export] public KH2MeshInstance3D Mesh;
    [Export] public ModelCollisionResource ModelCollisions;

    [Export] public int BoneCount = -1;
    [Export] public Array<int> BoneSibling = new();
    [Export] public Array<int> BoneParent = new();
    [Export] public Array<int> BoneChild = new();
    [Export] public Array<int> BoneFlags = new();
    [Export] public Array<Vector4> BoneRestScale = new();
    [Export] public Array<Vector4> BoneRestRotation = new();
    [Export] public Array<Vector4> BoneRestPosition = new();

    [Export] public InterpolatedMotionResource CurrentAnimation;
    [Export] public bool Animating = false;
    [Export] public float AnimationTime;
    [Export] public float AnimationDeltaMultiplier = 1;
    [Export] public bool AnimateLoop = true;

    public IEnumerable<KH2Bone> BoneList
    {
        get
        {
            return Enumerable.Range(0, BoneCount).Select(i => new KH2Bone
            {
                Sibling = BoneSibling[i],
                Parent = BoneParent[i],
                Child = BoneChild[i],
                Flags = BoneFlags[i],
                Scale = BoneRestScale[i].ToSystem(),
                Rotation = BoneRestRotation[i].ToSystem(),
                Position = BoneRestPosition[i].ToSystem(),
            });
        }
        set
        {
            if (value is null) return;
            var list = value.ToList();
            var count = list.Count;
            if (count != BoneCount)
            {
                BoneCount = count;
                BoneSibling = new Array<int>(Enumerable.Repeat(0, count));
                BoneParent = new Array<int>(Enumerable.Repeat(0, count));
                BoneChild = new Array<int>(Enumerable.Repeat(0, count));
                BoneFlags = new Array<int>(Enumerable.Repeat(0, count));
                BoneRestScale = new Array<Vector4>(Enumerable.Repeat(Vector4.Zero, count));
                BoneRestRotation = new Array<Vector4>(Enumerable.Repeat(Vector4.Zero, count));
                BoneRestPosition = new Array<Vector4>(Enumerable.Repeat(Vector4.Zero, count));
            }
            for (var i = 0; i < list.Count; i++)
            {
                var v = list[i];
                BoneSibling[i] = v.Sibling;
                BoneParent[i] = v.Parent;
                BoneChild[i] = v.Child;
                BoneFlags[i] = v.Flags;
                BoneRestScale[i] = v.Scale.ToGodot();
                BoneRestRotation[i] = v.Rotation.ToGodot();
                BoneRestPosition[i] = v.Position.ToGodot();
            }
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        if (Animating && Mesh is not null && CurrentAnimation is not null)
        {
            AnimationTime += ((float)delta) * AnimationDeltaMultiplier;
            if (AnimateLoop) AnimationTime = AnimationHelpers.Loop(CurrentAnimation.Value, AnimationTime);
            
            AnimationHelpers.ApplyInterpolatedMotion(CurrentAnimation.Value, this, AnimationTime);
        }
    }
}
