using System.IO;
using Godot;
using OpenKh.Kh2;

namespace OpenKh.Godot.Resources
{
    [Tool]
    public partial class AnimationBinaryResource : BinaryResource<AnimationBinary>
    {
        public override AnimationBinary Value => _value ??= new AnimationBinary(new MemoryStream(Binary));
        private AnimationBinary _value;
    }
}
