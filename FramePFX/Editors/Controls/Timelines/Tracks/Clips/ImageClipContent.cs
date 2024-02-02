using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.ResourceHelpers;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editors.Controls.Timelines.Tracks.Clips {
    public class ImageClipContent : TimelineClipContent {
        private WriteableBitmap bitmap;
        private bool arePixelsDirty;

        public new ImageVideoClip Model => (ImageVideoClip) base.Model;

        public ImageClipContent() {
        }

        protected override void OnConnected() {
            base.OnConnected();
            this.arePixelsDirty = true;
            IResourcePathKey<ResourceImage> imgKey = this.Model.ResourceImageKey;
            imgKey.ResourceChanged += this.ResourceImageKeyOnResourceChanged;
            imgKey.OnlineStateChanged += this.ResourceImageKeyOnOnlineStateChanged;

            ScrollViewer scroller = this.ClipControl?.TimelineControl?.TimelineScrollViewer;
            if (scroller != null) {
                scroller.ScrollChanged += this.ScrollerOnScrollChanged;
            }
        }

        private void ScrollerOnScrollChanged(object sender, ScrollChangedEventArgs e) {
            this.InvalidateVisual();
        }

        private void ResourceImageKeyOnResourceChanged(IResourcePathKey<ResourceImage> key, ResourceImage olditem, ResourceImage newitem) {
            if (olditem != null)
                olditem.ImageChanged -= this.OnImageChanged;
            if (newitem != null)
                newitem.ImageChanged += this.OnImageChanged;
            this.OnImageChanged(newitem);
        }

        private void OnImageChanged(BaseResource resource) {
            this.arePixelsDirty = true;
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

            Rect2d clipRect = Rect2d.Floor(renderSize.Width, renderSize.Height);
            Rect2d imgRect = new Rect2d(bmp.Width, bmp.Height).ResizeToHeight(clipRect.Height);
            if (this.bitmap == null || this.bitmap.PixelWidth != bmp.Width || this.bitmap.PixelHeight != bmp.Height) {
                this.bitmap = new WriteableBitmap(bmp.Width, bmp.Height, 96, 96, PixelFormats.Pbgra32, null);
                this.arePixelsDirty = true;
            }

            if (this.arePixelsDirty) {
                this.bitmap.WritePixels(new Int32Rect(0, 0, bmp.Width, bmp.Height), bmp.GetPixels(), bmp.ByteCount, bmp.RowBytes, 0, 0);
                this.arePixelsDirty = false;
            }

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
                    dc.DrawImage(this.bitmap, rect);
                    dc.Pop();
                    break;
                }
                else {
                    dc.DrawImage(this.bitmap, rect);
                }

                i++;
            } while (true);
        }
    }
}