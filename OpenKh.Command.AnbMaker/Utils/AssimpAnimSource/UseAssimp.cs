using OpenKh.Command.AnbMaker.Extensions;
using OpenKh.Command.AnbMaker.Utils.Builder.Models;
using System.Numerics;

namespace OpenKh.Command.AnbMaker.Utils.AssimpAnimSource
{
    public class UseAssimp
    {
        public IEnumerable<BasicSourceMotion> Parameters { get; }

        public UseAssimp(
            string inputModel,
            string? meshName,
            string? rootName,
            string? animationName,
            float nodeScaling,
            float positionScaling
        )
        {
            var outputList = new List<BasicSourceMotion>();
            Parameters = outputList;

            var assimp = new Assimp.AssimpContext();
            var scene = assimp.ImportFile(inputModel, Assimp.PostProcessSteps.None);

            bool IsMeshNameMatched(string it) =>
                string.IsNullOrEmpty(meshName)
                    ? true
                    : it == meshName;

            bool IsAnimationNameMatched(string it) =>
                string.IsNullOrEmpty(animationName)
                    ? true
                    : it == animationName;

            foreach (var fbxMesh in scene.Meshes.Where(mesh => IsMeshNameMatched(mesh.Name)))
            {
                var fbxArmatureRoot = AssimpHelper.FindRootBone(scene.RootNode, rootName);
                var fbxArmatureNodes = AssimpHelper.FlattenNodes(fbxArmatureRoot, fbxMesh);
                var fbxArmatureBoneCount = fbxArmatureNodes.Length;

                foreach (var fbxAnim in scene.Animations.Where(anim => IsAnimationNameMatched(anim.Name)))
                {
                    AScalarKey GetAScalarKey(double time, float value) =>
                        new AScalarKey
                        {
                            Time = (float)time,
                            Value = value,
                        };

                    var output = new BasicSourceMotion
                    {
                        DurationInTicks = (int)fbxAnim.DurationInTicks,
                        TicksPerSecond = (float)fbxAnim.TicksPerSecond,
                        BoneCount = fbxArmatureBoneCount,
                        NodeScaling = nodeScaling,
                        PositionScaling = positionScaling,
                        GetAChannel = boneIdx =>
                        {
                            var armatureNode = fbxArmatureNodes[boneIdx].ArmatureNode;
                            var name = armatureNode.Name;
                            var hit = fbxAnim.NodeAnimationChannels.FirstOrDefault(it => it.NodeName == name);
                            if (hit != null)
                            {
                                var pseudoRotations = hit.RotationKeys
                                    .Select(
                                        key => new PseudoRotation
                                        {
                                            time = key.Time,
                                            rotation = key.Value.ToDotNetQuaternion().ToEulerAngles()
                                        }
                                    )
                                    .ToArray();

                                return new AChannel
                                {
                                    ScaleXKeys = hit.ScalingKeys
                                        .Select(it => GetAScalarKey(it.Time, it.Value.X))
                                        .ToArray(),

                                    ScaleYKeys = hit.ScalingKeys
                                        .Select(it => GetAScalarKey(it.Time, it.Value.Y))
                                        .ToArray(),

                                    ScaleZKeys = hit.ScalingKeys
                                        .Select(it => GetAScalarKey(it.Time, it.Value.Z))
                                        .ToArray(),

                                    RotationXKeys = pseudoRotations
                                        .Select(it => GetAScalarKey(it.time, it.rotation.X))
                                        .ToArray(),
                                    RotationYKeys = pseudoRotations
                                        .Select(it => GetAScalarKey(it.time, it.rotation.Y))
                                        .ToArray(),
                                    RotationZKeys = pseudoRotations
                                        .Select(it => GetAScalarKey(it.time, it.rotation.Z))
                                        .ToArray(),

                                    PositionXKeys = hit.PositionKeys
                                        .Select(it => GetAScalarKey(it.Time, it.Value.X))
                                        .ToArray(),

                                    PositionYKeys = hit.PositionKeys
                                        .Select(it => GetAScalarKey(it.Time, it.Value.Y))
                                        .ToArray(),

                                    PositionZKeys = hit.PositionKeys
                                        .Select(it => GetAScalarKey(it.Time, it.Value.Z))
                                        .ToArray(),
                                };
                            }

                            return null;
                        },
                        Bones = fbxArmatureNodes
                            .Select(
                                node => new ABone
                                {
                                    NodeName = node.ArmatureNode.Name,
                                    ParentIndex = node.ParentIndex,
                                }
                            )
                            .ToArray(),
                        GetInitialMatrix = (boneIdx) =>
                        {
                            return fbxArmatureNodes[boneIdx].ArmatureNode.Transform
                                .ToDotNetMatrix4x4();
                        },
                    };

                    outputList.Add(output);
                }
            }
        }

        private class PseudoRotation
        {
            internal double time;
            internal Vector3 rotation;
        }
    }
}
