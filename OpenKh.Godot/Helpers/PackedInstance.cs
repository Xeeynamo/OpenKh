using Godot;

namespace OpenKh.Godot.Helpers
{
    public class PackedInstance<T> where T : Node
    {
        public readonly PackedScene Scene;
        public PackedInstance(string path) => Scene = ResourceLoader.Load<PackedScene>(path);
        public T Create() => Scene.Instantiate<T>();
    }
}
