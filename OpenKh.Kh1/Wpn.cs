using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;
using static OpenKh.Kh1.Mdls;
using static OpenKh.Kh1.Wpn.MenvFile;

namespace OpenKh.Kh1
{
    public class Wpn
    {
        private static int CLUT_SIZE = 256 * 4; // 1024 = 256 colors, 4 bytes each

        public WpnHeader Header { get; set; }
        public MenvFile ModelEnvFile { get; set; }
        public List<MdlsImage> Images { get; set; }
        // Note about joints: The file doesn't have bones. Ingame they are taken from the model wielding the weapon (Last X bones)

        public Wpn(string filepath)
        {
            var imageCount = 0;
            using var stream = new FileStream(filepath, FileMode.Open);
            // Header
            Header = BinaryMapping.ReadObject<WpnHeader>(stream);

            //Reserved (16 * 7 bytes)

            // Main file - Unknown data - Read MainFile for some info
            //stream.Position = Header.MainOffset;

            // Model Environment
            stream.Position = Header.MenvOffset;
            ModelEnvFile = new MenvFile();
            ModelEnvFile.MenvHeader = BinaryMapping.ReadObject<MenvModelHeader>(stream);
            var meshListPointer = stream.Position;
            ModelEnvFile.ModelHeader = BinaryMapping.ReadObject<MdlsModelHeader>(stream);

            for(var i = 0; i < ModelEnvFile.ModelHeader.MeshCount; i++)
            {
                var mesh = new MdlsMesh();
                mesh.Header = BinaryMapping.ReadObject<MdlsMeshHeader>(stream);
                ModelEnvFile.Meshes.Add(mesh);
            }

            for (var i = 0; i < ModelEnvFile.ModelHeader.MeshCount; i++)
            {
                var mesh = ModelEnvFile.Meshes[i];
                stream.Position = meshListPointer + mesh.Header.MeshPacketOffset;

                long nextOffset = Header.MenvOffset + ModelEnvFile.MenvHeader.UnkOffset;
                if (i + 1 < ModelEnvFile.ModelHeader.MeshCount)
                    nextOffset = meshListPointer + ModelEnvFile.Meshes[i + 1].Header.MeshPacketOffset;
                var packetSize = nextOffset - stream.Position;

                mesh.packet = new MdlsMeshPacket();
                mesh.packet.RawData = stream.ReadBytes((int)packetSize);
                mesh.packet.UnpackData();
            }

            // Textures
            Images = [];
            imageCount = ModelEnvFile.MenvHeader.TextureInfoSize / 0x10;
            stream.Position = Header.MenvOffset + ModelEnvFile.MenvHeader.TextureInfoOffset;
            for (var i = 0; i < imageCount; i++)
            {
                var image = new MdlsImage
                {
                    Info = BinaryMapping.ReadObject<MdlsImage.MdlsImageInfo>(stream),
                };
                Images.Add(image);
            }
            stream.Position = Header.MenvOffset + ModelEnvFile.MenvHeader.TextureDataOffset;
            for (var i = 0; i < imageCount; i++)
            {
                var imageDataLength = Images[i].Info.Width * Images[i].Info.Height;
                Images[i].Data = stream.ReadBytes(imageDataLength);
            }
            for (var i = 0; i < imageCount; i++) Images[i].Clut = stream.ReadBytes(CLUT_SIZE);
            for (var i = 0; i < imageCount; i++) Images[i].loadBitmap();
        }

        public class WpnHeader
        {
            [Data] public int Unk { get; set; }
            [Data] public int MainOffset { get; set; }
            [Data] public int MenvOffset { get; set; }
            [Data] public int FileSize { get; set; } // size + 0x80 (Or maybe rounded up to 0xXX00)
        }

        public class MainFile
        {
            // HEADER
            // Unk * 3
            // Entry count

            // ENTRY HEADERS
            // 16 * 2 bytes each
            // 1st int is the offset (Entries may share offsets)
            // 3rd int is the identifier

            // OFFSET LIST
            // Offset count followed by the list of offsets used by the entries padded to 16 bytes
            // Followed by some 00 and FF data

            // At some point it contains effects such as the Keyblade swing trails. It's an image followed by the CLUT
        }

        public class MenvFile
        {
            public MenvModelHeader MenvHeader { get; set; }
            public MdlsModelHeader ModelHeader { get; set; }
            public List<MdlsMesh> Meshes { get; set; }

            public MenvFile() => Meshes = [];

            public class MenvModelHeader
            {
                [Data] public int MenvTag { get; set; } // MENV
                [Data] public int MenvModelSize { get; set; } // Actual size = this + padding up to 16 OR simply +8
                [Data] public int TextureInfoOffset { get; set; }
                [Data] public int TextureInfoSize { get; set; } // 0x10 per image
                [Data] public int TextureDataOffset { get; set; }
                [Data] public int TextureDataSize { get; set; }
                [Data] public int ClutOffset { get; set; }
                [Data] public int ClutSize { get; set; }
                [Data] public int ModelOffset { get; set; }
                [Data] public int ModelSize { get; set; }
                [Data] public int UnkOffset { get; set; }
                [Data] public int UnkSize { get; set; }
                [Data] public int Unk2Offset { get; set; }
                [Data] public int Unk2Size { get; set; }
                [Data(Count = 2)] public int[] Unk { get; set; }
            }
        }
    }
}
