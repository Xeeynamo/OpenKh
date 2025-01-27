using System.Numerics;

namespace OpenKh.Godot.Animation
{
    public struct KH2Bone
    {
        public int Sibling;
        public int Parent;
        public int Child;
        public int Flags;
        public Vector4 Scale;
        public Vector4 Rotation;
        public Vector4 Position;
    }
}
