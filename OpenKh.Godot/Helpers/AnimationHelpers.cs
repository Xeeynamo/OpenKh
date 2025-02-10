using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Godot;
using OpenKh.Godot.Nodes;
using OpenKh.Kh2;
using Quaternion = Godot.Quaternion;
using Vector3 = Godot.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace OpenKh.Godot.Helpers
{
    public static class AnimationHelpers
    {
        public static bool IsPositionChannel(this Motion.Channel channel) => channel is Motion.Channel.TRANSLATION_X or Motion.Channel.TRANSLATION_Y or Motion.Channel.TRANSLATION_Z;
        public static bool IsRotationChannel(this Motion.Channel channel) => channel is Motion.Channel.ROTATION_X or Motion.Channel.ROTATION_Y or Motion.Channel.ROTATION_Z;
        public static bool IsScaleChannel(this Motion.Channel channel) => channel is Motion.Channel.SCALE_X or Motion.Channel.SCALE_Y or Motion.Channel.SCALE_Z;

        public static bool IsXChannel(this Motion.Channel channel) => channel is Motion.Channel.TRANSLATION_X or Motion.Channel.ROTATION_X or Motion.Channel.SCALE_X;
        public static bool IsYChannel(this Motion.Channel channel) => channel is Motion.Channel.TRANSLATION_Y or Motion.Channel.ROTATION_Y or Motion.Channel.SCALE_Y;
        public static bool IsZChannel(this Motion.Channel channel) => channel is Motion.Channel.TRANSLATION_Z or Motion.Channel.ROTATION_Z or Motion.Channel.SCALE_Z;
        
        public static float CubicHermite(float t, float p0, float p1, float m0, float m1)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            return (2 * t3 - 3 * t2 + 1) * p0 + (t3 - 2 * t2 + t) * m0 + (-2 * t3 + 3 * t2) * p1 + (t3 - t2) * m1;
        }
        
        public static Quaternion AnimationRotation(Vector3 rotation) => ImportHelpers.CommonRotation(rotation.X, rotation.Y, rotation.Z);
        public static Transform3D CreateTransform(Vector3 pos, Vector3 rot, Vector3 scale)
        {
            var scaleTransform = Transform3D.Identity.Scaled(scale);
            var rotationTransform = new Transform3D(new Basis(AnimationRotation(rot).Normalized()), Vector3.Zero);
            var translationTransform = Transform3D.Identity.Translated(pos);

            return translationTransform * rotationTransform * scaleTransform;
        }

        private class AnimationSimulation
        {
            private Vector4[] Positions;
            private Vector4[] Rotations;
            private Vector4[] Scales;
            private int[] Parent;
            private int[] Siblings;
            private int[] Children;
            private Transform3D[] Transforms;
            private int BoneCount;
            private int IKCount;
            private int TotalCount;
            private Motion.InterpolatedMotion MotionFile;
            private float Time;
            private bool IgnoreScale;
            
            private Transform3D GetGlobalTransform(int index)
            {
                if (index < 0) return Transform3D.Identity;
                var currentTransform = Transforms[index];
                var parent = Parent[index];
                if (parent < 0) return currentTransform;
                return GetGlobalTransform(parent) * currentTransform;
            }
            private void SetGlobalTransform(int index, Transform3D value)
            {
                var parent = Parent[index];
                if (parent < 0) Transforms[index] = value;
                var parentTransform = GetGlobalTransform(parent);
                var newTransform = parentTransform.AffineInverse() * value;
                Transforms[index] = newTransform;
            }
            private void SetBoneValue(Motion.Channel channel, int index, float value)
            {
                Vector4[] array;
                if (channel.IsPositionChannel()) array = Positions;
                else if (channel.IsRotationChannel()) array = Rotations;
                else array = Scales;
                
                if (channel.IsXChannel()) array[index].X = value;
                else if (channel.IsYChannel()) array[index].Y = value;
                else array[index].Z = value;
            }
            private void ExpressionSetBoneValue(Motion.Channel channel, int index, float value)
            {
                var val = value;
                Vector4[] array;
                if (channel.IsPositionChannel()) array = Positions;
                else if (channel.IsRotationChannel())
                {
                    array = Rotations;
                    val = Mathf.DegToRad(val);
                }
                else array = Scales;
                
                if (channel.IsXChannel()) array[index].X = val;
                else if (channel.IsYChannel()) array[index].Y = val;
                else array[index].Z = val;
            }
            private void CalculateMatrices()
            {
                for (var i = 0; i < TotalCount; i++) CalculateMatrix(i);
            }
            private void CalculateMatrix(int i) => Transforms[i] = CreateTransform(Positions[i].ToGodot().XYZ(), Rotations[i].ToGodot().XYZ(), Scales[i].ToGodot().XYZ());

            public AnimationSimulation(Motion.InterpolatedMotion motion, KH2Skeleton3D skeleton, float time)
            {
                MotionFile = motion;
                Time = time;

                IgnoreScale = motion.MotionHeader.SubType == 1;
                
                var skeletonBoneCount = skeleton.GetBoneCount();

                var totalCount = motion.Joints.Count;

                BoneCount = motion.InterpolatedMotionHeader.BoneCount;
                TotalCount = motion.InterpolatedMotionHeader.TotalBoneCount;
                IKCount = TotalCount - BoneCount;
                
                Positions = Enumerable.Repeat(Vector4.Zero, totalCount).ToArray();
                Rotations = Enumerable.Repeat(Vector4.Zero, totalCount).ToArray();
                Scales = Enumerable.Repeat(Vector4.One, totalCount).ToArray();
                Parent = Enumerable.Repeat(-1, totalCount).ToArray();
                Siblings = Enumerable.Repeat(-1, totalCount).ToArray();
                Children = Enumerable.Repeat(-1, totalCount).ToArray();
                Transforms = Enumerable.Repeat(Transform3D.Identity, totalCount).ToArray();

                var kh2Skeleton = skeleton.BoneList.ToList();
                
                for (var i = 0; i < motion.InterpolatedMotionHeader.BoneCount; i++)
                {
                    if (i >= skeletonBoneCount) break;
                    var khBone = kh2Skeleton[i];
                    
                    Positions[i] = khBone.Position;
                    Rotations[i] = khBone.Rotation;
                    Scales[i] = khBone.Scale;
                    Parent[i] = khBone.Parent;
                    Children[i] = khBone.Child;
                    Siblings[i] = khBone.Sibling;
                }

                foreach (var ikHelper in motion.IKHelpers)
                {
                    var index = ikHelper.Index; //TODO
                    Positions[index] = new Vector4(ikHelper.TranslateX, ikHelper.TranslateY, ikHelper.TranslateZ, ikHelper.TranslateW);
                    Rotations[index] = new Vector4(ikHelper.RotateX, ikHelper.RotateY, ikHelper.RotateZ, ikHelper.RotateW);
                    Scales[index] = new Vector4(ikHelper.ScaleX, ikHelper.ScaleY, ikHelper.ScaleZ, ikHelper.ScaleW);
                    Parent[index] = ikHelper.ParentId;
                    Children[index] = ikHelper.ChildId;
                    Siblings[index] = ikHelper.SiblingId;
                }
                
                //apply rest values
                foreach (var rest in motion.InitialPoses) SetBoneValue(rest.ChannelValue, rest.BoneId, rest.Value);
                
                //apply fcurves
                foreach (var fCurve in motion.FCurvesForward) HandleCurve(fCurve);
                foreach (var fCurve in motion.FCurvesInverse) HandleCurve(fCurve, true);
                
                CalculateMatrices();
                
                var currentExpression = 0;
                var currentConstraint = 0;
                foreach (var joint in motion.Joints)
                {
                    var i = joint.JointId;
                    while (true)
                    {
                        if (currentExpression >= motion.Expressions.Count) break;
                        var expression = motion.Expressions[currentExpression];
                        if (expression.TargetId != i) break;
                        ExpressionSetBoneValue((Motion.Channel)expression.TargetChannel, i, Expression(motion.ExpressionNodes, expression.NodeId));
                        CalculateMatrix(i);
                        currentExpression++;
                    }
                    while (true)
                    {
                        if (currentConstraint >= motion.Constraints.Count) break;
                        var constraint = motion.Constraints[currentConstraint];
                        if (constraint.ConstrainedJointId != i) break;
                        CalculateConstraint(constraint);
                        //CalculateMatrix(i);
                        currentConstraint++;
                    }
                }

                /*
                foreach (var expression in motion.Expressions)
                {
                    ExpressionSetBoneValue((Motion.Channel)expression.TargetChannel, expression.TargetId, Expression(motion.ExpressionNodes, expression.NodeId));
                    CalculateMatrix(expression.TargetId);
                }
                foreach (var constraint in motion.Constraints) CalculateConstraint(constraint);
                */

                //apply to skeleton
                for (var i = 0; i < TotalCount; i++)
                {
                    DebugDraw3D.DrawPosition(GetGlobalTransform(i).Scaled(Vector3.One * ImportHelpers.KH2PositionScale), i < motion.InterpolatedMotionHeader.BoneCount ? Colors.Aqua : Colors.DarkRed);

                    if (i >= BoneCount) continue;
                    
                    var transform = Transforms[i];
                    transform = transform with
                    {
                        Origin = transform.Origin * ImportHelpers.KH2PositionScale,
                    };
                    skeleton.SetBonePose(i, transform);
                }

                /*
                var collisionBones = new List<int>();

                if (skeleton.ModelCollisions is not null)
                {
                    foreach (var collision in skeleton.ModelCollisions.Value.EntryList.Where(collision => (ObjectCollision.TypeEnum)collision.Type == ObjectCollision.TypeEnum.IK))
                    {
                        //TODO: actual collision
                        var centerBone = collision.Bone;
                        var firstBone = Parent[centerBone];
                        var tipBone = Children[centerBone];
                    
                        collisionBones.AddRange([centerBone, firstBone, tipBone]);

                        CalculateIK(skeleton, firstBone, centerBone, tipBone, false);
                    }
                }
                
                for (var i = 0; i < motion.Joints.Count - 2; i++)
                {
                    var a = motion.Joints[i]; //first
                    var b = motion.Joints[i + 1]; //center
                    var c = motion.Joints[i + 2]; //tip
                    
                    var firstBone = a.JointId;
                    var centerBone = b.JointId;
                    var tipBone = c.JointId;
                    
                    if ((a.CalcMatrix2Rot && b.Trans && c.Calculated) && 
                        (firstBone < centerBone && centerBone < tipBone) && 
                        (!collisionBones.Contains(firstBone) && !collisionBones.Contains(centerBone) && !collisionBones.Contains(tipBone)))
                    {
                        CalculateIK(skeleton, firstBone, centerBone, tipBone, true);
                    }
                }
                */
            }
            private void CalculateConstraint(Motion.Constraint constraint)
            {
                var motion = MotionFile;
                var source = constraint.SourceJointId;
                var target = constraint.ConstrainedJointId;

                var limiter = constraint.LimiterId == -1 ? null : motion.Limiters[constraint.LimiterId];
                    
                if (constraint.ActivationCount > 0)
                {
                    var activators = motion.ConstraintActivations.Skip(constraint.ActivationStartId - 1).Take(constraint.ActivationCount);
                    var current = activators.Where(i => i.Time <= Time).MaxBy(i => i.Time);
                    var currentActive = current is not null && current.Active > 0;
                    if (!currentActive) return;
                }

                var type = constraint.Type;
                // 0 	Position                Set global position to another bone
                // 1 	Path                    Follow a path TODO: uhhhhh?
                // 2 	Orientation             Set global rotation to another bone
                // 3 	Direction               Point an object at another object, using a specified axis. TODO how do we know what axis, it's default is X
                // 4 	Up Vector               Point an object at another object, using the Y axis.
                // 5 	Two Points              Set global position between two other bones
                // 6 	Scale                   Set global scale to another bone
                // 7 	Camera                  Custom, no idea, probably put the bone directly on the camera?
                // 8 	Camera Path             Custom, no idea
                // 9 	Int Path                Custom, no idea
                // 10 	Int                     Custom, no idea
                // 11 	Camera Up Vector        Custom, no idea
                // 12 	Position Limit
                // 13 	Rotation Limit
                
                switch (type)
                {
                    case 0:
                    {
                        SetGlobalTransform(target, GetGlobalTransform(target) with { Origin = GetGlobalTransform(source).Origin });
                        break;
                    }
                    case 2:
                    {
                        var targetRot = GetGlobalTransform(source).Basis.GetRotationQuaternion();
                        
                        var current = GetGlobalTransform(target);
                        SetGlobalTransform(target, current with { Basis = new Basis(targetRot).Scaled(current.Basis.Scale) });
                        break;
                    }
                    case 3:
                    case 4:
                    {
                        var sourceTransform = GetGlobalTransform(source);
                        var currentParentTransform = GetGlobalTransform(Parent[target]);
                        var current = GetGlobalTransform(target);
                        var up = (currentParentTransform.Basis * Vector3.Up).Normalized();
                            
                        if (sourceTransform.Origin.Normalized().Abs().IsEqualApprox(up))
                        {
                            //TODO: point upward or downward
                            GD.Print("IMPLEMENT: point upward");
                        }
                        else
                        {
                            var offset = type == 3 ? Quaternion.FromEuler(new Vector3(0, Mathf.Pi / 2, 0)) : Quaternion.FromEuler(new Vector3(Mathf.Pi / 2, 0, 0));
                                
                            var newTransform = current.LookingAt(sourceTransform.Origin, up) * new Transform3D(new Basis(offset), Vector3.Zero); //TODO: is this correct?
                            SetGlobalTransform(target, newTransform);
                        }
                            
                        break;
                    }
                    case 6:
                    {
                        var targetScale = GetGlobalTransform(source).Basis.Scale;

                        var current = GetGlobalTransform(target);
                        SetGlobalTransform(target, current with { Basis = new Basis(current.Basis.GetRotationQuaternion()).Scaled(targetScale) });
                        break;
                    }
                    case 12:
                    {
                        if (limiter is null) return;
                            
                        var sourceTransform = GetGlobalTransform(source);
                        var targetTransform = GetGlobalTransform(target);

                        var targetRelative = sourceTransform.AffineInverse() * targetTransform;
                            
                        //GD.Print($"position type {(int)limiter.Type} min {limiter.MinX} {limiter.MinY} {limiter.MinZ} {limiter.MinW} max {limiter.MaxX} {limiter.MaxY} {limiter.MaxZ} {limiter.MaxW}");

                        //if (limiter.Type == Motion.LimiterType.Box)
                        if (true)
                        {
                            var pos = targetRelative.Origin.Max(new Vector3(limiter.MinX, limiter.MinY, limiter.MinZ)).Min(new Vector3(limiter.MaxX, limiter.MaxY, limiter.MaxZ));
                            var newRelative = targetRelative with { Origin = pos };
                            var rel = sourceTransform * newRelative;
                            //SetGlobalTransform(target, rel);
                        }
                        else //sphere
                        {
                            var length = targetRelative.Origin.Length();
                                
                        }
                        break;
                    }
                    case 13:
                    {
                        if (limiter is null) return;
                            
                        var transform = Transforms[target];
                        var rotation = transform.Basis.GetRotationQuaternion().GetEuler(EulerOrder.Zyx);
                        rotation = rotation.Clamp(new Vector3(limiter.MinX, limiter.MinY, limiter.MinZ), new Vector3(limiter.MaxX, limiter.MaxY, limiter.MaxZ));
                            
                        //GD.Print($"rotation type {(int)limiter.Type} min {limiter.MinX} {limiter.MinY} {limiter.MinZ} {limiter.MinW} max {limiter.MaxX} {limiter.MaxY} {limiter.MaxZ} {limiter.MaxW}");

                        Transforms[target] = CreateTransform(transform.Origin, rotation, transform.Basis.Scale);
                            
                        //TODO: ehhhh?
                            
                        break;
                    }
                    default:
                    {
                        GD.Print($"not implemented constraint: {type}");
                        break;
                    }
                }
            }
            private void CalculateIK(Skeleton3D skeleton, int first, int center, int tip, bool flip)
            {
                {
                    var sourceTransform = GetGlobalTransform(tip);
                    var currentParentTransform = GetGlobalTransform(Parent[first]);
                    var current = GetGlobalTransform(first);
                    var up = (currentParentTransform.Basis * Vector3.Up).Normalized();
                            
                    if (sourceTransform.Origin.Normalized().Abs().IsEqualApprox(up))
                    {
                    }
                    else
                    {
                        var offset = Quaternion.FromEuler(new Vector3(Mathf.Pi / 2, 0, 0));
                                
                        var newTransform = current.LookingAt(sourceTransform.Origin, up) * new Transform3D(new Basis(offset), Vector3.Zero); //TODO: is this correct?
                        SetGlobalTransform(first, newTransform);
                    }
                }
                
                var targetTransform = skeleton.GetBoneGlobalPose(tip);
                skeleton.SetBonePoseRotation(center, Quaternion.Identity);
                skeleton.ResetBonePose(center);
                skeleton.ResetBonePose(tip);

                var firstGlobalPose = skeleton.GetBoneGlobalPose(first);
                var centerGlobalPose = skeleton.GetBoneGlobalPose(center);
                var tipGlobalPose = skeleton.GetBoneGlobalPose(tip);

                var firstToTarget = firstGlobalPose.Origin.DistanceTo(targetTransform.Origin);
                var firstBoneLength = firstGlobalPose.Origin.DistanceTo(centerGlobalPose.Origin);
                var secondBoneLength = centerGlobalPose.Origin.DistanceTo(tipGlobalPose.Origin);

                if (firstBoneLength + secondBoneLength < firstToTarget) return;
                
                var aAngle = Mathf.Acos(((firstBoneLength*firstBoneLength)+(secondBoneLength*secondBoneLength)-(firstToTarget*firstToTarget)) / (2 * firstBoneLength * secondBoneLength));
                var bAngle = Mathf.Acos(((firstToTarget*firstToTarget)+(secondBoneLength*secondBoneLength)-(firstBoneLength*firstBoneLength)) / (2 * firstToTarget * secondBoneLength));
                var cAngle = Mathf.Acos(((firstToTarget*firstToTarget)+(firstBoneLength*firstBoneLength)-(secondBoneLength*secondBoneLength)) / (2 * firstToTarget * firstBoneLength));

                var firstAngle = -cAngle;
                var centerAngle = Mathf.Pi - aAngle;
                //var transformAngle = bAngle;

                if (flip)
                {
                    centerAngle *= -1;
                    firstAngle *= -1;
                }
                
                skeleton.SetBonePoseRotation(center, Quaternion.FromEuler(new Vector3(0,0,centerAngle)));
                
                //skeleton.SetBoneGlobalPose(first, new Transform3D(new Basis(targetTransform.Basis.GetRotationQuaternion()).Scaled(firstGlobalPose.Basis.Scale), firstGlobalPose.Origin));
                
                skeleton.SetBonePoseRotation(first, skeleton.GetBonePoseRotation(first) * Quaternion.FromEuler(new Vector3(0,0,firstAngle)));
                skeleton.SetBoneGlobalPose(tip, targetTransform);

                //GD.Print($"{rotationDiff}, {rotationDiff2}, {rotationDiff3}, {rotationDiff4}");
            }
            private float GetCurveValue(Motion.FCurve fCurve, float time)
            {
                var keyCount = fCurve.KeyCount;
                var keys = MotionFile.FCurveKeys;
                
                for (var i = keyCount - 1; i >= 0; i--)
                {
                    var keyId = fCurve.KeyStartId + i;
                    
                    var key = keys[keyId];
                    var keyTime = MotionFile.KeyTimes[key.Time];
                    
                    if (keyTime > time) continue;
                    
                    var nextKey = i < keyCount - 1 ? keys[keyId + 1] : keys[fCurve.KeyStartId];
                    var nextKeyTime = MotionFile.KeyTimes[nextKey.Time];

                    var preType = fCurve.Pre;
                    var postType = fCurve.Post;
                    
                    var n = Mathf.Remap(time, keyTime, nextKeyTime, 0, 1);
                    
                    var value = key.Type switch
                    {
                        Motion.Interpolation.Nearest => MotionFile.KeyValues[key.ValueId],
                        Motion.Interpolation.Linear => Mathf.Lerp(MotionFile.KeyValues[key.ValueId], MotionFile.KeyValues[nextKey.ValueId], n),
                        Motion.Interpolation.Hermite or Motion.Interpolation.Hermite3 or Motion.Interpolation.Hermite4 => CubicHermite(n, MotionFile.KeyValues[key.ValueId],
                            MotionFile.KeyValues[nextKey.ValueId], MotionFile.KeyTangents[key.LeftTangentId], MotionFile.KeyTangents[key.RightTangentId]),
                        _ => 0,
                    };

                    return value;
                }
                return 0;
            }
            private void HandleCurve(Motion.FCurve fCurve, bool ik = false)
            {
                var bone = ik ? fCurve.JointId + BoneCount : fCurve.JointId;
                var channel = fCurve.ChannelValue;
                
                Vector4[] array;
                if (channel.IsPositionChannel()) array = Positions;
                else if (channel.IsRotationChannel()) array = Rotations;
                else array = Scales;

                var value = GetCurveValue(fCurve, Time);
                
                if (channel.IsXChannel()) array[bone].X = value;
                else if (channel.IsYChannel()) array[bone].Y = value;
                else array[bone].Z = value;
            }
            
            //GD.Print($"{(Motion.ExpressionType)node.Type}, {node.Element}, {node.IsGlobal}, {node.CAR}, {node.CDR}, {node.Value}");
            //GD.Print($"{boneCount}, {motion.InterpolatedMotionHeader.TotalBoneCount}");

            private float Expression(List<Motion.ExpressionNode> expressionNodes, int index)
            {
                if (index < 0 || index >= expressionNodes.Count) return 0;
                var node = expressionNodes[index];
                var nodeType = (Motion.ExpressionType)node.Type;
                
                switch (nodeType)
                {
                    case Motion.ExpressionType.FUNC_SIN: return Mathf.Sin(Mathf.DegToRad(Expression(expressionNodes, node.CAR)));
                    case Motion.ExpressionType.FUNC_COS: return Mathf.Cos(Mathf.DegToRad(Expression(expressionNodes, node.CAR)));
                    case Motion.ExpressionType.FUNC_TAN: return Mathf.Tan(Mathf.DegToRad(Expression(expressionNodes, node.CAR)));
                    case Motion.ExpressionType.FUNC_ASIN:
                    {
                        var val = Expression(expressionNodes, node.CAR);
                        return val switch
                        {
                            >= 1 => 90,
                            <= -1 => -90,
                            _ => Mathf.RadToDeg(Mathf.Asin(val)),
                        };
                    }
                    case Motion.ExpressionType.FUNC_ACOS:
                    {
                        var val = Expression(expressionNodes, node.CAR);
                        return val switch
                        {
                            >= 1 => 0,
                            <= -1 => 180,
                            _ => Mathf.RadToDeg(Mathf.Acos(val)),
                        };
                    }
                    case Motion.ExpressionType.FUNC_ATAN: return Mathf.RadToDeg(Mathf.Atan(Expression(expressionNodes, node.CAR)));
                    case Motion.ExpressionType.FUNC_LOG: return Mathf.Log(Expression(expressionNodes, node.CAR));
                    case Motion.ExpressionType.FUNC_EXP: return Mathf.Exp(Expression(expressionNodes, node.CAR));
                    case Motion.ExpressionType.FUNC_ABS: return Mathf.Abs(Expression(expressionNodes, node.CAR));
                    case Motion.ExpressionType.FUNC_POW: return Mathf.Pow(Expression(expressionNodes, node.CAR), Expression(expressionNodes, node.CDR));
                    case Motion.ExpressionType.FUNC_SQRT: return Mathf.Sqrt(Mathf.Abs(Expression(expressionNodes, node.CAR))); //according to debug symbols, we abs before doing sqrt
                    case Motion.ExpressionType.FUNC_MIN: return Mathf.Min(Expression(expressionNodes, node.CAR), Expression(expressionNodes, node.CDR));
                    case Motion.ExpressionType.FUNC_MAX: return Mathf.Max(Expression(expressionNodes, node.CAR), Expression(expressionNodes, node.CDR));
                    //case Motion.ExpressionType.FUNC_AV: return (Expression(expressionNodes, node.CAR) + Expression(expressionNodes, node.CDR)) * 0.5f;
                    case Motion.ExpressionType.FUNC_AV:
                    {
                        var a = node.CAR;
                        var b = node.CDR;
                        if (a >= 0 && (Motion.ExpressionType)expressionNodes[a].Type == Motion.ExpressionType.LIST && b < 0)
                        {
                            var list = ListExpression(expressionNodes, a);
                            if (list.Count == 0) return 0;
                            return list.Sum() / list.Count;
                        }
                        break;
                    }
                    case Motion.ExpressionType.FUNC_COND:
                    {
                        var a = node.CAR;
                        var b = node.CDR;
                        if (a >= 0 && (Motion.ExpressionType)expressionNodes[a].Type == Motion.ExpressionType.LIST && b < 0)
                        {
                            var list = ListExpression(expressionNodes, a);
                            if (list.Count == 3) return list[0] >= 0 ? list[1] : list[2];
                            GD.Print($"List count 3 expected, actual: {list.Count}");
                        }
                        break;
                    }
                    case Motion.ExpressionType.FUNC_AT_FRAME:
                    case Motion.ExpressionType.FUNC_AT_FRAME_ROT:
                    {
                        var time = Expression(expressionNodes, node.CAR);
                        time = (time * 60) / MotionFile.InterpolatedMotionHeader.FrameData.FramesPerSecond;
                        
                        var curve = (int)Expression(expressionNodes, node.CDR);
                        var value = GetCurveValue(curve < MotionFile.FCurvesForward.Count ? 
                            MotionFile.FCurvesForward[curve] : 
                            MotionFile.FCurvesInverse[curve - MotionFile.FCurvesForward.Count], 
                            time);

                        return nodeType == Motion.ExpressionType.FUNC_AT_FRAME_ROT ? Mathf.RadToDeg(value) : value;
                    }
                    case Motion.ExpressionType.FUNC_CTR_DIST:
                    {
                        var bone1 = node.CAR;
                        var bone2 = node.CDR;
                        if (bone1 >= TotalCount) bone1 = 0;
                        if (bone2 >= TotalCount) bone2 = 0;
                        
                        CalculateMatrices();

                        var trans1 = GetGlobalTransform(bone1);
                        var trans2 = GetGlobalTransform(bone2);

                        return trans1.Origin.DistanceTo(trans2.Origin);
                    }
                    case Motion.ExpressionType.FUNC_FMOD: return Expression(expressionNodes, node.CAR) % Expression(expressionNodes, node.CDR); //fmod returns the mod without casting
                    case Motion.ExpressionType.OP_PLUS: return Expression(expressionNodes, node.CAR) + Expression(expressionNodes, node.CDR);
                    case Motion.ExpressionType.OP_MINUS: return Expression(expressionNodes, node.CAR) - Expression(expressionNodes, node.CDR);
                    case Motion.ExpressionType.OP_MUL: return Expression(expressionNodes, node.CAR) * Expression(expressionNodes, node.CDR);
                    case Motion.ExpressionType.OP_DIV: return Expression(expressionNodes, node.CAR) / Expression(expressionNodes, node.CDR); //according to debug symbols, throw on divide by 0
                    case Motion.ExpressionType.OP_MOD: return Mathf.RoundToInt(Expression(expressionNodes, node.CAR)) % Mathf.RoundToInt(Expression(expressionNodes, node.CDR)); //mod rounds before modulo, in softimage this is described as a "cast to the nearest int", TODO is this a floor, round, or ceil?
                    case Motion.ExpressionType.OP_EQ: return Expression(expressionNodes, node.CAR) == Expression(expressionNodes, node.CDR) ? 1 : 0;
                    case Motion.ExpressionType.OP_GT: return Expression(expressionNodes, node.CAR) > Expression(expressionNodes, node.CDR) ? 1 : 0;
                    case Motion.ExpressionType.OP_GE: return Expression(expressionNodes, node.CAR) >= Expression(expressionNodes, node.CDR) ? 1 : 0;
                    case Motion.ExpressionType.OP_LT: return Expression(expressionNodes, node.CAR) < Expression(expressionNodes, node.CDR) ? 1 : 0;
                    case Motion.ExpressionType.OP_LE: return Expression(expressionNodes, node.CAR) <= Expression(expressionNodes, node.CDR) ? 1 : 0;
                    case Motion.ExpressionType.OP_AND: return Expression(expressionNodes, node.CAR) >= 1 && Expression(expressionNodes, node.CDR) >= 1 ? 1 : 0;
                    case Motion.ExpressionType.OP_OR: return Expression(expressionNodes, node.CAR) >= 1 || Expression(expressionNodes, node.CDR) >= 1 ? 1 : 0;
                    case Motion.ExpressionType.VARIABLE_FC: return Time;
                    case Motion.ExpressionType.CONSTANT_NUM: return node.Value;
                    case Motion.ExpressionType.FCURVE_ETRNX: return Positions[node.Element].X;
                    case Motion.ExpressionType.FCURVE_ETRNY: return Positions[node.Element].Y;
                    case Motion.ExpressionType.FCURVE_ETRNZ: return Positions[node.Element].Z;
                    case Motion.ExpressionType.FCURVE_ROTX: return Mathf.RadToDeg(Rotations[node.Element].X);
                    case Motion.ExpressionType.FCURVE_ROTY: return Mathf.RadToDeg(Rotations[node.Element].Y);
                    case Motion.ExpressionType.FCURVE_ROTZ: return Mathf.RadToDeg(Rotations[node.Element].Z);
                    case Motion.ExpressionType.FCURVE_SCALX: return Scales[node.Element].X;
                    case Motion.ExpressionType.FCURVE_SCALY: return Scales[node.Element].Y;
                    case Motion.ExpressionType.FCURVE_SCALZ: return Scales[node.Element].Z;
                    case Motion.ExpressionType.LIST:
                        //this is a special case, don't handle it here, if this case is met something went wrong
                        break;
                    case Motion.ExpressionType.ELEMENT_NAME: return 0; //this actually does literally nothing lmao
                    default:
                    {
                        GD.Print(
                            $"Unknown node: (Type: {(Motion.ExpressionType)node.Type}, Element Value: {node.Element}, Global: {node.IsGlobal}, First connection index: {node.CAR}, second connection index: {node.CDR}, float value: {node.Value})");
                        break;
                    }
                }
                return 0;
            }
            private List<float> ListExpression(List<Motion.ExpressionNode> expressionNodes, int index)
            {
                var node = expressionNodes[index];
                var a = node.CAR;
                var b = node.CDR;
                var list = new List<float>();
                if (a >= 0)
                {
                    var aNode = expressionNodes[a];
                    if ((Motion.ExpressionType)aNode.Type == Motion.ExpressionType.LIST) list.AddRange(ListExpression(expressionNodes, a));
                    else list.Add(Expression(expressionNodes, a));
                }
                if (b >= 0)
                {
                    var bNode = expressionNodes[b];
                    if ((Motion.ExpressionType)bNode.Type == Motion.ExpressionType.LIST) list.AddRange(ListExpression(expressionNodes, b));
                    else list.Add(Expression(expressionNodes, b));
                }
                return list;
            }
        }

        public static float Loop(Motion.InterpolatedMotion motion, float time)
        {
            var frameTime = time * motion.InterpolatedMotionHeader.FrameData.FramesPerSecond;
            var diff = 60 / motion.InterpolatedMotionHeader.FrameData.FramesPerSecond;
            
            var actualFrame = Mathf.Wrap(frameTime * diff, motion.InterpolatedMotionHeader.FrameData.FrameStart * diff, motion.InterpolatedMotionHeader.FrameData.FrameEnd * diff);

            return (actualFrame / diff) / motion.InterpolatedMotionHeader.FrameData.FramesPerSecond;
        }
        public static void ApplyInterpolatedMotion(Motion.InterpolatedMotion motion, KH2Skeleton3D skeleton, float time)
        {
            var frameTime = time * motion.InterpolatedMotionHeader.FrameData.FramesPerSecond;
            var diff = 60 / motion.InterpolatedMotionHeader.FrameData.FramesPerSecond;
            
            var actualFrame = Mathf.Wrap(frameTime * diff, motion.InterpolatedMotionHeader.FrameData.FrameStart * diff, motion.InterpolatedMotionHeader.FrameData.FrameEnd * diff);

            var sim = new AnimationSimulation(motion, skeleton, actualFrame);
        }
    }
}
