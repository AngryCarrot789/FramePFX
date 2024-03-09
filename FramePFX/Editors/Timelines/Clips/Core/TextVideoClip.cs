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
using System.Collections.Specialized;
using System.Numerics;
using FramePFX.Editors.DataTransfer;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.ResourceHelpers;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Utils.Accessing;
using SkiaSharp;

namespace FramePFX.Editors.Timelines.Clips.Core
{
    public class TextVideoClip : VideoClip
    {
        public static readonly DataParameterString TextParameter =
            DataParameter.Register(
                new DataParameterString(
                    typeof(TextVideoClip),
                    nameof(Text), null,
                    ValueAccessors.Reflective<string>(typeof(TextVideoClip), nameof(text)),
                    DataParameterFlags.StandardProjectVisual));

        private BitVector32 clipProps;
        private SKTextBlob[] TextBlobs;

        private Vector2 TextBlobBoundingBox;
        private string text = TextParameter.DefaultValue;

        public string Text {
            get => this.text;
            set => DataParameter.SetValueHelper(this, TextParameter, ref this.text, value);
        }

        public IResourcePathKey<ResourceTextStyle> TextStyleKey { get; }

        public TextVideoClip()
        {
            this.TextStyleKey = this.ResourceHelper.RegisterKeyByTypeName<ResourceTextStyle>();
            this.TextStyleKey.ResourceChanged += this.OnResourceTextStyleChanged;
            this.clipProps = new BitVector32();
        }

        static TextVideoClip()
        {
            SerialisationRegistry.Register<TextVideoClip>(0, (clip, data, ctx) =>
            {
                ctx.SerialiseBaseType(data);
                if (!string.IsNullOrEmpty(clip.text))
                    data.SetString(nameof(clip.Text), clip.text);
                data.SetInt("ClipPropData0", clip.clipProps.Data);
            }, (clip, data, ctx) =>
            {
                ctx.DeserialiseBaseType(data);
                clip.text = data.GetString(nameof(clip.Text), null);
                clip.clipProps = new BitVector32(data.GetInt("ClipPropData0"));
            });
        }

        protected void OnResourceTextStyleChanged(IResourcePathKey<ResourceTextStyle> key, ResourceTextStyle oldItem, ResourceTextStyle newItem)
        {
            this.InvalidateTextCache();
            if (oldItem != null)
            {
                oldItem.RenderDataInvalidated -= this.OnResourceRenderDataInvalidated;
            }

            if (newItem != null)
            {
                this.GenerateTextCache();
                newItem.RenderDataInvalidated += this.OnResourceRenderDataInvalidated;
            }
        }

        private void OnResourceRenderDataInvalidated(BaseResource resource)
        {
            this.InvalidateTextCache();
        }

        protected override void LoadDataIntoClone(Clip clone, ClipCloneOptions options)
        {
            base.LoadDataIntoClone(clone, options);
            TextVideoClip clip = (TextVideoClip) clone;
            clip.text = this.text;
            clip.clipProps = this.clipProps;
        }

        public override Vector2? GetRenderSize()
        {
            return this.TextBlobs != null ? this.TextBlobBoundingBox : (Vector2?) null;
        }

        public void RegenerateText()
        {
            this.InvalidateTextCache();
            this.GenerateTextCache();
        }

        public void InvalidateTextCache()
        {
            this.TextBlobBoundingBox = new Vector2();
            ResourceTextStyle.DisposeTextBlobs(ref this.TextBlobs);
        }

        public void GenerateTextCache()
        {
            if (this.TextBlobs != null || string.IsNullOrEmpty(this.text))
            {
                return;
            }

            if (!this.TextStyleKey.TryGetResource(out ResourceTextStyle resource))
            {
                return;
            }

            resource.GenerateCachedData();
            if (resource.GeneratedFont != null)
            {
                this.TextBlobs = ResourceTextStyle.CreateTextBlobs(this.text, resource.GeneratedPaint, resource.GeneratedFont);
                float w = 0, h = 0;
                foreach (SKTextBlob blob in this.TextBlobs)
                {
                    if (blob != null)
                    {
                        SKRect bound = blob.Bounds;
                        w = Math.Max(w, bound.Width);
                        h = Math.Max(h, bound.Height);
                    }
                }

                this.TextBlobBoundingBox = new Vector2(w, h);
            }
        }

        public override bool PrepareRenderFrame(PreRenderContext rc, long frame)
        {
            if (!this.TextStyleKey.TryGetResource(out ResourceTextStyle _))
                return false;
            if (this.TextBlobs == null && !string.IsNullOrEmpty(this.text))
                this.RegenerateText();
            return this.TextBlobs != null;
        }

        public override void RenderFrame(RenderContext rc, ref SKRect renderArea)
        {
            if (!this.TextStyleKey.TryGetResource(out ResourceTextStyle r))
            {
                return;
            }

            SKPaint paint = r.GeneratedPaint;
            if (this.TextBlobs == null || paint == null)
            {
                return;
            }

            foreach (SKTextBlob blob in this.TextBlobs)
            {
                if (blob != null)
                {
                    // fd.cachedFont.GetFontMetrics(out SKFontMetrics metrics);
                    // // we can get away with this since we just use numbers and not any 'special'
                    // // characters with bits below the baseline and whatnot
                    // SKRect realFinalRenderArea = new SKRect(0, 0, blob.Bounds.Right, blob.Bounds.Bottom - metrics.Ascent - metrics.Descent);
                    // rc.Canvas.DrawText(blob, 0, -blob.Bounds.Top - metrics.Descent, paint);
                    //
                    // // we still need to tell the track the rendering area, otherwise we're copying the entire frame which is
                    // // unacceptable. Even though there will most likely be a bunch of transparent padding pixels, it's still better
                    // renderArea = rc.TranslateRect(realFinalRenderArea);

                    rc.Canvas.DrawText(blob, 0, blob.Bounds.Height / 2f, r.GeneratedPaint);
                }
            }
        }
    }
}