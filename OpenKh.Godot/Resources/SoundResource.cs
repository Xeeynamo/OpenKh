using Godot;

namespace OpenKh.Godot.Resources;

[Tool]
public partial class SoundResource : Resource
{
    public enum Codec
    {
        Ogg,
        msadpcm,
    }
    [Export] public AudioStream Sound;
    [Export] public uint SampleRate;
    [Export] public uint ChannelCount;
    [Export] public uint LoopStartRaw;
    [Export] public uint LoopEndRaw;
    [Export] public float LoopStart;
    [Export] public float LoopEnd;
    [Export] public bool Loop;
    [Export] public Codec OriginalCodec;
}
