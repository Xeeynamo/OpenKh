using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using OpenKh.Common;

//Godot's DDS importer + mipmap patch, converted to c#

namespace OpenKh.Godot.Conversion
{
    public static class DDSConverter
    {
        //private static uint PF_FOURCC(string s) => unchecked((uint)(s[3] << 24 | s[2] << 16 | s[1] << 8 | s[0]));
        //#define PF_FOURCC(s) ((uint32_t)(((s)[3] << 24U) | ((s)[2] << 16U) | ((s)[1] << 8U) | ((s)[0])))

        private enum DDSFourCC : uint
        {
            DDFCC_DXT1 = /*PF_FOURCC("DXT1")*/ 0x44585431,
            DDFCC_DXT2 = /*PF_FOURCC("DXT2")*/ 0x44585432,
            DDFCC_DXT3 = /*PF_FOURCC("DXT3")*/ 0x44585433,
            DDFCC_DXT4 = /*PF_FOURCC("DXT4")*/ 0x44585434,
            DDFCC_DXT5 = /*PF_FOURCC("DXT5")*/ 0x44585435,
            DDFCC_ATI1 = /*PF_FOURCC("ATI1")*/ 0x41544931,
            DDFCC_BC4U = /*PF_FOURCC("BC4U")*/ 0x42433455,
            DDFCC_ATI2 = /*PF_FOURCC("ATI2")*/ 0x41544932,
            DDFCC_BC5U = /*PF_FOURCC("BC5U")*/ 0x42433555,
            DDFCC_A2XY = /*PF_FOURCC("A2XY")*/ 0x41325859,
            DDFCC_DX10 = /*PF_FOURCC("DX10")*/ 0x44583130,
            DDFCC_R16F = 111,
            DDFCC_RG16F = 112,
            DDFCC_RGBA16F = 113,
            DDFCC_R32F = 114,
            DDFCC_RG32F = 115,
            DDFCC_RGBA32F = 116
        }

        private static uint DDS_MAGIC = 0x20534444;
        private static uint DDSD_PITCH = 0x00000008;
        private static uint DDSD_LINEARSIZE = 0x00080000;
        private static uint DDSD_MIPMAPCOUNT = 0x00020000;
        private static uint DDPF_ALPHAPIXELS = 0x00000001;
        private static uint DDPF_ALPHAONLY = 0x00000002;
        private static uint DDPF_FOURCC = 0x00000004;
        private static uint DDPF_RGB = 0x00000040;
        private static uint DDPF_RG_SNORM = 0x00080000;

        private enum DXGIFormat
        {
            DXGI_R32G32B32A32_FLOAT = 2,
            DXGI_R32G32B32_FLOAT = 6,
            DXGI_R16G16B16A16_FLOAT = 10,
            DXGI_R32G32_FLOAT = 16,
            DXGI_R10G10B10A2_UNORM = 24,
            DXGI_R8G8B8A8_UNORM = 28,
            DXGI_R8G8B8A8_UNORM_SRGB = 29,
            DXGI_R16G16_FLOAT = 34,
            DXGI_R32_FLOAT = 41,
            DXGI_R8G8_UNORM = 49,
            DXGI_R16_FLOAT = 54,
            DXGI_R8_UNORM = 61,
            DXGI_A8_UNORM = 65,
            DXGI_R9G9B9E5 = 67,
            DXGI_BC1_UNORM = 71,
            DXGI_BC1_UNORM_SRGB = 72,
            DXGI_BC2_UNORM = 74,
            DXGI_BC2_UNORM_SRGB = 75,
            DXGI_BC3_UNORM = 77,
            DXGI_BC3_UNORM_SRGB = 78,
            DXGI_BC4_UNORM = 80,
            DXGI_BC5_UNORM = 83,
            DXGI_B5G6R5_UNORM = 85,
            DXGI_B5G5R5A1_UNORM = 86,
            DXGI_B8G8R8A8_UNORM = 87,
            DXGI_BC6H_UF16 = 95,
            DXGI_BC6H_SF16 = 96,
            DXGI_BC7_UNORM = 98,
            DXGI_BC7_UNORM_SRGB = 99,
            DXGI_B4G4R4A4_UNORM = 115
        }

