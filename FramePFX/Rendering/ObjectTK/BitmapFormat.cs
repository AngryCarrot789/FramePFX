//
// BitmapFormat.cs
//
// Copyright (C) 2018 OpenTK
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//

using OpenTK.Graphics.OpenGL;

namespace FramePFX.Rendering.ObjectTK {
    internal class BitmapFormat {
        public SizedInternalFormat InternalFormat;
        public PixelFormat PixelFormat;
        public PixelType PixelType;

        // prevent instantiation
        protected BitmapFormat() { }

        static BitmapFormat() {
        }
    }
}