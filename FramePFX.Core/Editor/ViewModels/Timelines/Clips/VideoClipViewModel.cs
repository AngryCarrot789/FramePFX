using System;
using System.Diagnostics;
using System.Numerics;
using FramePFX.Core.Automation;
using FramePFX.Core.Automation.ViewModels.Keyframe;
using FramePFX.Core.Editor.Timelines;
using FramePFX.Core.Editor.Timelines.VideoClips;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels.Timelines.Clips
{
    // TODO: Maybe instead of using inheritance, instead, use composition?
    // Maybe using some sort of trait system, where a clip can have, for example, a
    // transformation trait (pos, scale, origin), video media trait, etc. Or maybe other
    // editors just call them "effects".

    // Premiere pro's "effect controls" contains the "Motion" effect, and it automatically adds
    // an instance that cannot be removed. Maybe that's better than inheritance? And to save/load
    // clips, traits/effects can be serialised and deserialised (using a factory) just like everything else

    // Vegas also does a similar thing but uses "Event Pan/Crop", which is an effect added
    // to clips by default (or at least added when you open the effect window by clicking the button on the clip)

    /// <summary>
    /// Base view model class for video clips that are placed on a video track
    /// </summary>
    public abstract class VideoClipViewModel : ClipViewModel
    {
        public new VideoClip Model => (VideoClip) base.Model;

        #region Media/Visual properties

        public float MediaPositionX
        {
            get => this.MediaPosition.X;
            set => this.MediaPosition = new Vector2(value, this.MediaPosition.Y);
        }

        public float MediaPositionY
        {
            get => this.MediaPosition.Y;
            set => this.MediaPosition = new Vector2(this.MediaPosition.X, value);
        }

        /// <summary>
        /// The x and y coordinates of the video's media
        /// </summary>
        public Vector2 MediaPosition
        {
            get => this.Model.MediaPosition;
            set
            {
                this.ValidateNotInAutomationChange();
                TimelineViewModel timeline = this.Timeline;
                if (TimelineUtilCore.CanAddKeyFrame(timeline, this, VideoClip.MediaPositionKey))
                {
                    this.AutomationData[VideoClip.MediaPositionKey].GetActiveKeyFrameOrCreateNew(timeline.PlayHeadFrame - this.FrameBegin).SetVector2Value(value);
                }
                else
                {
                    this.AutomationData[VideoClip.MediaPositionKey].GetOverride().SetVector2Value(value);
                    this.AutomationData[VideoClip.MediaPositionKey].RaiseOverrideValueChanged();
                }
            }
        }

        public float MediaScaleX
        {
            get => this.MediaScale.X;
            set => this.MediaScale = new Vector2(value, this.MediaScale.Y);
        }

        public float MediaScaleY
        {
            get => this.MediaScale.Y;
            set => this.MediaScale = new Vector2(this.MediaScale.X, value);
        }

        /// <summary>
        /// The x and y scale of the video's media (relative to <see cref="MediaScaleOrigin"/>)
        /// </summary>
        public Vector2 MediaScale
        {
            get => this.Model.MediaScale;
            set
            {
                this.ValidateNotInAutomationChange();
                TimelineViewModel timeline = this.Timeline;
                if (TimelineUtilCore.CanAddKeyFrame(timeline, this, VideoClip.MediaScaleKey))
                {
                    this.AutomationData[VideoClip.MediaScaleKey].GetActiveKeyFrameOrCreateNew(timeline.PlayHeadFrame - this.FrameBegin).SetVector2Value(value);
                }
                else
                {
                    this.AutomationData[VideoClip.MediaScaleKey].GetOverride().SetVector2Value(value);
                    this.AutomationData[VideoClip.MediaScaleKey].RaiseOverrideValueChanged();
                }
            }
        }

        public float MediaScaleOriginX
        {
            get => this.MediaScaleOrigin.X;
            set => this.MediaScaleOrigin = new Vector2(value, this.MediaScaleOrigin.Y);
        }

        public float MediaScaleOriginY
        {
            get => this.MediaScaleOrigin.Y;
            set => this.MediaScaleOrigin = new Vector2(this.MediaScaleOrigin.X, value);
        }

        /// <summary>
        /// The scaling origin point of this video's media. Default value is 0.5,0.5 (the center of the frame)
        /// </summary>
        public Vector2 MediaScaleOrigin
        {
            get => this.Model.MediaScaleOrigin;
            set
            {
                this.ValidateNotInAutomationChange();
                TimelineViewModel timeline = this.Timeline;
                if (TimelineUtilCore.CanAddKeyFrame(timeline, this, VideoClip.MediaScaleOriginKey))
                {
                    this.AutomationData[VideoClip.MediaScaleOriginKey].GetActiveKeyFrameOrCreateNew(timeline.PlayHeadFrame - this.FrameBegin).SetVector2Value(value);
                }
                else
                {
                    this.AutomationData[VideoClip.MediaScaleOriginKey].GetOverride().SetVector2Value(value);
                    this.AutomationData[VideoClip.MediaScaleOriginKey].RaiseOverrideValueChanged();
                }
            }
        }

        public double Opacity
        {
            get => this.Model.Opacity;
            set
            {
                this.ValidateNotInAutomationChange();
                TimelineViewModel timeline = this.Timeline;
                if (TimelineUtilCore.CanAddKeyFrame(timeline, this, VideoClip.OpacityKey))
                {
                    this.AutomationData[VideoClip.OpacityKey].GetActiveKeyFrameOrCreateNew(timeline.PlayHeadFrame - this.FrameBegin).SetDoubleValue(value);
                }
                else
                {
                    this.AutomationData[VideoClip.OpacityKey].GetOverride().SetDoubleValue(value);
                    this.AutomationData[VideoClip.OpacityKey].RaiseOverrideValueChanged();
                }
            }
        }

        #endregion

        public RelayCommand ResetTransformationCommand { get; }
        public RelayCommand ResetMediaPositionCommand { get; }
        public RelayCommand ResetMediaScaleCommand { get; }
        public RelayCommand ResetMediaScaleOriginCommand { get; }
        public RelayCommand ResetOpacityCommand { get; }

        public RelayCommand InsertMediaPositionKeyFrameCommand { get; }
        public RelayCommand InsertMediaScaleKeyFrameCommand { get; }
        public RelayCommand InsertMediaScaleOriginKeyFrameCommand { get; }
        public RelayCommand InsertOpacityKeyFrameCommand { get; }

        public RelayCommand ToggleMediaPositionActiveCommand { get; }
        public RelayCommand ToggleMediaScaleActiveCommand { get; }
        public RelayCommand ToggleMediaScaleOriginActiveCommand { get; }
        public RelayCommand ToggleOpacityActiveCommand { get; }

        // binding helpers
        public AutomationSequenceViewModel MediaPositionAutomationSequence => this.AutomationData[VideoClip.MediaPositionKey];
        public AutomationSequenceViewModel MediaScaleAutomationSequence => this.AutomationData[VideoClip.MediaScaleKey];
        public AutomationSequenceViewModel MediaScaleOriginAutomationSequence => this.AutomationData[VideoClip.MediaScaleOriginKey];
        public AutomationSequenceViewModel OpacityAutomationSequence => this.AutomationData[VideoClip.OpacityKey];

        private readonly ClipRenderInvalidatedEventHandler renderCallback;

        #region Cached refresh event handlers

        private static readonly RefreshAutomationValueEventHandler RefreshMediaPositionHandler = (s, e) =>
        {
            VideoClipViewModel clip = (VideoClipViewModel) s.AutomationData.Owner;
            clip.RaisePropertyChanged(nameof(clip.MediaPosition));
            clip.RaisePropertyChanged(nameof(clip.MediaPositionX));
            clip.RaisePropertyChanged(nameof(clip.MediaPositionY));
            clip.InvalidateRenderForAutomationRefresh(in e);
        };

        private static readonly RefreshAutomationValueEventHandler RefreshMediaScaleHandler = (s, e) =>
        {
            VideoClipViewModel clip = (VideoClipViewModel) s.AutomationData.Owner;
            clip.RaisePropertyChanged(nameof(clip.MediaScale));
            clip.RaisePropertyChanged(nameof(clip.MediaScaleX));
            clip.RaisePropertyChanged(nameof(clip.MediaScaleY));
            clip.InvalidateRenderForAutomationRefresh(in e);
        };

        private static readonly RefreshAutomationValueEventHandler RefreshMediaScaleOriginHandler = (s, e) =>
        {
            VideoClipViewModel clip = (VideoClipViewModel) s.AutomationData.Owner;
            clip.RaisePropertyChanged(nameof(clip.MediaScaleOrigin));
            clip.RaisePropertyChanged(nameof(clip.MediaScaleOriginX));
            clip.RaisePropertyChanged(nameof(clip.MediaScaleOriginY));
            clip.InvalidateRenderForAutomationRefresh(in e);
        };

        private static readonly RefreshAutomationValueEventHandler RefreshOpacityHandler = (s, e) =>
        {
            VideoClipViewModel clip = (VideoClipViewModel) s.AutomationData.Owner;
            clip.RaisePropertyChanged(nameof(clip.Opacity));
            clip.InvalidateRenderForAutomationRefresh(in e);
        };

        #endregion

        public readonly Func<bool> CanInsertKeyFrame;

        protected VideoClipViewModel(VideoClip model) : base(model)
        {
            this.CanInsertKeyFrame = () => this.Track != null && this.Model.GetRelativeFrame(this.Timeline.PlayHeadFrame, out long _);
            this.ResetTransformationCommand = new RelayCommand(() =>
            {
                this.MediaPosition = VideoClip.MediaPositionKey.Descriptor.DefaultValue;
                this.MediaScale = VideoClip.MediaScaleKey.Descriptor.DefaultValue;
                this.MediaScaleOrigin = VideoClip.MediaScaleOriginKey.Descriptor.DefaultValue;
            });

            this.ResetMediaPositionCommand = new RelayCommand(() => this.MediaPosition = VideoClip.MediaPositionKey.Descriptor.DefaultValue);
            this.ResetMediaScaleCommand = new RelayCommand(() => this.MediaScale = VideoClip.MediaScaleKey.Descriptor.DefaultValue);
            this.ResetMediaScaleOriginCommand = new RelayCommand(() => this.MediaScaleOrigin = VideoClip.MediaScaleOriginKey.Descriptor.DefaultValue);
            this.ResetOpacityCommand = new RelayCommand(() => this.Opacity = VideoClip.OpacityKey.Descriptor.DefaultValue);

            this.InsertMediaPositionKeyFrameCommand = new RelayCommand(() => this.AutomationData[VideoClip.MediaPositionKey].GetActiveKeyFrameOrCreateNew(Math.Max(this.RelativePlayHead, 0)).SetVector2Value(this.MediaPosition), this.CanInsertKeyFrame);
            this.InsertMediaScaleKeyFrameCommand = new RelayCommand(() => this.AutomationData[VideoClip.MediaScaleKey].GetActiveKeyFrameOrCreateNew(Math.Max(this.RelativePlayHead, 0)).SetVector2Value(this.MediaScale), this.CanInsertKeyFrame);
            this.InsertMediaScaleOriginKeyFrameCommand = new RelayCommand(() => this.AutomationData[VideoClip.MediaScaleOriginKey].GetActiveKeyFrameOrCreateNew(this.RelativePlayHead).SetVector2Value(this.MediaScaleOrigin), this.CanInsertKeyFrame);
            this.InsertOpacityKeyFrameCommand = new RelayCommand(() => this.AutomationData[VideoClip.OpacityKey].GetActiveKeyFrameOrCreateNew(Math.Max(this.RelativePlayHead, 0)).SetDoubleValue(this.Opacity), this.CanInsertKeyFrame);

            this.ToggleMediaPositionActiveCommand = new RelayCommand(() => this.AutomationData[VideoClip.MediaPositionKey].ToggleOverrideAction());
            this.ToggleMediaScaleActiveCommand = new RelayCommand(() => this.AutomationData[VideoClip.MediaScaleKey].ToggleOverrideAction());
            this.ToggleMediaScaleOriginActiveCommand = new RelayCommand(() => this.AutomationData[VideoClip.MediaScaleOriginKey].ToggleOverrideAction());
            this.ToggleOpacityActiveCommand = new RelayCommand(() => this.AutomationData[VideoClip.OpacityKey].ToggleOverrideAction());

            this.renderCallback = (x, s) =>
            {
                // assert ReferenceEquals(this.Model, x)
                this.OnInvalidateRender(s);
            };

            this.Model.RenderInvalidated += this.renderCallback;
            this.AutomationData.AssignRefreshHandler(VideoClip.MediaPositionKey, RefreshMediaPositionHandler);
            this.AutomationData.AssignRefreshHandler(VideoClip.MediaScaleKey, RefreshMediaScaleHandler);
            this.AutomationData.AssignRefreshHandler(VideoClip.MediaScaleOriginKey, RefreshMediaScaleOriginHandler);
            this.AutomationData.AssignRefreshHandler(VideoClip.OpacityKey, RefreshOpacityHandler);
        }

        // TODO: implement "OnPlayHeadEnter", "OnPlayHeadMoved", and "OnPlayHeadLeave" to refresh
        // the key frame insertion commands

        public override void OnUserSeekedFrame(long oldFrame, long newFrame)
        {
            base.OnUserSeekedFrame(oldFrame, newFrame);
            this.UpdateCommands();
        }

        public override void OnClipMovedToPlayeHeadFrame(long frame)
        {
            base.OnClipMovedToPlayeHeadFrame(frame);
            this.UpdateCommands();
        }

        public override void OnPlayHeadLeaveClip(bool isPlayheadLeaveClip)
        {
            base.OnPlayHeadLeaveClip(isPlayheadLeaveClip);
            this.UpdateCommands();
        }

        private void UpdateCommands()
        {
            this.InsertMediaPositionKeyFrameCommand.RaiseCanExecuteChanged();
            this.InsertMediaScaleKeyFrameCommand.RaiseCanExecuteChanged();
            this.InsertMediaScaleOriginKeyFrameCommand.RaiseCanExecuteChanged();
            this.InsertOpacityKeyFrameCommand.RaiseCanExecuteChanged();
        }

        public override void OnFrameSpanChanged(FrameSpan oldSpan)
        {
            base.OnFrameSpanChanged(oldSpan);
            this.Model.InvalidateRender();
        }

        protected override void OnMediaFrameOffsetChanged(long oldFrame, long newFrame)
        {
            base.OnMediaFrameOffsetChanged(oldFrame, newFrame);
            this.Model.InvalidateRender();
        }

        public virtual void OnInvalidateRender(bool schedule = true)
        {
            this.Track?.Timeline.DoRender(schedule);
        }

        protected override void DisposeCore(ExceptionStack stack)
        {
            base.DisposeCore(stack);
            this.Model.RenderInvalidated -= this.renderCallback;
        }

        protected void InvalidateRenderForAutomationRefresh(in RefreshAutomationValueEventArgs e)
        {
            if (!e.IsDuringPlayback && !e.IsPlaybackTick)
            {
                this.Model.InvalidateRender(true);
            }
        }

        [Conditional("DEBUG")]
        private void ValidateNotInAutomationChange()
        {
            if (this.IsAutomationRefreshInProgress)
            {
                Debugger.Break();
                throw new Exception("Cannot modify view-model parameter property while automation refresh is in progress. " +
                                    $"Only the model value should be modified, and {nameof(this.RaisePropertyChanged)} should be called in the view-model");
            }
        }
    }
}