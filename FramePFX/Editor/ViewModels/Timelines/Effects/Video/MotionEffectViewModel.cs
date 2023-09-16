using System;
using System.Diagnostics;
using System.Numerics;
using FramePFX.Automation;
using FramePFX.Automation.Events;
using FramePFX.Automation.ViewModels.Keyframe;
using FramePFX.Commands;
using FramePFX.Editor.Timelines.Effects.Video;
using FramePFX.Editor.Timelines.Events;
using FramePFX.Editor.ViewModels.Timelines.Events;
using FramePFX.Utils;

namespace FramePFX.Editor.ViewModels.Timelines.Effects.Video {
    public class MotionEffectViewModel : VideoEffectViewModel {
        private static readonly RefreshAutomationValueEventHandler RefreshMediaPositionHandler = (s, e) => {
            MotionEffectViewModel vfx = (MotionEffectViewModel) s.AutomationData.Owner;
            vfx.RaisePropertyChanged(nameof(vfx.MediaPosition));
            vfx.RaisePropertyChanged(nameof(vfx.MediaPositionX));
            vfx.RaisePropertyChanged(nameof(vfx.MediaPositionY));
            vfx.InvalidateRenderForAutomationRefresh(in e);
        };

        private static readonly RefreshAutomationValueEventHandler RefreshMediaScaleHandler = (s, e) => {
            MotionEffectViewModel vfx = (MotionEffectViewModel) s.AutomationData.Owner;
            vfx.RaisePropertyChanged(nameof(vfx.MediaScale));
            vfx.RaisePropertyChanged(nameof(vfx.MediaScaleX));
            vfx.RaisePropertyChanged(nameof(vfx.MediaScaleY));
            vfx.InvalidateRenderForAutomationRefresh(in e);
        };

        private static readonly RefreshAutomationValueEventHandler RefreshMediaScaleOriginHandler = (s, e) => {
            MotionEffectViewModel vfx = (MotionEffectViewModel) s.AutomationData.Owner;
            vfx.RaisePropertyChanged(nameof(vfx.MediaScaleOrigin));
            vfx.RaisePropertyChanged(nameof(vfx.MediaScaleOriginX));
            vfx.RaisePropertyChanged(nameof(vfx.MediaScaleOriginY));
            vfx.InvalidateRenderForAutomationRefresh(in e);
        };

        public new MotionEffect Model => (MotionEffect) base.Model;

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

        // binding helpers

        public AutomationSequenceViewModel MediaPositionAutomationSequence => this.AutomationData[MotionEffect.MediaPositionKey];
        public AutomationSequenceViewModel MediaScaleAutomationSequence => this.AutomationData[MotionEffect.MediaScaleKey];
        public AutomationSequenceViewModel MediaScaleOriginAutomationSequence => this.AutomationData[MotionEffect.MediaScaleOriginKey];

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

        private readonly FrameSeekedEventHandler handler1;
        private readonly ClipMovedOverPlayeHeadEventHandler handler2;
        private readonly PlayHeadLeaveClipEventHandler handler3;

        public MotionEffectViewModel(MotionEffect model) : base(model) {
            this.handler1 = (sender, frame, newFrame) => this.UpdateCommands();
            this.handler2 = (clip, frame) => this.UpdateCommands();
            this.handler3 = (clip, movement) => this.UpdateCommands();

            this.ResetTransformationCommand = new RelayCommand(() => {
                this.MediaPosition = MotionEffect.MediaPositionKey.Descriptor.DefaultValue;
                this.MediaScale = MotionEffect.MediaScaleKey.Descriptor.DefaultValue;
                this.MediaScaleOrigin = MotionEffect.MediaScaleOriginKey.Descriptor.DefaultValue;
            });

            this.ResetMediaPositionCommand = new RelayCommand(() => this.MediaPosition = MotionEffect.MediaPositionKey.Descriptor.DefaultValue);
            this.ResetMediaScaleCommand = new RelayCommand(() => this.MediaScale = MotionEffect.MediaScaleKey.Descriptor.DefaultValue);
            this.ResetMediaScaleOriginCommand = new RelayCommand(() => this.MediaScaleOrigin = MotionEffect.MediaScaleOriginKey.Descriptor.DefaultValue);

            Func<bool> CanInsertKeyFrame = () => this.OwnerClip != null && this.OwnerClip.CanInsertKeyFrame();
            this.InsertMediaPositionKeyFrameCommand = new RelayCommand(() => this.AutomationData[MotionEffect.MediaPositionKey].GetActiveKeyFrameOrCreateNew(Math.Max(this.OwnerClip.RelativePlayHead, 0)).SetVector2Value(this.MediaPosition), CanInsertKeyFrame);
            this.InsertMediaScaleKeyFrameCommand = new RelayCommand(() => this.AutomationData[MotionEffect.MediaScaleKey].GetActiveKeyFrameOrCreateNew(Math.Max(this.OwnerClip.RelativePlayHead, 0)).SetVector2Value(this.MediaScale), CanInsertKeyFrame);
            this.InsertMediaScaleOriginKeyFrameCommand = new RelayCommand(() => this.AutomationData[MotionEffect.MediaScaleOriginKey].GetActiveKeyFrameOrCreateNew(this.OwnerClip.RelativePlayHead).SetVector2Value(this.MediaScaleOrigin), CanInsertKeyFrame);

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

        public override void OnAddedToClip() {
            base.OnAddedToClip();
            this.OwnerClip.Model.FrameSeeked += this.handler1;
            this.OwnerClip.ClipMovedOverPlayHead += this.handler2;
            this.OwnerClip.PlayHeadLeaveClip += this.handler3;
            this.UpdateCommands();
        }

        public override void OnRemovedFromClip() {
            base.OnRemovedFromClip();
            this.OwnerClip.Model.FrameSeeked -= this.handler1;
            this.OwnerClip.ClipMovedOverPlayHead -= this.handler2;
            this.OwnerClip.PlayHeadLeaveClip -= this.handler3;

            // probably not necessary
            this.UpdateCommands();
        }

        private void UpdateCommands() {
            this.InsertMediaPositionKeyFrameCommand.RaiseCanExecuteChanged();
            this.InsertMediaScaleKeyFrameCommand.RaiseCanExecuteChanged();
            this.InsertMediaScaleOriginKeyFrameCommand.RaiseCanExecuteChanged();
        }
    }
}