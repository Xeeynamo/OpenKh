using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenKh.Common;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class Bop
    {
        public class BopEntry
        {
            [Data] public float PositionX { get; set; }
            [Data] public float PositionY { get; set; }
            [Data] public float PositionZ { get; set; }
            
            [Data] public float RotationX { get; set; }
            [Data] public float RotationY { get; set; }
            [Data] public float RotationZ { get; set; }
            
            [Data] public float ScaleX { get; set; }
            [Data] public float ScaleY { get; set; }
            [Data] public float ScaleZ { get; set; }
            
            [Data] public uint BobIndex { get; set; }
            [Data] public uint Group { get; set; }
            [Data] public int MotionIndex { get; set; }
            [Data] public uint MotionOffset { get; set; }
            [Data] public uint Flag { get; set; }
            
            [Data] public float ModelHUpper { get; set; }
            [Data] public float ModelHLower { get; set; }
            [Data] public float ModelMUpper { get; set; }
            [Data] public float ModelMLower { get; set; }
            [Data] public float ModelLUpper { get; set; }
            [Data] public float ModelLLower { get; set; }
            
            [Data] public float PartsHUpper { get; set; }
            [Data] public float PartsHLower { get; set; }
            [Data] public float PartsMUpper { get; set; }
            [Data] public float PartsMLower { get; set; }
            [Data] public float PartsLUpper { get; set; }
            [Data] public float PartsLLower { get; set; }
        }

        public List<BopEntry> Entries = new();
        private Bop()
        {
            
        }
        public static Bop Read(Stream stream)
        {
            var bop = new Bop();

            stream.ReadUInt32(); //the number 8
            var fileSize = stream.ReadUInt32();
            var count = (int)(fileSize / 0x68);
            
            bop.Entries = Enumerable.Range(0, count).Select(_ => BinaryMapping.ReadObject<BopEntry>(stream)).ToList();
            
            return bop;
        }
    }
}
