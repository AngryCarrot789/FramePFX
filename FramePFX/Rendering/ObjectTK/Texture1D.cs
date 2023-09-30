//
// Texture1D.cs
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
    /// Represents a 1D texture.<br/>
    /// Images in this texture all are 1-dimensional. They have width, but no height or depth.
    /// </summary>
    public sealed class Texture1D : Texture
    {
        public override TextureTarget TextureTarget { get { return TextureTarget.Texture1D; } }

        /// <summary>
        /// The width of the texture.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Allocates immutable texture storage with the given parameters.
        /// </summary>
        /// <param name="internalFormat">The internal format to allocate.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="levels">The number of mipmap levels.</param>
        public Texture1D(SizedInternalFormat internalFormat, int width, int levels = 0) : base(internalFormat, GetLevels(levels, width))
        {
            this.Width = width;
            GL.BindTexture(this.TextureTarget, Handle);
            GL.TexStorage1D((TextureTarget1d)this.TextureTarget, Levels, internalFormat, this.Width);
        }
    }
}