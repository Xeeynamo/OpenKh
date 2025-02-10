using System.Collections.Generic;
using Godot;
using OpenKh.Godot.Resources;

namespace OpenKh.Godot.Nodes;

[Tool]
public partial class KH2InterfaceLayoutPlayer : Node2D
{
    [Export] public KH2InterfaceLayout Layout;
    [Export]
    public int GroupIndex
    {
        get => _groupIndex;
        set
        {
            if (Layout is null) return;
            if (value == _groupIndex) return;
            if (value >= Layout.Groups.Count) return;
            if (value < 0) return;
            
            _groupIndex = value;
        }
    }

    private int _groupIndex;
    protected int PreviousGroupIndex = -1;
    public List<KH2InterfaceSequencePlayer> DeployedSequences = new();

    public override void _Process(double delta)
    {
        base._Process(delta);

        var deltaf = (float)delta;

        var currentGroup = Layout.Groups[GroupIndex];

        if (PreviousGroupIndex != GroupIndex)
        {
            PreviousGroupIndex = GroupIndex;
            
            foreach (var s in DeployedSequences) s.QueueFree();
            DeployedSequences.Clear();
            for (var index = 0; index < currentGroup.Sequences.Count; index++)
            {
                var sequence = currentGroup.Sequences[index];
                var animIndex = currentGroup.SequenceIndices[index];
                var position = currentGroup.SequencePositions[index];

                var sequenceNode = new KH2InterfaceSequencePlayer();
                AddChild(sequenceNode);

                sequenceNode.Sequence = sequence;
                sequenceNode.AnimationIndex = animIndex;
                sequenceNode.Position = position;
                sequenceNode.CurrentTime = currentGroup.ShowAtTime;
                
                DeployedSequences.Add(sequenceNode);
                
                sequenceNode.Update(deltaf);
            }
        }
    }
}
