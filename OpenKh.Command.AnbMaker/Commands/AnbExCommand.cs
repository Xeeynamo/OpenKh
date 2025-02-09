using McMaster.Extensions.CommandLineUtils;
using NLog;
using OpenKh.Command.AnbMaker.Commands.Interfaces;
using OpenKh.Command.AnbMaker.Commands.Utils;
using OpenKh.Command.AnbMaker.Utils.AssimpAnimSource;
using OpenKh.Command.AnbMaker.Utils.Builder;
using OpenKh.Command.AnbMaker.Utils.Builder.Models;
using OpenKh.Command.AnbMaker.Utils.GltfAnimSource;
using OpenKh.Command.AnbMaker.Utils.JsonAnimSource;
using OpenKh.Kh2;
using System.ComponentModel.DataAnnotations;

namespace OpenKh.Command.AnbMaker.Commands
{
    [HelpOption]
    [Command(Description = "fbx file: fbx to interpolated motion anb")]
    internal class AnbExCommand : IFbxSourceItemSelector, IMsetInjector
    {
        [Required]
        [FileExists]
        [Argument(0, Description = "fbx input")]
        public string InputModel { get; set; } = null!;

        [Argument(1, Description = "anb output")]
        public string? OutputMset { get; set; }

        [Option(Description = "specify root armature node name", ShortName = "r")]
        public string? RootName { get; set; }

        [Option(Description = "specify mesh name to read bone data", ShortName = "m")]
        public string? MeshName { get; set; }

        [Option(Description = "specify animation name to read bone data", ShortName = "a")]
        public string? AnimationName { get; set; }

        [Option(Description = "apply scaling to each source node", ShortName = "x", LongName = "node-scaling")]
        public float NodeScaling { get; set; } = 1;

        [Option(Description = "apply scaling to each bone position", ShortName = "p", LongName = "position-scaling")]
        public float PositionScaling { get; set; } = 1;

        [Option(Description = "optionally inject new motion into mset directly", ShortName = "w")]
        public string? MsetFile { get; set; }

        [Option(Description = "zero based target index of bar entry in mset file", ShortName = "i")]
        public int MsetIndex { get; set; }

        [Option(Description = "prefer UseJson as BasicSourceMotion", LongName = "use-json", ShortName = "j")]
        public bool UseJson { get; set; }

        [Option(Description = "prefer UseGltf as BasicSourceMotion", LongName = "use-gltf", ShortName = "g")]
        public bool UseGltf { get; set; }

        [Option(Description = "prefer UseAssimp as BasicSourceMotion", LongName = "use-assimp", ShortName = "s")]
        public bool UseAssimp { get; set; }

