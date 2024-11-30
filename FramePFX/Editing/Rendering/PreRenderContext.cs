//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System.Numerics;
using SkiaSharp;

namespace FramePFX.Editing.Rendering;

public readonly struct PreRenderContext {
    /// <summary>
    /// The image info associated with the surface that will be used to do the final render
    /// </summary>
    public SKImageInfo ImageInfo { get; }

    /// <summary>
    /// A vector2 containing our <see cref="ImageInfo"/>'s width and height
    /// </summary>
    public Vector2 FrameSize => new Vector2(this.ImageInfo.Width, this.ImageInfo.Height);

    public EnumRenderQuality RenderQuality { get; }

    public SKFilterQuality FilterQuality { get; }

    public PreRenderContext(SKImageInfo imageInfo, EnumRenderQuality renderQuality) {
        this.ImageInfo = imageInfo;
        this.RenderQuality = renderQuality;
        this.FilterQuality = renderQuality.ToFilterQuality();
    }
}