        private enum DDSFormat
        {
            DDS_DXT1,
            DDS_DXT3,
            DDS_DXT5,
            DDS_ATI1,
            DDS_ATI2,
            DDS_BC6U,
            DDS_BC6S,
            DDS_BC7,
            DDS_R16F,
            DDS_RG16F,
            DDS_RGBA16F,
            DDS_R32F,
            DDS_RG32F,
            DDS_RGB32F,
            DDS_RGBA32F,
            DDS_RGB9E5,
            DDS_RGB8,
            DDS_RGBA8,
            DDS_BGR8,
            DDS_BGRA8,
            DDS_BGR5A1,
            DDS_BGR565,
            DDS_B2GR3,
            DDS_B2GR3A8,
            DDS_BGR10A2,
            DDS_RGB10A2,
            DDS_BGRA4,
            DDS_LUMINANCE,
            DDS_LUMINANCE_ALPHA,
            DDS_LUMINANCE_ALPHA_4,
            DDS_MAX
        }

        private struct DDSFormatInfo
        {
            public string name = null;
            public bool compressed = false;
            public uint divisor = 0;
            public uint block_size = 0;
            public Image.Format format = Image.Format.BptcRgba;

            public DDSFormatInfo()
            {
            }
            public DDSFormatInfo(string n, bool c, uint d, uint b, Image.Format f)
            {
                name = n;
                compressed = c;
                divisor = d;
                block_size = b;
                format = f;
            }
        }

        private static readonly DDSFormatInfo[] dds_format_info =
        [
            new("DXT1/BC1", true, 4, 8, Image.Format.Dxt1),
            new("DXT2/DXT3/BC2", true, 4, 16, Image.Format.Dxt3),
            new("DXT4/DXT5/BC3", true, 4, 16, Image.Format.Dxt5),
            new("ATI1/BC4", true, 4, 8, Image.Format.RgtcR),
            new("ATI2/A2XY/BC5", true, 4, 16, Image.Format.RgtcRg),
            new("BC6UF", true, 4, 16, Image.Format.BptcRgbfu),
            new("BC6SF", true, 4, 16, Image.Format.BptcRgbf),
            new("BC7", true, 4, 16, Image.Format.BptcRgba),
            new("R16F", false, 1, 2, Image.Format.Rh),
            new("RG16F", false, 1, 4, Image.Format.Rgh),
            new("RGBA16F", false, 1, 8, Image.Format.Rgbah),
            new("R32F", false, 1, 4, Image.Format.Rf),
            new("RG32F", false, 1, 8, Image.Format.Rgf),
            new("RGB32F", false, 1, 12, Image.Format.Rgbf),
            new("RGBA32F", false, 1, 16, Image.Format.Rgbaf),
            new("RGB9E5", false, 1, 4, Image.Format.Rgbe9995),
            new("RGB8", false, 1, 3, Image.Format.Rgb8),
            new("RGBA8", false, 1, 4, Image.Format.Rgba8),
            new("BGR8", false, 1, 3, Image.Format.Rgb8),
            new("BGRA8", false, 1, 4, Image.Format.Rgba8),
            new("BGR5A1", false, 1, 2, Image.Format.Rgba8),
            new("BGR565", false, 1, 2, Image.Format.Rgb8),
            new("B2GR3", false, 1, 1, Image.Format.Rgb8),
            new("B2GR3A8", false, 1, 2, Image.Format.Rgba8),
            new("BGR10A2", false, 1, 4, Image.Format.Rgba8),
            new("RGB10A2", false, 1, 4, Image.Format.Rgba8),
            new("BGRA4", false, 1, 2, Image.Format.Rgba8),
            new("GRAYSCALE", false, 1, 1, Image.Format.L8),
            new("GRAYSCALE_ALPHA", false, 1, 2, Image.Format.La8),
            new("GRAYSCALE_ALPHA_4", false, 1, 1, Image.Format.La8),
        ];

