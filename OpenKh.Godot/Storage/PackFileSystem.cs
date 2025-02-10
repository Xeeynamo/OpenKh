using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenKh.Common;
using OpenKh.Egs;
using OpenKh.Godot.Configuration;

namespace OpenKh.Godot.Storage
{
    public enum Game
    {
        Kh1,
        Kh2,
        Recom,
        Bbs,
        /*
        Theater,
        SettingsMenu,
        Mare,
        */
    }
    public static class PackFileSystem
    {
        private static readonly Dictionary<Game, string[]> _1525gameFileNames = new()
        {
            {
                Game.Kh1, 
                [
                    "kh1_first",
                    "kh1_second",
                    "kh1_third",
                    "kh1_fourth",
                    "kh1_fifth",
                ]
            },
            {
                Game.Kh2, 
                [
                    "kh2_first",
                    "kh2_second",
                    "kh2_third",
                    "kh2_fourth",
                    "kh2_fifth",
                    "kh2_sixth",
                ]
            },
            {
                Game.Bbs, 
                [
                    "bbs_first",
                    "bbs_second",
                    "bbs_third",
                    "bbs_fourth",
                ]
            },
            {
                Game.Recom, 
                [
                    "Recom",
                ]
            },
        };

        private class GameFileSystem
        {
            public Dictionary<string, Hed.Entry> Entries = new();
            public IPakReader Reader;
        }
        private interface IPakReader
        {
            public void Close();
            public void Open();
            public Stream Stream { get; }
            public bool Status { get; }
        }
        private class FilePakReader : IPakReader
        {
            private string FilePath;
            private FileStream _str;
            public FilePakReader(string path) => FilePath = path;
            public bool Status => _str is not null;
            public void Close()
            {
                if (_str is null) return;
                _str.Close();
                _str = null;
            }
            public void Open() => _str = File.OpenRead(FilePath);
            public Stream Stream => _str;
        }
        
        private static readonly Dictionary<Game, GameFileSystem[]> _gameFileSystems = new();
        public static bool HD1525Initialized = false;

        static PackFileSystem()
        {
            if (!PackFileSystemSettings.AutomaticInitialization) return;

            var remixCfg = Config.HDRemixConfig;
            switch (remixCfg.GamePlatform)
            {
                case Platform.Steam:
                {
                    InitializeSteam1525(remixCfg.GamePath);
                    break;
                }
            }
        }
        public static void InitializeSteam1525(string steamGamePath)
        {
            if (HD1525Initialized) return;
            HD1525Initialized = true;
            foreach (var pair in _1525gameFileNames)
            {
                var heds = pair.Value.Select(i => 
                    (File.OpenRead(Path.Combine(steamGamePath, "Image", "dt", $"{i}.hed")) as Stream, 
                        Path.Combine(steamGamePath, "Image", "dt", $"{i}.pkg"))).ToArray();
                
                Initialize(pair.Key, heds);
            }
        }
        public static void Initialize(Game game, params (Stream hed, string pakPath)[] files)
        {
            var fileSystems = new List<GameFileSystem>();
            foreach (var f in files)
            {
                var hedObject = Hed.Read(f.hed).ToList();

                var fs = new GameFileSystem
                {
                    Reader = new FilePakReader(f.pakPath), //TODO
                };

                foreach (var h in hedObject)
                {
                    var hash = Egs.Helpers.ToString(h.MD5);
                    if (!EgsTools.Names.TryGetValue(hash, out var fileName))
                        fileName = $"{hash}.dat";
                    fs.Entries.Add(fileName, h);
                }
                fileSystems.Add(fs);
            }
            _gameFileSystems.Add(game, fileSystems.ToArray());
        }
        public static IEnumerable<string> GetFiles(Game game) => _gameFileSystems[game].SelectMany(i => i.Entries.Select(j => j.Key));
        public static EgsHdAsset Open(Game game, string path, bool close = true)
        {
            var fsList = _gameFileSystems[game];
            foreach (var fs in fsList)
            {
                if (!fs.Entries.TryGetValue(path, out var entry)) continue;
                
                var reader = fs.Reader;
                if (!reader.Status) reader.Open();
                var stream = reader.Stream;
                    
                var hdAsset = new EgsHdAsset(stream.SetPosition(entry.Offset));
                if (close) reader.Close();
                return hdAsset;
            }
            return null;
        }
        public static void Close(Game game)
        {
            foreach (var fs in _gameFileSystems[game]) fs.Reader.Close();
        }
    }
}
