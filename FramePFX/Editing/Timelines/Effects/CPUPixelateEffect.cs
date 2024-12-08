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
using FramePFX.Editing.Automation.Params;
using FramePFX.Editing.Rendering;
using FramePFX.Editing.Timelines.Clips.Video;
using FramePFX.Editing.Utils;
using FramePFX.Natives;
using FramePFX.Utils.Accessing;
using SkiaSharp;

namespace FramePFX.Editing.Timelines.Effects;

public class CPUPixelateEffect : VideoEffect
{
    public static readonly ParameterLong BlockSizeParameter =
        Parameter.RegisterLong(
            typeof(CPUPixelateEffect),
            nameof(BlockSize),
            nameof(BlockSize),
            16, 0, 1024, // def/min/max
            ValueAccessors.LinqExpression<long>(typeof(CPUPixelateEffect), nameof(BlockSize)),
            ParameterFlags.StandardProjectVisual);

    public long BlockSize;

    private Vector2 renderSize;

    public override void PrepareRender(PreRenderContext ctx, long frame)
    {
        base.PrepareRender(ctx, frame);
        if (this.Owner is VideoClip)
        {
            this.renderSize = this.OwnerClip.GetRenderSize() ?? new Vector2();
        }
        else
        {
            this.renderSize = ctx.FrameSize;
        }
    }

    public override void PostProcessFrame(RenderContext rc, ref SKRect renderArea)
    {
        base.PostProcessFrame(rc, ref renderArea);

        // It should never be negative as it's guarded by the parameter system.... buuuuut just in case ;)
        if (this.BlockSize <= 0)
        {
            return;
        }

        unsafe
        {
            uint* pImg = (uint*) rc.Bitmap.GetPixels().ToPointer();
            SKRectI visibleI = renderArea.CeilAndFloorI();
            int left = Math.Max(0, visibleI.Left);
            int top = Math.Max(0, visibleI.Top);
            int right = Math.Min(rc.ImageInfo.Width, visibleI.Right);
            int bottom = Math.Min(rc.ImageInfo.Height, visibleI.Bottom);

            PFXNative.PFXCE_PixelateVfx(pImg, rc.ImageInfo.Width, rc.ImageInfo.Height, left, top, right, bottom, (int) this.BlockSize);
        }
    }
}