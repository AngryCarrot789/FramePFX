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

using System.Collections.Specialized;
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

public class TextVideoClip : VideoClip {
    public static readonly DataParameterString TextParameter = DataParameter.Register(new DataParameterString(typeof(TextVideoClip), nameof(Text), null, ValueAccessors.Reflective<string?>(typeof(TextVideoClip), nameof(myText)), DataParameterFlags.StandardProjectVisual));
    public static readonly DataParameterString FontFamilyParameter = DataParameter.Register(new DataParameterString(typeof(TextVideoClip), nameof(FontFamily), "Consolas", ValueAccessors.Reflective<string?>(typeof(TextVideoClip), nameof(fontFamily))));
    public static readonly ParameterFloat FontSizeParameter = Parameter.RegisterFloat(typeof(TextVideoClip), nameof(TextVideoClip), nameof(FontSize), 40.0F, ValueAccessors.Reflective<float>(typeof(TextVideoClip), nameof(FontSize)));
    public static readonly ParameterFloat BorderThicknessParameter = Parameter.RegisterFloat(typeof(TextVideoClip), nameof(TextVideoClip), nameof(BorderThickness), 0.0F, ValueAccessors.Reflective<float>(typeof(TextVideoClip), nameof(BorderThickness)));
    public static readonly ParameterFloat SkewXParameter = Parameter.RegisterFloat(typeof(TextVideoClip), nameof(TextVideoClip), nameof(SkewX), 0.0F, ValueAccessors.Reflective<float>(typeof(TextVideoClip), nameof(SkewX)));
    public static readonly ParameterFloat LineSpacingParameter = Parameter.RegisterFloat(typeof(TextVideoClip), nameof(TextVideoClip), nameof(LineSpacing), 0.0F, ValueAccessors.Reflective<float>(typeof(TextVideoClip), nameof(LineSpacing)));
    public static readonly DataParameter<SKColor> ForegroundParameter = DataParameter.Register(new DataParameter<SKColor>(typeof(TextVideoClip), nameof(Foreground), SKColors.Red, ValueAccessors.Reflective<SKColor>(typeof(TextVideoClip), nameof(foreground))));

    public string? Text {
        get => this.myText;
        set => DataParameter.SetValueHelper(this, TextParameter, ref this.myText, value);
    }

    public string? FontFamily {
        get => this.fontFamily;
        set => DataParameter.SetValueHelper(this, FontFamilyParameter, ref this.fontFamily, value);
    }

    public SKColor Foreground {
        get => this.foreground;
        set => DataParameter.SetValueHelper(this, ForegroundParameter, ref this.foreground, value);
    }

    private BitVector32 clipProps;
    private SKSize TextBlobBoundingBox;
    private SKColor foreground;

    private string? myText;
    private string? fontFamily;
    private float FontSize = FontSizeParameter.Descriptor.DefaultValue;
    private float BorderThickness = BorderThicknessParameter.Descriptor.DefaultValue;
    private float SkewX = SkewXParameter.Descriptor.DefaultValue;
    private float LineSpacing = LineSpacingParameter.Descriptor.DefaultValue;

    private class RenderingInfo : IDisposable {
        public SKPaint? GeneratedPaint;
        public SKFont? GeneratedFont;
        public SKTextBlob?[]? TextBlobs;

        public void Dispose() {
            this.GeneratedFont?.Dispose();
            this.GeneratedFont = null;
            this.GeneratedPaint?.Dispose();
            this.GeneratedPaint = null;
            DisposeTextBlobs(ref this.TextBlobs);
        }
    }

    private readonly DisposableRef<RenderingInfo> renderInfoLock;

    public TextVideoClip() {
        this.UsesCustomOpacityCalculation = true;
        this.myText = TextParameter.GetDefaultValue(this);
        this.fontFamily = FontFamilyParameter.GetDefaultValue(this);
        this.foreground = ForegroundParameter.GetDefaultValue(this);
        this.clipProps = new BitVector32();
        this.renderInfoLock = new DisposableRef<RenderingInfo>(new RenderingInfo(), true);
        this.AutomationData.AddParameterChangedHandler(OpacityParameter, (o) => ((TextVideoClip) o.AutomationData.Owner).InvalidateFontData());
    }

    static TextVideoClip() {
        SerialisationRegistry.Register<TextVideoClip>(0, (clip, data, ctx) => {
            ctx.DeserialiseBaseType(data);
            clip.fontFamily = data.GetString("FontFamily", FontFamilyParameter.GetDefaultValue(clip)!);
            clip.clipProps = data.GetStruct<BitVector32>("ClipProps");
            clip.myText = data.GetString("Text");
            clip.foreground = data.GetUInt("Foreground");
        }, (clip, data, ctx) => {
            ctx.SerialiseBaseType(data);
            data.SetString("FontFamily", clip.fontFamily!);
            data.SetStruct("ClipProps", clip.clipProps);
            data.SetString("Text", clip.myText);
            data.SetUInt("Foreground", (uint) clip.foreground);
        });

        Parameter.AddMultipleHandlers((s) => ((TextVideoClip) s.AutomationData.Owner).InvalidateFontData(), FontSizeParameter, BorderThicknessParameter, SkewXParameter, LineSpacingParameter);
        DataParameter.AddMultipleHandlers((_, owner) => ((TextVideoClip) owner).InvalidateFontData(), FontFamilyParameter, TextParameter, ForegroundParameter);
    }

