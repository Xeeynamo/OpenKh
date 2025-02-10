using System.IO;
using System.Linq;
using Godot;
using OpenKh.Godot.Storage;

namespace OpenKh.Godot.Test;

[Tool]
public partial class TestEntityModelLoader : Node3D
{
    [Export]
    public int Entity
    {
        get => _entity;
        set
        {
            if (value == _entity) return;
            if (value < 0) return;

            var diff = value - _entity;

            if (KH2ObjectEntryTable.Entries.ContainsKey(value)) _entity = value;
            else
            {
                var direction = diff > 0;
                _entity = KH2ObjectEntryTable.Entries.FirstOrDefault(i => direction ? i.Key > value : i.Key < value).Key;
            }

            foreach (var c in GetChildren())
            {
                RemoveChild(c);
                c.QueueFree();
            }

            var objEntry = KH2ObjectEntryTable.Entries[_entity];

            var modelPath = $"obj/{objEntry.ModelName}.mdlx";
            var animationPath = $"obj/{objEntry.AnimationName}";
            
            var mdlx = PackAssetLoader.GetMdlx(modelPath);
            var moveset = PackAssetLoader.GetMoveset(animationPath);

            AddChild(mdlx);

            mdlx.Skeleton.CurrentAnimation = moveset.Entries.FirstOrDefault(i => i?.Motion is not null)?.Motion;
            mdlx.Skeleton.Animating = true;

            //GD.Print(objEntry.ModelName);
            //GD.Print(objEntry.AnimationName);
        }
    }
    private int _entity;

    public override void _Ready()
    {
        base._Ready();

        var a = KH2Preferences.Commands is null;
    }
}
