#if TOOLS

using System;
using System.IO;
using System.Linq;
using FFMpegCore;
using FFMpegCore.Pipes;
using Godot;
using Godot.Collections;
using OpenKh.Bbs;
using OpenKh.Godot.Conversion;
using OpenKh.Godot.Resources;

namespace OpenKh.Godot.addons.OpenKHImporter;

public partial class ScdImporter : EditorImportPlugin
{
    public enum Presets
    {
        Default,
    }
    public override string _GetImporterName() => "ScdImporter";
    public override string _GetVisibleName() => "KH Sound Container Importer";
    public override string[] _GetRecognizedExtensions() => ["scd"];
    public override string _GetSaveExtension() => "tres";
    public override string _GetResourceType() => "SoundContainer";
    public override float _GetPriority() => 1;
    public override int _GetImportOrder() => 0;
    public override int _GetPresetCount() => Enum.GetValues<Presets>().Length;
    public override string _GetPresetName(int presetIndex) => ((Presets)presetIndex).ToString();
    public override bool _GetOptionVisibility(string path, StringName optionName, Dictionary options) => true;
    public override Array<Dictionary> _GetImportOptions(string path, int presetIndex) => base._GetImportOptions(path, presetIndex);
    
    public override Error _Import(string sourceFile, string savePath, Dictionary options, Array<string> platformVariants, Array<string> genFiles)
    {
        using var stream = File.Open(ProjectSettings.GlobalizePath(sourceFile), FileMode.Open, System.IO.FileAccess.Read);
        
        var container = Converters.FromScd(Scd.Read(stream));

        ResourceSaver.Save(container, $"{savePath}.{_GetSaveExtension()}");

        return Error.Ok;
    }
}

#endif