    protected override void LoadDataIntoClone(Clip clone, ClipCloneOptions options) {
        base.LoadDataIntoClone(clone, options);
        TextVideoClip clip = (TextVideoClip) clone;
        clip.myText = this.myText;
        clip.clipProps = this.clipProps;
        clip.fontFamily = this.fontFamily;
        clip.foreground = this.foreground;
    }

    public override Vector2? GetRenderSize() {
        return new Vector2(this.TextBlobBoundingBox.Width, this.TextBlobBoundingBox.Height);
    }

    public override bool PrepareRenderFrame(PreRenderContext rc, long frame) {
        return !string.IsNullOrEmpty(this.Text);
    }

    public override void RenderFrame(RenderContext rc, ref SKRect renderArea) {
        this.renderInfoLock.BeginUsage(this, (clip, info) => {
            if (info.GeneratedFont == null) {
                SKTypeface typeface = SKTypeface.FromFamilyName(string.IsNullOrEmpty(clip.FontFamily) ? "Consolas" : clip.FontFamily) ?? SKTypeface.Default;
                info.GeneratedFont = new SKFont(typeface, clip.FontSize, 1f, clip.SkewX);
            }

            info.GeneratedPaint ??= new SKPaint() {
                StrokeWidth = clip.BorderThickness,
                ColorF = RenderUtils.BlendAlpha(clip.foreground, clip.RenderOpacity),
                TextAlign = SKTextAlign.Left
            };

            if (!string.IsNullOrEmpty(clip.Text)) {
                SKTextBlob?[]? blobs = info.TextBlobs = clip.CreateTextBlobs(clip.Text, info.GeneratedPaint, info.GeneratedFont);
                if (blobs != null) {
                    info.GeneratedFont!.GetFontMetrics(out SKFontMetrics metrics);
                    float w = 0, h = 0, myLineHeight = 0.0F;
                    for (int i = 0, endIndex = blobs.Length - 1; i <= endIndex; i++) {
                        SKTextBlob? blob = blobs[i];
                        if (blob != null) {
                            SKRect bound = blob.Bounds;
                            w = Math.Max(w, bound.Width);

                            float height = Math.Abs(blob.Bounds.Bottom - blob.Bounds.Top) - metrics.Bottom;
                            h += height + myLineHeight;
                            myLineHeight = clip.LineSpacing;
                        }
                    }

                    clip.TextBlobBoundingBox = new SKSize(w, h);

                    // Since OnRenderSizeChanged will typically immediately update the GUI components,
                    // it has to be fired on the main thread. However, this means that the UI parts
                    // might start "flickering", the values might start jumping between 0 and whatever.

                    // InvalidateFontData sets TextBlobBoundingBox to default, but if we don't do that,
                    // the flickering is gone. This is the issue with lazily generating the render size :/
                    Application.Instance.Dispatcher.Post(clip.OnRenderSizeChanged);
                }
            }
        });

        RenderingInfo renderInfo = this.renderInfoLock.Value;
        try {
            SKPaint? paint = renderInfo.GeneratedPaint;
            if (paint != null)
                paint.FilterQuality = rc.FilterQuality;
            renderInfo.GeneratedFont!.GetFontMetrics(out SKFontMetrics metrics);
            SKTextBlob?[] blobs = renderInfo.TextBlobs!;
            float offset = 0.0F;
            for (int i = 0, endIndex = blobs.Length - 1; i <= endIndex; i++) {
                SKTextBlob? blob = blobs[i];
                if (blob != null) {
                    float y = -blob.Bounds.Top - metrics.Descent + offset;
                    rc.Canvas.DrawText(blob, 0, y, paint);
                    offset += this.FontSize;
                    if (i != endIndex)
                        offset += this.LineSpacing;
                }
            }

            renderArea = rc.TranslateRect(new SKRect(0, 0, this.TextBlobBoundingBox.Width, this.TextBlobBoundingBox.Height));
        }
        finally {
            this.renderInfoLock.CompleteUsage();
        }
    }

    #region Text Blob Generation and Life Time

    /// <summary>
    /// Invalidates the cached font and paint information. This is called automatically when any of our properties change
    /// </summary>
    public void InvalidateFontData() {
        this.renderInfoLock.Dispose();
        this.TextBlobBoundingBox = default;
        this.InvalidateRender();
    }

    public SKTextBlob[]? CreateTextBlobs(string input, SKPaint paint, SKFont font) {
        return CreateTextBlobs(input, font, this.LineSpacing); // * 1.2f
    }

    public static SKTextBlob[]? CreateTextBlobs(string input, SKFont font, float lineHeight) {
        if (string.IsNullOrEmpty(input)) {
            return null;
        }

        string[] lines = input.Split('\n');
        SKTextBlob[] blobs = new SKTextBlob[lines.Length];
        for (int i = 0; i < lines.Length; i++) {
            float y = i * lineHeight;
            blobs[i] = SKTextBlob.Create(lines[i], font, new SKPoint(0, y));
        }

        return blobs;
    }

    public static void DisposeTextBlobs(ref SKTextBlob?[]? blobs) {
        if (blobs == null)
            return;
        foreach (SKTextBlob? blob in blobs)
            blob?.Dispose();
        blobs = null;
    }

    #endregion
}