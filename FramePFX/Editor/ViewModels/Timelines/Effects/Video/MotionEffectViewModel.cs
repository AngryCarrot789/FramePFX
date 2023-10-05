using System;
using System.Numerics;
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

        private static readonly RefreshAutomationValueEventHandler RefreshMediaRotationHandler = (s, e) => {
            MotionEffectViewModel vfx = (MotionEffectViewModel) s.AutomationData.Owner;
            vfx.RaisePropertyChanged(nameof(vfx.MediaRotation));
            vfx.InvalidateRenderForAutomationRefresh(in e);
        };

        private static readonly RefreshAutomationValueEventHandler RefreshMediaRotationOriginHandler = (s, e) => {
            MotionEffectViewModel vfx = (MotionEffectViewModel) s.AutomationData.Owner;
            vfx.RaisePropertyChanged(nameof(vfx.MediaRotationOrigin));
            vfx.RaisePropertyChanged(nameof(vfx.MediaRotationOriginX));
            vfx.RaisePropertyChanged(nameof(vfx.MediaRotationOriginY));
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
            set => AutomationUtils.GetKeyFrameForPropertyChanged(this, MotionEffect.MediaPositionKey).SetVector2Value(value);
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
            set => AutomationUtils.GetKeyFrameForPropertyChanged(this, MotionEffect.MediaScaleKey).SetVector2Value(value);
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
            set => AutomationUtils.GetKeyFrameForPropertyChanged(this, MotionEffect.MediaScaleOriginKey).SetVector2Value(value);
        }

        /// <summary>
        /// The clockwise rotation of the video's media (relative to <see cref="MediaScaleOrigin"/>)
        /// </summary>
        public double MediaRotation {
            get => this.Model.MediaRotation;
            set => AutomationUtils.GetKeyFrameForPropertyChanged(this, MotionEffect.MediaRotationKey).SetDoubleValue(value);
        }

        public float MediaRotationOriginX {
            get => this.MediaRotationOrigin.X;
            set => this.MediaRotationOrigin = new Vector2(value, this.MediaRotationOrigin.Y);
        }

        public float MediaRotationOriginY {
            get => this.MediaRotationOrigin.Y;
            set => this.MediaRotationOrigin = new Vector2(this.MediaRotationOrigin.X, value);
        }

        /// <summary>
        /// The scaling origin point of this video's media. Default value is 0.5,0.5 (the center of the frame)
        /// </summary>
        public Vector2 MediaRotationOrigin {
            get => this.Model.MediaRotationOrigin;
            set => AutomationUtils.GetKeyFrameForPropertyChanged(this, MotionEffect.MediaRotationOriginKey).SetVector2Value(value);
        }

        // binding helpers

        public AutomationSequenceViewModel MediaPositionAutomationSequence => this.AutomationData[MotionEffect.MediaPositionKey];
        public AutomationSequenceViewModel MediaScaleAutomationSequence => this.AutomationData[MotionEffect.MediaScaleKey];
        public AutomationSequenceViewModel MediaScaleOriginAutomationSequence => this.AutomationData[MotionEffect.MediaScaleOriginKey];

        public AutomationSequenceViewModel MediaRotationAutomationSequence => this.AutomationData[MotionEffect.MediaRotationKey];

        public AutomationSequenceViewModel MediaRotationOriginAutomationSequence => this.AutomationData[MotionEffect.MediaRotationOriginKey];

        public RelayCommand ResetTransformationCommand { get; }
        public RelayCommand ResetMediaPositionCommand { get; }
        public RelayCommand ResetMediaScaleCommand { get; }
        public RelayCommand ResetMediaScaleOriginCommand { get; }
        public RelayCommand ResetMediaRotationCommand { get; }
        public RelayCommand ResetMediaRotationOriginCommand { get; }

        public RelayCommand InsertMediaPositionKeyFrameCommand { get; }
        public RelayCommand InsertMediaScaleKeyFrameCommand { get; }
        public RelayCommand InsertMediaScaleOriginKeyFrameCommand { get; }
        public RelayCommand InsertMediaRotationKeyFrameCommand { get; }
        public RelayCommand InsertMediaRotationOriginKeyFrameCommand { get; }

        public RelayCommand ToggleMediaPositionActiveCommand { get; }
        public RelayCommand ToggleMediaScaleActiveCommand { get; }
        public RelayCommand ToggleMediaScaleOriginActiveCommand { get; }
        public RelayCommand ToggleMediaRotationActiveCommand { get; }
        public RelayCommand ToggleMediaRotationOriginActiveCommand { get; }

        private readonly FrameSeekedEventHandler handler1;
        private readonly ClipMovedOverPlayeHeadEventHandler handler2;
        private readonly PlayHeadLeaveClipEventHandler handler3;

        public MotionEffectViewModel(MotionEffect model) : base(model) {
            this.handler1 = (sender, frame, newFrame) => this.UpdateCommands();
            this.handler2 = (clip, frame) => this.UpdateCommands();
            this.handler3 = (clip, movement) => this.UpdateCommands();

            this.ResetTransformationCommand = new RelayCommand(() => {
                this.AutomationData[MotionEffect.MediaPositionKey].AssignDefaultValue();
                this.AutomationData[MotionEffect.MediaScaleKey].AssignDefaultValue();
                this.AutomationData[MotionEffect.MediaScaleOriginKey].AssignDefaultValue();
                this.AutomationData[MotionEffect.MediaRotationKey].AssignDefaultValue();
                this.AutomationData[MotionEffect.MediaRotationOriginKey].AssignDefaultValue();

                // this.MediaPosition = MotionEffect.MediaPositionKey.Descriptor.DefaultValue;
                // this.MediaScale = MotionEffect.MediaScaleKey.Descriptor.DefaultValue;
                // this.MediaScaleOrigin = MotionEffect.MediaScaleOriginKey.Descriptor.DefaultValue;
                // this.MediaRotation = MotionEffect.MediaRotationKey.Descriptor.DefaultValue;
                // this.MediaRotationOrigin = MotionEffect.MediaRotationOriginKey.Descriptor.DefaultValue;
            });

            this.ResetMediaPositionCommand = new RelayCommand(() => this.AutomationData[MotionEffect.MediaPositionKey].AssignDefaultValue());
            this.ResetMediaScaleCommand = new RelayCommand(() => this.AutomationData[MotionEffect.MediaScaleKey].AssignDefaultValue());
            this.ResetMediaScaleOriginCommand = new RelayCommand(() => this.AutomationData[MotionEffect.MediaScaleOriginKey].AssignDefaultValue());
            this.ResetMediaRotationCommand = new RelayCommand(() => this.AutomationData[MotionEffect.MediaRotationKey].AssignDefaultValue());
            this.ResetMediaRotationOriginCommand = new RelayCommand(() => this.AutomationData[MotionEffect.MediaRotationOriginKey].AssignDefaultValue());

            Func<bool> CanInsertKeyFrame = () => this.OwnerClip != null && this.OwnerClip.IsPlayHeadFrameInRange();
            this.InsertMediaPositionKeyFrameCommand = new RelayCommand(() => this.AutomationData[MotionEffect.MediaPositionKey].GetActiveKeyFrameOrCreateNew(Math.Max(this.OwnerClip.RelativePlayHead, 0)).SetVector2Value(this.MediaPosition), CanInsertKeyFrame);
            this.InsertMediaScaleKeyFrameCommand = new RelayCommand(() => this.AutomationData[MotionEffect.MediaScaleKey].GetActiveKeyFrameOrCreateNew(Math.Max(this.OwnerClip.RelativePlayHead, 0)).SetVector2Value(this.MediaScale), CanInsertKeyFrame);
            this.InsertMediaScaleOriginKeyFrameCommand = new RelayCommand(() => this.AutomationData[MotionEffect.MediaScaleOriginKey].GetActiveKeyFrameOrCreateNew(this.OwnerClip.RelativePlayHead).SetVector2Value(this.MediaScaleOrigin), CanInsertKeyFrame);
            this.InsertMediaRotationKeyFrameCommand = new RelayCommand(() => this.AutomationData[MotionEffect.MediaRotationKey].GetActiveKeyFrameOrCreateNew(Math.Max(this.OwnerClip.RelativePlayHead, 0)).SetDoubleValue(this.MediaRotation), CanInsertKeyFrame);
            this.InsertMediaRotationOriginKeyFrameCommand = new RelayCommand(() => this.AutomationData[MotionEffect.MediaRotationOriginKey].GetActiveKeyFrameOrCreateNew(this.OwnerClip.RelativePlayHead).SetVector2Value(this.MediaRotationOrigin), CanInsertKeyFrame);

            this.ToggleMediaPositionActiveCommand = new RelayCommand(() => this.AutomationData[MotionEffect.MediaPositionKey].ToggleOverrideAction());
            this.ToggleMediaScaleActiveCommand = new RelayCommand(() => this.AutomationData[MotionEffect.MediaScaleKey].ToggleOverrideAction());
            this.ToggleMediaScaleOriginActiveCommand = new RelayCommand(() => this.AutomationData[MotionEffect.MediaScaleOriginKey].ToggleOverrideAction());
            this.ToggleMediaRotationActiveCommand = new RelayCommand(() => this.AutomationData[MotionEffect.MediaRotationKey].ToggleOverrideAction());
            this.ToggleMediaRotationOriginActiveCommand = new RelayCommand(() => this.AutomationData[MotionEffect.MediaRotationOriginKey].ToggleOverrideAction());

            this.AutomationData.AssignRefreshHandler(MotionEffect.MediaPositionKey, RefreshMediaPositionHandler);
            this.AutomationData.AssignRefreshHandler(MotionEffect.MediaScaleKey, RefreshMediaScaleHandler);
            this.AutomationData.AssignRefreshHandler(MotionEffect.MediaScaleOriginKey, RefreshMediaScaleOriginHandler);
            this.AutomationData.AssignRefreshHandler(MotionEffect.MediaRotationKey, RefreshMediaRotationHandler);
            this.AutomationData.AssignRefreshHandler(MotionEffect.MediaRotationOriginKey, RefreshMediaRotationOriginHandler);
        }

        protected override void OnAddedToClip() {
            base.OnAddedToClip();
            this.OwnerClip.Model.FrameSeeked += this.handler1;
            this.OwnerClip.ClipMovedOverPlayHead += this.handler2;
            this.OwnerClip.PlayHeadLeaveClip += this.handler3;
            this.UpdateCommands();
        }

        protected override void OnRemovingFromClip() {
            base.OnRemovingFromClip();
            this.OwnerClip.Model.FrameSeeked -= this.handler1;
            this.OwnerClip.ClipMovedOverPlayHead -= this.handler2;
            this.OwnerClip.PlayHeadLeaveClip -= this.handler3;
            this.UpdateCommands();
        }

        private void UpdateCommands() {
            this.InsertMediaPositionKeyFrameCommand.RaiseCanExecuteChanged();
            this.InsertMediaScaleKeyFrameCommand.RaiseCanExecuteChanged();
            this.InsertMediaScaleOriginKeyFrameCommand.RaiseCanExecuteChanged();
            this.InsertMediaRotationKeyFrameCommand.RaiseCanExecuteChanged();
            this.InsertMediaRotationOriginKeyFrameCommand.RaiseCanExecuteChanged();
        }
    }
}