using System;
using System.Diagnostics;
using System.Numerics;
using FramePFX.Automation;
using FramePFX.Automation.ViewModels.Keyframe;
using FramePFX.Editor.Timelines.Effects.Video;
using FramePFX.Editor.Timelines.Effects.ViewModels.Video;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Editor.ViewModels.Timelines.Clips;
using FramePFX.Utils;

namespace FramePFX.Editor.Timelines.Effects.ViewModels {
    public class MotionEffectViewModel : VideoEffectViewModel {
        public new MotionEffect Model => (MotionEffect) ((BaseEffectViewModel) this).Model;

        public float MediaPositionX {
            get => this.MediaPosition.X;
            set => this.MediaPosition = new Vector2(value, this.MediaPosition.Y);
        }

        public float MediaPositionY {
            get => this.MediaPosition.Y;
            set => this.MediaPosition = new Vector2(this.MediaPosition.X, value);
        }

        /// <summary>
        /// The x and y coordinates of the video's media
        /// </summary>
        public Vector2 MediaPosition {
            get => this.Model.MediaPosition;
            set {
                this.ValidateNotInAutomationChange();
                TimelineViewModel timeline = this.Timeline;
                if (AutomationUtils.CanAddKeyFrame(timeline, this, MotionEffect.MediaPositionKey)) {
                    this.AutomationData[MotionEffect.MediaPositionKey].GetActiveKeyFrameOrCreateNew(timeline.PlayHeadFrame - this.OwnerClip.FrameBegin).SetVector2Value(value);
                }
                else {
                    this.AutomationData[MotionEffect.MediaPositionKey].GetOverride().SetVector2Value(value);
                    this.AutomationData[MotionEffect.MediaPositionKey].RaiseOverrideValueChanged();
                }
            }
        }

        public float MediaScaleX {
            get => this.MediaScale.X;
            set => this.MediaScale = new Vector2(value, this.MediaScale.Y);
        }

        public float MediaScaleY {
            get => this.MediaScale.Y;
            set => this.MediaScale = new Vector2(this.MediaScale.X, value);
        }

        /// <summary>
        /// The x and y scale of the video's media (relative to <see cref="MediaScaleOrigin"/>)
        /// </summary>
        public Vector2 MediaScale {
            get => this.Model.MediaScale;
            set {
                this.ValidateNotInAutomationChange();
                TimelineViewModel timeline = this.Timeline;
                if (AutomationUtils.CanAddKeyFrame(timeline, this, MotionEffect.MediaScaleKey)) {
                    this.AutomationData[MotionEffect.MediaScaleKey].GetActiveKeyFrameOrCreateNew(timeline.PlayHeadFrame - this.OwnerClip.FrameBegin).SetVector2Value(value);
                }
                else {
                    this.AutomationData[MotionEffect.MediaScaleKey].GetOverride().SetVector2Value(value);
                    this.AutomationData[MotionEffect.MediaScaleKey].RaiseOverrideValueChanged();
                }
            }
        }

        public float MediaScaleOriginX {
            get => this.MediaScaleOrigin.X;
            set => this.MediaScaleOrigin = new Vector2(value, this.MediaScaleOrigin.Y);
        }

        public float MediaScaleOriginY {
            get => this.MediaScaleOrigin.Y;
            set => this.MediaScaleOrigin = new Vector2(this.MediaScaleOrigin.X, value);
        }

        /// <summary>
        /// The scaling origin point of this video's media. Default value is 0.5,0.5 (the center of the frame)
        /// </summary>
        public Vector2 MediaScaleOrigin {
            get => this.Model.MediaScaleOrigin;
            set {
                this.ValidateNotInAutomationChange();
                TimelineViewModel timeline = this.Timeline;
                if (AutomationUtils.CanAddKeyFrame(timeline, this, MotionEffect.MediaScaleOriginKey)) {
                    this.AutomationData[MotionEffect.MediaScaleOriginKey].GetActiveKeyFrameOrCreateNew(timeline.PlayHeadFrame - this.OwnerClip.FrameBegin).SetVector2Value(value);
                }
                else {
                    this.AutomationData[MotionEffect.MediaScaleOriginKey].GetOverride().SetVector2Value(value);
                    this.AutomationData[MotionEffect.MediaScaleOriginKey].RaiseOverrideValueChanged();
                }
            }
        }

        public AutomationSequenceViewModel MediaPositionAutomationSequence => this.AutomationData[MotionEffect.MediaPositionKey];
        public AutomationSequenceViewModel MediaScaleAutomationSequence => this.AutomationData[MotionEffect.MediaScaleKey];
        public AutomationSequenceViewModel MediaScaleOriginAutomationSequence => this.AutomationData[MotionEffect.MediaScaleOriginKey];

        private static readonly RefreshAutomationValueEventHandler RefreshMediaPositionHandler = (s, e) => {
            MotionEffectViewModel effect = (MotionEffectViewModel) s.AutomationData.Owner;
            effect.RaisePropertyChanged(nameof(effect.MediaPosition));
            effect.RaisePropertyChanged(nameof(effect.MediaPositionX));
            effect.RaisePropertyChanged(nameof(effect.MediaPositionY));
            effect.InvalidateRenderForAutomationRefresh(in e);
        };

