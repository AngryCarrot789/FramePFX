//
// Texture3D.cs
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
    /// Represents a 3D texture.<br/>
    /// Images in this texture all are 3-dimensional. They have width, height, and depth.
    /// </summary>
    public sealed class Texture3D : LayeredTexture
    {
        public override TextureTarget TextureTarget { get { return TextureTarget.Texture3D; } }

        /// <summary>
        /// The width of the texture.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// The height of the texture.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// The depth of the texture.
        /// </summary>
        public int Depth { get; private set; }

        /// <summary>
        /// Allocates immutable texture storage with the given parameters.
        /// </summary>
        /// <param name="internalFormat">The internal format to allocate.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="depth">The depth of the texture.</param>
        /// <param name="levels">The number of mipmap levels.</param>
        public Texture3D(SizedInternalFormat internalFormat, int width, int height, int depth, int levels = 0) : base(internalFormat, GetLevels(levels, width, height, depth))
        {
            this.Width = width;
            this.Height = height;
            this.Depth = depth;
            GL.BindTexture(this.TextureTarget, this.Handle);
            GL.TexStorage3D((TextureTarget3d) this.TextureTarget, this.Levels, internalFormat, this.Width, this.Height, this.Depth);
        }
    }
}