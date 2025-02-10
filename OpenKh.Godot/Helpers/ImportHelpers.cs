using System;
using System.IO;
using System.Numerics;
using Godot;
using OpenKh.Kh1;
using OpenKh.Kh2;
using OpenKh.Kh2.Models;
using Quaternion = Godot.Quaternion;
using Vector3 = Godot.Vector3;

namespace OpenKh.Godot.Helpers;

public static class ImportHelpers
{
    //Common
    public static readonly string ExtractPath = Path.Combine(System.Environment.CurrentDirectory, "Extracted");
    public static readonly string ImportPath = Path.Combine(System.Environment.CurrentDirectory, "Imported");

    public static readonly string Kh1Path = Path.Combine(ExtractPath, "kh1");
    public static readonly string Kh1OriginalPath = Path.Combine(Kh1Path, "original");
    public static readonly string Kh1RemasteredPath = Path.Combine(Kh1Path, "remastered");

    public static readonly string Kh1ImportPath = Path.Combine(ImportPath, "kh1");
    public static readonly string Kh1ImportOriginalPath = Path.Combine(Kh1ImportPath, "original");
    public static readonly string Kh1ImportRemasteredPath = Path.Combine(Kh1ImportPath, "remastered");

    public static readonly string Kh2Path = Path.Combine(ExtractPath, "kh2");
    public static readonly string Kh2OriginalPath = Path.Combine(Kh2Path, "original");
    public static readonly string Kh2RemasteredPath = Path.Combine(Kh2Path, "remastered");

    public static readonly string Kh2ImportPath = Path.Combine(ImportPath, "kh2");
    public static readonly string Kh2ImportOriginalPath = Path.Combine(Kh2ImportPath, "original");
    public static readonly string Kh2ImportRemasteredPath = Path.Combine(Kh2ImportPath, "remastered");
    public static void SetOwner(this Node node)
    {
        foreach (var c in node.FindChildren("*", "", true, false)) c.Owner = node;
        //foreach (var c in node.GetChildren()) SetOwnerRecursive(c, node);
    }
    private static void SetOwnerRecursive(this Node node, Node owner)
    {
        node.Owner = owner;
        foreach (var c in node.GetChildren()) SetOwnerRecursive(c, owner);
    }

    public static Transform3D CreateTransform(Vector3 pos, Vector3 rot, Vector3 scale)
    {
        var scaleTransform = Transform3D.Identity.Scaled(scale);
        var rotationTransform = new Transform3D(new Basis(CommonRotation(rot.X, rot.Y, rot.Z)), Vector3.Zero);
        var translationTransform = Transform3D.Identity.Translated(pos);

        return translationTransform * rotationTransform * scaleTransform;
    }
    public static Quaternion CommonRotation(float x, float y, float z)
    {
        var rotationMatrix = RotationMatrix(x, y, z);
        Matrix4x4.Decompose(rotationMatrix, out _, out var rot, out _);
        return new Quaternion(rot.X, rot.Y, rot.Z, rot.W);
    }
    public static Matrix4x4 RotationMatrix(float x, float y, float z)
    {
        var rotationMatrixX = Matrix4x4.CreateRotationX(x);
        var rotationMatrixY = Matrix4x4.CreateRotationY(y);
        var rotationMatrixZ = Matrix4x4.CreateRotationZ(z);
        return rotationMatrixX * rotationMatrixY * rotationMatrixZ;
    }
    public static byte[] BGRAToRGBA(this byte[] data)
    {
        var result = new byte[data.Length];
        for (var i = 0; i < data.Length; i += 4)
        {
            result[i] = data[i + 2];
            result[i + 1] = data[i + 1];
            result[i + 2] = data[i];
            result[i + 3] = data[i + 3];
        }
        return result;
    }

    //KH1
    public const float KH1PositionScale = 1f / 200f;
    public static Vector3 FromKH1Position(float x, float y, float z) => new Vector3(x, y, z) * KH1PositionScale;
    public static Vector3 Position(this Mdls.MdlsJoint joint) => FromKH1Position(joint.TranslateX, joint.TranslateY, joint.TranslateZ);
    public static Vector3 Scale(this Mdls.MdlsJoint joint) => new(joint.ScaleX, joint.ScaleY, joint.ScaleZ);
    public static Transform3D Transform(this Mdls.MdlsJoint joint) => new(new Basis(joint.Rotation()).Scaled(joint.Scale()), joint.Position());
    public static Quaternion Rotation(this Mdls.MdlsJoint joint) => CommonRotation(joint.RotateX, joint.RotateY, joint.RotateZ);
    public static Vector3 Position(this Mdls.MdlsVertex vert) => FromKH1Position(vert.TranslateX, vert.TranslateY, vert.TranslateZ);

    //KH2

    public const float KH2PositionScale = 1f / 200f;
    public static Vector3 FromKH2Position(float x, float y, float z) => new Vector3(x, y, z) * KH2PositionScale;
    public static Vector3 Position(this ModelCommon.Bone bone) => FromKH2Position(bone.TranslationX, bone.TranslationY, bone.TranslationZ);

    public static Quaternion Rotation(this ModelCommon.Bone bone) => CommonRotation(bone.RotationX, bone.RotationY, bone.RotationZ);

    public static Vector3 Scale(this ModelCommon.Bone bone) => new(bone.ScaleX, bone.ScaleY, bone.ScaleZ);
    public static Transform3D Transform(this ModelCommon.Bone bone)
    {
        var scaleTransform = Transform3D.Identity.Scaled(bone.Scale());
        var rotationTransform = new Transform3D(new Basis(bone.Rotation()), Vector3.Zero);
        var translationTransform = Transform3D.Identity.Translated(bone.Position());

        return translationTransform * rotationTransform * scaleTransform;
    }
    public static void PrintEntries(this Bar bar)
    {
        foreach (var entry in bar)
        {
            GD.Print($"{entry.Index}, {entry.Name}, {entry.Type}");
        }
    }
    public static Color ConvertColor(this uint c)
    {
        var bytes = BitConverter.GetBytes(c);

        var red = bytes[0] / 128f;
        var green = bytes[1] / 128f;
        var blue = bytes[2] / 128f;
        var alpha = bytes[3] / 128f;

        return new Color(red, green, blue, alpha);
    }
}
