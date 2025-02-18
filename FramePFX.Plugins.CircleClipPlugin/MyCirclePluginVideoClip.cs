// 
// Copyright (c) 2024-2024 REghZy
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
using FramePFX.Editing.Rendering;
using FramePFX.Editing.Timelines.Clips.Video;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Utils.Accessing;
using SkiaSharp;

namespace FramePFX.Plugins.CircleClipPlugin;

public class MyCirclePluginVideoClip : VideoClip {
    public static readonly DataParameterFloat RadiusParameter =
        DataParameter.Register(
            new DataParameterFloat(
                typeof(MyCirclePluginVideoClip),
                nameof(Radius), 25.0F, 1.0F, 1000.0F,
                ValueAccessors.Reflective<float>(typeof(MyCirclePluginVideoClip), nameof(radius))));

    private float radius;

    public float Radius {
        get => this.radius;
        set => DataParameter.SetValueHelper(this, RadiusParameter, ref this.radius, value);
    }

    public MyCirclePluginVideoClip() {
        this.radius = RadiusParameter.GetDefaultValue(this);
    }

    static MyCirclePluginVideoClip() {
        RadiusParameter.ValueChanged += (parameter, owner) => ((MyCirclePluginVideoClip) owner).OnRenderSizeChanged();
    }

    public override Vector2? GetRenderSize() {
        return new Vector2(this.radius * 2.0F, this.radius * 2.0F);
    }

    public override bool PrepareRenderFrame(PreRenderContext rc, long frame) {
        return true;
    }

    public override void RenderFrame(RenderContext rc, ref SKRect renderArea) {
        float d = this.radius * 2.0F;

        using SKPaint paint = new SKPaint();
        paint.Color = SKColors.Orange;
        paint.IsAntialias = true;
        rc.Canvas.DrawCircle(this.radius, this.radius, this.radius, paint);

        renderArea = rc.TranslateRect(new SKRect(0, 0, d, d));
    }
}