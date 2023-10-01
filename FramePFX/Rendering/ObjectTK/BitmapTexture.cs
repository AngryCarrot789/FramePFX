//
// BitmapTexture.cs
//
// Copyright (C) 2018 OpenTK
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//

using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace FramePFX.Rendering.ObjectTK
{
    /// <summary>
    /// Contains extension methods for texture types.
    /// </summary>
    public static class BitmapTexture
    {
        private static void CheckError()
        {
            GL.Finish();
        }

        /// <summary>
        /// Retrieves the texture data.
        /// </summary>
        public static T[,] GetContent<T>(this Texture2D texture, PixelFormat pixelFormat, PixelType pixelType, int level = 0) where T : struct
        {
            var data = new T[texture.Width, texture.Height];
            texture.Bind();
            GL.GetTexImage(texture.TextureTarget, level, pixelFormat, pixelType, data);
            return data;
        }
    }
}