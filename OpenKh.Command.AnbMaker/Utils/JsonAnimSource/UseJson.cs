using NLog;
using OpenKh.Command.AnbMaker.Extensions;
using OpenKh.Command.AnbMaker.Utils.Builder.Models;
using System.Numerics;
using System.Text.Json;

namespace OpenKh.Command.AnbMaker.Utils.JsonAnimSource
{
    public class UseJson
    {
        public IEnumerable<BasicSourceMotion> Parameters { get; }

        public UseJson(
            string inputModel,
            string? meshName,
            string? rootName,
            string? animationName,
            float nodeScaling
        )
        {
            var logger = LogManager.GetCurrentClassLogger();

            logger.Warn("JSON model importer is still incomplete! There will be problem on skeleton processing of scale, rotation, and location");

            var model = JsonSerializer.Deserialize<BRoot>(
                File.ReadAllText(inputModel),
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                }
            );

            if (model == null)
            {
                throw new NullReferenceException("model");
            }

            if (model.Version != "1")
            {
                throw new Exception("Invalid JSON source version");
            }

            bool TestAnimName(string it) => string.IsNullOrEmpty(animationName) ? true : animationName == it;

            Parameters = model.Objects
                .Where(obj => true
                    && obj.Type == "ARMATURE"
                    && obj.Bones != null
                    && obj.AnimationAction != null
                    && TestAnimName(obj.AnimationAction.Name)
                )
                .Select(
                    obj =>
                    {
                        var action = obj.AnimationAction;

                        AScalarKey GetKey(BKeyFrame source, float valueAdd = 0)
                        {
                            return new AScalarKey
                            {
                                Time = source.Time - action.FrameStart,
                                Value = source.Value + valueAdd,
                            };
                        }

                        AScalarKey GetRotationKey(PseudoRotation rotation, float value)
                        {
                            return new AScalarKey
                            {
                                Time = (float)(rotation.time - action.FrameStart),
                                Value = value,
                            };
                        }

                        return new BasicSourceMotion
                        {
                            BoneCount = obj.Bones.Count(),
                            DurationInTicks = (int)(action.FrameEnd - action.FrameStart),
                            TicksPerSecond = NzFloat(obj.Fps, 25),
                            NodeScaling = nodeScaling,
                            GetAChannel = boneIdx =>
                            {
                                var bone = obj.Bones[boneIdx];
                                var group = action.Groups.FirstOrDefault(it => it.Name == bone.Name);

                                var parent = (bone.Parent == -1)
                                    ? Matrix4x4.Identity
                                    : GetMatrix4x4(obj.Bones[bone.Parent].MatrixLocal);
                                Matrix4x4.Invert(parent, out Matrix4x4 parentInv);

                                var matrix = parentInv * GetMatrix4x4(bone.MatrixLocal);
                                var head = ((bone.Parent == -1)
                                    ? Vector3.Zero
                                    : -GetVector3(obj.Bones[bone.Parent].HeadLocal)
                                )
                                    + GetVector3(bone.HeadLocal);

                                Matrix4x4.Decompose(
                                    matrix,
                                    out Vector3 boneScale,
                                    out Quaternion boneRotation,
                                    out Vector3 boneTranslation
                                );

                                var fcurves = group?.Channels ?? new BFCurve[0];

                                BFCurve ProvideFallback(string channelRef, float fallbackValue)
                                {
                                    return new BFCurve
                                    {
                                        ChannelRef = channelRef,
                                        KeyFrames = new BKeyFrame[]
                                        {
                                            new BKeyFrame
                                            {
                                                Time = 0,
                                                Value = fallbackValue,
                                            },
                                            new BKeyFrame
                                            {
                                                Time = action.FrameEnd - action.FrameStart,
                                                Value = fallbackValue
                                            },
                                        }
                                    };
                                }

                                var Qw = (fcurves
                                    .FirstOrDefault(it => it.ChannelRef == "rotation_quaternion.0") ?? ProvideFallback("rotation_quaternion.0", 1))
                                    .KeyFrames;
                                var Qx = (fcurves
                                    .FirstOrDefault(it => it.ChannelRef == "rotation_quaternion.1") ?? ProvideFallback("rotation_quaternion.1", 0))
                                    .KeyFrames;
                                var Qy = (fcurves
                                    .FirstOrDefault(it => it.ChannelRef == "rotation_quaternion.2") ?? ProvideFallback("rotation_quaternion.2", 0))
                                    .KeyFrames;
                                var Qz = (fcurves
                                    .FirstOrDefault(it => it.ChannelRef == "rotation_quaternion.3") ?? ProvideFallback("rotation_quaternion.3", 0))
                                    .KeyFrames;

                                var pseudoRotations = new List<PseudoRotation>();

                                if (new int[] { Qw.Length, Qx.Length, Qy.Length, Qz.Length }.Distinct().Count() == 1)
                                {
                                    pseudoRotations.AddRange(
                                        Enumerable.Range(0, Qw.Length)
                                            .Select(
                                                idx =>
                                                {
                                                    return new PseudoRotation
                                                    {
                                                        time = Qw[idx].Time,
                                                        rotation = (
                                                            Quaternion.Identity
                                                            * boneRotation
                                                            * new Quaternion(
                                                                x: Qx[idx].Value,
                                                                y: Qy[idx].Value,
                                                                z: Qz[idx].Value,
                                                                w: Qw[idx].Value
                                                            )
                                                        )
                                                            .ToEulerAngles()
                                                    };
                                                }
                                            )
                                    );
                                }

                                if (!pseudoRotations.Any())
                                {
                                    pseudoRotations.Add(
                                        new PseudoRotation
                                        {
                                            time = 0,
                                            rotation = boneRotation
                                                .ToEulerAngles(),
                                        }
                                    );
                                }

                                return new AChannel
                                {
                                    ScaleXKeys = (fcurves
                                        .FirstOrDefault(it => it.ChannelRef == "scale.0") ?? ProvideFallback("scale.0", boneScale.X))
                                        .KeyFrames
                                        .Select(it => GetKey(it))
                                        .ToArray(),

                                    ScaleYKeys = (fcurves
                                        .FirstOrDefault(it => it.ChannelRef == "scale.1") ?? ProvideFallback("scale.1", boneScale.Y))
                                        .KeyFrames
                                        .Select(it => GetKey(it))
                                        .ToArray(),

                                    ScaleZKeys = (fcurves
                                        .FirstOrDefault(it => it.ChannelRef == "scale.2") ?? ProvideFallback("scale.2", boneScale.Z))
                                        .KeyFrames
                                        .Select(it => GetKey(it))
                                        .ToArray(),

                                    RotationXKeys = pseudoRotations
                                        .Select(it => GetRotationKey(it, it.rotation.X))
                                        .ToArray(),

                                    RotationYKeys = pseudoRotations
                                        .Select(it => GetRotationKey(it, it.rotation.Y))
                                        .ToArray(),

                                    RotationZKeys = pseudoRotations
                                        .Select(it => GetRotationKey(it, it.rotation.Z))
                                        .ToArray(),

                                    PositionXKeys = (fcurves
                                        .FirstOrDefault(it => it.ChannelRef == "location.0") ?? ProvideFallback("location.0", boneTranslation.X))
                                        .KeyFrames
                                        .Select(it => GetKey(it, head.X))
                                        .ToArray(),

                                    PositionYKeys = (fcurves
                                        .FirstOrDefault(it => it.ChannelRef == "location.1") ?? ProvideFallback("location.1", boneTranslation.Y))
                                        .KeyFrames
                                        .Select(it => GetKey(it, head.Y))
                                        .ToArray(),

                                    PositionZKeys = (fcurves
                                        .FirstOrDefault(it => it.ChannelRef == "location.2") ?? ProvideFallback("location.2", boneTranslation.Z))
                                        .KeyFrames
                                        .Select(it => GetKey(it, head.Z))
                                        .ToArray(),
                                };
                            }
                        };
                    }
                )
                .ToArray();
        }

        private Vector3 GetVector3(float[] headLocal)
        {
            return new Vector3(
                headLocal[0],
                headLocal[1],
                headLocal[2]
            );
        }

        private static Matrix4x4 GetMatrix4x4(float[] m)
        {
            return new Matrix4x4(
                m[0], m[1], m[2], m[3],
                m[4], m[5], m[6], m[7],
                m[8], m[9], m[10], m[11],
                m[12], m[13], m[14], m[15]
            );
        }

        private class PseudoRotation
        {
            internal double time;
            internal Vector3 rotation;
        }

        private static float NzFloat(float value, float fallback)
        {
            return (value == 0) ? fallback : value;
        }
    }
}
