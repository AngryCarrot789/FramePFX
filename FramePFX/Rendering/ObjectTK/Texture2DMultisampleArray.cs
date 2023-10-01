//
// Texture2DMultisampleArray.cs
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
    /// Represents a 2D multisample array texture.<br/>
    /// Combines 2D array and 2D multisample types. No mipmapping.
    /// </summary>
    public sealed class Texture2DMultisampleArray : LayeredTexture
    {
        public override TextureTarget TextureTarget { get { return TextureTarget.Texture2DMultisampleArray; } }
        public override bool SupportsMipmaps { get { return false; } }

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
        /// The number of samples per texel.
        /// </summary>
        public int Samples { get; private set; }

        /// <summary>
        /// Specifies whether the texels will use identical sample locations.
        /// </summary>
        public bool FixedSampleLocations { get; private set; }

        /// <summary>
        /// Allocates immutable texture storage with the given parameters.
        /// </summary>
        /// <param name="internalFormat">The internal format to allocate.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="layers">The number of layers to allocate.</param>
        /// <param name="samples">The number of samples per texel.</param>
        /// <param name="fixedSampleLocations">Specifies whether the texels will use identical sample locations.</param>
        public Texture2DMultisampleArray(SizedInternalFormat internalFormat, int width, int height, int layers, int samples, bool fixedSampleLocations) : base(internalFormat, 1)
        {
            this.Width = width;
            this.Height = height;
            this.Layers = layers;
            this.Samples = samples;
            this.FixedSampleLocations = fixedSampleLocations;
            GL.BindTexture(this.TextureTarget, this.Handle);
            GL.TexStorage3DMultisample((TextureTargetMultisample3d) this.TextureTarget, this.Samples, internalFormat, this.Width, this.Height, this.Layers, this.FixedSampleLocations);
        }
    }
}