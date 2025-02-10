using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using OpenKh.Godot.Resources;

namespace OpenKh.Godot.Nodes;

[Tool]
public partial class KH2InterfaceSequencePlayer : Node2D
{
    [Export] public bool UpdateTime = true;
    [Export] public float CurrentTime;
    [Export] public KH2InterfaceSpriteSequence Sequence;
    [Export]
    public int AnimationIndex
    {
        get => _animationIndex;
        set
        {
            if (Sequence is null) return;
            if (value == _animationIndex) return;
            if (value >= Sequence.AnimationList.Count) return;
            if (value < 0) return;
            _animationIndex = value;

            if (Engine.IsEditorHint()) CurrentTime = 0;
        }
    }
    private int _animationIndex;
    protected int LastAnimationIndex = -1;
    public List<KH2InterfaceSprite> DeployedSprites = new();
    public float CurrentTimeRounded;
    
    public override void _Process(double delta)
    {
        base._Process(delta);
        Update((float)delta);
    }
    public void Update(float deltaf)
    {
        var currentAnimationGroup = Sequence.AnimationList[AnimationIndex];

        if (LastAnimationIndex != AnimationIndex)
        {
            LastAnimationIndex = AnimationIndex;
            foreach (var s in DeployedSprites) s.QueueFree();
            DeployedSprites.Clear();
            foreach (var anim in currentAnimationGroup.Animations)
            {
                var sprite = KH2InterfaceSprite.Create(Sequence.Sprites[anim.SpriteIndex], Sequence.Texture);
                AddChild(sprite);
                sprite.Animation = anim;
                DeployedSprites.Add(sprite);
            }
        }

        if (UpdateTime)
        {
            CurrentTime += deltaf;
            if (currentAnimationGroup.Loop && CurrentTime >= currentAnimationGroup.LoopEnd) CurrentTime -= (currentAnimationGroup.LoopEnd - currentAnimationGroup.LoopStart);
            CurrentTimeRounded = Mathf.Snapped(CurrentTime, 1f / 60f);
        }
        foreach (var sprite in DeployedSprites) sprite.Animation.Apply(this, sprite);
    }
}
