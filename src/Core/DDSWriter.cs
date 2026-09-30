using System;
using System.IO;

namespace KimeraCS
{
    //
    // Writes uncompressed 32-bit DDS textures (legacy header, A8R8G8B8 = BGRA bytes) with a
    // box-filtered mipmap chain. FFNx only loads textures for gltf models as .dds.
    //
    public static class DDSWriter
    {
        private const uint DDSD_CAPS = 0x1, DDSD_HEIGHT = 0x2, DDSD_WIDTH = 0x4, DDSD_PITCH = 0x8,
                           DDSD_PIXELFORMAT = 0x1000, DDSD_MIPMAPCOUNT = 0x20000;
        private const uint DDPF_ALPHAPIXELS = 0x1, DDPF_RGB = 0x40;
        private const uint DDSCAPS_COMPLEX = 0x8, DDSCAPS_TEXTURE = 0x1000, DDSCAPS_MIPMAP = 0x400000;

        // bgra: width * height * 4 bytes, rows top to bottom, B G R A order.
        // maxMipLevels: 0 = full chain down to 1x1; otherwise the total number of levels to write.
        public static void Write(string fileName, byte[] bgra, int width, int height, int maxMipLevels)
        {
            int fullLevels = 1;
            for (int w = width, h = height; w > 1 || h > 1; w = Math.Max(1, w / 2), h = Math.Max(1, h / 2))
                fullLevels++;
            int levels = maxMipLevels > 0 ? Math.Min(maxMipLevels, fullLevels) : fullLevels;

            using (BinaryWriter bw = new BinaryWriter(File.Create(fileName)))
            {
                bw.Write(0x20534444u);                  // "DDS "
                bw.Write(124u);                         // header size
                uint flags = DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | DDSD_PITCH | DDSD_PIXELFORMAT;
                if (levels > 1) flags |= DDSD_MIPMAPCOUNT;
                bw.Write(flags);
                bw.Write((uint)height);
                bw.Write((uint)width);
                bw.Write((uint)(width * 4));            // pitch
                bw.Write(0u);                           // depth
                bw.Write((uint)levels);
                for (int i = 0; i < 11; i++) bw.Write(0u);

                // pixel format
                bw.Write(32u);
                bw.Write(DDPF_RGB | DDPF_ALPHAPIXELS);
                bw.Write(0u);                           // fourCC
                bw.Write(32u);                          // bits per pixel
                bw.Write(0x00FF0000u);                  // R mask
                bw.Write(0x0000FF00u);                  // G mask
                bw.Write(0x000000FFu);                  // B mask
                bw.Write(0xFF000000u);                  // A mask

                uint caps = DDSCAPS_TEXTURE;
                if (levels > 1) caps |= DDSCAPS_COMPLEX | DDSCAPS_MIPMAP;
                bw.Write(caps);
                bw.Write(0u); bw.Write(0u); bw.Write(0u); bw.Write(0u);

                byte[] level = bgra;
                int lw = width, lh = height;
                for (int l = 0; l < levels; l++)
                {
                    bw.Write(level, 0, lw * lh * 4);
                    if (l + 1 < levels) level = Downsample(level, ref lw, ref lh);
                }
            }
        }

        // 2x2 box filter (clamped at odd edges).
        private static byte[] Downsample(byte[] src, ref int width, ref int height)
        {
            int nw = Math.Max(1, width / 2), nh = Math.Max(1, height / 2);
            byte[] dst = new byte[nw * nh * 4];

            for (int y = 0; y < nh; y++)
            {
                int y0 = Math.Min(y * 2, height - 1), y1 = Math.Min(y * 2 + 1, height - 1);
                for (int x = 0; x < nw; x++)
                {
                    int x0 = Math.Min(x * 2, width - 1), x1 = Math.Min(x * 2 + 1, width - 1);
                    for (int c = 0; c < 4; c++)
                    {
                        int sum = src[(y0 * width + x0) * 4 + c] + src[(y0 * width + x1) * 4 + c] +
                                  src[(y1 * width + x0) * 4 + c] + src[(y1 * width + x1) * 4 + c];
                        dst[(y * nw + x) * 4 + c] = (byte)((sum + 2) / 4);
                    }
                }
            }

            width = nw;
            height = nh;
            return dst;
        }
    }
}
