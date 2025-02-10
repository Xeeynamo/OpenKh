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

namespace OpenKh.Godot.addons.OpenKHImporter;

public partial class CvblImporter : EditorImportPlugin
{
    public enum Presets
    {
        Default,
    }
    public override string _GetImporterName() => "CvblImporter";
    public override string _GetVisibleName() => "KH1 Remastered Model Importer";
    public override string[] _GetRecognizedExtensions() => ["cvbl"];
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
        return Error.Bug; //TODO: remove this once i actually start focusing on KH1 assets, for now im focused on KH2 assets and this reimporting every bootup causes a giant headache
        var realPath = ProjectSettings.GlobalizePath(sourceFile);

        var name = new DirectoryInfo(Path.GetDirectoryName(realPath)).Name;

        var root = new Node3D();
        root.Name = /*Path.GetFileNameWithoutExtension(realPath)*/ name;
        
        var relativePath = Path.GetRelativePath(ImportHelpers.Kh1ImportRemasteredPath, Path.GetFullPath(Path.Combine(Path.GetDirectoryName(realPath), "..")));
        
        var mdlsPath = Path.GetFullPath(Path.Combine(ImportHelpers.Kh1ImportOriginalPath, relativePath, name));
        
        //GD.Print(relativePath);
        
        Mdls mdls = null;

        if (File.Exists(mdlsPath)) mdls = new Mdls(mdlsPath);
        
        var joints = mdls?.Joints;

        var cvbl = new Cvbl(File.OpenRead(realPath), /*joints*/ null);
        
        var arrayMesh = new ArrayMesh();
        var skeleton = new Skeleton3D();
        
        root.AddChild(skeleton);
        
        skeleton.Owner = root;
        skeleton.Name = "Skeleton";

        if (mdls is not null)
        {
            foreach (var joint in joints)
            {
                //TODO: for certain models, like sora, create name mappings
                skeleton.AddBone(joint.Index.ToString());
            }
            foreach (var joint in joints.Where(joint => joint.ParentId != 1023)) skeleton.SetBoneParent((int)joint.Index, (int)joint.ParentId); //1023 = no parent
            foreach (var joint in joints) skeleton.SetBoneRest((int)joint.Index, joint.Transform());
        }
        skeleton.ResetBonePoses();
        
        for (var meshIndex = 0; meshIndex < cvbl.Submeshes.Count; meshIndex++)
        {
            var submesh = cvbl.Submeshes[meshIndex];
            var material = submesh.Material;

            var mat = new StandardMaterial3D();
            mat.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;

            if (cvbl.MeshEntries[meshIndex].Unk1 == 1)
            {
                mat.Transparency = BaseMaterial3D.TransparencyEnum.AlphaScissor;
                mat.AlphaScissorThreshold = 0.5f;
            }

            GD.Print($"{meshIndex}, {material}, {cvbl.MeshEntries[meshIndex].Unk1}, {cvbl.MeshEntries[meshIndex].Unk2}");

            var positions = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var bones = new List<int>();
            var weights = new List<float>();

            var array = new Array();
            array.Resize((int)Mesh.ArrayType.Max);

            foreach (var face in submesh.Faces)
            {
                foreach (var index in face.Reverse())
                {
                    var vert = submesh.Vertices[(int)index];

                    var p = vert.Position;
                    var pos = ImportHelpers.FromKH1Position(p.X, p.Y, p.Z);
                    var nor = vert.Normal;
                    var normal = new Vector3(nor.X, nor.Y, nor.Z);
                    var uv = vert.UV;

                    if (vert.Joints.Length == 1)
                    {
                        var transform = skeleton.GetBoneGlobalPose(vert.Joints.First());

                        pos = transform * pos;
                        //normal = transform.Basis.GetRotationQuaternion() * normal;
                    }
                    else
                    {
                        GD.Print("More than 1 weight");
                    }

                    var boneList = new List<int>();
                    var weightList = new List<float>();

                    if (mdls is not null)
                    {
                        var boneCount = vert.Joints.Length;
                    
                        switch (boneCount)
                        {
                            case < 4:
                            {
                                boneList.AddRange(vert.Joints.Select(b => (int)b));
                                weightList.AddRange(vert.Weights);
                                while (boneList.Count < 4)
                                {
                                    boneList.Add(0);
                                    weightList.Add(0);
                                }
                                break;
                            }
                            case > 4:
                            {
                                boneList.AddRange(vert.Joints.Take(4).Select(b => (int)b));
                                weightList.AddRange(vert.Weights.Take(4));
                                break;
                            }
                            default:
                            {
                                boneList.AddRange(vert.Joints.Select(b => (int)b));
                                weightList.AddRange(vert.Weights);
                                break;
                            }
                        }
                    }
                    else while (boneList.Count < 4)
                    {
                        boneList.Add(0);
                        weightList.Add(0);
                    }
                    
                    positions.Add(pos);
                    normals.Add(normal);
                    uvs.Add(new Vector2(uv.X, uv.Y));
                    bones.AddRange(boneList);
                    weights.AddRange(weightList);
                }
            }

            array[(int)Mesh.ArrayType.Vertex] = positions.ToArray();
            array[(int)Mesh.ArrayType.Normal] = normals.ToArray();
            array[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
            array[(int)Mesh.ArrayType.Bones] = bones.ToArray();
            array[(int)Mesh.ArrayType.Weights] = weights.ToArray();

            arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, array, flags:
                Mesh.ArrayFormat.FormatVertex | Mesh.ArrayFormat.FormatBones | Mesh.ArrayFormat.FormatNormal | Mesh.ArrayFormat.FormatTexUV | Mesh.ArrayFormat.FormatWeights);
            arrayMesh.SurfaceSetMaterial(meshIndex, mat);
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
