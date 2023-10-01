//
// TextureFactory.cs
//
// Copyright (C) 2018 OpenTK
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//

using System;
using System.Linq;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Rendering.ObjectTK
{
    /// <summary>
    /// Provides methods for creating texture objects in ways not covered by constructors.
    /// </summary>
    public static class TextureFactory
    {
        /// <summary>
        /// Creates a new Texture2D instance using the given texture handle.<br/>
        /// The width, height and internal format are queried from OpenGL and passed to the instance.
        /// The number of mipmap levels can not be queried and must be specified, otherwise it is set to one.
        /// TODO: somehow find out the number of mipmap levels because otherwise <see cref="Texture.AssertLevel"/> does not work correctly.
        /// </summary>
        /// <param name="textureHandle">An active handle to a 2D texture.</param>
        /// <param name="levels">The number of mipmap levels.</param>
        /// <returns>A new Texture2D instance.</returns>
        public static Texture2D AquireTexture2D(int textureHandle, int levels = 1)
        {
            int width, height, internalFormat;
            GL.BindTexture(TextureTarget.Texture2D, textureHandle);
            GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureWidth, out width);
            GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureHeight, out height);
            GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureInternalFormat, out internalFormat);
            return new Texture2D(textureHandle, (SizedInternalFormat) internalFormat, width, height, levels);
        }

        /// <summary>
        /// Calculates the maximum number of mipmap levels allowed, based on the size of all dimensions given.
        /// </summary>
        /// <param name="dimensions">Specifies the size in all dimensions.</param>
        /// <returns>The maximum number of mipmap levels allowed. The last level would consist of 1 texel.</returns>
        public static int CalculateMaxMipmapLevels(params int[] dimensions)
        {
            return 1 + (int) Math.Floor(Math.Log(dimensions.Max()));
        }
    }
}