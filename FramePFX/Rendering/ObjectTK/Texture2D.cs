//
// Texture2D.cs
//
// Copyright (C) 2018 OpenTK
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//

using OpenTK.Graphics.OpenGL;

namespace FramePFX.Rendering.ObjectTK
{
    /// <summary>
    /// Represents a 2D texture.<br/>
    /// Images in this texture all are 2-dimensional. They have width and height, but no depth.
    /// </summary>
    public sealed class Texture2D : Texture
    {
        public override TextureTarget TextureTarget => TextureTarget.Texture2D;

        /// <summary>
        /// The width of the texture.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// The height of the texture.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Allocates immutable texture storage with the given parameters.<br/>
        /// A value of zero for the number of mipmap levels will default to the maximum number of levels possible for the given bitmaps width and height.
        /// </summary>
        /// <param name="internalFormat">The internal format to allocate.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="levels">The number of mipmap levels.</param>
        public Texture2D(SizedInternalFormat internalFormat, int width, int height, int levels = 0) : base(internalFormat, GetLevels(levels, width, height))
        {
            this.Width = width;
            this.Height = height;
            GL.BindTexture(this.TextureTarget, this.Handle);
            GL.TexStorage2D((TextureTarget2d) this.TextureTarget, this.Levels, internalFormat, this.Width, this.Height);
        }

        /// <summary>
        /// Internal constructor used by <see cref="TextureFactory"/> to wrap a Texture2D instance around an already existing texture.
        /// </summary>
        internal Texture2D(int textureHandle, SizedInternalFormat internalFormat, int width, int height, int levels) : base(textureHandle, internalFormat, levels)
        {
            this.Width = width;
            this.Height = height;
        }
    }
}