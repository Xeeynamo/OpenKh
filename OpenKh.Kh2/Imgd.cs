using OpenKh.Common;
using OpenKh.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Xe.BinaryMapper;
using Xe.IO;
using static OpenKh.Imaging.Tm2;

namespace OpenKh.Kh2
{
    public partial class Imgd : IImageRead
    {
        private class Header
        {
            [Data] public uint MagicCode { get; set; }
            [Data] public int Version { get; set; }
            [Data] public int BitmapOffset { get; set; }
            [Data] public int BitmapLength { get; set; }
            [Data] public int ClutOffset { get; set; }
            [Data] public int ClutLength { get; set; }
            [Data] public int Unk18 { get; set; }
            [Data] public short Width { get; set; }
            [Data] public short Height { get; set; }
            [Data] public short PowWidth { get; set; }
            [Data] public short PowHeight { get; set; }
            [Data] public short WidthDiv64 { get; set; }
            [Data] public ushort GsPsm { get; set; }
            [Data] public int Unk28 { get; set; }
            [Data] public ushort ClutWidth { get; set; }
            [Data] public ushort ClutHeight { get; set; }
            [Data] public short Unk30 { get; set; }
            [Data] public ushort ClutFormat { get; set; }
            [Data] public ushort ImageType { get; set; }
            [Data] public ushort ClutType { get; set; }
            [Data] public int Reserved { get; set; }
            [Data] public int Flags { get; set; }
        }

        private const uint MagicCode = 0x44474D49U;
        private const int HeaderLength = 0x40;
        private const int SwizzledFlag = 4;
        private const int FacAlignment = 0x800;
        private static readonly InvalidDataException InvalidHeaderException = new InvalidDataException("Invalid header");

        public static bool IsValid(Stream stream) =>
            stream.Length >= HeaderLength && stream.SetPosition(0).ReadInt32() == MagicCode;

        private readonly GsPSM _format;
        private readonly int _flags;

        private Imgd(Stream stream)
        {
            stream
                .MustReadAndSeek()
                .MustHaveHeaderLengthOf(HeaderLength);

            var reader = new BinaryReader(stream);
            var header = BinaryMapping.ReadObject<Header>(stream);
            if (header.MagicCode != MagicCode)
                throw InvalidHeaderException;

            Size = new Size(header.Width, header.Height);
            _format = (GsPSM)header.GsPsm;
            _flags = header.Flags;

            stream.SetPosition(header.BitmapOffset);
            var data = reader.ReadBytes(header.BitmapLength);

            // Swap pixel order for only unswizzled 4-bpp IMGD.
            Data = (_format == GsPSM.GS_PSMT4 && (_flags & SwizzledFlag) == 0)
                ? GetSwappedPixelData(data)
                : data;

            stream.SetPosition(header.ClutOffset);
            Clut = reader.ReadBytes(header.ClutLength);
        }

        public static Imgd Read(Stream stream) => new Imgd(stream.SetPosition(0));

        public void Write(Stream stream)
        {
            stream.MustWriteAndSeek();

            BinaryMapping.WriteObject(stream, new Header
            {
                MagicCode = MagicCode,
                Version = 0x100,
                BitmapOffset = HeaderLength,
                BitmapLength = Data.Length,
                ClutOffset = HeaderLength + Data.Length,
                ClutLength = Clut?.Length ?? 0,
                Unk18 = -1,
                Width = (short)Size.Width,
                Height = (short)Size.Height,
                PowWidth = GetPow(Size.Width),
                PowHeight = GetPow(Size.Height),
                WidthDiv64 = (short)(Size.Width / 64),
                GsPsm = (ushort)_format,
                Unk28 = -1,
                ClutWidth = (ushort)(_format == GsPSM.GS_PSMT4 ? 8 : 16),
                ClutHeight = (ushort)(_format == GsPSM.GS_PSMT4 ? 2 : 16),
                Unk30 = 1,
                ClutFormat = (ushort)(_format == GsPSM.GS_PSMCT32 ? (GsCPSM)19: GsCPSM.GS_PSMCT32),
                ImageType = (ushort)GetImageType(_format),
                ClutType = (ushort)(_format == GsPSM.GS_PSMCT32 ? 0 : CLT_TYPE.CT_ABGR8),
                Reserved = 0,
                Flags = _flags,
            });

            // Swap pixel order for only unswizzled 4-bpp IMGD.
            var data = (_format == GsPSM.GS_PSMT4 && (_flags & SwizzledFlag) == 0)
                ? GetSwappedPixelData(Data)
                : Data;

            stream.Write(data, 0, data.Length);

            if (Clut != null)
                stream.Write(Clut, 0, Clut.Length);
        }

        public static bool IsFac(Stream stream)
        {
            if (stream.Length < HeaderLength)
                return false;

            stream.MustReadAndSeek().SetPosition(0);
            var header = BinaryMapping.ReadObject<Header>(stream);
            if (header.MagicCode != MagicCode)
                return false;

            stream
                .SetPosition(header.ClutOffset + header.ClutLength)
                .AlignPosition(FacAlignment);

            if (stream.Position + HeaderLength >= stream.Length)
                return false;

            header = BinaryMapping.ReadObject<Header>(stream);
            if (header.MagicCode != MagicCode)
                return false;

            return true;
        }

