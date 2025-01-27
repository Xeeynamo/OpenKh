#if TOOLS

using System;
using System.IO;
using System.Linq;
using System.Numerics;
using Godot;
using Godot.Collections;
using OpenKh.Common;
using OpenKh.Godot.Helpers;
using OpenKh.Godot.Nodes;
using OpenKh.Godot.Resources;
using OpenKh.Kh2;
using OpenKh.Kh2.Models;
using OpenKh.Kh2Anim.Mset;
using Quaternion = Godot.Quaternion;
using Vector3 = Godot.Vector3;

namespace OpenKh.Godot.addons.OpenKHImporter;

//TODO: this takes wayyyy too long to import one file
public partial class MsetImporter : EditorImportPlugin
{
    public enum Presets
    {
        Default,
    }
    public override string _GetImporterName() => "MsetImporter";
    public override string _GetVisibleName() => "KH2 Moveset Importer";
    public override string[] _GetRecognizedExtensions() => ["mset"];
    public override string _GetSaveExtension() => "tres";
    public override string _GetResourceType() => "KH2Moveset";
    public override float _GetPriority() => 1;
    public override int _GetImportOrder() => 0;
    public override int _GetPresetCount() => Enum.GetValues<Presets>().Length;
    public override string _GetPresetName(int presetIndex) => ((Presets)presetIndex).ToString();
    public override bool _GetOptionVisibility(string path, StringName optionName, Dictionary options) => true;
    public override Array<Dictionary> _GetImportOptions(string path, int presetIndex) => base._GetImportOptions(path, presetIndex);

    public override Error _Import(string sourceFile, string savePath, Dictionary options, Array<string> platformVariants, Array<string> genFiles)
    {
        var realPath = ProjectSettings.GlobalizePath(sourceFile);
        
        using var stream = File.Open(realPath, FileMode.Open, System.IO.FileAccess.Read);

        if (!BinaryArchive.IsValid(stream)) return Error.InvalidData;
        
        var msetBinarc = BinaryArchive.Read(stream);
        
        var container = new KH2Moveset();

        container.Entries = new Array<KH2MovesetEntry>(msetBinarc.Entries.Select(entry => entry.Link < 0 || msetBinarc.Subfiles[entry.Link].Length == 0
            ? null
            : new KH2MovesetEntry
            { 
                Motion = new Func<InterpolatedMotionResource>(() =>
                {
                    var bar = Bar.Read(new MemoryStream(msetBinarc.Subfiles[entry.Link]));
                    var a = new InterpolatedMotionResource
                    {
                        Binary = bar.First(i => i.Type == Bar.EntryType.Motion).Stream.ReadAllBytes(),
                    };
                    return a;
                }).Invoke(),
            }));
        
        ResourceSaver.Save(container, $"{savePath}.{_GetSaveExtension()}");

        return Error.Ok;
    }
}

#endif
