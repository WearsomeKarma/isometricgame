﻿using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace isometricgame.GameEngine.Rendering
{
    public struct Texture2D
    {
        private int id;
        private Vector2 size;

        public Texture2D(Bitmap bitmap, bool pixelated = true)
        {
            BitmapData bmpd = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            size = new Vector2(bitmap.Width, bitmap.Height);

            id = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, id);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmpd.Scan0);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, pixelated ? (int)TextureMinFilter.Nearest : (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, pixelated ? (int)TextureMagFilter.Nearest : (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.ClampToEdge, pixelated ? (int)TextureMagFilter.Nearest : (int)TextureMagFilter.Linear);

            bitmap.UnlockBits(bmpd);
        }

        public int ID => id;
        public Vector2 Size => size;
        public int Width => (int)size.X;
        public int Height => (int)size.Y;

        public int Area => Width * Height;
    }
}
