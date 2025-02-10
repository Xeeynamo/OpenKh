using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using Godot.Collections;
using OpenKh.Godot.Conversion;
using OpenKh.Godot.Helpers;
using OpenKh.Kh2;

namespace OpenKh.Godot.addons.OpenKHImporter
{
    public partial class InterfaceSequenceImporter : EditorImportPlugin
    {
        public enum Presets
        {
            Default,
        }

        public override string _GetImporterName() => "InterfaceSequenceImporter";
        public override string _GetVisibleName() => "KH2 Interface Sequence Importer";
        public override string[] _GetRecognizedExtensions() => ["2dd"];
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
                    var textureNameNoExtension = Path.GetFileNameWithoutExtension(filePath);

                    var match = InterfaceTextureIndexMatch().Match(textureNameNoExtension);

                    if (!match.Success || match.Groups.Count < 2) continue;
                    if (!int.TryParse(match.Groups[2].Value, out var number)) continue;
                    
                        
                    var localPath = ProjectSettings.LocalizePath(filePath);
                    hdTextures[number] = localPath;
                }
            }
            
            using var stream = File.Open(realPath, FileMode.Open, System.IO.FileAccess.Read);

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
            
            GD.Print(name);
            
            barFile.PrintEntries();

            var result = Converters.FromInterfaceSequence(barFile, images);
            result.Name = name;
            result.SetOwner();

            var packed = new PackedScene();
            packed.Pack(result);
            ResourceSaver.Save(packed, $"{savePath}.{_GetSaveExtension()}");
            
            return Error.Ok;
        }
        [GeneratedRegex("(.+?)([0-9]+)$")]
        private static partial Regex InterfaceTextureIndexMatch();
    }
}
