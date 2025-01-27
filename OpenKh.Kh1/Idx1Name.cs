using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Kh1
{
    public record Idx1Name
    {
        public static readonly string[] Names = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "resources/kh1idx.txt"));
        private static readonly Dictionary<uint, string> NameDictionary = Names.ToDictionary(Idx1.GetHash, name => name);

        public Idx1 Entry { get; set; }
        public string Name { get; set; }

        public static IEnumerable<Idx1Name> Lookup(IEnumerable<Idx1> entries) => entries
            .Select(entry => new Idx1Name
            {
                Entry = entry,
                Name = Lookup(entry)
            });

        public static string Lookup(Idx1 entry) => NameDictionary.GetValueOrDefault(entry.Hash);
    }
}
