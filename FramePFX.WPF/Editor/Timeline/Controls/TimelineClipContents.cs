using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Editor.ViewModels.Timelines.AudioClips;
using FramePFX.Editor.ViewModels.Timelines.VideoClips;
using FramePFX.Utils;

namespace FramePFX.WPF.Editor.Timeline.Controls {
    /// <summary>
    /// A basic control that draws the bottom of a clip based on the <see cref="FrameworkElement.DataContext"/>
    /// </summary>
    public class TimelineClipContents : Border {
        private static readonly DependencyPropertyChangedEventHandler DataContextChangedHandler;

        public static readonly DependencyProperty IsClipRenderingEnabledProperty = DependencyProperty.Register("IsClipRenderingEnabled", typeof(bool), typeof(TimelineClipContents), new FrameworkPropertyMetadata(BoolBox.True, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool IsClipRenderingEnabled {
            get => (bool) this.GetValue(IsClipRenderingEnabledProperty);
            set => this.SetValue(IsClipRenderingEnabledProperty, value.Box());
        }

        private const int ROLE_NONE = 0;
        private const int ROLE_VIDEO_IMAGE = 1;
        private const int ROLE_VIDEO_MEDIA = 2;
        private const int ROLE_VIDEO_COMPOSITION = 3;

        private const int ROLE_AUDIO_WAVEFORM = 4;
        // private const string CachedDataKey = nameof(TimelineClipContents) + "_CACHED_DATA_KEY";

        private int role;
        private IDisposable cached;

        private static readonly FormattedText DisabledText;

        public TimelineClipContents() {
            this.DataContextChanged += DataContextChangedHandler;
        }

        static TimelineClipContents() {
            DisabledText = new FormattedText("Disabled", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Consolas"), 15, Brushes.DimGray, 96);
            DataContextChangedHandler = (s, e) => {
                if (!ReferenceEquals(e.OldValue, e.NewValue))
                    ((TimelineClipContents) s).OnDataContextChanged(e.OldValue as ClipViewModel, e.NewValue as ClipViewModel);
            };
        }

        private void OnDataContextChanged(ClipViewModel oldClip, ClipViewModel newClip) {
            this.cached?.Dispose();
            switch (newClip) {
                case ImageVideoClipViewModel clip:
                    this.role = ROLE_VIDEO_IMAGE;
                    this.cached = new CachedImageData(clip);
                    break;
                case AVMediaVideoClipViewModel clip:
                    this.role = ROLE_VIDEO_MEDIA;
                    this.cached = new CachedVideoData(clip);
                    break;
                case CompositionVideoClipViewModel clip:
                    this.role = ROLE_VIDEO_COMPOSITION;
                    this.cached = new CachedCompositionData(clip);
                    break;
                case AudioClipViewModel clip:
                    this.role = ROLE_AUDIO_WAVEFORM;
                    this.cached = new CachedAudioData(clip);
                    break;
                default:
                    this.role = ROLE_NONE;
                    this.cached = null;
                    break;
            }
        }

        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);
            if (!(this.DataContext is ClipViewModel clip))
                return;

            bool isDisabled = !clip.Model.IsRenderingEnabled;
            if (isDisabled)
                dc.PushOpacity(0.75);

            if (this.role > 0) {
                if (this.cached == null)
                    throw new Exception("Invalid state: cached data was null");

                switch (this.role) {
                    case ROLE_VIDEO_IMAGE:
                        RenderImage((ImageVideoClipViewModel) clip, (CachedImageData) this.cached, dc);
                        break;
                    case ROLE_VIDEO_MEDIA:
                        RenderVideo((AVMediaVideoClipViewModel) clip, (CachedVideoData) this.cached, dc);
                        break;
                    case ROLE_VIDEO_COMPOSITION:
                        RenderComposition((CompositionVideoClipViewModel) clip, (CachedCompositionData) this.cached, dc);
                        break;
                    case ROLE_AUDIO_WAVEFORM:
                        RenderAudio((AudioClipViewModel) clip, (CachedAudioData) this.cached, dc);
                        break;
                }
            }

            if (isDisabled) {
                dc.Pop();
                System.Windows.Rect rect = new System.Windows.Rect(new Point(), this.RenderSize);
                dc.DrawRectangle(Brushes.LightGray, null, rect);
                dc.PushClip(new RectangleGeometry(rect));
                dc.DrawText(DisabledText, new Point((this.ActualWidth / 2d) - (DisabledText.Width / 2d), (this.ActualHeight / 2d) - (DisabledText.Height / 2d)));
                dc.Pop();
            }
        }

        private static void RenderImage(ImageVideoClipViewModel clip, CachedImageData data, DrawingContext dc) {
        }

        private static void RenderVideo(AVMediaVideoClipViewModel clip, CachedVideoData data, DrawingContext dc) {
        }

        private static void RenderComposition(CompositionVideoClipViewModel clip, CachedCompositionData data, DrawingContext dc) {
        }

        private static void RenderAudio(AudioClipViewModel clip, CachedAudioData data, DrawingContext dc) {
        }

        private class CachedImageData : IDisposable {
            private readonly ImageVideoClipViewModel clip;

            public CachedImageData(ImageVideoClipViewModel newClip) {
                this.clip = newClip;
            }

            public void Dispose() {
            }
        }

        private class CachedVideoData : IDisposable {
            private readonly AVMediaVideoClipViewModel clip;

            public CachedVideoData(AVMediaVideoClipViewModel newClip) {
                this.clip = newClip;
            }

            public void Dispose() {
            }
        }

        private class CachedCompositionData : IDisposable {
            private readonly CompositionVideoClipViewModel clip;

            public CachedCompositionData(CompositionVideoClipViewModel newClip) {
                this.clip = newClip;
            }

            public void Dispose() {
            }
        }

        private class CachedAudioData : IDisposable {
            private readonly AudioClipViewModel clip;

            public CachedAudioData(AudioClipViewModel newClip) {
                this.clip = newClip;
            }

            public void Dispose() {
            }
        }
    }
}