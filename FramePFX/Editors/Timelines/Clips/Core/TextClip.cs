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

using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Rendering;
using FramePFX.Utils.Accessing;
using SkiaSharp;

namespace FramePFX.Editors.Timelines.Clips.Core {
    public class TextClip : VideoClip {
        public static readonly ParameterDouble FontSizeParameter = Parameter.RegisterDouble(typeof(TextClip), nameof(TextClip), nameof(FontSize), 40, ValueAccessors.LinqExpression<double>(typeof(TextClip), nameof(FontSize)), ParameterFlags.StandardProjectVisual);

        public double FontSize;

        public override bool PrepareRenderFrame(PreRenderContext rc, long frame) {
            throw new System.NotImplementedException();
        }

        public override void RenderFrame(RenderContext rc, ref SKRect renderArea) {
            throw new System.NotImplementedException();
        }
    }
}