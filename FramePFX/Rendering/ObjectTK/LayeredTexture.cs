//
// LayeredTexture.cs
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
    /// Represents a layered texture.<br/>
    /// Layered textures are all array, cube map and 3D textures.
    /// </summary>
    public abstract class LayeredTexture
        : Texture
    {
        public override bool SupportsLayers { get { return true; } }

        internal LayeredTexture(SizedInternalFormat internalFormat, int levels) : base(internalFormat, levels)
        {
        }
    }
}