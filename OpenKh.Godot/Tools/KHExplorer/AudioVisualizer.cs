using System;
using Godot;

namespace OpenKh.Godot.Tools.KHExplorer
{
    public partial class AudioVisualizer : Control
    {
        public enum ColorMode
        {
            Default,
            HalloweenTown,
            TimelessRiver,
        }
        [Export] public float FrequencyMax;
        [Export] public uint BarCount = 24;
        [Export] public AudioEffectSpectrumAnalyzerInstance Analyzer;
        [Export] public ColorMode VisualizerColorMode;

        private float[] _maxValues;
        private float[] _lastValue;
        public override void _Draw()
        {
            base._Draw();

            if (Analyzer is null) return;

            if (_maxValues is null || _maxValues.Length != BarCount) _maxValues = new float[BarCount];
            if (_lastValue is null || _lastValue.Length != BarCount) _lastValue = new float[BarCount];
            
            var segmentSize = Size.X / BarCount;

            for (var i = 0; i < BarCount; i++)
            {
                var from = (float)i / BarCount;
                var to = (float)(i + 1) / BarCount;
                var result = Analyzer.GetMagnitudeForFrequencyRange(from * FrequencyMax, to * FrequencyMax);
                
                var avg = (result.X + result.Y) * 0.5f;
                if (avg > _maxValues[i]) _maxValues[i] = avg;
                var max = _maxValues[i];
                var value = max > 0 ? avg / _maxValues[i] : 0;

                var lerp = Damp(_lastValue[i], value, 15, (float)GetProcessDeltaTime());
                _lastValue[i] = lerp;

                var color = VisualizerColorMode switch
                {
                    ColorMode.HalloweenTown => Color.FromHsv(from, 0.75f, 0.75f),
                    ColorMode.TimelessRiver => Color.FromHsv(0, 0, from),
                    _ => Color.FromHsv(from, 1, 1),
                };
                
                DrawRect(new Rect2(i * segmentSize, Size.Y - (lerp * Size.Y), segmentSize, lerp * Size.Y), color);
            }
        }
        private static float Damp(float a, float b, float lambda, float dt) => Mathf.Lerp(a, b, 1 - Mathf.Exp(-lambda * dt));
        public override void _Process(double delta)
        {
            base._Process(delta);
            QueueRedraw();
        }
    }
}
