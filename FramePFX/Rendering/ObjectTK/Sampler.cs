//
// Sampler.cs
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
    /// Represents a sampler object.
    /// </summary>
    public sealed class Sampler : OGLObject
    {
        /// <summary>
        /// Initializes a new sampler object.
        /// </summary>
        public Sampler() : base(GL.GenSampler())
        {
        }

        protected override void Dispose(bool manual)
        {
            if (!manual)
                return;
            GL.DeleteSampler(this.Handle);
        }

        /// <summary>
        /// Binds the sampler to the given texture unit.
        /// </summary>
        /// <param name="textureUnit">The texture unit to bind to.</param>
        public void Bind(TextureUnit textureUnit)
        {
            this.Bind((int) textureUnit - (int) TextureUnit.Texture0);
        }

        /// <summary>
        /// Binds the sampler to the given texture unit.
        /// </summary>
        /// <param name="unit">The texture unit to bind to.</param>
        public void Bind(int unit)
        {
            GL.BindSampler(unit, this.Handle);
        }

        /// <summary>
        /// Sets the given wrap mode on all dimensions R, S and T.
        /// </summary>
        /// <param name="wrapMode">The wrap mode to apply.</param>
        public void SetWrapMode(TextureWrapMode wrapMode)
        {
            var mode = (int) wrapMode;
            this.SetParameter(SamplerParameterName.TextureWrapR, mode);
            this.SetParameter(SamplerParameterName.TextureWrapS, mode);
            this.SetParameter(SamplerParameterName.TextureWrapT, mode);
        }

        /// <summary>
        /// Sets sampler parameters.
        /// </summary>
        /// <param name="parameterName">The parameter name to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetParameter(SamplerParameterName parameterName, int value)
        {
            GL.SamplerParameter(this.Handle, parameterName, value);
        }
    }
}