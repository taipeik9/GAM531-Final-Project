using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp.Processing;

namespace Collision3D
{
    static class Utils
    {
        public static int LoadTexture(string path)
        {
            int tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tex);

            using (Image<Rgba32> img = Image.Load<Rgba32>(path))
            {
                img.Mutate(x => x.Flip(FlipMode.Vertical)); // OpenGL origin

                var pixels = new byte[img.Width * img.Height * 4];
                img.CopyPixelDataTo(pixels);

                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.Rgba,
                    img.Width,
                    img.Height,
                    0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    pixels
                );
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            return tex;
        }
    }
}