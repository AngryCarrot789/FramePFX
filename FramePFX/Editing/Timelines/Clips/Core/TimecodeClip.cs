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
using FramePFX.DataTransfer;
using FramePFX.Editing.Automation.Params;
using FramePFX.Editing.Rendering;
using FramePFX.Editing.Timelines.Clips.Video;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Utils;
using FramePFX.Utils.Accessing;
using SkiaSharp;

namespace FramePFX.Editing.Timelines.Clips.Core;

public class TimecodeClip : VideoClip {
    public static readonly ParameterDouble FontSizeParameter = Parameter.RegisterDouble(typeof(TimecodeClip), nameof(TimecodeClip), nameof(FontSize), 40, ValueAccessors.LinqExpression<double>(typeof(TimecodeClip), nameof(FontSize)), ParameterFlags.StandardProjectVisual);
    public static readonly DataParameterBool UseClipStartTimeParameter = DataParameter.Register(new DataParameterBool(typeof(TimecodeClip), nameof(UseClipStartTime), true, ValueAccessors.Reflective<bool>(typeof(TimecodeClip), nameof(UseClipStartTime)), DataParameterFlags.StandardProjectVisual));
    public static readonly DataParameter<SKColor> ForegroundParameter = DataParameter.Register(new DataParameter<SKColor>(typeof(TimecodeClip), nameof(Foreground), SKColors.Red, ValueAccessors.Reflective<SKColor>(typeof(TimecodeClip), nameof(foreground)), DataParameterFlags.StandardProjectVisual));

    public static readonly DataParameterBool UseClipEndTimeParameter = DataParameter.Register(new DataParameterBool(typeof(TimecodeClip), nameof(UseClipEndTime), true, ValueAccessors.Reflective<bool>(typeof(TimecodeClip), nameof(UseClipEndTime)), DataParameterFlags.StandardProjectVisual));
    public static readonly DataParameterDouble StartTimeParameter = DataParameter.Register(new DataParameterDouble(typeof(TimecodeClip), nameof(StartTime), 0.0, ValueAccessors.Reflective<double>(typeof(TimecodeClip), nameof(StartTime)), DataParameterFlags.StandardProjectVisual));
    public static readonly DataParameterDouble EndTimeParameter = DataParameter.Register(new DataParameterDouble(typeof(TimecodeClip), nameof(EndTime), 0.0, ValueAccessors.Reflective<double>(typeof(TimecodeClip), nameof(EndTime)), DataParameterFlags.StandardProjectVisual));

    private SKColor foreground;
    private double FontSize;
    private bool UseClipStartTime;
    private bool UseClipEndTime;
    private double StartTime;
    private double EndTime;
    private string? fontFamily = "Consolas";

    private class LockedFontData : IDisposable {
        public SKFont? cachedFont;
        public SKTypeface? cachedTypeFace;

        public void Dispose() {
            this.cachedFont?.Dispose();
            this.cachedFont = null;
            this.cachedTypeFace?.Dispose();
            this.cachedTypeFace = null;
        }
    }

    private readonly DisposableRef<LockedFontData> fontData;

    private TimeSpan render_StartTime;
    private TimeSpan render_EndTime;
    private FrameSpan render_Span;
    private long render_Frame;
    private double renderFontSize;
    private SKRect lastRenderRect;

    public string? FontFamily {
        get => this.fontFamily;
        set {
            if (this.fontFamily == value)
                return;
            this.fontFamily = value;
            this.fontData.Dispose();
            this.FontFamilyChanged?.Invoke(this);
            this.InvalidateRender();
        }
    }

    public SKColor Foreground {
        get => this.foreground;
        set => DataParameter.SetValueHelper(this, ForegroundParameter, ref this.foreground, value);
    }

    public event ClipEventHandler? FontFamilyChanged;

    public TimecodeClip() {
        this.UsesCustomOpacityCalculation = true;
        this.fontData = new DisposableRef<LockedFontData>(new LockedFontData(), true);
        this.FontSize = FontSizeParameter.Descriptor.DefaultValue;
        this.UseClipStartTime = UseClipStartTimeParameter.DefaultValue;
        this.UseClipEndTime = UseClipEndTimeParameter.DefaultValue;
        this.StartTime = StartTimeParameter.DefaultValue;
        this.EndTime = EndTimeParameter.DefaultValue;
        this.foreground = ForegroundParameter.GetDefaultValue(this);
    }

    private string GetCurrentTimeString() {
        double percent = Maths.InverseLerp(this.render_Span.Begin, this.render_Span.EndIndex, this.render_Frame);
        TimeSpan time = TimeSpan.FromSeconds(Maths.Lerp(this.render_StartTime.TotalSeconds, this.render_EndTime.TotalSeconds, percent));
        return string.Format("{0:00}:{1:00}:{2:00}.{3:00}", (int) time.TotalHours, time.Minutes, time.Seconds, time.Milliseconds / 10.0);
    }