        private static DDSFormat dxgi_to_dds_format(DXGIFormat p_dxgi_format) =>
            p_dxgi_format switch
            {
                DXGIFormat.DXGI_R32G32B32A32_FLOAT => DDSFormat.DDS_RGBA32F,
                DXGIFormat.DXGI_R32G32B32_FLOAT => DDSFormat.DDS_RGB32F,
                DXGIFormat.DXGI_R16G16B16A16_FLOAT => DDSFormat.DDS_RGBA16F,
                DXGIFormat.DXGI_R32G32_FLOAT => DDSFormat.DDS_RG32F,
                DXGIFormat.DXGI_R10G10B10A2_UNORM => DDSFormat.DDS_RGB10A2,
                DXGIFormat.DXGI_R8G8B8A8_UNORM or DXGIFormat.DXGI_R8G8B8A8_UNORM_SRGB => DDSFormat.DDS_RGBA8,
                DXGIFormat.DXGI_R16G16_FLOAT => DDSFormat.DDS_RG16F,
                DXGIFormat.DXGI_R32_FLOAT => DDSFormat.DDS_R32F,
                DXGIFormat.DXGI_R8_UNORM or DXGIFormat.DXGI_A8_UNORM => DDSFormat.DDS_LUMINANCE,
                DXGIFormat.DXGI_R16_FLOAT => DDSFormat.DDS_R16F,
                DXGIFormat.DXGI_R8G8_UNORM => DDSFormat.DDS_LUMINANCE_ALPHA,
                DXGIFormat.DXGI_R9G9B9E5 => DDSFormat.DDS_RGB9E5,
                DXGIFormat.DXGI_BC1_UNORM or DXGIFormat.DXGI_BC1_UNORM_SRGB => DDSFormat.DDS_DXT1,
                DXGIFormat.DXGI_BC2_UNORM or DXGIFormat.DXGI_BC2_UNORM_SRGB => DDSFormat.DDS_DXT3,
                DXGIFormat.DXGI_BC3_UNORM or DXGIFormat.DXGI_BC3_UNORM_SRGB => DDSFormat.DDS_DXT5,
                DXGIFormat.DXGI_BC4_UNORM => DDSFormat.DDS_ATI1,
                DXGIFormat.DXGI_BC5_UNORM => DDSFormat.DDS_ATI2,
                DXGIFormat.DXGI_B5G6R5_UNORM => DDSFormat.DDS_BGR565,
                DXGIFormat.DXGI_B5G5R5A1_UNORM => DDSFormat.DDS_BGR5A1,
                DXGIFormat.DXGI_B8G8R8A8_UNORM => DDSFormat.DDS_BGRA8,
                DXGIFormat.DXGI_BC6H_UF16 => DDSFormat.DDS_BC6U,
                DXGIFormat.DXGI_BC6H_SF16 => DDSFormat.DDS_BC6S,
                DXGIFormat.DXGI_BC7_UNORM or DXGIFormat.DXGI_BC7_UNORM_SRGB => DDSFormat.DDS_BC7,
                DXGIFormat.DXGI_B4G4R4A4_UNORM => DDSFormat.DDS_BGRA4,
                _ => throw new ArgumentOutOfRangeException(nameof(p_dxgi_format), p_dxgi_format, null)
            };

