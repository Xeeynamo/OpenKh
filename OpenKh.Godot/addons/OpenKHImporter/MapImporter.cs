#if TOOLS

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Godot.Collections;
using OpenKh.Common;
using OpenKh.Godot.Conversion;
using OpenKh.Godot.Helpers;
using OpenKh.Godot.Nodes;
using OpenKh.Godot.Resources;
using OpenKh.Imaging;
using OpenKh.Kh2;
using OpenKh.Kh2.Models;
using OpenKh.Kh2.TextureFooter;
using Array = Godot.Collections.Array;
using FileAccess = System.IO.FileAccess;

namespace OpenKh.Godot.addons.OpenKHImporter;

public partial class MapImporter : EditorImportPlugin
{
    public enum Presets
    {
        Default,
    }

    public override string _GetImporterName() => "MapImporter";
    public override string _GetVisibleName() => "KH2 Map Importer";
    public override string[] _GetRecognizedExtensions() => ["map"];
    public override string _GetSaveExtension() => "scn";
    public override string _GetResourceType() => "PackedScene";
    public override float _GetPriority() => 1;
    public override int _GetImportOrder() => 0;
    public override int _GetPresetCount() => Enum.GetValues<Presets>().Length;
    public override string _GetPresetName(int presetIndex) => ((Presets)presetIndex).ToString();
    public override bool _GetOptionVisibility(string path, StringName optionName, Dictionary options) => true;
    public override Array<Dictionary> _GetImportOptions(string path, int presetIndex) => base._GetImportOptions(path, presetIndex);

    public override Error _Import(string sourceFile, string savePath, Dictionary options, Array<string> platformVariants, Array<string> genFiles)
    {
        var realPath = ProjectSettings.GlobalizePath(sourceFile);
        
        var name = Path.GetFileNameWithoutExtension(realPath);
        
        var relativePath = Path.GetRelativePath(ImportHelpers.Kh2ImportOriginalPath, realPath);

        var hdTexturesPath = Path.Combine(ImportHelpers.Kh2ImportRemasteredPath, relativePath);
        
        var usesHdTextures = false;
        var hdTextures = new System.Collections.Generic.Dictionary<int, string>();
        
        if (Directory.Exists(hdTexturesPath))
        {
            usesHdTextures = true;
            foreach (var filePath in Directory.GetFiles(hdTexturesPath))
            {
                var textureName = Path.GetFileName(filePath);
                
                if (!textureName.StartsWith('-') || !int.TryParse(textureName[1..^4], out var number)) continue;

                var localPath = ProjectSettings.LocalizePath(filePath);
                hdTextures[number] = localPath;
            }
        }
        
        using var stream = File.Open(realPath, FileMode.Open, FileAccess.Read);

        stream.Seek(0, SeekOrigin.Begin);

        if (!Bar.IsValid(stream))
            return Error.Failed;
        
        var barFile = Bar.Read(stream);
        
        var images = usesHdTextures ? Enumerable.Repeat((Texture2D)null, hdTextures.Max(i => i.Key) + 1).ToList() : null;

        if (usesHdTextures)
        {
            foreach (var index in hdTextures)
            {
                try
                {
                    var texture = ResourceLoader.Load<Texture2D>(index.Value);
                    images[index.Key] = texture;
                }
                catch
                {
                    images[index.Key] = null;
                    // ignored
                }
            }
        }
        
        var result = ModelConverters.FromMap(barFile, images);
        result.Name = name;
        result.SetOwner();
        
        var packed = new PackedScene();
        packed.Pack(result);
        ResourceSaver.Save(packed, $"{savePath}.{_GetSaveExtension()}");

        return Error.Ok;
    }
}

#endif
