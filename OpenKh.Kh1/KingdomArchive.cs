using OpenKh.Common;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Kh1
{
    public class KingdomArchive
    {
        private const int Alignment = 0x80;

        public static List<byte[]> Read(Stream stream)
        {
            var itemCount = stream.ReadInt32();
            var offsets = new int[itemCount + 1];
            for (var i = 0; i < offsets.Length; i++)
                offsets[i] = stream.ReadInt32();

            var items = new List<byte[]>(itemCount);
            for (var i = 0; i < itemCount; i++)
            {
                stream.SetPosition(offsets[i]);
                items.Add(stream.ReadBytes(offsets[i + 1] - offsets[i]));
            }

            return items;
        }

        public static void Write(Stream stream, ICollection<byte[]> entries)
        {
            stream.MustWriteAndSeek();
            stream.Write(entries.Count);

            var accumulator = (entries.Count + 2) * 4;
            foreach (var entry in entries)
            {
                accumulator = Helpers.Align(accumulator, Alignment);
                stream.Write(accumulator);
                accumulator += entry.Length;
            }
            stream.Write(Helpers.Align(accumulator, Alignment));

            foreach (var entry in entries)
            {
                stream.AlignPosition(Alignment);
                stream.Write(entry);
            }
        }

        public static void Write<TStream>(Stream stream, ICollection<TStream> entries)
            where TStream : Stream
        {
            stream.MustWriteAndSeek();
            stream.Write(entries.Count);

            var accumulator = (entries.Count + 2) * 4;
            foreach (var entry in entries) //unused?
            {
                accumulator = Helpers.Align(accumulator, Alignment);
                stream.Write(accumulator);
                accumulator += (int)stream.Length;
            }
            stream.Write(Helpers.Align(accumulator, Alignment));

            foreach (var entry in entries)
            {
                stream.AlignPosition(Alignment);
                entry.FromBegin().CopyTo(stream);
            }
        }
    }
}
