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
    public static readonly ParameterBool IsAntiAliasedParameter = Parameter.RegisterBool(typeof(TextVideoClip), nameof(TextVideoClip), nameof(IsAntiAliased), true, ValueAccessors.Reflective<bool>(typeof(TextVideoClip), nameof(IsAntiAliased)));
    public static readonly ParameterFloat LineSpacingParameter = Parameter.RegisterFloat(typeof(TextVideoClip), nameof(TextVideoClip), nameof(LineSpacing), 0.0F, ValueAccessors.Reflective<float>(typeof(TextVideoClip), nameof(LineSpacing)));

    public string? Text {
        get => this.myText;
        set => DataParameter.SetValueHelper(this, TextParameter, ref this.myText, value);
    }

    public string? FontFamily {
        get => this.fontFamily;
        set => DataParameter.SetValueHelper(this, FontFamilyParameter, ref this.fontFamily, value);
    }


    private BitVector32 clipProps;
    private SKSize TextBlobBoundingBox;
    private SKColor foreground;

    private string? myText;
    private string? fontFamily;
    private float FontSize = FontSizeParameter.Descriptor.DefaultValue;
    private float BorderThickness = BorderThicknessParameter.Descriptor.DefaultValue;
    private float SkewX = SkewXParameter.Descriptor.DefaultValue;
    private bool IsAntiAliased = IsAntiAliasedParameter.Descriptor.DefaultValue;
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
        this.myText = TextParameter.GetDefaultValue(this);
        this.fontFamily = FontFamilyParameter.GetDefaultValue(this);
        this.clipProps = new BitVector32();
        this.foreground = SKColors.Black; // TODO: add automatable colour??? maybe an int
        this.renderInfoLock = new DisposableRef<RenderingInfo>(new RenderingInfo(), true);
    }

    static TextVideoClip() {
        SerialisationRegistry.Register<TextVideoClip>(0, (clip, data, ctx) => {
            ctx.DeserialiseBaseType(data);
            clip.fontFamily = data.GetString("FontFamily", FontFamilyParameter.GetDefaultValue(clip)!);
            clip.clipProps = data.GetStruct<BitVector32>("ClipProps");
        }, (layer, data, ctx) => {
            ctx.SerialiseBaseType(data);
            data.SetString("FontFamily", layer.fontFamily!);
            data.SetStruct("ClipProps", layer.clipProps);
        });

        Parameter.AddMultipleHandlers((s) => ((TextVideoClip) s.AutomationData.Owner).InvalidateFontData(), FontSizeParameter, BorderThicknessParameter, SkewXParameter, IsAntiAliasedParameter, LineSpacingParameter);
        DataParameter.AddMultipleHandlers((_, owner) => ((TextVideoClip) owner).InvalidateFontData(), FontFamilyParameter, TextParameter);
    }

    protected override void LoadDataIntoClone(Clip clone, ClipCloneOptions options) {
        base.LoadDataIntoClone(clone, options);
        TextVideoClip clip = (TextVideoClip) clone;
        clip.myText = this.myText;
        clip.clipProps = this.clipProps;
    }

    public override Vector2? GetRenderSize() {
        return new Vector2(this.TextBlobBoundingBox.Width, this.TextBlobBoundingBox.Height);
    }

    public override bool PrepareRenderFrame(PreRenderContext rc, long frame) {
        RenderingInfo info = this.renderInfoLock.Value;
        lock (this.renderInfoLock) {
            // Try to use the data. If it has been disposed, then regenerate
            if (!this.renderInfoLock.TryBeginUsage()) {
                if (info.GeneratedFont == null) {
                    SKTypeface typeface = SKTypeface.FromFamilyName(string.IsNullOrEmpty(this.FontFamily) ? "Consolas" : this.FontFamily);
                    if (typeface != null) {
                        info.GeneratedFont = new SKFont(typeface, this.FontSize, 1f, this.SkewX);
                    }
                }

                info.GeneratedPaint ??= new SKPaint() {
                    StrokeWidth = this.BorderThickness,
                    Color = this.foreground,
                    TextAlign = SKTextAlign.Left,
                    IsAntialias = this.IsAntiAliased
                };

                if (info.GeneratedFont != null && info.GeneratedPaint != null && !string.IsNullOrEmpty(this.Text)) {
                    SKTextBlob?[]? blobs = info.TextBlobs = this.CreateTextBlobs(this.Text, info.GeneratedPaint, info.GeneratedFont);
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
                                myLineHeight = this.LineSpacing;
                            }
                        }

                        this.TextBlobBoundingBox = new SKSize(w, h);
                        this.OnRenderSizeChanged();
                    }
                }

                // Begin usage to prevent premature disposal again
                this.renderInfoLock.ResetAndBeginUsage();
            }
        }

        // Check the info was generated right. If not, complete usage and do not render.
        // Info will be disposed when a property changes that will actually allow the
        // info to be regneerated successfully, then we can draw. Hopefully that was english
        if (info.TextBlobs != null && info.GeneratedPaint != null) {
            return true;
        }
        else {
            this.renderInfoLock.CompleteUsage();
        }

        return false;
    }

    public override void RenderFrame(RenderContext rc, ref SKRect renderArea) {
        RenderingInfo info = this.renderInfoLock.Value;
        try {
            SKPaint? paint = info.GeneratedPaint;
            info.GeneratedFont!.GetFontMetrics(out SKFontMetrics metrics);
            SKTextBlob?[] blobs = info.TextBlobs!;
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