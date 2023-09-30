//
// Texture2DMultisample.cs
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
    /// Represents a 2D multisample texture.<br/>
    /// The image in this texture (only one image. No mipmapping) is 2-dimensional.
    /// Each pixel in this image contains multiple samples instead of just one value.
    /// </summary>
    public sealed class Texture2DMultisample : Texture
    {
        public override TextureTarget TextureTarget { get { return TextureTarget.Texture2DMultisample; } }
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
        /// <param name="samples">The number of samples per texel.</param>
        /// <param name="fixedSampleLocations">Specifies whether the texels will use identical sample locations.</param>
        public Texture2DMultisample(SizedInternalFormat internalFormat, int width, int height, int samples, bool fixedSampleLocations) : base(internalFormat, 1)
        {
            this.Width = width;
            this.Height = height;
            this.Samples = samples;
            this.FixedSampleLocations = fixedSampleLocations;
            GL.BindTexture(this.TextureTarget, Handle);
            GL.TexStorage2DMultisample((TextureTargetMultisample2d)this.TextureTarget, this.Samples, internalFormat, this.Width, this.Height, this.FixedSampleLocations);
        }
    }
}