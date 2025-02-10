using System.IO;
using Godot;
using OpenKh.Kh2;

namespace OpenKh.Godot.Resources
{
    [Tool]
    public partial class InterpolatedMotionResource : BinaryResource<Motion.InterpolatedMotion>
    {
        public override Motion.InterpolatedMotion Value => _value ??= new Motion.InterpolatedMotion(new MemoryStream(Binary));
        private Motion.InterpolatedMotion _value;
    }
}