        protected int OnExecute(CommandLineApplication app)
        {
            var logger = LogManager.GetLogger("InterpolatedMotionMaker");

            IEnumerable<BasicSourceMotion> parms;
            OutputMset = Path.GetFullPath(OutputMset ?? Path.GetFileNameWithoutExtension(InputModel) + ".mset");
            IEnumerable<BasicSourceMotion> LoadByUseJson()
            {
                logger.Debug("UseJson as BasicSourceMotion");
                return new UseJson(
                    inputModel: InputModel,
                    meshName: MeshName,
                    rootName: RootName,
                    animationName: AnimationName,
                    nodeScaling: 1
                )
                    .Parameters;
            }

            IEnumerable<BasicSourceMotion> LoadByUseAssimp()
            {
                logger.Debug("UseAssimp as BasicSourceMotion");
                return new UseAssimp(
                    inputModel: InputModel,
                    meshName: MeshName,
                    rootName: RootName,
                    animationName: AnimationName,
                    nodeScaling: NodeScaling,
                    positionScaling: PositionScaling
                )
                    .Parameters;
            }

            IEnumerable<BasicSourceMotion> LoadByUseGltf()
            {
                logger.Debug("UseGltf as BasicSourceMotion");
                return new UseGltf().Load(
                    inputModel: InputModel,
                    meshName: MeshName,
                    rootName: RootName,
                    animationName: AnimationName,
                    nodeScaling: 1
                );
            }

            var fileExtension = Path.GetExtension(InputModel).ToLowerInvariant();

            if (false)
            { }
            else if (UseAssimp)
            {
                parms = LoadByUseAssimp();
            }
            else if (UseJson)
            {
                parms = LoadByUseJson();
            }
            else if (UseGltf)
            {
                parms = LoadByUseGltf();
            }
            else if (fileExtension == ".json")
            {
                parms = LoadByUseJson();
            }
            else
            {
                parms = LoadByUseAssimp();
            }

            OutputMset = Path.GetFullPath(OutputMset ?? Path.GetFileNameWithoutExtension(InputModel) + ".anb");

            logger.Info($"Writing to: {0}", OutputMset);

            foreach (var parm in parms.Take(1))
            {
                logger.Debug("Printing summary of BasicSourceMotion");

                logger.Debug($"DurationInTicks = {parm.DurationInTicks}");
                logger.Debug($"TicksPerSecond = {parm.TicksPerSecond}");
                logger.Debug($"BoneCount = {parm.BoneCount}");
                logger.Debug($"NodeScaling = {parm.NodeScaling}");
                logger.Debug($"PositionScaling = {parm.PositionScaling}");

                logger.Debug("Invoking InterpolatedMotionBuilder");

                var builder = new InterpolatedMotionBuilder(parm);

                var ipm = builder.Ipm;

                logger.Debug($"{ipm.ConstraintActivations.Count,6:#,##0} ConstraintActivations");
                logger.Debug($"{ipm.Constraints.Count,6:#,##0} Constraints");
                logger.Debug($"{ipm.ExpressionNodes.Count,6:#,##0} ExpressionNodes");
                logger.Debug($"{ipm.Expressions.Count,6:#,##0} Expressions");
                logger.Debug($"{ipm.ExternalEffectors.Count,6:#,##0} ExternalEffectors");
                logger.Debug($"{ipm.FCurveKeys.Count,6:#,##0} FCurveKeys");
                logger.Debug($"{ipm.FCurvesForward.Count,6:#,##0} FCurvesForward");
                logger.Debug($"{ipm.FCurvesInverse.Count,6:#,##0} FCurvesInverse");
                logger.Debug($"{ipm.IKHelpers.Count,6:#,##0} IKHelpers");
                logger.Debug($"{ipm.InitialPoses.Count,6:#,##0} InitialPoses");
                logger.Debug($"{ipm.Joints.Count,6:#,##0} Joints");
                logger.Debug($"{ipm.KeyTangents.Count,6:#,##0} KeyTangents");
                logger.Debug($"{ipm.KeyTimes.Count,6:#,##0} KeyTimes");
                logger.Debug($"{ipm.KeyValues.Count,6:#,##0} KeyValues");

                var motionStream = (MemoryStream)ipm.toStream();

                var anbBarStream = new MemoryStream();
                Bar.Write(
                    anbBarStream,
                    new Bar.Entry[]
                    {
                        new Bar.Entry
                        {
                            Type = Bar.EntryType.Motion,
                            Name = "A999",
                            Stream = motionStream,
                        },
                        new Bar.Entry
                        {
                            Type = Bar.EntryType.MotionTriggers,
                            Name = "A999",
                            Stream = new MemoryStream()  // Replace null with MemoryStream containing "0"
                        }
                    }
                );

                var msetBarStream = new MemoryStream();
                Bar.Write(
                msetBarStream,
                    new Bar.Entry[]
                    {
                        new Bar.Entry
                        {
                            Type = Bar.EntryType.Anb,
                            Name = "A999",
                            Stream = anbBarStream
                        }
                    }
               );

                File.WriteAllBytes(OutputMset, anbBarStream.ToArray());
                File.WriteAllBytes(OutputMset + ".raw", motionStream.ToArray());
                File.WriteAllBytes(OutputMset, msetBarStream.ToArray());

                logger.Info($"Motion data generation successful");

                new MsetInjector().InjectMotionTo(this, motionStream.ToArray());
            }

            return 0;
        }
    }
}