        private static readonly RefreshAutomationValueEventHandler RefreshMediaScaleHandler = (s, e) => {
            MotionEffectViewModel clip = (MotionEffectViewModel) s.AutomationData.Owner;
            clip.RaisePropertyChanged(nameof(clip.MediaScale));
            clip.RaisePropertyChanged(nameof(clip.MediaScaleX));
            clip.RaisePropertyChanged(nameof(clip.MediaScaleY));
            clip.InvalidateRenderForAutomationRefresh(in e);
        };

        private static readonly RefreshAutomationValueEventHandler RefreshMediaScaleOriginHandler = (s, e) => {
            MotionEffectViewModel clip = (MotionEffectViewModel) s.AutomationData.Owner;
            clip.RaisePropertyChanged(nameof(clip.MediaScaleOrigin));
            clip.RaisePropertyChanged(nameof(clip.MediaScaleOriginX));
            clip.RaisePropertyChanged(nameof(clip.MediaScaleOriginY));
            clip.InvalidateRenderForAutomationRefresh(in e);
        };

        public RelayCommand ResetTransformationCommand { get; }
        public RelayCommand ResetMediaPositionCommand { get; }
        public RelayCommand ResetMediaScaleCommand { get; }
        public RelayCommand ResetMediaScaleOriginCommand { get; }

        public RelayCommand InsertMediaPositionKeyFrameCommand { get; }
        public RelayCommand InsertMediaScaleKeyFrameCommand { get; }
        public RelayCommand InsertMediaScaleOriginKeyFrameCommand { get; }

        public RelayCommand ToggleMediaPositionActiveCommand { get; }
        public RelayCommand ToggleMediaScaleActiveCommand { get; }
        public RelayCommand ToggleMediaScaleOriginActiveCommand { get; }

        public long RelativePlayHead => this.OwnerClip.RelativePlayHead;

        public readonly Func<bool> CanInsertKeyFrame;

        public MotionEffectViewModel(VideoEffect model) : base(model) {
            this.CanInsertKeyFrame = () => ((VideoClipViewModel) this.OwnerClip)?.CanInsertKeyFrame?.Invoke() ?? false;
            this.ResetTransformationCommand = new RelayCommand(() => {
                this.MediaPosition = MotionEffect.MediaPositionKey.Descriptor.DefaultValue;
                this.MediaScale = MotionEffect.MediaScaleKey.Descriptor.DefaultValue;
                this.MediaScaleOrigin = MotionEffect.MediaScaleOriginKey.Descriptor.DefaultValue;
            });

            this.ResetMediaPositionCommand = new RelayCommand(() => this.MediaPosition = MotionEffect.MediaPositionKey.Descriptor.DefaultValue);
            this.ResetMediaScaleCommand = new RelayCommand(() => this.MediaScale = MotionEffect.MediaScaleKey.Descriptor.DefaultValue);
            this.ResetMediaScaleOriginCommand = new RelayCommand(() => this.MediaScaleOrigin = MotionEffect.MediaScaleOriginKey.Descriptor.DefaultValue);

            this.InsertMediaPositionKeyFrameCommand = new RelayCommand(() => this.AutomationData[MotionEffect.MediaPositionKey].GetActiveKeyFrameOrCreateNew(Math.Max(this.RelativePlayHead, 0)).SetVector2Value(this.MediaPosition), this.CanInsertKeyFrame);
            this.InsertMediaScaleKeyFrameCommand = new RelayCommand(() => this.AutomationData[MotionEffect.MediaScaleKey].GetActiveKeyFrameOrCreateNew(Math.Max(this.RelativePlayHead, 0)).SetVector2Value(this.MediaScale), this.CanInsertKeyFrame);
            this.InsertMediaScaleOriginKeyFrameCommand = new RelayCommand(() => this.AutomationData[MotionEffect.MediaScaleOriginKey].GetActiveKeyFrameOrCreateNew(this.RelativePlayHead).SetVector2Value(this.MediaScaleOrigin), this.CanInsertKeyFrame);

            this.ToggleMediaPositionActiveCommand = new RelayCommand(() => this.AutomationData[MotionEffect.MediaPositionKey].ToggleOverrideAction());
            this.ToggleMediaScaleActiveCommand = new RelayCommand(() => this.AutomationData[MotionEffect.MediaScaleKey].ToggleOverrideAction());
            this.ToggleMediaScaleOriginActiveCommand = new RelayCommand(() => this.AutomationData[MotionEffect.MediaScaleOriginKey].ToggleOverrideAction());


            this.AutomationData.AssignRefreshHandler(MotionEffect.MediaPositionKey, RefreshMediaPositionHandler);
            this.AutomationData.AssignRefreshHandler(MotionEffect.MediaScaleKey, RefreshMediaScaleHandler);
            this.AutomationData.AssignRefreshHandler(MotionEffect.MediaScaleOriginKey, RefreshMediaScaleOriginHandler);
        }

        [Conditional("DEBUG")]
        private void ValidateNotInAutomationChange() {
            if (this.IsAutomationRefreshInProgress) {
                Debugger.Break();
                throw new Exception("Cannot modify view-model parameter property while automation refresh is in progress. " +
                                    $"Only the model value should be modified, and {nameof(this.RaisePropertyChanged)} should be called in the view-model");
            }
        }
    }
}