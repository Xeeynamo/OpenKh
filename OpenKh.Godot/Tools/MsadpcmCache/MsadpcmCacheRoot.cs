using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using OpenKh.Bbs;
using OpenKh.Godot.Conversion;
using OpenKh.Godot.Storage;

namespace OpenKh.Godot.Tools.MsadpcmCache
{
    public partial class MsadpcmCacheRoot : PanelContainer
    {
        [Export] public Label DisplayLabel;
        [Export] public Button StartButton;

        private Stack<(string file, Game game)> _remainingPaths = new();
        private int _count;

        public override void _Ready()
        {
            base._Ready();
            StartButton.Pressed += StartButtonOnPressed;
        }
        private void StartButtonOnPressed()
        {
            foreach (var game in Enum.GetValues<Game>())
            {
                var files = PackFileSystem.GetFiles(game).Where(i => i.ToLower().EndsWith(".scd")).Distinct().ToList();
                foreach (var file in files) _remainingPaths.Push((file, game));
            }
            _count = _remainingPaths.Count;
            StartButton.Disabled = true;
        }
        public override void _Process(double delta)
        {
            base._Process(delta);

            var currentCount = _remainingPaths.Count;

            if (currentCount == 0)
            {
                DisplayLabel.Text = _count > 0 ? $"Done\n{_count}/{_count}" : "";
                return;
            }
            
            var next = _remainingPaths.Pop();
            
            var file = PackFileSystem.Open(next.game, next.file);
            if (file is null) return;

            var scd = Scd.Read(new MemoryStream(file.OriginalData));

            if (scd.StreamHeaders.Any(i => i.Codec == 12)) _ = Converters.FromScd(Scd.Read(new MemoryStream(file.OriginalData)));

            DisplayLabel.Text = $"{next.game.ToString().ToUpper()} {next.file}\n{_count - currentCount}/{_count}";
        }
    }
}