    static TimecodeClip() {
        SerialisationRegistry.Register<TimecodeClip>(0, (clip, data, ctx) => {
            ctx.DeserialiseBaseType(data);
            clip.UseClipStartTime = data.GetBool("UseClipStart");
            clip.UseClipEndTime = data.GetBool("UseClipEnd");
            clip.StartTime = data.GetDouble("StartTime");
            clip.EndTime = data.GetDouble("EndTime");
            clip.fontFamily = data.GetString("FontFamily", null);
            clip.foreground = data.GetUInt("Foreground");
        }, (clip, data, ctx) => {
            ctx.SerialiseBaseType(data);
            data.SetBool("UseClipStart", clip.UseClipStartTime);
            data.SetBool("UseClipEnd", clip.UseClipEndTime);
            data.SetDouble("StartTime", clip.StartTime);
            data.SetDouble("EndTime", clip.EndTime);
            if (clip.fontFamily != null)
                data.SetString("FontFamily", clip.fontFamily);
            data.SetUInt("Foreground", (uint) clip.foreground);
        });

        DataParameter.AddMultipleHandlers((parameter, owner) => ((TimecodeClip) owner).InvalidateRender(), ForegroundParameter);

        DataParameter.AddMultipleHandlers((parameter, owner) => {
            ((TimecodeClip) owner).InvalidateRender();
            ((TimecodeClip) owner).OnRenderSizeChanged();
        }, UseClipStartTimeParameter, UseClipEndTimeParameter, StartTimeParameter, EndTimeParameter);

        FontSizeParameter.ValueChanged += sequence => {
            TimecodeClip owner = (TimecodeClip) sequence.AutomationData.Owner;
            owner.fontData.Dispose();
            owner.InvalidateRender();
            owner.OnRenderSizeChanged();
        };
    }

    protected override void LoadDataIntoClone(Clip clone, ClipCloneOptions options) {
        base.LoadDataIntoClone(clone, options);
        TimecodeClip timer = (TimecodeClip) clone;
        timer.UseClipStartTime = this.UseClipStartTime;
        timer.UseClipEndTime = this.UseClipEndTime;
        timer.StartTime = this.StartTime;
        timer.EndTime = this.EndTime;
        timer.foreground = this.foreground;
    }

    public override Vector2? GetRenderSize() {
        return new Vector2(this.lastRenderRect.Width, this.lastRenderRect.Height);
    }

    public override bool PrepareRenderFrame(PreRenderContext rc, long frame) {
        long playHead = this.FrameSpan.Begin + frame;
        this.render_Frame = playHead;
        this.render_Span = this.FrameSpan;
        this.render_StartTime = this.UseClipStartTime ? default : TimeSpan.FromSeconds(this.StartTime);
        this.render_EndTime =
            this.UseClipEndTime ? TimeSpan.FromSeconds(this.FrameSpan.Duration / this.Project!.Settings.FrameRateDouble) : TimeSpan.FromSeconds(this.EndTime);
        if (this.UseClipEndTime)
            this.render_EndTime += this.render_StartTime;
        this.renderFontSize = this.FontSize;
        return true;
    }

    public override void RenderFrame(RenderContext rc, ref SKRect renderArea) {
        this.fontData.BeginUsage(this, (clip, data) => {
            data.cachedTypeFace = clip.fontFamily != null ? SKTypeface.FromFamilyName(clip.fontFamily) : SKTypeface.CreateDefault();
            data.cachedFont = new SKFont(data.cachedTypeFace, (float) clip.renderFontSize);
        });

        string text = this.GetCurrentTimeString();
        LockedFontData fd = this.fontData.Value;

        using SKPaint paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Color = RenderUtils.BlendAlpha(this.foreground, this.RenderOpacityByte);
        paint.FilterQuality = rc.FilterQuality;
        using (SKTextBlob blob = SKTextBlob.Create(text, fd.cachedFont)) {
            fd.cachedFont!.GetFontMetrics(out SKFontMetrics metrics);
            // we can get away with this since we just use numbers and not any 'special'
            // characters with bits below the baseline and whatnot
            SKRect realFinalRenderArea = new SKRect(0, 0, blob.Bounds.Right, blob.Bounds.Bottom - metrics.Ascent - metrics.Descent);
            rc.Canvas.DrawText(blob, 0, -blob.Bounds.Top - metrics.Descent, paint);

            // we still need to tell the track the rendering area, otherwise we're copying the entire frame which is
            // unacceptable. Even though there will most likely be a bunch of transparent padding pixels, it's still better
            renderArea = rc.TranslateRect(realFinalRenderArea);
            this.lastRenderRect = realFinalRenderArea;
        }

        this.fontData.CompleteUsage();
    }
}