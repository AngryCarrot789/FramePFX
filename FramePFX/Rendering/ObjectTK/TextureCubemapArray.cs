//
// TextureCubemapArray.cs
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
    /// Represents a cubemap texture array.<br/>
    /// Images in this texture are all cube maps. It contains multiple sets of cube maps, all within one texture.
    /// The array length * 6 (number of cube faces) is part of the texture size.
    /// </summary>
    public sealed class TextureCubemapArray : LayeredTexture
    {
        public override TextureTarget TextureTarget { get { return TextureTarget.TextureCubeMapArray; } }

        /// <summary>
        /// The size of the texture.<br/>
        /// This represents both width and height of the texture, because cube maps have to be square.
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// The number of layers.
        /// </summary>
        public int Layers { get; private set; }

        /// <summary>
        /// Allocates immutable texture storage with the given parameters.
        /// </summary>
        /// <param name="internalFormat">The internal format to allocate.</param>
        /// <param name="size">The width and height of the cube map faces.</param>
        /// <param name="layers">The number of layers to allocate.</param>
        /// <param name="levels">The number of mipmap levels.</param>
        public TextureCubemapArray(SizedInternalFormat internalFormat, int size, int layers, int levels = 0) : base(internalFormat, GetLevels(levels, size))
        {
            this.Size = size;
            this.Layers = layers;
            GL.BindTexture(this.TextureTarget, this.Handle);
            // note: the depth parameter is the number of layer-faces hence the multiplication by six,
            // see https://www.opengl.org/wiki/Texture_Storage#Immutable_storage
            GL.TexStorage3D((TextureTarget3d) this.TextureTarget, this.Levels, internalFormat, this.Size, this.Size, 6 * this.Layers);
        }
    }
}