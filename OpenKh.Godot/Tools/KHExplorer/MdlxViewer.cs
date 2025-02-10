using Godot;
using OpenKh.Godot.Helpers;
using OpenKh.Godot.Nodes;

namespace OpenKh.Godot.Tools.KHExplorer
{
    public partial class MdlxViewer : PanelContainer
    {
        [Export] public SubViewport SubViewport;
        [Export] public Node3D ModelParent;
        [Export] public Camera3D Camera;
        [Export] public Control TexturePanel;
        [Export] public Control AnimatedTexturePanel;
        [Export] public KH2Mdlx Mdlx;
        [Export] public KH2Mdlx RemasteredMdlx;
        [Export] public Button RemasteredToggle;
        
        public static PackedInstance<MdlxViewer> Packed = new("res://Scenes/Tools/KHExplorer/MdlxViewer.tscn");

        public override void _Ready()
        {
            base._Ready();
            ModelParent.AddChild(Mdlx);
            var hasRemastered = RemasteredMdlx is not null;
            if (hasRemastered)
            {
                ModelParent.AddChild(RemasteredMdlx);
                RemasteredToggle.Toggled += RemasteredToggleOnToggled;
            }
            RemasteredToggle.Visible = hasRemastered;
            Mdlx.Visible = !hasRemastered;
            
            var textures = Mdlx.Skeleton.Mesh.Textures;
            var remasteredTextures = RemasteredMdlx?.Skeleton.Mesh.Textures;
            
            var texturesAnimated = Mdlx.Skeleton.Mesh.AnimatedTextures;
            var remasteredTexturesAnimated = RemasteredMdlx?.Skeleton.Mesh.AnimatedTextures;

            for (var i = 0; i < textures.Count; i++)
            {
                var texViewer = ImageViewer.Packed.Create();
                texViewer.Texture = textures[i];
                texViewer.RemasteredTexture = remasteredTextures?[i];
                TexturePanel.AddChild(texViewer);
                texViewer.Name = i.ToString();
            }
            for (var i = 0; i < texturesAnimated.Count; i++)
            {
                var texViewer = ImageViewer.Packed.Create();
                texViewer.Texture = texturesAnimated[i];
                texViewer.RemasteredTexture = remasteredTexturesAnimated?[i];
                TexturePanel.AddChild(texViewer);
                texViewer.Name = i.ToString();
            }
        }
        private void RemasteredToggleOnToggled(bool toggledon)
        {
            Mdlx.Visible = !toggledon;
            RemasteredMdlx.Visible = toggledon;
        }
    }
}
