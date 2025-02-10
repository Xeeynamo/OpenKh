using Godot;
using OpenKh.Godot.Helpers;
using OpenKh.Imaging;
using OpenKh.Kh2;
using OpenKh.Kh2.Extensions;

namespace OpenKh.Godot.Conversion
{
    public static class TextureConverters
    {
        public static Texture2D FromImgd(Imgd imgd)
        {
            var data = imgd.GetData();
            
            switch (imgd.PixelFormat)
            {
                case PixelFormat.Indexed4:
                {
                    var img = ImageDataHelpers.FromIndexed4ToBitmap32(data, imgd.GetClut(), ImageDataHelpers.RGBA);
                    return ImageTexture.CreateFromImage(Image.CreateFromData(imgd.Size.Width, imgd.Size.Height, false, Image.Format.Rgba8, img.BGRAToRGBA()));
                }
                case PixelFormat.Indexed8:
                {
                    var img = ImageDataHelpers.FromIndexed8ToBitmap32(data, imgd.GetClut(), ImageDataHelpers.RGBA);
                    return ImageTexture.CreateFromImage(Image.CreateFromData(imgd.Size.Width, imgd.Size.Height, false, Image.Format.Rgba8, img.BGRAToRGBA()));
                }
                case PixelFormat.Rgba8888:
                {
                    return ImageTexture.CreateFromImage(Image.CreateFromData(imgd.Size.Width, imgd.Size.Height, false, Image.Format.Rgba8, data.BGRAToRGBA()));
                }
            }
            return null;
        }
        public static Texture2D FromTm2(Tm2 tm2) => FromImgd(tm2.AsImgd()); //TODO: theres a better way to do this but im lazy
    }
}
