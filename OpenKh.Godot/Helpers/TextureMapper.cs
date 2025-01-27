using System.Collections.Generic;
using Godot;

namespace OpenKh.Godot.Helpers
{
    public class TextureMapper
    {
        public readonly List<Texture2D> HdTextures = [];
        public readonly Dictionary<int, int> TextureMap = new();
        public int CurrentIndex { get; private set; }

        public TextureMapper(IEnumerable<Texture2D> hdTextures, Dictionary<int, int> map = null, int index = 0)
        {
            if (hdTextures is not null) HdTextures.AddRange(hdTextures);
            if (map is not null) TextureMap = map;
            CurrentIndex = index;
        }
        public void ResetMap() => TextureMap.Clear();

        public TextureMapper(TextureMapper old)
        {
            HdTextures = old.HdTextures;
            CurrentIndex = old.CurrentIndex;
        }

        public Texture2D GetTexture(int index, Texture2D fallback)
        {
            if (!TextureMap.TryGetValue(index, out var value))
            {
                value = CurrentIndex;
                TextureMap[index] = value;
                CurrentIndex++;
            }
            if (value >= HdTextures.Count) return fallback;
            
            return HdTextures[value] ?? fallback;
        }

        public Texture2D GetNextTexture(Texture2D fallback)
        {
            var value = CurrentIndex;
            CurrentIndex++;
            if (value >= HdTextures.Count) return fallback;
            
            return HdTextures[value] ?? fallback;
        }
    }
}
