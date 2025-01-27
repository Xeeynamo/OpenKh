using Assimp;
using McMaster.Extensions.CommandLineUtils;
using NLog;
using OpenKh.Command.AnbMaker.Commands.Interfaces;
using OpenKh.Command.AnbMaker.Commands.Utils;
using OpenKh.Command.AnbMaker.Extensions;
using OpenKh.Command.AnbMaker.Utils;
using OpenKh.Command.AnbMaker.Utils.Builder;
using OpenKh.Command.AnbMaker.Utils.Builder.Models;
using OpenKh.Kh2;
using System.ComponentModel.DataAnnotations;
using static OpenKh.Command.AnbMaker.Utils.Builder.RawMotionBuilder;
using static OpenKh.Kh2.Motion;

namespace OpenKh.Command.AnbMaker.Commands
{
    [HelpOption]
    [Command(Description = "fbx file: fbx to raw anb")]
    internal class AnbCommand : IFbxSourceItemSelector, IMsetInjector
    {
        [Required]
        [FileExists]
        [Argument(0, Description = "fbx input")]
        public string InputModel { get; set; } = null!;

        [Argument(1, Description = "anb output")]
        public string? Output { get; set; }

        [Option(Description = "specify root armature node name", ShortName = "r")]
        public string? RootName { get; set; }

        [Option(Description = "specify mesh name to read bone data", ShortName = "m")]
        public string? MeshName { get; set; }

        [Option(Description = "apply scaling to each source node", ShortName = "x", LongName = "node-scaling")]
        public float NodeScaling { get; set; } = 1;

        [Option(Description = "apply scaling to each bone position", ShortName = "p", LongName = "position-scaling")]
        public float PositionScaling { get; set; } = 1;

        [Option(Description = "specify animation name to read bone data", ShortName = "a")]
        public string? AnimationName { get; set; }

        [Option(Description = "optionally inject new motion into mset directly", ShortName = "w")]
        public string? MsetFile { get; set; }

        [Option(Description = "zero based target index of bar entry in mset file", ShortName = "i")]
        public int MsetIndex { get; set; }

        protected int OnExecute(CommandLineApplication app)
        {
            var logger = LogManager.GetLogger("RawMotionMaker");

            Output = Path.GetFullPath(Output ?? Path.GetFileNameWithoutExtension(InputModel) + ".anb");

            Console.WriteLine($"Writing to: {Output}");

            var parms = new UseAssimpForRaw(
                InputModel,
                MeshName,
                RootName,
                AnimationName,
                NodeScaling,
                PositionScaling
            )
                .Parameters;

            foreach (var parm in parms.Take(1))
            {
                var builder = new RawMotionBuilder(
                    parm
                );

                logger.Debug($"(frameCount {parm.DurationInTicks:#,##0}) x (boneCount {parm.BoneCount:#,##0}) -> {builder.Raw.AnimationMatrices.Count:#,##0} matrices ({64 * builder.Raw.AnimationMatrices.Count:#,##0} bytes)");

                var rawMotionStream = new MemoryStream();
                RawMotion.Write(rawMotionStream, builder.Raw);

                var anbBarStream = new MemoryStream();
                Bar.Write(
                    anbBarStream,
                    new Bar.Entry[]
                    {
                        new Bar.Entry
                        {
                            Type = Bar.EntryType.Motion,
                            Name = "raw",
                            Stream = rawMotionStream,
                        }
                    }
                );

                File.WriteAllBytes(Output, anbBarStream.ToArray());
                File.WriteAllBytes(Output + ".raw", rawMotionStream.ToArray());

                logger.Debug("Raw motion data generation successful");

                new MsetInjector().InjectMotionTo(this, rawMotionStream.ToArray());
            }

            return 0;
        }

        internal class UseAssimpForRaw
        {
            public IEnumerable<RawMotionBuilder.Parameter> Parameters { get; }

