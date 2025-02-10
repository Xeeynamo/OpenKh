using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using IronSoftware.Drawing;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh1
{
    public class Mdls
    {
        private static byte INITIAL_ADDRESS = 0x80;
        private static int MAGIC_CODE = 0x4A424F4D; // MOBJ
        private static int CLUT_SIZE = 256 * 4; // 1024 = 256 colors, 4 bytes each

        private static bool DEBUG_PRINT = false;

        public MdlsHeader Header { get; set; }
        public MdlsModelHeader ModelHeader { get; set; }
        public List<MdlsMesh> Meshes { get; set; }
        public List<MdlsJoint> Joints { get; set; }
        public List<MdlsImage> Images { get; set; }

        public Mdls(string filepath)
        {
            var imageCount = 0;
            using (var stream = new FileStream(filepath, FileMode.Open))
            {
                //uint tess = testing(0x8B8B8B8A);

                stream.Position = INITIAL_ADDRESS;

                if (stream.ReadInt32() != MAGIC_CODE)
                {
                    throw new Exception("Invalid file!");
                }

                var dataSize = stream.ReadInt32(); // Unused for now

                // Header
                Header = BinaryMapping.ReadObject<MdlsHeader>(stream);
                stream.Position = INITIAL_ADDRESS + Header.ModelOffset;
                ModelHeader = BinaryMapping.ReadObject<MdlsModelHeader>(stream);

                // Mesh headers
                Meshes = new List<MdlsMesh>();
                for (var i = 0; i < ModelHeader.MeshCount; i++)
                {
                    var mesh = new MdlsMesh();
                    mesh.Header = BinaryMapping.ReadObject<MdlsMeshHeader>(stream);
                    Meshes.Add(mesh);
                }

                // Bone Data (Should be 16 x 17 bytes, requires confirmation)

                // Joints
                Joints = new List<MdlsJoint>();
                stream.Position = INITIAL_ADDRESS + Header.ModelOffset + ModelHeader.JointInfoOffset;
                for (var i = 0; i < ModelHeader.JointCount; i++)
                {
                    var joint = BinaryMapping.ReadObject<MdlsJoint>(stream);
                    Joints.Add(joint);
                }

                // Mesh data
                for (var i = 0; i < ModelHeader.MeshCount; i++)
                {
                    var meshDataOffset = INITIAL_ADDRESS + Header.ModelOffset + Meshes[i].Header.MeshPacketOffset;
                    stream.Position = meshDataOffset;
                    var nextOffset = INITIAL_ADDRESS + Header.UnkOffset;
                    if(i + 1 < ModelHeader.MeshCount)
                        nextOffset = INITIAL_ADDRESS + Header.ModelOffset + Meshes[i+1].Header.MeshPacketOffset;
                    var packetSize = nextOffset - meshDataOffset;

                    Meshes[i].packet = new MdlsMeshPacket();
                    Meshes[i].packet.RawData = stream.ReadBytes(packetSize);
                    Meshes[i].packet.UnpackData();
                }

                // Textures
                Images = new List<MdlsImage>();
                imageCount = Header.TextureInfoSize / 0x10;
                stream.Position = INITIAL_ADDRESS + Header.TextureInfoOffset;
                for (var i = 0; i < imageCount; i++)
                {
                    var image = new MdlsImage();
                    image.Info = BinaryMapping.ReadObject<MdlsImage.MdlsImageInfo>(stream);
                    Images.Add(image);
                }
                stream.Position = INITIAL_ADDRESS + Header.TextureDataOffset;
                for (var i = 0; i < imageCount; i++)
                {
                    var imageDataLength = Images[i].Info.Width * Images[i].Info.Height;
                    Images[i].Data = stream.ReadBytes(imageDataLength);
                }
                for (var i = 0; i < imageCount; i++)
                {
                    Images[i].Clut = stream.ReadBytes(CLUT_SIZE);
                }
            }

            var directory = Path.GetDirectoryName(filepath);
            for (var i = 0; i < imageCount; i++)
            {
                //Images[i].printClut();

                Images[i].loadBitmap();

                var outPath = Path.Combine(directory, "TEXTURE_"+i+".png");
                //string outPath = Path.Combine(directory, "TEXTURE_"+i+".bmp");
                //Images[i].bitmap.Save(outPath, ImageFormat.Png);
                //MdlsImage.CreateAndSavePng(Images[i].Data, Images[i].Clut, Images[i].Width, Images[i].Height, outPath);

            }
        }

        public static int reverseInt(int reverseMe)
        {
            return ((reverseMe & 0xFF) << 24) |       // Reverse the first byte
                   ((reverseMe & 0xFF00) << 8) |     // Reverse the second byte
                   ((reverseMe & 0xFF0000) >> 8) |   // Reverse the third byte
                   ((reverseMe >> 24) & 0xFF);
        }

        public static byte IndexFix(byte pixelIndex)
        {
            if ((pixelIndex & 31) >= 8)
            {
                if ((pixelIndex & 31) < 16)
                {
                    pixelIndex += 8;                // +8 - 15 to +16 - 23
                }
                else if ((pixelIndex & 31) < 24)
                {
                    pixelIndex -= 8;                // +16 - 23 to +8 - 15
                }
            }

            return pixelIndex;
        }

        public class MdlsJoint
        {
            [Data] public float ScaleX { get; set; }
            [Data] public float ScaleY { get; set; }
            [Data] public float ScaleZ { get; set; }
            [Data] public uint Index { get; set; }
            [Data] public float RotateX { get; set; }
            [Data] public float RotateY { get; set; }
            [Data] public float RotateZ { get; set; }
            [Data] public int Padding { get; set; }
            [Data] public float TranslateX { get; set; }
            [Data] public float TranslateY { get; set; }
            [Data] public float TranslateZ { get; set; }
            //[Data] public int HierarchyBitfield { get; set; } // First 10 bytes is parent, second 10 bytes is sibling, third 10 bytes is child => I'm not smart enough to correctly read this
            [Data] public uint HierarchyBitfield { get; set; }
            //[Data] public byte HierarchyByte1 { get; set; }
            //[Data] public byte HierarchyByte2 { get; set; }
            //[Data] public byte HierarchyByte3 { get; set; }
            //[Data] public byte HierarchyByte4 { get; set; }

            public uint ParentId {
                get
                {
                    uint mask = 0x000003FF; // 0b00000000000000000000001111111111;
                    var ret = HierarchyBitfield & mask;
                    return ret;
                }
                set {
                    var mask = 0xFFFFFC00; // 0b11111111111111111111110000000000;
                    var wihtoutValue = HierarchyBitfield & mask;
                    HierarchyBitfield = wihtoutValue + value;
                }
            }

            public override string ToString()
            {
                return "[" + Index + " | "+ ParentId + "] S:<" + ScaleX + ", " + ScaleY + ", " + ScaleZ + ">, R:<" + RotateX + ", " + RotateY + ", " + RotateZ + "> , T:<" + TranslateX + ", " + TranslateY + ", " + TranslateZ + ">";
            }
        }

        public class MdlsMesh
        {
            public MdlsMeshHeader Header { get; set; }
            public MdlsMeshPacket packet { get; set; }
        }

        public class MdlsHeader
        {
            [Data] public int TextureInfoOffset { get; set; }
            [Data] public int TextureInfoSize { get; set; } // 0x10 per image
            [Data] public int TextureDataOffset { get; set; }
            [Data] public int TextureDataSize { get; set; }
            [Data] public int ClutOffset { get; set; }
            [Data] public int ClutSize { get; set; }
            [Data] public int ModelOffset { get; set; }
            [Data] public int ModelSize { get; set; }
            [Data] public int UnkOffset { get; set; } // Points to imageInfoOffset?
            [Data] public int UnkSize { get; set; } // Probably the size of the previous offset?
            [Data(Count=4)] public int[] Unk { get; set; }
        }

        public class MdlsModelHeader
        {
            [Data] public int JointCount { get; set; }
            [Data] public int JointInfoOffset { get; set; }
            [Data] public int BoneDataOffset { get; set; } // Actually unknown, but probably the same as in KH2
            [Data] public int MeshCount { get; set; }
        }

        public class MdlsMeshHeader
        {
            [Data] public int JointCount { get; set; }
            [Data] public int TextureIndex { get; set; }
            [Data] public int TextureIndex2 { get; set; } // Same as TextureIndex?
            [Data] public int MeshPacketOffset { get; set; }
        }

        // NOTE: This is for a single weight vertex. This is not ready for multiweight
        public class MdlsMeshPacket
        {
            public List<MdlsPacketVertices> StripHeaders { get; set; }
            public List<List<MdlsVertex>> TriangleStrips { get; set; }
            public List<MdlsVertex> Vertices { get; set; }
            public List<int[]> Faces { get; set; }
            //public List<MdlsPacketMatrices> Matrices { get; set; } // For multiweight

            public byte[] RawData { get; set; }

            public void UnpackData()
            {
                if(RawData == null || RawData.Length == 0)
                {
                    throw new Exception("Mdls mesh packet can't be unpacket because it's null or empty");
                }

                StripHeaders = new List<MdlsPacketVertices>();
                TriangleStrips = new List<List<MdlsVertex>>();
                Vertices = new List<MdlsVertex>();
                Faces = new List<int[]>();
                using (var stream = new MemoryStream(RawData))
                {
                    var weightMatrix = new int[16];
                    while (stream.Position < RawData.Length)
                    {
                        var header = BinaryMapping.ReadObject<MdlsMeshSubPacketHeader>(stream);

                        while(stream.PeekInt32() != 0x00008000)
                        {
                            var subPacketType = stream.PeekInt32();
                            // Weight matrix definition
                            if (subPacketType == 0)
                            {
                                var matrixPointer = BinaryMapping.ReadObject<MdlsPacketMatrixPointer>(stream);
                                if (matrixPointer.TableIndex1 == 0)
                                {
                                    weightMatrix[matrixPointer.TableIndex0] = matrixPointer.JointId;
                                    stream.Position += 0x70; // Reserved - All 0s
                                }
                                else // Multiweight - Not implemented
                                {

                                }
                            }
                            // Triangle strip
                            else if (subPacketType == 1)
                            {
                                var vertices = BinaryMapping.ReadObject<MdlsPacketVertices>(stream);
                                StripHeaders.Add(vertices);
                                var stripVertices = new List<MdlsVertex>();
                                for (var i = 0; i < vertices.VertexCount; i++)
                                {
                                    var vertex = BinaryMapping.ReadObject<MdlsVertex>(stream);
                                    vertex.Index = Vertices.Count;
                                    vertex.JointId = weightMatrix[vertex.MatrixId];
                                    Vertices.Add(vertex);
                                    stripVertices.Add(vertex);

                                    if (stripVertices.Count >= 3)
                                    {
                                        // Counterclockwise
                                        if (vertices.Unknown == 0)
                                        {
                                            if (stripVertices.Count % 2 == 0)
                                            {
                                                Faces.Add(new int[3] { vertex.Index, vertex.Index - 1, vertex.Index - 2 });
                                            }
                                            else
                                            {
                                                Faces.Add(new int[3] { vertex.Index, vertex.Index - 2, vertex.Index - 1 });
                                            }
                                        }
                                        // Clockwise
                                        else
                                        {
                                            if (stripVertices.Count % 2 == 0)
                                            {
                                                Faces.Add(new int[3] { vertex.Index, vertex.Index - 2, vertex.Index - 1 });
                                            }
                                            else
                                            {
                                                Faces.Add(new int[3] { vertex.Index, vertex.Index - 1, vertex.Index - 2 });
                                            }
                                        }
                                    }
                                }
                                TriangleStrips.Add(stripVertices);
                            }
                            // Multiweight
                            else if (subPacketType == 2)
                            {
                                throw new Exception("Multiweight is not implemented!");
                            }
                            else
                            {
                                throw new Exception("Unknown mesh packet command!");
                            }
                        }
                        stream.Position += 0x20;
                    }
                }
            }

            // Not implemented
            public void PackData()
            {
            }
        }

        public class MdlsMeshSubPacketHeader
        {
            [Data] public short Qwc { get; set; } // Size of the subpacket in qwc (16b)
            [Data] public byte Padding { get; set; }
            [Data] public byte Unk { get; set; }
            [Data] public int Padding2 { get; set; }
            [Data] public int VifUnpack { get; set; } // 0x01010001
            [Data] public byte Padding3 { get; set; }
            [Data] public byte UnpackOptions { get; set; }
            [Data] public byte EntryCount { get; set; }
            [Data] public byte EntryFormat { get; set; }
        }

        public class MdlsPacketMatrixPointer
        {
            [Data] public int PacketType { get; set; } // 0
            [Data] public int JointId { get; set; }
            [Data] public int TableIndex0 { get; set; }
            [Data] public int TableIndex1 { get; set; }
            // And then 28 ints to 0
        }

        public class MdlsPacketVertices
        {
            [Data] public int PacketType { get; set; } // 1
            [Data] public int DataSize { get; set; }
            [Data] public int VertexCount { get; set; }
            [Data] public int Unknown { get; set; }
            [Data] public int VertexCount2 { get; set; }
            [Data] public int Unknown2 { get; set; }
            [Data] public int Unknown3 { get; set; }
            [Data] public int Unknown4 { get; set; }
        }

        // NOTE: This is for a single weight vertex. This is not ready for multiweight
        public class MdlsVertex
        {
            public int Index { get; set; }
            [Data] public float NormalX { get; set; }
            [Data] public float NormalY { get; set; }
            [Data] public float NormalZ { get; set; }
            [Data] public uint MatrixId { get; set; } // Used for position count if multiweight
            [Data] public float TranslateX { get; set; }
            [Data] public float TranslateY { get; set; }
            [Data] public float TranslateZ { get; set; }
            [Data] public float Weight { get; set; }
            [Data] public float TexCoordU { get; set; }
            [Data] public float TexCoordV { get; set; }
            [Data] public float TexCoord1 { get; set; }
            [Data] public int Padding { get; set; }
            public int JointId { get; set; }

            public override string ToString()
            {
                return "["+JointId+" | "+Weight+"] T:<"+TranslateX+", " + TranslateY + ", " + TranslateZ + ">, UV:<"+ TexCoordU + ", " + TexCoordV + ", " + TexCoord1 + ">";
            }
        }

        public class MdlsPacketMatrices
        {
            [Data] public int PacketType { get; set; } // 2
        }

        public class MdlsImage
        {
            public MdlsImageInfo Info { get; set; }
            public byte[] Data { get; set; }
            public byte[] Clut { get; set; }
            public int Width { get { return Info.Width ; } }
            public int Height { get { return Info.Height; } }
            public AnyBitmap bitmap { get; set; }

            public void loadBitmap()
            {
                if (Data == null || Clut == null || Width <= 0 || Height <= 0)
                {
                    throw new ArgumentException("Can't create bitmap");
                }

                if (Data.Length != Width * Height || Clut.Length != CLUT_SIZE)
                {
                    throw new ArgumentException("Image data length or CLUT length is invalid");
                }

                bitmap = new AnyBitmap(Width, Height/*, PixelFormat.Format32bppArgb*/);

                for (var y = 0; y < Height; y++)
                {
                    for (var x = 0; x < Width; x++)
                    {
                        var pixelValue = Data[y * Width + x];

                        pixelValue = IndexFix(pixelValue);

                        if (DEBUG_PRINT)
                        {
                            Debug.Write("[" + x + "|" + y + "] ["+ pixelValue + "] ");
                        }

                        var pixelColor = GetColorFromCLUT(Clut, pixelValue);

                        bitmap.SetPixel(x, y, pixelColor);
                    }
                }
            }

            public void loadBitmap2()
            {
                if (Data == null || Clut == null || Width <= 0 || Height <= 0)
                {
                    throw new ArgumentException("Can't create bitmap");
                }

                if (Data.Length != Width * Height || Clut.Length != CLUT_SIZE)
                {
                    throw new ArgumentException("Image data length or CLUT length is invalid");
                }

                bitmap = new AnyBitmap(Width, Height/*, PixelFormat.Format32bppArgb*/);

                using (var stream = new MemoryStream(Data))
                {
                    for (var y = 0; y < Height; y++)
                    {
                        for (var x = 0; x < Width; x += 4)
                        {
                            var pixelIndex = stream.ReadInt32();

                            if ((pixelIndex & 31) >= 8)
                            {
                                if ((pixelIndex & 31) < 16)
                                {
                                    pixelIndex += 8;                // +8 - 15 to +16 - 23
                                }
                                else if ((pixelIndex & 31) < 24)
                                {
                                    pixelIndex -= 8;                // +16 - 23 to +8 - 15
                                }
                            }

                            pixelIndex <<= 2;

                            var pixelGroup = BitConverter.GetBytes(pixelIndex);
                            var pixel1 = GetColorFromCLUT(Clut, pixelGroup[0]);
                            var pixel2 = GetColorFromCLUT(Clut, pixelGroup[1]);
                            var pixel3 = GetColorFromCLUT(Clut, pixelGroup[2]);
                            var pixel4 = GetColorFromCLUT(Clut, pixelGroup[3]);

                            bitmap.SetPixel(x, y, pixel1);
                            bitmap.SetPixel(x + 1, y, pixel2);
                            bitmap.SetPixel(x + 2, y, pixel3);
                            bitmap.SetPixel(x + 3, y, pixel4);
                        }
                    }
                }
            }

            public void printClut()
            {
                for(var i = 0; i < 256; i ++)
                {
                    var temp = new byte[4];
                    temp[0] = Clut[i*4];
                    temp[1] = Clut[i*4 + 1];
                    temp[2] = Clut[i*4 + 2];
                    temp[3] = Clut[i*4 + 3];
                    //Color color = Color.FromArgb(Clut[i+3], Clut[i], Clut[i + 1], Clut[i + 2]);
                    //Debug.WriteLine("["+i/4+"] "+getColorAsHex(color));
                    Debug.WriteLine("[" + i + "] <" + BitConverter.ToString(temp).Replace("-", "") + ">");
                    
                }
            }

            /*public static void CreateAndSavePng(byte[] imageData, byte[] clut, int width, int height, string outputPath)
            {
                if (imageData == null || clut == null || width <= 0 || height <= 0 || string.IsNullOrEmpty(outputPath))
                {
                    throw new ArgumentException("Invalid input parameters");
                }

                if (imageData.Length != width * height || clut.Length != 64 * 16)
                {
                    throw new ArgumentException("Image data length or CLUT length is invalid");
                }

                Bitmap bitmap = new Bitmap(width, height);

                int blankCount = 0;
                // Stored horizontally
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int pixelValue = imageData[y * width + x];

                        //Debug.Write("["+x+"|"+y+"] ");

                        //if (y == 4 && pixelValue != 0)
                        //    Debug.WriteLine("DEBUG");
                        //else
                        //    blankCount++;

                        Color pixelColor = GetColorFromCLUT(clut, pixelValue);

                        // Set the pixel color in the Bitmap
                        bitmap.SetPixel(x, y, pixelColor);
                    }
                }

                // Save the Bitmap as a PNG file
                //bitmap.Save(outputPath, ImageFormat.Png);
                bitmap.Save(outputPath, ImageFormat.Bmp);

                // Dispose of the Bitmap to free up resources
                bitmap.Dispose();
            }*/

            private static Color GetColorFromCLUT(byte[] clut, int index)
            {
                var start = index * 4;
                var red = clut[start];
                var green = clut[start + 1];
                var blue = clut[start + 2];
                //byte alpha = (byte)(clut[start + 3] / 2);
                //byte alpha = clut[start + 3];
                //byte alpha = (byte)(255 - clut[start + 3]);
                var alpha = (byte)((clut[start + 3] * 0xFF) >> 7);

                var color = Color.FromArgb(alpha, red, green, blue);

                return color;
            }

            private static string getColorAsHex(Color color)
            {
                var byteArray = new byte[4];
                byteArray[0] = color.R;
                byteArray[1] = color.G;
                byteArray[2] = color.B;
                byteArray[3] = color.A;
                return "<" + BitConverter.ToString(byteArray).Replace("-","") + ">";
            }

            public class MdlsImageInfo
            {
                [Data] public short Size { get; set; } // Image size in QWC (16b)
                [Data] public byte WidthExp { get; set; } // Eg: For width = 128 = 2^7, then WidthExp = 7
                [Data] public byte HeightExp { get; set; }
                [Data] public short Width { get; set; }
                [Data] public short Height { get; set; }
                [Data(Count = 2)] public int[] Unk { get; set; } // Padding?
            }
        }
    }
}
