using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using OpenKh.Common;
using Xe.BinaryMapper;

namespace OpenKh.Kh1
{
    public class Cvbl
    {
        public class CvblHeader
        {
            [Data] public uint Unk1 { get; set; }
            [Data] public uint NumMeshes { get; set; }
            [Data] public ushort NumUnknownEntries { get; set; }
            [Data] public ushort HasUnknownEntries { get; set; }
            [Data] public uint Unk2 { get; set; }
        }
        public class MeshEntry
        {
            [Data] public ushort Unk1 { get; set; }
            [Data] public ushort JointStyle { get; set; }
            [Data] public int Material { get; set; }
            [Data] public int Unk2 { get; set; }
            [Data] public uint MeshOffset { get; set; }
        }

        public class VertexStyle8 : IVertex
        {
            [Data] public float NormalX { get; set; }
            [Data] public float NormalY { get; set; }
            [Data] public float NormalZ { get; set; }
            [Data] public ushort JointSlotId { get; set; }
            [Data] public byte FaceType { get; set; } // 1 = (-2, -1, 0), 2 = (0, -1, -2)
            [Data] public byte UseFace { get; set; } // must == 0 for face
            [Data] public float U { get; set; }
            [Data] public float V { get; set; }
            
            [Data] public float Unk1 { get; set; } // is this the weird Y UV coordinate that mdls has?
            [Data] public uint Unk2 { get; set; }
            
            [Data] public float PositionX { get; set; }
            [Data] public float PositionY { get; set; }
            [Data] public float PositionZ { get; set; }
            [Data] public float Weight { get; set; }
            


            public Vector3 Position
            {
                get => new(PositionX, PositionY, PositionZ);
                set
                {
                    PositionX = value.X;
                    PositionY = value.Y;
                    PositionZ = value.Z;
                }
            }
            public Vector3 Normal => new(NormalX, NormalY, NormalZ);
            public Vector2 UV => new(U, V);
            public ushort[] Joints
            {
                get => [JointSlotId];
                set
                {
                    if (value is null || value.Length == 0) return;
                    JointSlotId = value.First();
                }
            }
            public float[] Weights => [Weight];
            
            public int Face => UseFace == 0 ? FaceType : 0;
            public int Size => 48;
        }

        public class VertexStyle9 : IVertex
        {
            [Data] public float NormalX { get; set; }
            [Data] public float NormalY { get; set; }
            [Data] public float NormalZ { get; set; }
            [Data] public ushort JointCount { get; set; }
            [Data] public byte FaceType { get; set; } // 1 = (-2, -1, 0), 2 = (0, -1, -2)
            [Data] public byte UseFace { get; set; } // must == 0 for face
            [Data] public float U { get; set; }
            [Data] public float V { get; set; }
            [Data(Count = 8)] public byte[] JointSlotIds { get; set; }
            [Data] public float PositionX { get; set; }
            [Data] public float PositionY { get; set; }
            [Data] public float PositionZ { get; set; }
            [Data] public float PositionW { get; set; } // ??? maybe unk?
            [Data(Count = 8)] public float[] JointWeights { get; set; }
            
            public Vector3 Position
            {
                get => new(PositionX, PositionY, PositionZ);
                set
                {
                    PositionX = value.X;
                    PositionY = value.Y;
                    PositionZ = value.Z;
                }
            }
            public Vector3 Normal => new(NormalX, NormalY, NormalZ);
            public Vector2 UV => new(U, V);
            public ushort[] Joints
            {
                get => JointSlotIds.Select(i => (ushort)i).Take(JointCount).ToArray();
                set
                {
                    if (value is null || value.Length != JointSlotIds.Length) return;
                    JointSlotIds = value.Select(i => (byte)i).ToArray();
                }
            }
            public float[] Weights => JointWeights.Take(JointCount).ToArray();
            
            public int Face => UseFace == 0 ? FaceType : 0;
            public int Size => 80;
        }

        public class VertexStyle10 : IVertex
        {
            [Data] public float PositionX { get; set; }
            [Data] public float PositionY { get; set; }
            [Data] public float PositionZ { get; set; }
            [Data] public ushort JointSlotId { get; set; }
            [Data] public byte FaceType { get; set; } // 1 = (-2, -1, 0), 2 = (0, -1, -2)
            [Data] public byte UseFace { get; set; } // must == 0 for face