        public static IEnumerable<Imgd> ReadAsFac(Stream stream)
        {
            stream.SetPosition(0);
            while (true)
            {
                stream.AlignPosition(FacAlignment);
                var subStreamLength = stream.Length - stream.Position;
                if (subStreamLength < HeaderLength)
                    yield break;

                yield return Imgd.Read(new SubStream(stream, stream.Position, subStreamLength));
            }
        }

        public static void WriteAsFac(Stream stream, IEnumerable<Imgd> images)
        {
            foreach (var image in images)
            {
                image.Write(stream);
                stream.SetLength(stream.AlignPosition(FacAlignment).Position);
            }
        }

        public Size Size { get; }

        /// <summary>
        /// Bitmap data
        /// </summary>
        /// <remarks>
        /// In the following case, this `Data` is not same bitmap data stored in imgd file.
        /// 
        /// In OpenKh:
        /// - NOT IsSwizzled && Format4bpp = storing Windows pixel order ([1, 2] to 0x12)
        /// 
        /// In IMGD file:
        /// - NOT IsSwizzled && Format4bpp = storing reversed pixel order ([1, 2] to 0x21)
        /// </remarks>
        public byte[] Data { get; }

        public byte[] Clut { get; }

        public bool IsSwizzled => (_flags & 4) != 0;

        public PixelFormat PixelFormat => GetPixelFormat(_format);

        public byte[] GetData()
        {
            switch (_format)
            {
                case GsPSM.GS_PSMCT32:
                    return GetData32bpp();
                case GsPSM.GS_PSMT8:
                    return IsSwizzled ? Ps2.Decode8(Ps2.Encode32(Data, Size.Width / 128, Size.Height / 64), Size.Width / 128, Size.Height / 64) : Data;
                case GsPSM.GS_PSMT4:
                    return IsSwizzled ? Ps2.Decode4(Ps2.Encode32(Data, Size.Width / 128, Size.Height / 128), Size.Width / 128, Size.Height / 128) : Data;
                default:
                    throw new NotSupportedException($"The format {_format} is not supported.");
            }
        }

        public byte[] GetClut()
        {
            switch (_format)
            {
                case GsPSM.GS_PSMT8: return GetClut8();
                case GsPSM.GS_PSMT4: return GetClut4();
                default:
                    throw new NotSupportedException($"The format {_format} is not supported or does not contain any palette.");
            }
        }

        private byte[] GetClut4()
        {
            var data = new byte[16 * 4];
            for (var i = 0; i < 16; i++)
            {
                data[i * 4 + 0] = Clut[i * 4 + 0];
                data[i * 4 + 1] = Clut[i * 4 + 1];
                data[i * 4 + 2] = Clut[i * 4 + 2];
                data[i * 4 + 3] = Ps2.FromPs2Alpha(Clut[i * 4 + 3]);
            }

            return data;
        }

        private byte[] GetClut8()
        {
            var data = new byte[256 * 4];
            for (var i = 0; i < 256; i++)
            {
                var srcIndex = Ps2.Repl(i);
                if (srcIndex * 4 < Clut.Length)
                {
                    data[i * 4 + 0] = Clut[srcIndex * 4 + 0];
                    data[i * 4 + 1] = Clut[srcIndex * 4 + 1];
                    data[i * 4 + 2] = Clut[srcIndex * 4 + 2];
                    data[i * 4 + 3] = Ps2.FromPs2Alpha(Clut[srcIndex * 4 + 3]);
                }
            }

            return data;
        }

        private byte[] GetData32bpp()
        {
            var newData = new byte[Data.Length];
            for (var i = 0; i < newData.Length - 3; i += 4)
            {
                newData[i + 0] = Data[i + 2];
                newData[i + 1] = Data[i + 1];
                newData[i + 2] = Data[i + 0];
                newData[i + 3] = Ps2.FromPs2Alpha(Data[i + 3]);
            }

            return newData;
        }

        private static short GetPow(int value)
        {
            short pow = 1;
            while (value > (1 << pow))
                pow++;

            return pow;
        }

        private static PixelFormat GetPixelFormat(GsPSM format)
        {
            switch (format)
            {
                case GsPSM.GS_PSMCT32: return PixelFormat.Rgba8888;
                case GsPSM.GS_PSMT8: return PixelFormat.Indexed8;
                case GsPSM.GS_PSMT4: return PixelFormat.Indexed4;
                default: return PixelFormat.Undefined;
            }
        }

        private static GsPSM GetFormat(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Rgba8888: return GsPSM.GS_PSMCT32;
                case PixelFormat.Indexed4: return GsPSM.GS_PSMT4;
                case PixelFormat.Indexed8: return GsPSM.GS_PSMT8;
                default:
                    throw new ArgumentOutOfRangeException(
                        $"Pixel format {pixelFormat} is not supported.");
            }
        }

        private static IMG_TYPE GetImageType(GsPSM format)
        {
            return format switch
            {
                GsPSM.GS_PSMCT32 => IMG_TYPE.IT_RGBA,
                GsPSM.GS_PSMT4 => IMG_TYPE.IT_CLUT4,
                GsPSM.GS_PSMT8 => IMG_TYPE.IT_CLUT8,
                _ => throw new ArgumentOutOfRangeException($"Format {format} is not supported."),
            };
        }
    }
}
