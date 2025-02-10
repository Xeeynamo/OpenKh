using Godot;
using OpenKh.Godot.Storage;

namespace OpenKh.Godot.Test;


[Tool]
public partial class TestAssetPackImporter : Node3D
{
    public override void _Ready()
    {
        base._Ready();
        var mdlx = PackAssetLoader.GetMdlx("obj/P_EX100.mdlx");
        AddChild(mdlx);
    }
}