            public Vector3 Position
            {
                get => new(PositionX, PositionY, PositionZ);
                set
                {
                    PositionX = value.X;
                    PositionY = value.Y;
                    PositionZ = value.Z;
                }
            }

            public ushort[] Joints
            {
                get => [JointSlotId];
                set
                {
                    if (value is null || value.Length == 0) return;
                    JointSlotId = value.First();
                }
            }
            public float[] Weights => [1];

            public int Face => UseFace == 0 ? FaceType : 0;
            public int Size => 16;
        }

        public interface IVertex
        {
            public Vector3 Position { get; set; }
            public Vector3 Normal => Vector3.Zero;
            public Vector2 UV => Vector2.Zero;
            public ushort[] Joints { get; set; }
            public float[] Weights => Array.Empty<float>();
            public int Face => 0;
            public int Size => 0;
        }

        public class Submesh
        {
            public int Material;
            public ushort JointStyle;

            public List<IVertex> Vertices = new();
            public List<uint[]> Faces = new();
            public Dictionary<uint, uint> JointSlots = new();
            
            public Dictionary<uint, List<uint>> ExtraJointData = new();
        }

        public CvblHeader Header;
        public List<Submesh> Submeshes = new();
        public List<MeshEntry> MeshEntries = new();

        private static Dictionary<uint, Matrix4x4> MdlsJointsToDictionary(List<Mdls.MdlsJoint> joints)
        {
            if (joints is null) return null;
            var dict = new Dictionary<uint, Matrix4x4>();
            for (var i = 0u; i < joints.Count; i++)
            {
                var j = joints[(int)i];
                
                var scaleMatrix = Matrix4x4.CreateScale(new Vector3(j.ScaleX, j.ScaleY, j.ScaleZ));
                
                var rotationMatrixX = Matrix4x4.CreateRotationX(j.RotateX);
                var rotationMatrixY = Matrix4x4.CreateRotationY(j.RotateY);
                var rotationMatrixZ = Matrix4x4.CreateRotationZ(j.RotateZ);
                var rotationMatrix = rotationMatrixX * rotationMatrixY * rotationMatrixZ;
                
                var translationMatrix = Matrix4x4.CreateTranslation(new Vector3(j.TranslateX, j.TranslateY, j.TranslateZ));

                dict[i] = scaleMatrix * rotationMatrix * translationMatrix;
            }
            return dict;
        }
        
