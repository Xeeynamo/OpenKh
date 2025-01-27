using NLog;
using OpenKh.Command.AnbMaker.Utils.Builder.Models;
using SharpGLTF.Schema2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.AnbMaker.Utils.GltfAnimSource
{
    public class UseGltf
    {
        public IEnumerable<BasicSourceMotion> Load(
            string inputModel,
            string? meshName,
            string? rootName,
            string? animationName,
            float nodeScaling)
        {
            var logger = LogManager.GetCurrentClassLogger();

            logger.Warn("Gltf model importer is still in experimental stage.");

            var glb = ModelRoot.Load(inputModel);

            if (glb.LogicalNodes.FirstOrDefault(it => it.Name == rootName) is Node glbRootNode && glbRootNode is Node glbArmatureRoot)
            {
                // found
            }
            else
            {
                throw new Exception($"LogicalNode `{rootName}` not found. Choose one of {string.Join(", ", glb.LogicalNodes.Select(one => $"`{one.Name}`"))}.");
            }

            if (glb.LogicalAnimations.FirstOrDefault(it => it.Name == animationName) is Animation glbAnimation)
            {
                // found
            }
            else
            {
                throw new Exception($"LogicalAnimation `{animationName}` not found. Choose one of {string.Join(", ", glb.LogicalAnimations.Select(one => $"`{one.Name}`"))}.");
            }

            if (glb.LogicalMeshes.FirstOrDefault(it => it.Name == meshName) is Mesh glbMesh)
            {
                // found
            }
            else
            {
                throw new Exception($"LogicalMesh `{meshName}` not found. Choose one of {string.Join(", ", glb.LogicalMeshes.Select(one => $"`{one.Name}`"))}.");
            }

            yield return new BasicSourceMotion
            {
                BoneCount = glbRootNode.VisualChildren.Count(),
                DurationInTicks = (int)(30 * glbAnimation.Duration),
                TicksPerSecond = 30,
                //TODO
            };
        }
    }
}
