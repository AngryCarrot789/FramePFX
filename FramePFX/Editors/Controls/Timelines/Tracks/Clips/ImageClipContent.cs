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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.ResourceHelpers;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Editors.Timelines.Clips.Core;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editors.Controls.Timelines.Tracks.Clips {
    public class ImageClipContent : TimelineVideoClipContent {
        public new ImageVideoClip Model => (ImageVideoClip) base.Model;

        private static FormattedText ClipDisabledText;

        public ImageClipContent() {
        }

        protected override void OnConnected() {
            base.OnConnected();
            IResourcePathKey<ResourceImage> imgKey = this.Model.ResourceImageKey;
            imgKey.ResourceChanged += this.ResourceImageKeyOnResourceChanged;
            imgKey.OnlineStateChanged += this.ResourceImageKeyOnOnlineStateChanged;

            ScrollViewer scroller = this.ClipControl?.TimelineControl?.TimelineScrollViewer;
            if (scroller != null) {
                scroller.ScrollChanged += this.ScrollerOnScrollChanged;
            }
        }

        protected override void OnClipVisibilityChanged() {
            base.OnClipVisibilityChanged();
            this.InvalidateVisual();
        }

        private void ScrollerOnScrollChanged(object sender, ScrollChangedEventArgs e) {
            this.InvalidateVisual();
        }

        private void ResourceImageKeyOnResourceChanged(IResourcePathKey<ResourceImage> key, ResourceImage olditem, ResourceImage newitem) {
            if (olditem != null)
                olditem.ImageChanged -= this.OnImageChanged;
            if (newitem != null)
                newitem.ImageChanged += this.OnImageChanged;
            this.InvalidateVisual();
        }

        private void OnImageChanged(BaseResource resource) {
            ((ResourceImage) resource).Shared1 = true;
            this.InvalidateVisual();
        }

        private void ResourceImageKeyOnOnlineStateChanged(IResourcePathKey<ResourceImage> key) {
            this.InvalidateVisual();
        }

        protected override Size MeasureOverride(Size constraint) {
            return base.MeasureOverride(constraint);
        }

        protected override Size ArrangeOverride(Size arrangeBounds) {
            return base.ArrangeOverride(arrangeBounds);
        }

        protected override void OnRender(DrawingContext dc) {
            if (!this.IsClipVisible) {
                return;
            }

            Size renderSize = this.RenderSize;
            if (renderSize.Height < 1.0) {
                return;
            }

            ImageVideoClip clip = this.Model;

            IResourcePathKey<ResourceImage> imgKey = clip.ResourceImageKey;
            if (!imgKey.TryGetResource(out ResourceImage img) || img.image == null) {
                return;
            }

            SKBitmap bmp = img.bitmap;
            WriteableBitmap bitmap = (WriteableBitmap) img.Shared0;
            if (bitmap == null || bitmap.PixelWidth != bmp.Width || bitmap.PixelHeight != bmp.Height) {
                img.Shared0 = bitmap = new WriteableBitmap(bmp.Width, bmp.Height, 96, 96, PixelFormats.Pbgra32, null);
                img.Shared1 = true;
            }

            if (img.Shared1) {
                img.Shared1 = false;
                bitmap.WritePixels(new Int32Rect(0, 0, bmp.Width, bmp.Height), bmp.GetPixels(), bmp.ByteCount, bmp.RowBytes, 0, 0);
            }

            Rect2d clipRect = Rect2d.Floor(renderSize.Width, renderSize.Height);
            Rect2d imgRect = new Rect2d(bmp.Width, bmp.Height).ResizeToHeight(clipRect.Height);

            // this introduces slight glitching when rendering, mostly for the last few drawn images
            const double gap = 20;
            double segment = imgRect.Width + gap;
            ScrollViewer scroller = this.ClipControl?.TimelineControl?.TimelineScrollViewer;
            Rect visible = UIUtils.GetVisibleRect(scroller, this);
            int i = (int) Math.Floor(visible.Left / segment);
            int j = (int) Math.Ceiling(visible.Right / segment);
            // int i = 0;
            // int j = (int) Math.Ceiling(renderSize.Width / segment);

            do {
                if (i > j) {
                    break;
                }

                Rect rect = new Rect(Math.Floor(i * segment), 0, imgRect.Width, clipRect.Height);
                if (rect.Right > renderSize.Width) {
                    dc.PushClip(new RectangleGeometry(new Rect(new Point(), renderSize)));
                    dc.DrawImage(bitmap, rect);
                    dc.Pop();
                    break;
                }
                else {
                    dc.DrawImage(bitmap, rect);
                }

                i++;
            } while (true);
        }
    }
}