using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Godot;
using OpenKh.Bbs;
using OpenKh.Egs;
using OpenKh.Godot.Conversion;
using OpenKh.Godot.Helpers;
using OpenKh.Godot.Nodes;
using OpenKh.Godot.Resources;
using OpenKh.Kh2;

namespace OpenKh.Godot.Storage
{
    public static class PackAssetLoader
    {
        private class Cache<T>
        {
            public readonly Dictionary<string, T> Storage = new();
            public T this[string s]
            {
                get => Storage.GetValueOrDefault(s);
                set => Storage[s] = value;
            }
            public void Clear() => Storage.Clear();

            public void ClearBlacklist(ICollection<string> blacklist)
            {
                var keys = Storage.Select(i => i.Key).Distinct().Where(i => !blacklist.Contains(i)).ToList();
                foreach (var key in keys) Storage.Remove(key);
            }
        }
        private class GameCache<T>
        {
            public readonly IReadOnlyDictionary<Game, Dictionary<string, T>> Storage = Enum.GetValues<Game>().ToDictionary(i => i, i => new Dictionary<string, T>()).AsReadOnly();
            public T this[Game g, string s]
            {
                get => !Storage.TryGetValue(g, out var dict) ? default : dict.GetValueOrDefault(s);
                set => Storage[g][s] = value;
            }
            public void Clear()
            {
                foreach (var entry in Storage) entry.Value.Clear();
            }
            public void ClearBlacklist(ICollection<string> blacklist)
            {
                foreach (var dict in Storage.Select(i => i.Value))
                {
                    var keys = dict.Select(i => i.Key).Distinct().Where(i => !blacklist.Contains(i)).ToList();
                    foreach (var key in keys) dict.Remove(key);
                }
            }
        }

        public class OriginalRemasteredPair<T>
        {
            public readonly T Original;
            public readonly T Remastered;

            public OriginalRemasteredPair(T o, T r)
            {
                Original = o;
                Remastered = r;
            }
        }
        
        //temporary caches are cleared when ClearCache is called
        //non-temporary caches are not cleared, and are always persistent, should only be used for assets that are never unloaded

        private static Cache<PackedScene> _kh2MdlxCache = new();
        private static Cache<KH2Moveset> _kh2MsetCache = new();
        private static GameCache<Texture2D> _imageCache = new();
        private static GameCache<OriginalRemasteredPair<Texture2D>> _imageOriginalRemasteredCache = new();
        private static GameCache<SoundContainer> _soundCache = new();
        
        public static void ClearCache()
        {
            _kh2MdlxCache.Clear();
            _kh2MsetCache.Clear();
            _imageCache.Clear();
            _soundCache.Clear();
        }
        public static void ClearCacheBlacklist(ICollection<string> blacklist)
        {
            _kh2MdlxCache.ClearBlacklist(blacklist);
            _kh2MsetCache.ClearBlacklist(blacklist);
            _imageCache.ClearBlacklist(blacklist);
            _soundCache.ClearBlacklist(blacklist);
        }
        public static List<Texture2D> GetHDTextures(EgsHdAsset file)
        {
            if (file.Assets.Length <= 0) return null;
            var images = new List<Texture2D>();

            var order = new Dictionary<int, string>();
                    
            foreach (var filePath in file.Assets)
            {
                if (!filePath.StartsWith('-') || !int.TryParse(filePath[1..^4], out var number)) continue;
                order[number] = filePath;
            }
            foreach (var filePath in order.OrderBy(i => i.Key).Select(i => i.Value))
            {
                if (!IsImage(filePath))
                {
                    images.Add(null);
                    continue;
                }
                images.Add(GetTextureFromNameAndData(filePath, file.RemasteredAssetsDecompressedData[filePath]));
            }
            return images;
        }
        public static PackedScene CacheMdlx(string path, bool close = true)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var tryCache = _kh2MdlxCache[path];
            if (tryCache is not null) return tryCache;
            
            var file = PackFileSystem.Open(Game.Kh2, path, close);
            if (file is null) return null;

            var originalStream = new MemoryStream(file.OriginalData);
            var textures = GetHDTextures(file);
            var barFile = Bar.Read(originalStream);
            
            var result = ModelConverters.FromMdlx(barFile, textures);
            result.Name = path;
            result.SetOwner();
                
            var packed = new PackedScene();
            packed.Pack(result);
            result.QueueFree();
            _kh2MdlxCache[path] = packed;

            return packed;
        }
        public static KH2Mdlx GetMdlx(Objentry obj, bool close = true) => GetMdlx($"obj/{obj.ModelName}.mdlx", close);
        public static KH2Mdlx GetMdlx(string path, bool close = true) => CacheMdlx(path, close)?.Instantiate<KH2Mdlx>();

        public static KH2Moveset GetMoveset(Objentry obj, bool close = true) => GetMoveset($"obj/{obj.AnimationName}", close);
        public static KH2Moveset GetMoveset(string path, bool close = true)
        {
            if (string.IsNullOrEmpty(path)) return null;
            
            var tryGet = _kh2MsetCache[path];
            if (tryGet is not null) return tryGet;
            
            var file = PackFileSystem.Open(Game.Kh2, path, close);
            if (file is null) return null;
            
            var originalStream = new MemoryStream(file.OriginalData);
            
            var barFile = Bar.Read(originalStream);

            var result = ModelConverters.FromMoveset(barFile);

            _kh2MsetCache[path] = result;

            return result;
        }
        public static bool IsImage(string path)
        {
            var lower = path.ToLower();
            return lower.EndsWith(".png") || lower.EndsWith(".imd") || lower.EndsWith(".dds");
        }
        public static Texture2D GetImage(string path, Game game = Game.Kh2, bool close = true)
        {
            if (string.IsNullOrEmpty(path)) return null;
            
            var tryGet = _imageCache[game, path];
            if (tryGet is not null) return tryGet;
            
            var file = PackFileSystem.Open(game, path, close);
            if (file is null) return null;

            var fileName = path;
            var data = file.OriginalData;

            if (file.Assets.Length > 0)
            {
                fileName = file.Assets.First();
                data = file.RemasteredAssetsDecompressedData[fileName];
            }
            
            var tex = GetTextureFromNameAndData(fileName, data);

            if (tex is null) return null;

            _imageCache[game, path] = tex;

            return tex;
        }
        public static Texture2D GetTextureFromNameAndData(string name, byte[] data)
        {
            Texture2D tex = null;
            if (name.EndsWith(".png"))
            {
                var image = new Image();
                image.LoadPngFromBuffer(data);
                tex = ImageTexture.CreateFromImage(image);
            }
            else if (name.EndsWith(".imd")) tex = TextureConverters.FromImgd(Imgd.Read(new MemoryStream(data)));
            else if (name.EndsWith(".dds")) tex = ImageTexture.CreateFromImage(DDSConverter.GetImage(data));
            return tex;
        }
        public static SoundContainer GetSoundContainer(string path, Game game = Game.Kh2, bool close = true)
        {
            if (string.IsNullOrEmpty(path)) return null;

            var tryGet = _soundCache[game, path];
            if (tryGet is not null) return tryGet;
            
            var file = PackFileSystem.Open(game, path, close);
            if (file is null) return null;

            var scd = Converters.FromScd(Scd.Read(new MemoryStream(file.OriginalData)));
            
            _soundCache[game, path] = scd;

            return scd;
        }
    }
}
