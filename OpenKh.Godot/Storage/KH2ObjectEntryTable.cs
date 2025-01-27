using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenKh.Kh2;

namespace OpenKh.Godot.Storage
{
    public static class KH2ObjectEntryTable
    {
        public static Dictionary<int, Objentry> Entries;
        
        static KH2ObjectEntryTable()
        {
            Entries = Objentry.Read(new MemoryStream(PackFileSystem.Open(Game.Kh2, "00objentry.bin").OriginalData)).ToDictionary(i => (int)i.ObjectId);
        }
    }
}
