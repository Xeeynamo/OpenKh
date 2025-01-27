using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.IO;

namespace OpenKh.Kh1
{
    public class Img1 : IDisposable
    {
        public const int IsoBlockAlign = 0x800;

        private readonly Stream _stream;
        private readonly int _firstBlock;

        public Dictionary<string, Idx1> Entries { get; }

        public Img1(Stream stream, IEnumerable<Idx1> idxEntries, int firstBlock)
        {
            _stream = stream;
            _firstBlock = firstBlock;
            Entries = Idx1Name.Lookup(idxEntries).ToDictionary(x => x.Name ?? $"@noname/{x.Entry.Hash:X08}", x => x.Entry);
        }

        public void Dispose() => _stream.Dispose();

        public bool FileExists(string fileName) => Entries.ContainsKey(fileName);

        public bool TryFileOpen(string fileName, out Stream stream)
        {
            if (Entries.TryGetValue(fileName, out var entry))
            {
                stream = FileOpen(entry);
                return true;
            }

            stream = null;
            return false;
        }

        public bool FileOpen(string fileName, Action<Stream> callback)
        {
            if (!Entries.TryGetValue(fileName, out var entry)) return false;
            
            callback(FileOpen(entry));
            return true;
        }

        public Stream FileOpen(string fileName)
        {
            if (Entries.TryGetValue(fileName, out var entry))
                return FileOpen(entry);

            throw new FileNotFoundException("File not found", fileName);
        }

        public Stream FileOpen(Idx1 entry)
        {
            if (entry.CompressionFlag != 0)
            {
                var fileStream = new SubStream(_stream, (_firstBlock + entry.IsoBlock) * IsoBlockAlign, entry.Length);
                return new MemoryStream(Decompress(fileStream));
            }

            return new SubStream(_stream, (_firstBlock + entry.IsoBlock) * IsoBlockAlign, entry.Length);
        }

        public static byte[] Decompress(Stream src) => Decompress(new BinaryReader(src).ReadBytes((int)src.Length));

        public static byte[] Decompress(byte[] srcData)
        {
            var srcIndex = srcData.Length - 1;

            if (srcIndex == 0)
                return Array.Empty<byte>();

            var key = srcData[srcIndex--];
            var decSize = srcData[srcIndex--] |
                (srcData[srcIndex--] << 8) |
                (srcData[srcIndex--] << 16);

            var dstIndex = decSize - 1;
            var dstData = new byte[decSize];
            while (dstIndex >= 0 && srcIndex >= 0)
            {
                var data = srcData[srcIndex--];
                if (data == key && srcIndex >= 0)
                {
                    var copyIndex = srcData[srcIndex--];
                    if (copyIndex > 0 && srcIndex >= 0)
                    {
                        var copyLength = srcData[srcIndex--];
                        for (var i = 0; i < copyLength + 3 && dstIndex >= 0; i++)
                        {
                            if (dstIndex + copyIndex + 1 < dstData.Length)
                                dstData[dstIndex--] = dstData[dstIndex + copyIndex + 1];
                            else
                                dstData[dstIndex--] = 0;
                        }
                    }
                    else
                        dstData[dstIndex--] = data;

                }
                else
                    dstData[dstIndex--] = data;
            }

            return dstData;
        }

        public static byte[] Compress(byte[] srcData)
        {
            const int MaxWindowSize = 0x102;
            const int MaxSlidingIndex = 0xff;

            var decompressedLength = srcData.Length;
            var key = GetLeastUsedByte(srcData);
            var buffer = new List<byte>(decompressedLength)
            {
                key,
                (byte)(decompressedLength >> 0),
                (byte)(decompressedLength >> 8),
                (byte)(decompressedLength >> 16),
            };

            var sourceIndex = decompressedLength - 1;

            while (sourceIndex >= 0)
            {
                var ch = srcData[sourceIndex];
                if (ch == key)
                {
                    buffer.Add(key);
                    buffer.Add(0);
                    sourceIndex--;
                    continue;
                }

                var windowSizeCandidate = 0;
                var slidingIndexCandidate = -1;
                var maxWindowSize = Math.Min(sourceIndex, MaxWindowSize);
                for (var slidingIndex = MaxSlidingIndex; slidingIndex > 0; slidingIndex--)
                {
                    if (slidingIndex + sourceIndex >= decompressedLength)
                        continue;

                    int windowSize;
                    for (windowSize = 0; windowSize < maxWindowSize; windowSize++)
                    {
                        var startWindow = sourceIndex + slidingIndex - windowSize;
                        if (srcData[startWindow] != srcData[sourceIndex - windowSize])
                            break;
                    }

                    if (windowSize > windowSizeCandidate)
                    {
                        windowSizeCandidate = windowSize;
                        slidingIndexCandidate = slidingIndex;
                    }
                }

                if (windowSizeCandidate > 3)
                {
                    buffer.Add(key);
                    buffer.Add((byte)slidingIndexCandidate);
                    buffer.Add((byte)(windowSizeCandidate - 3));
                    sourceIndex -= windowSizeCandidate;
                }
                else
                {
                    buffer.Add(ch);
                    sourceIndex--;
                }
            }

            var compressedLength = buffer.Count;
            var cmp = new byte[compressedLength];
            for (var i = 0; i < compressedLength; i++) cmp[i] = buffer[compressedLength - i - 1];

            return cmp;
        }

        private static byte GetLeastUsedByte(byte[] data)
        {
            var dictionary = new int[0x100];
            foreach (var ch in data)
                dictionary[ch]++;

            var elementFound = int.MaxValue;
            var indexFound = 1; // the least used byte is never zero
            for (var i = 0; i < 0x100; i++)
            {
                if (dictionary[i] < elementFound)
                {
                    elementFound = dictionary[i];
                    indexFound = i;
                }
            }

            return (byte)indexFound;
        }
    }
}
