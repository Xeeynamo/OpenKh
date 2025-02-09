using libcsv;
using McMaster.Extensions.CommandLineUtils;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenKh.Kh2.Motion;

namespace OpenKh.Command.AnbMaker.Commands
{
    [HelpOption]
    [Command(Description = "anb file: dump scale rotation translation to csv")]
    internal class DumpSrtCommand
    {
        [Required]
        [FileExists]
        [Argument(0, Description = "anb input")]
        public string InputMotion { get; set; } = null!;

        [Argument(1, Description = "csv output")]
        public string? OutputCsv { get; set; }

        [Option(Description = "use radians instead of degrees", ShortName = "r", LongName = "use-radians")]
        public bool UseRadians { get; set; }

        private record MotionSource(
            InterpolatedMotion Motion,
            string Name);

        protected int OnExecute(CommandLineApplication app)
        {
            OutputCsv = Path.GetFullPath(OutputCsv ?? Path.GetFileNameWithoutExtension(InputMotion) + ".csv");

            Console.WriteLine($"Writing to: {OutputCsv}");

            var fileStream = new MemoryStream(
                File.ReadAllBytes(InputMotion)
            );

            var motionSourceList = new List<MotionSource>();

            string FilterName(string name) => name.Trim();

            var barFile = Bar.Read(fileStream);
            if (barFile.Any(it => it.Type == Bar.EntryType.Anb))
            {
                // this is mset
                motionSourceList.AddRange(
                    barFile
                        .Where(barEntry => barEntry.Type == Bar.EntryType.Anb && barEntry.Stream.Length >= 16)
                        .SelectMany(
                            (barEntry, barEntryIndex) =>
                                Bar.Read(barEntry.Stream)
                                    .Where(subBarEntry => subBarEntry.Type == Bar.EntryType.Motion)
                                    .Select(
                                        (subBarEntry, subBarEntryIndex) => new MotionSource(
                                            new InterpolatedMotion(subBarEntry.Stream),
                                            $"{barEntryIndex}_{FilterName(barEntry.Name)}_{subBarEntryIndex}_{FilterName(subBarEntry.Name)}"
                                        )
                                    )
                        )
                );
            }
            else if (barFile.Any(barEntry => barEntry.Type == Bar.EntryType.Motion))
            {
                // this is anb
                motionSourceList.AddRange(
                    barFile
                        .Where(barEntry => barEntry.Type == Bar.EntryType.Motion)
                        .Select(
                            (barEntry, barEntryIndex) =>
                                new MotionSource(
                                    new InterpolatedMotion(barEntry.Stream),
                                    $"{barEntryIndex}_{FilterName(barEntry.Name)}"
                                )
                        )
                        .ToArray()
                );
            }
            else
            {
                Console.Error.WriteLine("Error. Specify valid file of either mset or anb.");
                return 1;
            }

            Func<float, float> consumeRadians = UseRadians switch
            {
                true => radians => radians,
                false => radians => radians * 180 / MathF.PI,
            };

            var perMotionListAll = new List<PerMotion>();

            foreach (var motionSource in motionSourceList)
            {
                var keys = motionSource.Motion.FCurveKeys;
                var keyValues = motionSource.Motion.KeyValues;
                var keyTimes = motionSource.Motion.KeyTimes;

                var perMotionList = new List<PerMotion>();

                foreach (var curvesInput in new CurvesInput[0]
                    .Append(new CurvesInput(
                        FCurves: motionSource.Motion.FCurvesForward,
                        BaseJointIndex: 0,
                        GetFK: index => index,
                        GetIK: index => null))
                    .Append(new CurvesInput(
                        FCurves: motionSource.Motion.FCurvesInverse,
                        BaseJointIndex: motionSource.Motion.InterpolatedMotionHeader.BoneCount,
                        GetFK: index => null,
                        GetIK: index => index))
                )
                {
                    var baseJointId = curvesInput.BaseJointIndex;

                    foreach (var fCurve in curvesInput.FCurves)
                    {
                        Action<PerMotion, float> setter = fCurve.ChannelValue switch
                        {
                            Channel.SCALE_X => (it, value) => it.ScaleX = value,
                            Channel.SCALE_Y => (it, value) => it.ScaleY = value,
                            Channel.SCALE_Z => (it, value) => it.ScaleZ = value,
                            Channel.ROTATION_X => (it, value) => it.RotationX = consumeRadians(value),
                            Channel.ROTATION_Y => (it, value) => it.RotationY = consumeRadians(value),
                            Channel.ROTATION_Z => (it, value) => it.RotationZ = consumeRadians(value),
                            Channel.TRANSLATION_X => (it, value) => it.TranslationX = value,
                            Channel.TRANSLATION_Y => (it, value) => it.TranslationY = value,
                            Channel.TRANSLATION_Z => (it, value) => it.TranslationZ = value,
                            _ => throw new NotImplementedException("" + fCurve.ChannelValue),
                        };

                        var jointId = fCurve.JointId;

                        PerMotion AllocatePerMotion(float time)
                        {
                            var hit = perMotionList.SingleOrDefault(
                                it => it.Time == time && it.Joint == baseJointId + jointId
                            );
                            if (hit == null)
                            {
                                perMotionList.Add(
                                    hit = new PerMotion(
                                        motionSource.Name,
                                        baseJointId + jointId,
                                        curvesInput.GetFK(jointId),
                                        curvesInput.GetIK(jointId),
                                        time
                                    )
                                );
                            }

                            return hit;
                        }

                        var baseIdx = (ushort)fCurve.KeyStartId;
                        for (int idx = 0; idx < fCurve.KeyCount; idx++)
                        {
                            var key = keys[baseIdx + idx];

                            setter(
                                AllocatePerMotion(keyTimes[key.Time]),
                                keyValues[key.ValueId]
                            );
                        }
                    }
                }

                perMotionListAll.AddRange(perMotionList);
            }

            {
                using var writer = new StreamWriter(OutputCsv, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
                var csv = new Csvw(writer, ',', '"');

                {
                    csv.Write("Name");
                    csv.Write("Joint");
                    csv.Write("FK");
                    csv.Write("IK");
                    csv.Write("Time");
                    csv.Write("ScaleX");
                    csv.Write("ScaleY");
                    csv.Write("ScaleZ");
                    csv.Write("RotationX");
                    csv.Write("RotationY");
                    csv.Write("RotationZ");
                    csv.Write("TranslationX");
                    csv.Write("TranslationY");
                    csv.Write("TranslationZ");
                }
                foreach (var row in perMotionListAll)
                {
                    csv.NextRow();
                    csv.Write(row.Name);
                    csv.Write(row.Joint + "");
                    csv.Write(row.FK + "");
                    csv.Write(row.IK + "");
                    csv.Write(row.Time + "");
                    csv.Write(row.ScaleX + "");
                    csv.Write(row.ScaleY + "");
                    csv.Write(row.ScaleZ + "");
                    csv.Write(row.RotationX + "");
                    csv.Write(row.RotationY + "");
                    csv.Write(row.RotationZ + "");
                    csv.Write(row.TranslationX + "");
                    csv.Write(row.TranslationY + "");
                    csv.Write(row.TranslationZ + "");
                }
            }

            return 0;
        }

        private record CurvesInput(
            IEnumerable<FCurve> FCurves,
            int BaseJointIndex,
            Func<int, int?> GetFK,
            Func<int, int?> GetIK);

        private record PerMotion(
            string Name,
            int Joint,
            int? FK,
            int? IK,
            float Time)
        {
            public float? ScaleX { get; set; }
            public float? ScaleY { get; set; }
            public float? ScaleZ { get; set; }
            public float? RotationX { get; set; }
            public float? RotationY { get; set; }
            public float? RotationZ { get; set; }
            public float? TranslationX { get; set; }
            public float? TranslationY { get; set; }
            public float? TranslationZ { get; set; }
        }
    }
}
