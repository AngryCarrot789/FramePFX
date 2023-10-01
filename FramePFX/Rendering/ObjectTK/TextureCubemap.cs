//
// TextureCubemap.cs
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
    /// Represents a cubemap texture.<br/>
    /// There are exactly 6 distinct sets of 2D images, all of the same size. They act as 6 faces of a cube.
    /// </summary>
    public sealed class TextureCubemap : LayeredTexture
    {
        public override TextureTarget TextureTarget { get { return TextureTarget.TextureCubeMap; } }

        /// <summary>
        /// The size of the texture.<br/>
        /// This represents both width and height of the texture, because cube maps have to be square.
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Allocates immutable texture storage with the given parameters.
        /// </summary>
        /// <param name="internalFormat">The internal format to allocate.</param>
        /// <param name="size">The width and height of the cube map faces.</param>
        /// <param name="levels">The number of mipmap levels.</param>
        public TextureCubemap(SizedInternalFormat internalFormat, int size, int levels = 0) : base(internalFormat, GetLevels(levels, size))
        {
            this.Size = size;
            GL.BindTexture(this.TextureTarget, this.Handle);
            GL.TexStorage2D((TextureTarget2d) this.TextureTarget, this.Levels, internalFormat, this.Size, this.Size);
        }
    }
}