using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.Timelines.ResourceHelpers;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Editor.ViewModels.Timelines.AudioClips;
using FramePFX.Editor.ViewModels.Timelines.VideoClips;
using FramePFX.Utils;
using FramePFX.WPF.Utils;
using SkiaSharp;
using Rect = System.Windows.Rect;

namespace FramePFX.WPF.Editor.Timeline.Controls {
    /// <summary>
    /// A basic control that draws the bottom of a clip based on the <see cref="FrameworkElement.DataContext"/>
    /// </summary>
    public class TimelineClipContents : Border {
        private static readonly DependencyPropertyChangedEventHandler DataContextChangedHandler;
        private static readonly RoutedEventHandler UnloadedEventHandler;
        private static readonly RoutedEventHandler LoadedEventHandler;

        public static readonly DependencyProperty IsClipRenderingEnabledProperty = DependencyProperty.Register("IsClipRenderingEnabled", typeof(bool), typeof(TimelineClipContents), new FrameworkPropertyMetadata(BoolBox.True, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool IsClipRenderingEnabled {
            get => (bool) this.GetValue(IsClipRenderingEnabledProperty);
            set => this.SetValue(IsClipRenderingEnabledProperty, value.Box());
        }

        // private const string CachedDataKey = nameof(TimelineClipContents) + "_CACHED_DATA_KEY";
        private BaseCachedData cached;
        public ScrollViewer scroller;

        private static readonly FormattedText DisabledText;

        public TimelineClipContents() {
            this.DataContextChanged += DataContextChangedHandler;
            this.Unloaded += UnloadedEventHandler;
            this.Loaded += LoadedEventHandler;
        }

        static TimelineClipContents() {
            DisabledText = new FormattedText("Disabled", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Consolas"), 15, Brushes.DimGray, 96);
            DataContextChangedHandler = (s, e) => {
                if (!ReferenceEquals(e.OldValue, e.NewValue))
                    ((TimelineClipContents) s).OnDataContextChanged(e.OldValue as ClipViewModel, e.NewValue as ClipViewModel);
            };

            UnloadedEventHandler = (s, e) => ((TimelineClipContents) s).OnUnloadedCore();
            LoadedEventHandler = (s, e) => ((TimelineClipContents) s).OnLoadedCore();
        }

        private void OnUnloadedCore() {
            this.cached?.Dispose();
            this.cached = null;

            if (this.scroller != null) {
                this.scroller.SizeChanged -= this.OnScrollerOnSizeChanged;
                this.scroller.ScrollChanged -= this.OnScrollerOnScrollChanged;
            }
        }

        private void OnLoadedCore() {
            if (this.cached == null && this.DataContext is ClipViewModel clip) {
                this.SetupCachedData(clip);
            }

            this.scroller = VisualTreeUtils.GetParent<ScrollViewer>(this);
            if (this.scroller != null) {
                this.scroller.SizeChanged += this.OnScrollerOnSizeChanged;
                this.scroller.ScrollChanged += this.OnScrollerOnScrollChanged;
            }
        }

        private void OnScrollerOnSizeChanged(object o, SizeChangedEventArgs e) {
            this.InvalidateVisual();
        }

        private void OnScrollerOnScrollChanged(object o, ScrollChangedEventArgs e) {
            this.InvalidateVisual();
        }

        private void OnDataContextChanged(ClipViewModel oldClip, ClipViewModel newClip) {
            this.SetupCachedData(newClip);
        }

        [SuppressMessage("ReSharper", "UseSwitchCasePatternVariable")]
        private void SetupCachedData(ClipViewModel clip) {
            this.cached?.Dispose();
            switch (clip) {
                case ImageVideoClipViewModel _:
                    this.cached = new CachedImageData(this, (ImageVideoClipViewModel) clip);
                    break;
                case AVMediaVideoClipViewModel _:
                    this.cached = new CachedVideoData(this, (AVMediaVideoClipViewModel) clip);
                    break;
                case CompositionVideoClipViewModel _:
                    this.cached = new CachedCompositionData(this, (CompositionVideoClipViewModel) clip);
                    break;
                case AudioClipViewModel _:
                    this.cached = new CachedAudioData(this, (AudioClipViewModel) clip);
                    break;
                default:
                    this.cached = null;
                    break;
            }
        }

        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);
            BaseCachedData cache = this.cached;
            ClipViewModel clip = cache != null ? cache.BaseClip : this.DataContext as ClipViewModel;
            bool isDisabled = clip != null && !clip.Model.IsRenderingEnabled;
            if (isDisabled) {
                dc.PushOpacity(0.75);
            }

            cache?.Render(dc);
            if (isDisabled) {
                dc.Pop();
                Rect rect = new Rect(new Point(), this.RenderSize);
                dc.DrawRectangle(Brushes.LightGray, null, rect);
                dc.PushClip(new RectangleGeometry(rect));
                dc.DrawText(DisabledText, new Point((this.ActualWidth / 2d) - (DisabledText.Width / 2d), (this.ActualHeight / 2d) - (DisabledText.Height / 2d)));
                dc.Pop();
            }
        }

        private abstract class BaseCachedData {
            public readonly TimelineClipContents control;
            public readonly ClipViewModel BaseClip;

            protected BaseCachedData(TimelineClipContents control, ClipViewModel clip) {
                this.control = control;
                this.BaseClip = clip;
            }

            public abstract void Render(DrawingContext dc);

            public abstract void Dispose();
        }

        private class CachedImageData : BaseCachedData {
            public readonly ImageVideoClipViewModel clip;
            public WriteableBitmap bitmap;
            public bool HasImageChanged;

            public CachedImageData(TimelineClipContents control, ImageVideoClipViewModel clip) : base(control, clip) {
                this.clip = clip;
                IResourcePathKey<ResourceImage> imgKey = clip.Model.ResourceImageKey;
                imgKey.ResourceChanged += this.ResourceImageKeyOnResourceChanged;
                imgKey.ResourceDataModified += this.ResourceImageKeyOnResourceDataModified;
                imgKey.OnlineStateChanged += this.ResourceImageKeyOnOnlineStateChanged;
                this.HasImageChanged = true;
            }

            private void ResourceImageKeyOnOnlineStateChanged(IResourcePathKey<ResourceImage> key) {
                // image most likely hasn't changed just by switching the online state of the resource, so it's fine
                // this.HasImageChanged = true;
                this.control.InvalidateVisual();
            }

            private void ResourceImageKeyOnResourceDataModified(IResourcePathKey<ResourceImage> key, ResourceImage resource, string property) {
                if (property == nameof(ResourceImage.bitmap)) {
                    this.HasImageChanged = true;
                    this.control.InvalidateVisual();
                }
            }

            private void ResourceImageKeyOnResourceChanged(IResourcePathKey<ResourceImage> key, ResourceImage olditem, ResourceImage newitem) {
                this.HasImageChanged = true;
                this.control.InvalidateVisual();
            }

            public override void Dispose() {
                IResourcePathKey<ResourceImage> imgKey = this.clip.Model.ResourceImageKey;
                imgKey.ResourceChanged -= this.ResourceImageKeyOnResourceChanged;
                imgKey.ResourceDataModified -= this.ResourceImageKeyOnResourceDataModified;
                imgKey.OnlineStateChanged -= this.ResourceImageKeyOnOnlineStateChanged;
                this.bitmap = null;
            }

            public override void Render(DrawingContext dc) {
                IResourcePathKey<ResourceImage> imgKey = this.clip.Model.ResourceImageKey;
                if (imgKey.TryGetResource(out ResourceImage img) && img.image != null) {
                    SKBitmap bmp = img.bitmap;
                    Size renderSize = this.control.RenderSize;
                    double clipWidth = renderSize.Width;
                    Rect2d clipRect = Rect2d.Floor(clipWidth, renderSize.Height);
                    Rect2d imgRect = new Rect2d(bmp.Width, bmp.Height).ResizeToHeight(clipRect.Height);
                    if (this.bitmap == null || this.bitmap.PixelWidth != bmp.Width || this.bitmap.PixelHeight != bmp.Height) {
                        this.bitmap = new WriteableBitmap(bmp.Width, bmp.Height, 96, 96, PixelFormats.Pbgra32, null);
                    }

                    if (this.HasImageChanged) {
                        this.bitmap.WritePixels(new Int32Rect(0, 0, bmp.Width, bmp.Height), bmp.GetPixels(), bmp.ByteCount, bmp.RowBytes, 0, 0);
                        this.HasImageChanged = false;
                    }

                    // this introduces slight glitching when rendering, mostly along
                    // the left and right edges of the last few drawn images
                    const double gap = 10;
                    double segment = imgRect.Width + gap;
                    Rect visible = UIUtils.GetVisibleRect(this.control.scroller, this.control);
                    int i = (int) Math.Floor(visible.Left / segment);
                    int j = (int) Math.Ceiling(visible.Right / segment);

                    do {
                        if (i > j) {
                            break;
                        }

                        Rect rect = new Rect(Math.Floor(i * segment), 0, imgRect.Width, clipRect.Height);
                        if (rect.Right > clipWidth) {
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

        private class CachedVideoData : BaseCachedData {
            private readonly AVMediaVideoClipViewModel clip;

            public CachedVideoData(TimelineClipContents control, AVMediaVideoClipViewModel clip) : base(control, clip) {
                this.clip = clip;
            }

            public override void Dispose() {
            }

            public override void Render(DrawingContext dc) {
            }
        }

        private class CachedCompositionData : BaseCachedData {
            private readonly CompositionVideoClipViewModel clip;

            public CachedCompositionData(TimelineClipContents control, CompositionVideoClipViewModel clip) : base(control, clip) {
                this.clip = clip;
            }

            public override void Dispose() {
            }

            public override void Render(DrawingContext dc) {
            }
        }

        private class CachedAudioData : BaseCachedData {
            private readonly AudioClipViewModel clip;

            public CachedAudioData(TimelineClipContents control, AudioClipViewModel clip) : base(control, clip) {
                this.clip = clip;
            }

            public override void Dispose() {
            }

            public override void Render(DrawingContext dc) {
            }
        }
    }
}