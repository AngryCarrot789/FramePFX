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

using SkiaSharp;

namespace FramePFX.Editors.Rendering
{
    /// <summary>
    /// Contains information about the state of a frame, used to render a timeline
    /// </summary>
    public readonly struct RenderFrameInfo
    {
        public SKImageInfo ImageInfo { get; }

        /// <summary>
        /// Gets the timeline playhead frame that is being rendered
        /// </summary>
        public long PlayHeadFrame { get; }

        public RenderFrameInfo(SKImageInfo imageInfo, long playHeadFrame)
        {
            this.ImageInfo = imageInfo;
            this.PlayHeadFrame = playHeadFrame;
        }
    }
}