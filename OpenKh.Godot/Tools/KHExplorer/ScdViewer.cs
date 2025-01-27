using System.Linq;
using Godot;
using OpenKh.Godot.Helpers;
using OpenKh.Godot.Resources;

namespace OpenKh.Godot.Tools.KHExplorer
{
    public partial class ScdViewer : PanelContainer
    {
        [Export] public SoundContainer SoundContainer;
        [Export] public OptionButton IndexDropdown;
        [Export] public Button PlayButton;
        [Export] public Button PauseButton;
        [Export] public Button StopButton;
        [Export] public HSlider VolumeSlider;
        [Export] public HSlider ProgressSlider;
        [Export] public Button LoopButton;
        [Export] public AudioStreamPlayer Playback;
        [Export] public Label MetadataLabel;
        [Export] public Label ProgressFrontLabel;
        [Export] public Label ProgressBackLabel;
        [Export] public AudioVisualizer Visualizer;

        private float _lastPlaybackPos;
        private float _audioLength;
        private int _lastDropDown;

        public static PackedInstance<ScdViewer> Packed = new("res://Scenes/Tools/KHExplorer/ScdViewer.tscn");
        public override void _Ready()
        {
            base._Ready();
            
            var firstValid = SoundContainer.Sounds.FirstOrDefault(i => i is not null);
            if (firstValid is null) { QueueFree(); return; }
            
            Playback.Stop();
            Playback.Stream = firstValid.Sound;
            Playback.Play();

            var len = Playback.Stream.GetLength();

            _audioLength = (float)len;
            
            var currentIndex = SoundContainer.Sounds.IndexOf(SoundContainer.Sounds.First(i => i is not null && i.Sound == Playback.Stream));
            _lastDropDown = currentIndex;
            
            ProgressSlider.MinValue = 0;
            ProgressSlider.MaxValue = len;
            
            VolumeSlider.Value = Mathf.DbToLinear(Playback.VolumeDb);
            VolumeSlider.ValueChanged += VolumeSliderOnValueChanged;
            
            ProgressSlider.ValueChanged += ProgressSliderOnValueChanged;

            Visualizer.Analyzer = KHExplorer.KHExplorerRoot.Analyzer;
            Visualizer.FrequencyMax = 24000;
            
            var single = SoundContainer.Sounds.Where(i => i is not null).ToList().Count == 1;

            if (!single)
            {
                for (var index = 0; index < SoundContainer.Sounds.Count; index++)
                {
                    var entry = SoundContainer.Sounds[index];
                    if (entry is null) IndexDropdown.AddSeparator("---");
                    else IndexDropdown.AddItem($"{index}");
                }
                IndexDropdown.Select(currentIndex);
            }
            else
            {
                IndexDropdown.ProcessMode = ProcessModeEnum.Disabled;
                IndexDropdown.Visible = false;
            }
            
            if (firstValid.Loop)
            {
                LoopButton.ProcessMode = ProcessModeEnum.Inherit;
                LoopButton.Visible = true;
            }
            else
            {
                LoopButton.ProcessMode = ProcessModeEnum.Disabled;
                LoopButton.Visible = false;
            }
            
            IndexDropdown.ItemSelected += IndexDropdownOnItemSelected;
            PlayButton.Pressed += PlayButtonOnPressed;
            PauseButton.Pressed += PauseButtonOnPressed;
            StopButton.Pressed += StopButtonOnPressed;
            
            SetMetadataLabel();
        }
        private void StopButtonOnPressed() => Playback.Stop();
        private void PauseButtonOnPressed() => Playback.StreamPaused = !Playback.StreamPaused;
        private void PlayButtonOnPressed()
        {
            Playback.StreamPaused = false;
            Playback.Play();
        }
        private void VolumeSliderOnValueChanged(double value) => Playback.VolumeDb = Mathf.LinearToDb((float)value);
        private void IndexDropdownOnItemSelected(long index)
        {
            var sound = SoundContainer.Sounds[(int)index];
            
            Playback.Stop();
            Playback.Stream = sound.Sound;
            Playback.Play();
            _lastDropDown = (int)index;
            _lastPlaybackPos = 0;
            
            var len = sound.Sound.GetLength();
            
            _audioLength = (float)len;
            ProgressSlider.MaxValue = len;
            
            if (sound.Loop)
            {
                LoopButton.ProcessMode = ProcessModeEnum.Inherit;
                LoopButton.Visible = true;
            }
            else
            {
                LoopButton.ProcessMode = ProcessModeEnum.Disabled;
                LoopButton.Visible = false;
            }

            SetMetadataLabel();
        }
        private void ProgressSliderOnValueChanged(double value)
        {
            Playback.Stop();
            Playback.Play((float)value);
        }
        private void SetMetadataLabel()
        {
            var sound = SoundContainer.Sounds[_lastDropDown];
            var lengthTextBase = (float)sound.Sound.GetLength();
            MetadataLabel.Text = $"Length: {FormatTimeUpperMiddleLower(lengthTextBase)} Sample Rate: {sound.SampleRate} Codec: {sound.OriginalCodec.ToString()}";
            if (sound.Loop) MetadataLabel.Text += $"\nLoop Start: {FormatTimeUpperMiddleLower(sound.LoopStart)} Loop End: {FormatTimeUpperMiddleLower(sound.LoopEnd)}";
        }
        private static string FormatTimeUpperMiddleLower(float time)
        {
            var lower = Mathf.FloorToInt(time * 60) % 60;
            var middle = Mathf.FloorToInt(time) % 60;
            var upper = Mathf.FloorToInt(time) / 60;
            return $"{upper:00}:{middle:00}:{lower:00}";
        }
        public override void _Process(double delta)
        {
            base._Process(delta);
            
            var playbackPos = Playback.GetPlaybackPosition();

            if (_lastPlaybackPos != playbackPos)
            {
                _lastPlaybackPos = playbackPos;
                
                var pos = playbackPos + (Playback.Playing ? (float)AudioServer.GetTimeSinceLastMix() : 0);
                
                var inversePos = _audioLength - playbackPos;
                
                ProgressSlider.SetValueNoSignal(pos);
                ProgressFrontLabel.Text = FormatTimeUpperMiddleLower(playbackPos);
                ProgressBackLabel.Text = FormatTimeUpperMiddleLower(inversePos);
            }

            if (LoopButton.ButtonPressed && Playback.Playing)
            {
                var sound = SoundContainer.Sounds[_lastDropDown];
                
                //GD.Print($"{sound.SampleRate}, {sound.LoopStartRaw}, {sound.LoopEndRaw}, {sound.Sound.GetLength()}");

                if (sound.Loop)
                {
                    if (playbackPos > sound.LoopEnd)
                    {
                        var diff = sound.LoopStart - sound.LoopEnd;
                        //Playback.Seek(playbackPos + diff);
                        Playback.Seek(sound.LoopStart);
                    }
                }
            }
        }
    }
}