        public static Image GetImage(byte[] bytes)
        {
            var f = new MemoryStream(bytes);

            var magic = f.ReadUInt32();
            var hsize = f.ReadUInt32();
            var flags = f.ReadUInt32();
            var height = f.ReadUInt32();
            var width = f.ReadUInt32();
            var pitch = f.ReadUInt32();
            /* uint32_t depth = */
            f.ReadUInt32();
            var mipmaps = f.ReadUInt32();

            for (var i = 0; i < 11; i++) f.ReadUInt32();

            if (magic != DDS_MAGIC || hsize != 124) return null;

            /* uint32_t format_size = */
            f.ReadUInt32();
            var format_flags = f.ReadUInt32();
            var format_fourcc = f.ReadUInt32();
            var format_rgb_bits = f.ReadUInt32();
            var format_red_mask = f.ReadUInt32();
            var format_green_mask = f.ReadUInt32();
            var format_blue_mask = f.ReadUInt32();
            var format_alpha_mask = f.ReadUInt32();

            /* uint32_t caps_1 = */
            f.ReadUInt32();
            /* uint32_t caps_2 = */
            f.ReadUInt32();
            /* uint32_t caps_3 = */
            f.ReadUInt32();
            /* uint32_t caps_4 = */
            f.ReadUInt32();

            f.ReadUInt32();

            if (f.Position < 128) f.Seek(128, SeekOrigin.Begin);

            var dds_format = DDSFormat.DDS_MAX;

            if ((format_flags & DDPF_FOURCC) > 0)
            {
                switch ((DDSFourCC)format_fourcc)
                {
                    case DDSFourCC.DDFCC_DXT1:
                        dds_format = DDSFormat.DDS_DXT1;
                        break;
                    case DDSFourCC.DDFCC_DXT2:
                    case DDSFourCC.DDFCC_DXT3:
                        dds_format = DDSFormat.DDS_DXT3;
                        break;
                    case DDSFourCC.DDFCC_DXT4:
                    case DDSFourCC.DDFCC_DXT5:
                        dds_format = DDSFormat.DDS_DXT5;
                        break;
                    case DDSFourCC.DDFCC_ATI1:
                    case DDSFourCC.DDFCC_BC4U:
                        dds_format = DDSFormat.DDS_ATI1;
                        break;
                    case DDSFourCC.DDFCC_ATI2:
                    case DDSFourCC.DDFCC_BC5U:
                    case DDSFourCC.DDFCC_A2XY:
                        dds_format = DDSFormat.DDS_ATI2;
                        break;
                    case DDSFourCC.DDFCC_R16F:
                        dds_format = DDSFormat.DDS_R16F;
                        break;
                    case DDSFourCC.DDFCC_RG16F:
                        dds_format = DDSFormat.DDS_RG16F;
                        break;
                    case DDSFourCC.DDFCC_RGBA16F:
                        dds_format = DDSFormat.DDS_RGBA16F;
                        break;
                    case DDSFourCC.DDFCC_R32F:
                        dds_format = DDSFormat.DDS_R32F;
                        break;
                    case DDSFourCC.DDFCC_RG32F:
                        dds_format = DDSFormat.DDS_RG32F;
                        break;
                    case DDSFourCC.DDFCC_RGBA32F:
                        dds_format = DDSFormat.DDS_RGBA32F;
                        break;
                    case DDSFourCC.DDFCC_DX10:
                    {
                        var dxgi_format = f.ReadUInt32();
                        /* uint32_t dimension = */
                        f.ReadUInt32();
                        /* uint32_t misc_flags_1 = */
                        f.ReadUInt32();
                        /* uint32_t array_size = */
                        f.ReadUInt32();
                        /* uint32_t misc_flags_2 = */
                        f.ReadUInt32();
                        dds_format = dxgi_to_dds_format((DXGIFormat)dxgi_format);
                    }
                        break;
                    default: return null;
                }
            }
            else if ((format_flags & DDPF_RGB) > 0)
            {
                //thanks rider, i can't read this lol
                dds_format = (format_flags & DDPF_ALPHAPIXELS) switch
                {
                    > 0 => format_rgb_bits switch
                    {
                        32 when format_red_mask == 0xff0000 && format_green_mask == 0xff00 && format_blue_mask == 0xff && format_alpha_mask == 0xff000000 => DDSFormat.DDS_BGRA8,
                        32 when format_red_mask == 0xff && format_green_mask == 0xff00 && format_blue_mask == 0xff0000 && format_alpha_mask == 0xff000000 => DDSFormat.DDS_RGBA8,
                        16 when format_red_mask == 0x00007c00 && format_green_mask == 0x000003e0 && format_blue_mask == 0x0000001f && format_alpha_mask == 0x00008000 => DDSFormat.DDS_BGR5A1,
                        32 when format_red_mask == 0x3ff00000 && format_green_mask == 0xffc00 && format_blue_mask == 0x3ff && format_alpha_mask == 0xc0000000 => DDSFormat.DDS_BGR10A2,
                        32 when format_red_mask == 0x3ff && format_green_mask == 0xffc00 && format_blue_mask == 0x3ff00000 && format_alpha_mask == 0xc0000000 => DDSFormat.DDS_RGB10A2,
                        16 when format_red_mask == 0xf00 && format_green_mask == 0xf0 && format_blue_mask == 0xf && format_alpha_mask == 0xf000 => DDSFormat.DDS_BGRA4,
                        16 when format_red_mask == 0xe0 && format_green_mask == 0x1c && format_blue_mask == 0x3 && format_alpha_mask == 0xff00 => DDSFormat.DDS_B2GR3A8,
                        _ => dds_format,
                    },
                    _ => format_rgb_bits switch
                    {
                        24 when format_red_mask == 0xff0000 && format_green_mask == 0xff00 && format_blue_mask == 0xff => DDSFormat.DDS_BGR8,
                        24 when format_red_mask == 0xff && format_green_mask == 0xff00 && format_blue_mask == 0xff0000 => DDSFormat.DDS_RGB8,
                        16 when format_red_mask == 0x0000f800 && format_green_mask == 0x000007e0 && format_blue_mask == 0x0000001f => DDSFormat.DDS_BGR565,
                        8 when format_red_mask == 0xe0 && format_green_mask == 0x1c && format_blue_mask == 0x3 => DDSFormat.DDS_B2GR3,
                        _ => dds_format,
                    },
                };
            }
            else if ((format_flags & DDPF_ALPHAONLY) > 0 && format_rgb_bits == 8 && format_alpha_mask == 0xff) dds_format = DDSFormat.DDS_LUMINANCE;

            if (dds_format == DDSFormat.DDS_MAX)
            {
                if ((format_flags & DDPF_ALPHAPIXELS) > 0)
                    dds_format = format_rgb_bits switch
                    {
                        16 when format_red_mask == 0xff && format_alpha_mask == 0xff00 => DDSFormat.DDS_LUMINANCE_ALPHA,
                        8 when format_red_mask == 0xf && format_alpha_mask == 0xf0 => DDSFormat.DDS_LUMINANCE_ALPHA_4,
                        _ => dds_format
                    };
                else if (format_rgb_bits == 8 && format_red_mask == 0xff) dds_format = DDSFormat.DDS_LUMINANCE;
            }

            if (dds_format == DDSFormat.DDS_MAX) return null;

            if (!((flags & DDSD_MIPMAPCOUNT) > 0)) mipmaps = 1;

            List<byte> src_data;

            var info = dds_format_info[(int)dds_format];

            var w = width;
            var h = height;

            if (info.compressed)
            {
                var size = Math.Max(info.divisor, w) / info.divisor * Math.Max(info.divisor, h) / info.divisor * info.block_size;
                if ((flags & DDSD_LINEARSIZE) > 0)
                {
                    if (size != pitch) return null;
                }
                else if (pitch != 0) return null;
                //
                var last_mip_offset = 0u;
                //
                for (uint i = 1; i < mipmaps; i++)
                {
                    w = Math.Max(1u, w >> 1);
                    h = Math.Max(1u, h >> 1);

                    var bsize = Math.Max(info.divisor, w) / info.divisor * Math.Max(info.divisor, h) / info.divisor * info.block_size;
                    //
                    last_mip_offset = size;
                    //
                    size += bsize;
                }
                src_data = f.ReadBytes((int)size).ToList();
                
                //
                var last_mip_w = w;
                var last_mip_h = h;
                if (mipmaps >= 2u && (last_mip_w >= 2u || last_mip_h >= 2u)) {

                    var mip_gen_offset = size;
                    while ((w >= 2u) || (h >= 2u)) 
                    {
                        w = Math.Max(1u, w >> 1);
                        h = Math.Max(1u, h >> 1);

                        var bsize = Math.Max(info.divisor, w) / info.divisor * Math.Max(info.divisor, h) / info.divisor * info.block_size;
                        size += bsize;
                    }
                    
                    var toAdd = size - src_data.Count;
                    src_data.AddRange(Enumerable.Repeat((byte)0, (int)toAdd));
                    
                    w = last_mip_w;
                    h = last_mip_h;
                    while ((w >= 2u) || (h >= 2u)) 
                    {
                        w = Math.Max(1u, w >> 1);
                        h = Math.Max(1u, h >> 1);

                        var blocks_width = Math.Max(info.divisor, w) / info.divisor;
                        var blocks_height = Math.Max(info.divisor, h) / info.divisor;
                        for (var idx = 0u; idx < (blocks_width * blocks_height); idx++) 
                        {
                            foreach (var i in Enumerable.Range(0, (int)info.block_size)) src_data[i + (int)last_mip_offset] = src_data[i + (int)mip_gen_offset];
                            
                            mip_gen_offset += info.block_size;
                        }
                    }
                }
                //
            }
            else
            {
                var size = width * height * info.block_size;

                for (uint i = 1; i < mipmaps; i++)
                {
                    w = w + 1 >> 1;
                    h = h + 1 >> 1;
                    size += w * h * info.block_size;
                }

                switch (dds_format)
                {
                    case DDSFormat.DDS_BGR565:
                        size = size * 3 / 2;
                        break;

                    case DDSFormat.DDS_BGR5A1:
                    case DDSFormat.DDS_BGRA4:
                    case DDSFormat.DDS_B2GR3A8:
                    case DDSFormat.DDS_LUMINANCE_ALPHA_4:
                        size = size * 2;
                        break;

                    case DDSFormat.DDS_B2GR3:
                        size = size * 3;
                        break;
                    default:
                        break;
                }
                src_data = f.ReadBytes((int)size).ToList();

                switch (dds_format)
                {
                    case DDSFormat.DDS_BGR5A1:
                    {
                        var colcount = (int)(size / 4);

                        for (var i = colcount - 1; i >= 0; i--)
                        {
                            var src_ofs = i * 2;
                            var dst_ofs = i * 4;

                            var a = (byte)(src_data[src_ofs + 1] & 0x80);
                            var b = (byte)(src_data[src_ofs] & 0x1F);
                            var g = (byte)(src_data[src_ofs] >> 5 | (src_data[src_ofs + 1] & 0x3) << 3);
                            var r = (byte)(src_data[src_ofs + 1] >> 2 & 0x1F);

                            src_data[dst_ofs + 0] = (byte)(r << 3);
                            src_data[dst_ofs + 1] = (byte)(g << 3);
                            src_data[dst_ofs + 2] = (byte)(b << 3);
                            src_data[dst_ofs + 3] = (byte)(a > 0 ? 255 : 0);
                        }
                    } break;
                    case DDSFormat.DDS_BGR565:
                    {
                        var colcount = (int)(size / 3);

                        for (var i = colcount - 1; i >= 0; i--)
                        {
                            var src_ofs = i * 2;
                            var dst_ofs = i * 3;

                            var b = (byte)(src_data[src_ofs] & 0x1F);
                            var g = (byte)(src_data[src_ofs] >> 5 | (src_data[src_ofs + 1] & 0x7) << 3);
                            var r = (byte)(src_data[src_ofs + 1] >> 3);

                            src_data[dst_ofs + 0] = (byte)(r << 3);
                            src_data[dst_ofs + 1] = (byte)(g << 2);
                            src_data[dst_ofs + 2] = (byte)(b << 3);
                        }
                    } break;
                    case DDSFormat.DDS_BGRA4:
                    {
                        var colcount = (int)(size / 4);

                        for (var i = colcount - 1; i >= 0; i--)
                        {
                            var src_ofs = i * 2;
                            var dst_ofs = i * 4;

                            var b = (byte)(src_data[src_ofs] & 0x0F);
                            var g = (byte)(src_data[src_ofs] & 0xF0);
                            var r = (byte)(src_data[src_ofs + 1] & 0x0F);
                            var a = (byte)(src_data[src_ofs + 1] & 0xF0);

                            src_data[dst_ofs] = (byte)(r << 4 | r);
                            src_data[dst_ofs + 1] = (byte)(g | g >> 4);
                            src_data[dst_ofs + 2] = (byte)(b << 4 | b);
                            src_data[dst_ofs + 3] = (byte)(a | a >> 4);
                        }
                    } break;
                    case DDSFormat.DDS_B2GR3:
                    {
                        var colcount = (int)(size / 3);

                        for (var i = colcount - 1; i >= 0; i--)
                        {
                            var src_ofs = i;
                            var dst_ofs = i * 3;

                            var b = (byte)((src_data[src_ofs] & 0x3) << 6);
                            var g = (byte)((src_data[src_ofs] & 0x1C) << 3);
                            var r = (byte)(src_data[src_ofs] & 0xE0);

                            src_data[dst_ofs] = r;
                            src_data[dst_ofs + 1] = g;
                            src_data[dst_ofs + 2] = b;
                        }
                    } break;
                    case DDSFormat.DDS_B2GR3A8:
                    {
                        var colcount = (int)(size / 4);

                        for (var i = colcount - 1; i >= 0; i--)
                        {
                            var src_ofs = i * 2;
                            var dst_ofs = i * 4;

                            var b = (byte)((src_data[src_ofs] & 0x3) << 6);
                            var g = (byte)((src_data[src_ofs] & 0x1C) << 3);
                            var r = (byte)(src_data[src_ofs] & 0xE0);
                            var a = src_data[src_ofs + 1];

                            src_data[dst_ofs] = r;
                            src_data[dst_ofs + 1] = g;
                            src_data[dst_ofs + 2] = b;
                            src_data[dst_ofs + 3] = a;
                        }
                    } break;
                    case DDSFormat.DDS_RGB10A2:
                    {
                        // To RGBA8.
                        var colcount = (int)(size / 4);

                        for (var i = 0; i < colcount; i++)
                        {
                            var ofs = i * 4;

                            uint w32;

                            unchecked
                            {
                                w32 = src_data[ofs + 0] | (uint)src_data[ofs + 1] << 8 | (uint)src_data[ofs + 2] << 16 | (uint)src_data[ofs + 3] << 24;
                            }

                            // This method follows the 'standard' way of decoding 10-bit dds files,
                            // which means the ones created with DirectXTex will be loaded incorrectly.
                            var a = (byte)((w32 & 0xc0000000) >> 24);
                            var r = (byte)((w32 & 0x3ff) >> 2);
                            var g = (byte)((w32 & 0xffc00) >> 12);
                            var b = (byte)((w32 & 0x3ff00000) >> 22);

                            src_data[ofs + 0] = r;
                            src_data[ofs + 1] = g;
                            src_data[ofs + 2] = b;
                            src_data[ofs + 3] = (byte)(a == 0xc0 ? 255 : a); // 0xc0 should be opaque.
                        }
                    } break;
                    case DDSFormat.DDS_BGR10A2:
                    {
                        var colcount = (int)(size / 4);
                        for (var i = 0; i < colcount; i++)
                        {
                            var ofs = i * 4;

                            uint w32;

                            unchecked
                            {
                                w32 = src_data[ofs + 0] | (uint)src_data[ofs + 1] << 8 | (uint)src_data[ofs + 2] << 16 | (uint)src_data[ofs + 3] << 24;
                            }

                            var a = (byte)((w32 & 0xc0000000) >> 24);
                            var r = (byte)((w32 & 0x3ff00000) >> 22);
                            var g = (byte)((w32 & 0xffc00) >> 12);
                            var b = (byte)((w32 & 0x3ff) >> 2);

                            src_data[ofs + 0] = r;
                            src_data[ofs + 1] = g;
                            src_data[ofs + 2] = b;
                            src_data[ofs + 3] = (byte)(a == 0xc0 ? 255 : a);
                        }
                    } break;

                    case DDSFormat.DDS_BGRA8:
                    {
                        var colcount = (int)(size / 4);
                        for (var i = 0; i < colcount; i++) (src_data[i * 4 + 0], src_data[i * 4 + 2]) = (src_data[i * 4 + 2], src_data[i * 4 + 0]);
                        //SWAP(src_data[i * 4 + 0], src_data[i * 4 + 2]);
                    } break;
                    case DDSFormat.DDS_BGR8:
                    {
                        var colcount = (int)(size / 3);
                        for (var i = 0; i < colcount; i++) (src_data[i * 3 + 0], src_data[i * 3 + 2]) = (src_data[i * 3 + 2], src_data[i * 3 + 0]);
                        //SWAP(src_data[i * 3 + 0], src_data[i * 3 + 2]);
                    } break;

                    case DDSFormat.DDS_LUMINANCE_ALPHA_4:
                    {
                        var colcount = (int)(size / 2);
                        for (var i = colcount - 1; i >= 0; i--)
                        {
                            var src_ofs = i;
                            var dst_ofs = i * 2;

                            var l = (byte)(src_data[src_ofs] & 0x0F);
                            var a = (byte)(src_data[src_ofs] & 0xF0);

                            src_data[dst_ofs] = (byte)(l << 4 | l);
                            src_data[dst_ofs + 1] = (byte)(a | a >> 4);
                        }
                    } break;
                }
            }
            var image = Image.CreateFromData((int)width, (int)height, mipmaps > 1, info.format, src_data.ToArray());

            return image;
        }
    }
}
