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

using System;
using SkiaSharp;

namespace FramePFX.Editors.Rendering
{
    /// <summary>
    /// A quality factor for when drawing
    /// </summary>
    public enum EnumRenderQuality : byte
    {
        /// <summary>
        /// Unspecified quality settings; SkiaSharp default
        /// </summary>
        UnspecifiedQuality,

        /// <summary>
        /// Low quality but fast render times
        /// </summary>
        Low,

        /// <summary>
        /// Medium quality looks good but takes a bit longer to draw
        /// </summary>
        Medium,

        /// <summary>
        /// Highest quality at the cost of possibly long render times
        /// </summary>
        High
    }

    public static class EnumRenderQualityExtensions
    {
        public static SKFilterQuality ToFilterQuality(this EnumRenderQuality quality)
        {
            switch (quality)
            {
                case EnumRenderQuality.UnspecifiedQuality: return SKFilterQuality.None;
                case EnumRenderQuality.Low: return SKFilterQuality.Low;
                case EnumRenderQuality.Medium: return SKFilterQuality.Medium;
                case EnumRenderQuality.High: return SKFilterQuality.High;
                default: throw new ArgumentOutOfRangeException(nameof(quality), quality, null);
            }
        }
    }
}