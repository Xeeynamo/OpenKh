#if TOOLS

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Godot.Collections;
using OpenKh.Godot.Helpers;
using OpenKh.Kh1;
using Array = Godot.Collections.Array;
using Vector2 = Godot.Vector2;
using Vector3 = Godot.Vector3;

namespace OpenKh.Godot.addons.OpenKHImporter;

public partial class MdlsImporter : EditorImportPlugin
{
    public enum Presets
    {
        Default,
    }
    public override string _GetImporterName() => "MdlsImporter";
    public override string _GetVisibleName() => "KH1 Model Importer";
    public override string[] _GetRecognizedExtensions() => ["mdls"];
    public override string _GetSaveExtension() => "scn";
    public override string _GetResourceType() => "PackedScene";
    public override float _GetPriority() => 1;
    public override int _GetImportOrder() => 0;
    public override int _GetPresetCount() => Enum.GetValues<Presets>().Length;
    public override string _GetPresetName(int presetIndex) => ((Presets)presetIndex).ToString();
    public override bool _GetOptionVisibility(string path, StringName optionName, Dictionary options) => true;
    public override Array<Dictionary> _GetImportOptions(string path, int presetIndex) => base._GetImportOptions(path, presetIndex);
    public override Error _Import(string sourceFile, string savePath, Dictionary options, Array<string> platformVariants, Array<string> genFiles)
    {
        var realPath = ProjectSettings.GlobalizePath(sourceFile);

        var root = new Node3D();
        root.Name = Path.GetFileNameWithoutExtension(realPath);
        
        var mdls = new Mdls(realPath);
        
        var arrayMesh = new ArrayMesh();
        var skeleton = new Skeleton3D();
        
        root.AddChild(skeleton);
        
        skeleton.Owner = root;
        skeleton.Name = "Skeleton";
        
        var joints = mdls.Joints;

        foreach (var joint in joints)
        {
            //TODO: for certain models, like sora, create name mappings
            skeleton.AddBone(joint.Index.ToString());
        }
        foreach (var joint in joints.Where(joint => joint.ParentId != 1023)) skeleton.SetBoneParent((int)joint.Index, (int)joint.ParentId); //1023 = no parent
        foreach (var joint in joints) skeleton.SetBoneRest((int)joint.Index, joint.Transform());
        
        skeleton.ResetBonePoses();

        //thanks rider
        var images = mdls.Images
            .Select(texture => Image.CreateFromData(texture.bitmap.Width, texture.bitmap.Height, false, Image.Format.Rgb8, texture.bitmap.GetRGBBuffer()))
            .Select(ImageTexture.CreateFromImage).ToList();

        for (var m = 0; m < mdls.Meshes.Count; m++)
        {
            var mesh = mdls.Meshes[m];
            
            var material = new StandardMaterial3D();
            material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;

            var tex = images[mesh.Header.TextureIndex];
            
            //var image = mdls.Images[mesh.Header.TextureIndex];
            //GD.Print($"{m}, {image.Info.Unk[0]}, {image.Info.Unk[1]}");
            
            material.AlbedoTexture = tex;

            var packet = mesh.packet;

            var positions = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var uv2s = new List<Vector2>();
            var bones = new List<int>();
            var weights = new List<float>();

            var array = new Array();
            array.Resize((int)Mesh.ArrayType.Max);

            foreach (var verts in packet.Faces)
            {
                foreach (var index in verts.Reverse())
                {
                    var vert = packet.Vertices.First(j => j.Index == index);

                    var transform = skeleton.GetBoneGlobalPose(vert.JointId);

                    var pos = vert.Position();
                    var normal = new Vector3(vert.NormalX, vert.NormalY, vert.NormalZ);

                    pos = transform * pos;
                    normal = transform.Basis.GetRotationQuaternion() * normal;

                    positions.Add(pos);
                    normals.Add(normal);
                    uvs.Add(new Vector2(vert.TexCoordU, vert.TexCoordV));
                    uv2s.Add(new Vector2(vert.TexCoord1, 0));
                    bones.AddRange(new[]
                    {
                        vert.JointId,
                        0,
                        0,
                        0,
                    });
                    weights.AddRange(new[]
                    {
                        vert.Weight,
                        0,
                        0,
                        0,
                    });
                }
            }

            array[(int)Mesh.ArrayType.Vertex] = positions.ToArray();
            array[(int)Mesh.ArrayType.Normal] = normals.ToArray();
            array[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
            array[(int)Mesh.ArrayType.TexUV2] = uv2s.ToArray();
            array[(int)Mesh.ArrayType.Bones] = bones.ToArray();
            array[(int)Mesh.ArrayType.Weights] = weights.ToArray();

            arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, array, flags:
                Mesh.ArrayFormat.FormatVertex | Mesh.ArrayFormat.FormatBones | Mesh.ArrayFormat.FormatNormal | Mesh.ArrayFormat.FormatTexUV | Mesh.ArrayFormat.FormatTexUV2 |
                Mesh.ArrayFormat.FormatWeights);
            arrayMesh.SurfaceSetMaterial(m, material);
        }

        var model = new MeshInstance3D();
        skeleton.AddChild(model);
        model.Name = "Model";
        model.Mesh = arrayMesh;
        model.Owner = root;
        skeleton.CreateSkinFromRestTransforms();

        var packed = new PackedScene();
        packed.Pack(root);
        ResourceSaver.Save(packed, $"{savePath}.{_GetSaveExtension()}");
        
        return Error.Ok;
    }
}

#endif
