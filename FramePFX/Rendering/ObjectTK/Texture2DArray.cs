//
// Texture2DArray.cs
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
    /// Represents a 2D texture array.<br/>
    /// Images in this texture all are 2-dimensional. However, it contains multiple sets of 2-dimensional images,
    /// all within one texture. The array length is part of the texture's size.
    /// </summary>
    public sealed class Texture2DArray : LayeredTexture
    {
        public override TextureTarget TextureTarget { get { return TextureTarget.Texture2DArray; } }

        /// <summary>
        /// The width of the texture.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// The height of the texture.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// The number of layers.
        /// </summary>
        public int Layers { get; private set; }

        /// <summary>
        /// Allocates immutable texture storage with the given parameters.<br/>
        /// A value of zero for the number of mipmap levels will default to the maximum number of levels possible for the given bitmaps width and height.
        /// </summary>
        /// <param name="internalFormat">The internal format to allocate.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="layers">The number of layers to allocate.</param>
        /// <param name="levels">The number of mipmap levels.</param>
        public Texture2DArray(SizedInternalFormat internalFormat, int width, int height, int layers, int levels = 0) : base(internalFormat, GetLevels(levels, width, height))
        {
            this.Width = width;
            this.Height = height;
            this.Layers = layers;
            GL.BindTexture(this.TextureTarget, this.Handle);
            GL.TexStorage3D((TextureTarget3d) this.TextureTarget, this.Levels, internalFormat, this.Width, this.Height, this.Layers);
        }
    }
}