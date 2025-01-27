using Godot;

namespace OpenKh.Godot.Resources
{
    [Tool]
    public abstract partial class BinaryResource<T> : Resource
    {
        [Export] public byte[] Binary;
        public abstract T Value { get; }
    }
}
