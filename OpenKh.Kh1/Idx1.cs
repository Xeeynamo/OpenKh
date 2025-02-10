using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenKh.Common;
using Xe.BinaryMapper;

namespace OpenKh.Kh1
{
    public record Idx1
    {
        public const int EntryLength = 0x10;
        public const int MaxItemCount = 0xE00;
        public const int KingdomImg = 0x4d7000 / 0x800;

        [Data] public uint Hash { get; set; }
        [Data] public uint CompressionFlag { get; set; }
        [Data] public uint IsoBlock { get; set; }
        [Data] public uint Length { get; set; }

        public static List<Idx1> Read(Stream stream) => Enumerable
            .Range(0, MaxItemCount)
            .Select(_ => BinaryMapping.ReadObject<Idx1>(stream))
            .Where(x => x.Hash != 0)
            .ToList();

        public static void Write(Stream stream, ICollection<Idx1> entries)
        {
            stream.MustWriteAndSeek();

            if (entries.Count > MaxItemCount)
                throw new ArgumentOutOfRangeException($"Can not insert more than {MaxItemCount} IDX1 items.");

            foreach (var entry in entries)
                BinaryMapping.WriteObject(stream, entry);

            var remainingEntries = MaxItemCount - entries.Count;
            stream.Position += remainingEntries * EntryLength;
            if (stream.Length < stream.Position)
                stream.SetLength(stream.Position);
        }

        public static uint GetHash(string text) => GetHash(Encoding.UTF8.GetBytes(text));
        public static uint GetHash(byte[] data) => data.Aggregate(0U, (current, ch) => (2 * current) ^ (uint)((ch << 16) % 69665));
    }
}
