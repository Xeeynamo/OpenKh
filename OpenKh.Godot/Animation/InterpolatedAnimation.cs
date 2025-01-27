using Godot;

namespace OpenKh.Godot.Animation
{
    public partial class InterpolatedAnimation : Resource
    {
        [Export] public float TargetFPS; //within this class, this does nothing. this is intended for eventually serializing back to an anb
        [Export] public int BoneCount;
        [Export] public int AdditionalBoneCount;
        [Export] public float TimeStart;
        [Export] public float TimeEnd;
        [Export] public float TimeReturn;
        
        //TODO
    }
}
