// 
// Copyright (c) 2026-2026 REghZy
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

using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Utils.Accessing;
using PFXToolKitUI.Utils.Events;
using SkiaSharp;

namespace FramePFX.Editing.Video;

public abstract class VideoClip : Clip {
    /// <summary>
    /// Returns <see cref="Editing.ClipType.Video"/>
    /// </summary>
    public sealed override ClipType ClipType => ClipType.Video;

    public double Opacity {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, static t => {
            t.OpacityChanged?.Invoke(t, EventArgs.Empty);
            t.RaiseRenderInvalidated();
        });
    }

    public event EventHandler? OpacityChanged;
    
    /// <summary>
    /// An event fired when the render state of this video clip becomes invalid, such as from <see cref="Opacity"/> changing
    /// </summary>
    public event EventHandler? RenderInvalidated;
    
    protected VideoClip() {
    }
    
    /// <summary>
    /// Raises the <see cref="RenderInvalidated"/> event
    /// </summary>
    public void RaiseRenderInvalidated() {
        this.RenderInvalidated?.Invoke(this, EventArgs.Empty);
        ((VideoTrack?) this.Track)?.RaiseRenderInvalidated(this.Span);
    }
    
    /// <summary>
    /// Begins rendering this video clip. This is always called before <see cref="BeginRender"/>.
    /// Invoking this method multiple times before <see cref="EndRender"/> is permitted although unlikely
    /// </summary>
    /// <param name="offset">The play head position, relative to the clip's <see cref="Clip.Span"/></param>
    /// <param name="context">The pre-rendering context, containing frame info</param>
    protected internal abstract void BeginRender(long offset, PreRenderContext context);
    
    /// <summary>
    /// Ends rendering of this clip. This is always called after <see cref="BeginRender"/>
    /// </summary>
    /// <param name="offset">The play head position, relative to the clip's <see cref="Clip.Span"/></param>
    /// <param name="context">The rendering context, containing frame info and a render target</param>
    /// <returns>A task that completes once rendering has finished</returns>
    protected internal abstract ValueTask EndRender(long offset, RenderContext context);
}

public sealed class BlankVideoClip : VideoClip {
    public BlankVideoClip() {
    }

    protected internal override void BeginRender(long offset, PreRenderContext context) {
    }

    protected internal override ValueTask EndRender(long offset, RenderContext context) {
        return ValueTask.CompletedTask;
    }
}

public sealed class ShapeVideoClip : VideoClip {
    public static readonly DataParameterNumber<double> WidthParameter = DataParameter.Register(new DataParameterNumber<double>(typeof(ShapeVideoClip), nameof(Width), 0, ValueAccessors.Reflective<double>(typeof(ShapeVideoClip), nameof(width))));
    public static readonly DataParameterNumber<double> HeightParameter = DataParameter.Register(new DataParameterNumber<double>(typeof(ShapeVideoClip), nameof(Height), 0, ValueAccessors.Reflective<double>(typeof(ShapeVideoClip), nameof(height))));

    private double width, height;

    public double Width {
        get => this.width;
        set => DataParameter.SetValueHelper(this, WidthParameter, ref this.width, value);
    }

    public double Height {
        get => this.height;
        set => DataParameter.SetValueHelper(this, HeightParameter, ref this.height, value);
    }
    
    public ShapeVideoClip() {
        this.width = WidthParameter.GetDefaultValue(this);
        this.height = HeightParameter.GetDefaultValue(this);
    }

    protected internal override void BeginRender(long offset, PreRenderContext context) {
    }

    protected internal override ValueTask EndRender(long offset, RenderContext context) {
        using (SKPaint paint = new SKPaint() {Color = SKColors.Blue})
            context.Canvas.DrawRect(0, 0, (float) this.width, (float) this.height, paint);
        
        return ValueTask.CompletedTask;
    }
}