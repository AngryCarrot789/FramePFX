//
// Texture1DArray.cs
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
    /// Represents a 1D texture array.<br/>
    /// Images in this texture all are 1-dimensional. However, it contains multiple sets of 1-dimensional images,
    /// all within one texture. The array length is part of the texture's size.
    /// </summary>
    public sealed class Texture1DArray : LayeredTexture
    {
        public override TextureTarget TextureTarget { get { return TextureTarget.Texture1DArray; } }

        /// <summary>
        /// The width of the texture.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// The number of layers.<br/>
        /// note: OpenGL seems to call the second coordinate on a 1D texture array the "height",
        /// which would make the whole thing almost exactly equal to a 2D texture with the exception that
        /// a 1D texture array can be bound to a framebuffer via glFramebufferTextureLayer().
        /// </summary>
        public int Layers { get; private set; }

        /// <summary>
        /// Allocates immutable texture storage with the given parameters.
        /// </summary>
        /// <param name="internalFormat">The internal format to allocate.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="layers">The number of layers to allocate.</param>
        /// <param name="levels">The number of mipmap levels.</param>
        public Texture1DArray(SizedInternalFormat internalFormat, int width, int layers, int levels = 0) : base(internalFormat, GetLevels(levels, width))
        {
            this.Width = width;
            this.Layers = layers;
            GL.BindTexture(this.TextureTarget, this.Handle);
            GL.TexStorage2D((TextureTarget2d) this.TextureTarget, this.Levels, internalFormat, this.Width, this.Layers);
        }
    }
}