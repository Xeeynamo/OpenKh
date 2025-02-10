using System.IO;
using Godot;
using OpenKh.Kh2;

namespace OpenKh.Godot.Resources
{
    [Tool]
    public partial class ModelCollisionResource : BinaryResource<ModelCollision>
    {
        public override ModelCollision Value => _value ??= new ModelCollision(new MemoryStream(Binary));
        private ModelCollision _value;
    }
}
