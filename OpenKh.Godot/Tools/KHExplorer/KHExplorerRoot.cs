using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using OpenKh.Bbs;
using OpenKh.Godot.Conversion;
using OpenKh.Godot.Storage;
using OpenKh.Kh2;

namespace OpenKh.Godot.Tools.KHExplorer
{
    public partial class KHExplorerRoot : PanelContainer
    {
        [Export] public Tree FileTree;
        [Export] public Control PreviewArea;
        [Export] public AudioStreamPlayer AudioPlayer;
        [Export] public OptionButton GameSelector;
        [Export] public LineEdit SearchBar;
        [Export] public Label FileNameDisplay;
        [Export] public Game ActiveGame = Game.Kh2;
        
        public static AudioEffectSpectrumAnalyzerInstance Analyzer { get; private set; }

        public static readonly PackedScene Packed = ResourceLoader.Load<PackedScene>("res://Scenes/Tools/KHExplorer/ExplorerRoot.tscn");

        public override void _Ready()
        {
            base._Ready();

            CreateFileSystem();

            AudioPlayer.VolumeDb = Mathf.LinearToDb(0.5f);

            var analyzer = new AudioEffectSpectrumAnalyzer();
            AudioServer.AddBusEffect(0, analyzer);
            Analyzer = AudioServer.GetBusEffectInstance(0, 0) as AudioEffectSpectrumAnalyzerInstance;
            GD.Print(Analyzer is not null);

            foreach (var value in Enum.GetValues<Game>()) GameSelector.AddItem(value.ToString().ToUpper());
            GameSelector.Select((int)Game.Kh2);
            
            GameSelector.ItemSelected += GameSelectorOnItemSelected;
            FileTree.ItemActivated += FileTreeOnItemActivated;
            SearchBar.TextSubmitted += SearchBarOnTextSubmitted;
        }
        private void SearchBarOnTextSubmitted(string newtext) => RefreshSearchBar();
        private void GameSelectorOnItemSelected(long index)
        {
            FileTree.Clear();
            SearchBar.Text = "";
            RefreshSearchBar();
            ActiveGame = (Game)index;
            CreateFileSystem();
        }
        private void RefreshSearchBar() => RecursiveShow(FileTree.GetRoot(), SearchBar.Text);
        private bool RecursiveShow(TreeItem item, string text)
        {
            var children = item.GetChildren();
            var result = children.Select(i => RecursiveShow(i, text)).ToList();
            if (string.IsNullOrWhiteSpace(text) || result.Any(i => i) || item.GetText(0).Contains(text))
            {
                item.Visible = true;
                return true;
            }
            item.Visible = false;
            return false;
        }
        private void CreateFileSystem()
        {
            var files = PackFileSystem.GetFiles(ActiveGame).ToList();
            
            var dirMap = new DirectoryNode { Name = "Root" };
            
            foreach (var file in files) dirMap.Map(file);
            
            CreateTree(dirMap, FileTree.CreateItem());
            FileTree.HideRoot = true;
            FileTree.GetRoot().Collapsed = false;
        }
        private void FileTreeOnItemActivated()
        {
            var selected = FileTree.GetSelected();
            var meta = selected.GetMetadata(0);
            if (meta.VariantType == Variant.Type.String) ShowPreview(meta.AsString());
        }
        private void ShowPreview(string file)
        {
            foreach (var child in PreviewArea.GetChildren())
            {
                PreviewArea.RemoveChild(child);
                child.QueueFree();
            }
            //PackAssetLoader.ClearCache(); //don't leak memory
            AudioPlayer.Stop();
            AudioPlayer.Stream = null;
            AudioPlayer.StreamPaused = false;
            
            var f = PackFileSystem.Open(ActiveGame, file);
            var remasteredTextures = PackAssetLoader.GetHDTextures(f);
            
            if (PackAssetLoader.IsImage(file))
            {
                var viewer = ImageViewer.Packed.Create();

                var original = PackAssetLoader.GetTextureFromNameAndData(file, f.OriginalData);
                viewer.Texture = original;
                if (remasteredTextures.Count > 0) viewer.RemasteredTexture = remasteredTextures.First();
                
                PreviewArea.AddChild(viewer);
            }
            else if (file.EndsWith(".scd"))
            {
                var player = ScdViewer.Packed.Create();
                player.Playback = AudioPlayer;
                
                var scd = Converters.FromScd(Scd.Read(new MemoryStream(f.OriginalData)));
                
                player.SoundContainer = scd;
                if (ActiveGame == Game.Kh2)
                {
                    if (_trNames.Any(file.Contains)) player.Visualizer.VisualizerColorMode = AudioVisualizer.ColorMode.TimelessRiver;
                    else if (_htNames.Any(file.Contains)) player.Visualizer.VisualizerColorMode = AudioVisualizer.ColorMode.HalloweenTown;
                }
                PreviewArea.AddChild(player);
            }
            else if (file.EndsWith(".mdlx"))
            {
                var viewer = MdlxViewer.Packed.Create();
                
                var bar = Bar.Read(new MemoryStream(f.OriginalData));
                var originalMdlx = ModelConverters.FromMdlx(bar);
                viewer.Mdlx = originalMdlx;
                
                if (remasteredTextures.Count > 0)
                {
                    var remasteredMdlx = ModelConverters.FromMdlx(bar, remasteredTextures);
                    viewer.RemasteredMdlx = remasteredMdlx;
                }
                
                PreviewArea.AddChild(viewer);
            }
            
            FileNameDisplay.Text = file;
        }
        //:)
        private static readonly string[] _trNames = [ "music189", "music190", "music517", "music521" ];
        private static readonly string[] _htNames = [ "music064", "music065", "music144", "music149" ];
        private static void CreateTree(DirectoryNode node, TreeItem item)
        {
            item.SetText(0, node.Name);
            item.Collapsed = true;
            foreach (var dir in node.Subdirectories) CreateTree(dir, item.CreateChild());
            foreach (var file in node.Files.OrderBy(i => i.str))
            {
                var fileItem = item.CreateChild();
                fileItem.SetText(0, file.str);
                fileItem.SetMetadata(0, file.realStr);
            }
        }
        private class DirectoryNode
        {
            public string Name;
            public List<DirectoryNode> Subdirectories = new();
            public List<(string str, string realStr)> Files = new();
            public void Map(string str, string realStr = null)
            {
                realStr ??= str;
                if (str.Contains('/'))
                {
                    var split = str.Split('/');
                    var nextDir = split.First();

                    var sub = Subdirectories.FirstOrDefault(i => i.Name == nextDir);
                    if (sub is null)
                    {
                        sub = new DirectoryNode { Name = nextDir };
                        Subdirectories.Add(sub);
                    }
                    sub.Map(string.Join("/", split.Skip(1)), realStr);
                }
                else if (Files.All(i => i.realStr != realStr)) Files.Add((str, realStr));
            }
        }
    }
}
