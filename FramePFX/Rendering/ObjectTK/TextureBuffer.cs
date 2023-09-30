//
// TextureBuffer.cs
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
    /// Represents a buffer texture.<br/>
    /// The image in this texture (only one image. No mipmapping) is 1-dimensional.
    /// The storage for this data comes from a Buffer Object.
    /// </summary>
    public sealed class TextureBuffer : Texture
    {
        public override TextureTarget TextureTarget { get { return TextureTarget.TextureBuffer; } }
        public override bool SupportsMipmaps { get { return false; } }

        /// <summary>
        /// Creates a buffer texture and uses the given internal format to access a bound buffer, if not specified otherwise.
        /// </summary>
        /// <param name="internalFormat"></param>
        public TextureBuffer(SizedInternalFormat internalFormat) : base(internalFormat, 1)
        {
        }

        /// <summary>
        /// Binds the given buffer to this texture.<br/>
        /// Applies the internal format specified in the constructor.
        /// </summary>
        /// <param name="buffer">The buffer to bind.</param>
        public void BindBufferToTexture(int buffer) {
            this.BindBufferToTexture(buffer, this.InternalFormat);
        }

        /// <summary>
        /// Binds the given buffer to this texture using the given internal format.
        /// </summary>
        /// <param name="buffer">The buffer to bind.</param>
        /// <param name="internalFormat">The internal format used when accessing the buffer.</param>
        public void BindBufferToTexture(int buffer, SizedInternalFormat internalFormat) {
            GL.BindTexture(TextureTarget.TextureBuffer, this.Handle);
            GL.TexBuffer(TextureBufferTarget.TextureBuffer, internalFormat, buffer);
        }
    }
}