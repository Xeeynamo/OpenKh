using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenKh.Common;
using OpenKh.Kh2.SystemData;
using Xe.BinaryMapper;

namespace OpenKh.Kh2;

public class Bgm
{
    [Data] public uint Identifier { get; set; }
    [Data] public ushort SequenceID { get; set; }
    [Data] public ushort SoundfontID { get; set; }
    [Data] public byte TrackCount { get; set; }
    [Data] public byte Unk0 { get; set; }
    [Data] public byte Unk1 { get; set; }
    [Data] public byte Unk2 { get; set; }
    [Data] public sbyte Volume { get; set; }
    [Data] public byte Unk3 { get; set; }
    [Data] public ushort PartsPerQuarterNote { get; set; }
    [Data] public uint FileSize { get; set; }
        
    [Data] public uint Reserved0 { get; set; }
    [Data] public uint Reserved1 { get; set; }
    [Data] public uint Reserved2 { get; set; }

    public List<byte[]> Tracks = new();

    public class BgmTrack
    {
        [Data] public byte[] Raw { get; set; }
    }
        
    public static Bgm Read(Stream stream)
    {
        var bgm = BinaryMapping.ReadObject<Bgm>(stream);

        bgm.Tracks = Enumerable.Range(0, bgm.TrackCount).Select(_ => stream.ReadBytes((int)stream.ReadUInt32())).ToList();

        return bgm;
    }
}

public enum BgmTrackCommandType
{
    EndOfTrack = 0x00,
    LoopBegin = 0x02,
    LoopEnd = 0x03,
    SetTempo = 0x08,
    Unk0 = 0x0A,
    TimeSignature = 0x0C,
    Unk1 = 0x0D,
    NoteOnPreviousKeyVelocity = 0x10,
    NoteOn = 0x11,
    NoteOnPreviousVelocity = 0x12,
    NoteOnPreviousKey = 0x13,
    NoteOffPreviousNote = 0x18,
    Unk2 = 0x19,
    NoteOff = 0x1A,
    ProgramChange = 0x20,
    Volume = 0x22,
    Expression = 0x24,
    Pan = 0x26,
    Unk3 = 0x28,
    Unk4 = 0x31,
    Unk5 = 0x34,
    Unk6 = 0x35,
    SustainPedal = 0x3C,
    Unk7 = 0x3E,
    Unk8 = 0x40,
    Unk9 = 0x47,
    Unk10 = 0x48,
    Unk11 = 0x50,
    Unk12 = 0x58,
    Unk13 = 0x5C,
    Portamento = 0x5D,
}

public static class BgmTrackCommandHelper
{
    public static int ArgumentByteReadCount(this BgmTrackCommandType command)
    {
        switch (command)
        {
            case BgmTrackCommandType.SetTempo:
            case BgmTrackCommandType.Unk0:
            case BgmTrackCommandType.Unk1:
            case BgmTrackCommandType.NoteOnPreviousVelocity:
            case BgmTrackCommandType.NoteOnPreviousKey:
            case BgmTrackCommandType.NoteOff:
            case BgmTrackCommandType.ProgramChange:
            case BgmTrackCommandType.Volume:
            case BgmTrackCommandType.Expression:
            case BgmTrackCommandType.Pan:
            case BgmTrackCommandType.Unk3:
            case BgmTrackCommandType.Unk4:
            case BgmTrackCommandType.Unk5:
            case BgmTrackCommandType.Unk6:
            case BgmTrackCommandType.SustainPedal:
            case BgmTrackCommandType.Unk7:
            case BgmTrackCommandType.Unk12:
            case BgmTrackCommandType.Portamento:
                return 1;
            case BgmTrackCommandType.TimeSignature:
            case BgmTrackCommandType.NoteOn:
            case BgmTrackCommandType.Unk2:
            case BgmTrackCommandType.Unk9:
                return 2;
            case BgmTrackCommandType.Unk8:
            case BgmTrackCommandType.Unk10:
            case BgmTrackCommandType.Unk11:
                return 3;
            default:
                return 0;
        }
    }
}
