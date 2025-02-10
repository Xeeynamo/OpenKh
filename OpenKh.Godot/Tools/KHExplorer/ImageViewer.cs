using System;
using Godot;
using OpenKh.Godot.Helpers;

namespace OpenKh.Godot.Tools.KHExplorer
{
    public partial class ImageViewer : PanelContainer
    {
        [Export] public Texture2D Texture;
        [Export] public Texture2D RemasteredTexture;
        
        [Export] public CheckButton RemasteredToggle;
        [Export] public TextureRect Display;
        [Export] public Label MetadataLabel;

        public static PackedInstance<ImageViewer> Packed = new("res://Scenes/Tools/KHExplorer/ImageViewer.tscn");
        
        public override void _Ready()
        {
            base._Ready();

            if (RemasteredTexture is not null)
            {
                RemasteredToggle.Visible = true;
                RemasteredToggle.ProcessMode = ProcessModeEnum.Inherit;
                RemasteredToggle.ButtonPressed = true;
            }
            else
            {
                RemasteredToggle.Visible = false;
                RemasteredToggle.ProcessMode = ProcessModeEnum.Disabled;
                RemasteredToggle.ButtonPressed = false;
            }

            Display.Texture = RemasteredToggle.ButtonPressed ? RemasteredTexture : Texture;
            
            UpdateMetadataLabel();
            
            RemasteredToggle.Toggled += RemasteredToggleOnToggled;
        }
        private void RemasteredToggleOnToggled(bool toggledon)
        {
            Display.Texture = toggledon ? RemasteredTexture : Texture;
            UpdateMetadataLabel();
        }
        private void UpdateMetadataLabel()
        {
            var tex = Display.Texture;

            if (tex is null)
            {
                MetadataLabel.Text = Random.Shared.Next(32) == 0 ? "I made a boo boo yeah" : "Something went wrong";
                return;
            }
            var size = tex.GetSize();
            MetadataLabel.Text = $"Size: [{size.X}, {size.Y}]";
        }
    }
}