        public Cvbl(Stream stream, List<Mdls.MdlsJoint> mdlsJoints)
        {
            var file = stream.ReadAllBytes();
            var str = new MemoryStream(file);
            var joints = MdlsJointsToDictionary(mdlsJoints);
            
            Header = BinaryMapping.ReadObject<CvblHeader>(str);

            str.Position = 0;

            if (Header.HasUnknownEntries is not (0 or 1)) return;
            var meshEntries = new List<MeshEntry>();
                
            var meshEntriesOffset = 16 + (Header.HasUnknownEntries == 1 ? Header.NumUnknownEntries * 32 : 0);
                
            str.Seek(meshEntriesOffset, SeekOrigin.Begin);
                
            for (var i = 0; i < Header.NumMeshes; i++) meshEntries.Add(BinaryMapping.ReadObject<MeshEntry>(str));

            MeshEntries = meshEntries;

            foreach (var meshEntry in meshEntries)
            {
                var mesh = new Submesh
                {
                    Material = meshEntry.Material,
                    JointStyle = meshEntry.JointStyle,
                };
                var jointStyle = meshEntry.JointStyle;
                    
                var jointSlots = new Dictionary<uint, (uint, Matrix4x4)>();
                for (var i = 0u; i < 48; i++)
                {
                    jointSlots[i] = (0, Matrix4x4.Identity);
                }
                    
                var extraJointData = new Dictionary<uint, List<uint>>();
                    
                    
                var totalVertCount = 0u;
                var run = true;
                var subsectionOffset = meshEntry.MeshOffset + 16;
                while (run && subsectionOffset < str.Length)
                {
                    var subsectionDataOffset = subsectionOffset + 8;
                        
                    str.Seek(subsectionOffset, SeekOrigin.Begin);
                        
                    var subsectionType = str.ReadUInt32();
                    var subsectionLength = str.ReadUInt32();
                        
                    if (subsectionType == 1)
                    {
                        var numVerts = str.ReadUInt32();

                        var submeshDataOffset = subsectionDataOffset + 24;
                        
                        str.Seek(submeshDataOffset, SeekOrigin.Begin);
                            
                        for (var i = 0u; i < numVerts; i++)
                        {
                            IVertex vert = meshEntry.JointStyle switch
                            {
                                8 => BinaryMapping.ReadObject<VertexStyle8>(str),
                                9 => BinaryMapping.ReadObject<VertexStyle9>(str),
                                10 => BinaryMapping.ReadObject<VertexStyle10>(str),
                                _ => null,
                            };

                            if (vert is null) continue;
                            
                            var bones = vert.Joints.Select(j => (ushort)jointSlots[j].Item1).ToArray();
                            vert.Joints = bones;
                            
                            if (joints != null)
                                vert.Position = RelativeToGlobalVertex(vert.Position, joints, vert.Joints.Select(j => (uint)j).ToArray(), vert.Weights,
                                    vert.Joints.Select(j => jointSlots[j].Item2).ToArray());
                                    

                            str.Seek(submeshDataOffset + ((i + 1) * vert.Size), SeekOrigin.Begin);
                                
                            switch (vert.Face)
                            {
                                case 1:
                                    mesh.Faces.Add([totalVertCount + i - 2, totalVertCount + i - 1, totalVertCount + i]);
                                    break;
                                case 2:
                                    mesh.Faces.Add([totalVertCount + i, totalVertCount + i - 1, totalVertCount + i - 2]);
                                    break;
                            }
                            mesh.Vertices.Add(vert);
                        }
                        totalVertCount += numVerts;
                    }
                    else if (subsectionType == 17)
                    {
                        var jointId = str.ReadUInt32();
                        var jointSlotId = str.ReadUInt32();
                        
                        jointSlots[jointSlotId] = (jointId, jointSlots[jointSlotId].Item2);

                        switch (jointStyle)
                        {
                            case 8:
                            {
                                var data = new List<uint>();
                                for (var i = 0; i < 28; i++) data.Add(str.ReadUInt32());
                                extraJointData[jointSlotId] = data;

                                break;
                            }
                            case 9:
                            {
                                var data = new List<uint>();
                                for (var i = 0; i < 28; i++) data.Add(str.ReadUInt32());
                                extraJointData[jointSlotId] = data;
                                    
                                jointSlots[jointSlotId] = (jointId, new Matrix4x4(
                                    str.ReadSingle(), str.ReadSingle(), str.ReadSingle(), str.ReadSingle(),
                                    str.ReadSingle(), str.ReadSingle(), str.ReadSingle(), str.ReadSingle(),
                                    str.ReadSingle(), str.ReadSingle(), str.ReadSingle(), str.ReadSingle(),
                                    str.ReadSingle(), str.ReadSingle(), str.ReadSingle(), str.ReadSingle()
                                ));
                                break;
                            }
                            case 10:
                            {
                                var data = new List<uint>();
                                for (var i = 0; i < 16; i++) data.Add(str.ReadUInt32());
                                extraJointData[jointSlotId] = data;
                                break;
                            }
                            default:
                                Console.WriteLine($"Warning: Unknown joint style: {jointStyle} @ {subsectionDataOffset}");
                                return;
                        }
                    }
                    else if (subsectionType == 32768) run = false;
                    else break;
                    subsectionOffset += subsectionLength;
                }

                mesh.JointSlots = jointSlots.ToDictionary(i => i.Key, i => i.Value.Item1);
                mesh.ExtraJointData = extraJointData;
                    
                Submeshes.Add(mesh);
            }
        }
        
        private static Vector3 RelativeToGlobalVertex(Vector3 vertex, Dictionary<uint, Matrix4x4> joints, IReadOnlyList<uint> jointIds, IReadOnlyList<float> weights, IReadOnlyList<Matrix4x4> toLocalTransforms)
        {
            var vert = new Vector4(0, 0, 0, 0);
            for (var i = 0; i < jointIds.Count; i++)
            {
                var jointId = jointIds[i];
                var weight = weights[i];
                var tlt = i < toLocalTransforms.Count ? toLocalTransforms[i] : Matrix4x4.Identity;
                var npVert = new Vector4(vertex, 1) * weight;
                var globalMat = joints[jointId] * tlt;
                npVert = Vector4.Transform(npVert, globalMat);
                vert += npVert;
            }
            return new Vector3(vert.X, vert.Y, vert.Z);
        }
    }
}