            public UseAssimpForRaw(
                string inputModel,
                string? meshName,
                string? rootName,
                string? animationName,
                float nodeScaling,
                float positionScaling
            )
            {
                using var assimp = new Assimp.AssimpContext();
                var scene = assimp.ImportFile(inputModel, Assimp.PostProcessSteps.None);

                var outputList = new List<RawMotionBuilder.Parameter>();

                bool IsMeshNameMatched(string it) =>
                    string.IsNullOrEmpty(meshName)
                        ? true
                        : it == meshName;

                bool IsAnimationNameMatched(string it) =>
                    string.IsNullOrEmpty(animationName)
                        ? true
                        : it == animationName;

                var hits = new List<(Mesh mesh, Matrix4x4 transform)>();

                void WalkNode(Assimp.Node node, Matrix4x4 parentTransform)
                {
                    var transform = node.Transform * parentTransform;

                    foreach (var childNode in node.Children)
                    {
                        WalkNode(childNode, transform);
                    }

                    foreach (var meshIdx in node.MeshIndices)
                    {
                        var fbxMesh = scene.Meshes[meshIdx];
                        if (IsMeshNameMatched(fbxMesh.Name))
                        {
                            hits.Add((fbxMesh, transform));
                        }
                    }
                }

                WalkNode(scene.RootNode, Matrix4x4.Identity);

                foreach (var hit in hits.Take(1))
                {
                    var fbxArmatureRoot = AssimpHelper.FindRootBone(scene.RootNode, rootName);
                    var fbxArmatureNodes = AssimpHelper.FlattenNodes(fbxArmatureRoot, hit.mesh);
                    var fbxArmatureBoneCount = fbxArmatureNodes.Length;

                    foreach (var fbxAnim in scene.Animations.Where(anim => IsAnimationNameMatched(anim.Name)))
                    {
                        outputList.Add(
                            new Parameter
                            {
                                DurationInTicks = (int)fbxAnim.DurationInTicks,
                                TicksPerSecond = (float)fbxAnim.TicksPerSecond,
                                BoneCount = fbxArmatureNodes.Length,
                                NodeScaling = nodeScaling,
                                Bones = fbxArmatureNodes
                                    .Select(
                                        node => new ABone
                                        {
                                            NodeName = node.ArmatureNode.Name,
                                            ParentIndex = node.ParentIndex,
                                        }
                                    )
                                    .ToArray(),

                                GetRelativeMatrix = (frameIdx, boneIdx) =>
                                {
                                    var nodeRef = fbxArmatureNodes[boneIdx];
                                    var armatureNode = nodeRef.ArmatureNode;
                                    var name = armatureNode.Name;

                                    var oldTransform = armatureNode.Transform;

                                    oldTransform.Decompose(
                                        out Vector3D initialScaling,
                                        out Quaternion initialQuaternion,
                                        out Vector3D initialTranslation
                                    );

                                    var hit = fbxAnim.NodeAnimationChannels.FirstOrDefault(it => it.NodeName == name);

                                    var translation = (hit == null)
                                        ? initialTranslation
                                        : hit.PositionKeys.GetInterpolatedVector(frameIdx);

                                    var rotation = (hit == null)
                                        ? initialQuaternion
                                        : hit.RotationKeys.GetInterpolatedQuaternion(frameIdx);

                                    var scale = (hit == null)
                                        ? initialScaling
                                        : hit.ScalingKeys.GetInterpolatedVector(frameIdx);

                                    Vector3D FixScaling(Vector3D value) =>
                                        value;

                                    Vector3D FixPos(Vector3D value) =>
                                        value * positionScaling;

                                    var newTransform = Matrix4x4.Identity
                                        * new Matrix4x4(rotation.GetMatrix())
                                        * Matrix4x4.FromScaling(FixScaling(scale))
                                        * Matrix4x4.FromTranslation(FixPos(translation))
                                        ;

                                    return newTransform.ToDotNetMatrix4x4();
                                }
                            }
                        );
                    }
                }

                Parameters = outputList;
            }
        }
    }
}
