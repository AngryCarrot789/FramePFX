using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Automation;
using FramePFX.Automation.Events;
using FramePFX.Automation.ViewModels;
using FramePFX.Automation.ViewModels.Keyframe;
using FramePFX.Commands;
using FramePFX.Editor.History;
using FramePFX.Editor.Registries;
using FramePFX.Editor.Timelines;
using FramePFX.Editor.Timelines.Effects;
using FramePFX.Editor.ViewModels.Timelines.Dragging;
using FramePFX.Editor.ViewModels.Timelines.Effects;
using FramePFX.Editor.ViewModels.Timelines.Events;
using FramePFX.History;
using FramePFX.History.Tasks;
using FramePFX.History.ViewModels;
using FramePFX.Utils;
using FramePFX.Views.Dialogs.UserInputs;

namespace FramePFX.Editor.ViewModels.Timelines {
    /// <summary>
    /// The base view model for all types of clips (video, audio, etc)
    /// </summary>
    public abstract class ClipViewModel : BaseViewModel, IHistoryHolder, IAutomatableViewModel, IDisplayName, IProjectViewModelBound, IDisposable, IRenameTarget {
        protected readonly HistoryBuffer<HistoryVideoClipPosition> clipPositionHistory = new HistoryBuffer<HistoryVideoClipPosition>();
        private readonly ObservableCollection<BaseEffectViewModel> effects;
        private bool isSelected;

        public Clip Model { get; }

        /// <summary>
        /// The track this clip is located in
        /// </summary>
        public TrackViewModel Track { get; set; }

        /// <summary>
        /// Whether or not this clip's history is being changed, and therefore, no changes should be pushed to the history manager
        /// </summary>
        public bool IsHistoryChanging { get; set; }

        /// <summary>
        /// The clip's display/readable name, editable by a user
        /// </summary>
        public string DisplayName {
            get => this.Model.DisplayName;
            set {
                this.Model.DisplayName = value;
                this.RaisePropertyChanged();
                this.Track?.OnProjectModified();
            }
        }

        public FrameSpan FrameSpan {
            get => this.Model.FrameSpan;
            set {
                FrameSpan oldSpan = this.FrameSpan;
                if (oldSpan == value) {
                    return;
                }

                if (!this.IsHistoryChanging && !this.IsDraggingAny && this.Track != null) {
                    if (!this.clipPositionHistory.TryGetAction(out HistoryVideoClipPosition action))
                        this.clipPositionHistory.PushAction(HistoryManagerViewModel.Instance, action = new HistoryVideoClipPosition(this), "Edit media pos/duration");
                    action.Span.SetCurrent(value);
                }

                this.Model.FrameSpan = value;
                this.OnFrameSpanChanged(oldSpan);
            }
        }

        public long FrameBegin {
            get => this.FrameSpan.Begin;
            set => this.FrameSpan = this.FrameSpan.WithBegin(value);
        }

        public long FrameDuration {
            get => this.FrameSpan.Duration;
            set => this.FrameSpan = this.FrameSpan.WithDuration(value);
        }

        public long FrameEndIndex {
            get => this.FrameSpan.EndIndex;
            set => this.FrameSpan = this.FrameSpan.WithEndIndex(value);
        }

        public long MediaFrameOffset {
            get => this.Model.MediaFrameOffset;
            set {
                long oldValue = this.MediaFrameOffset;
                if (oldValue == value) {
                    return;
                }

                if (!this.IsHistoryChanging && !this.IsDraggingAny && this.Track != null) {
                    if (!this.clipPositionHistory.TryGetAction(out HistoryVideoClipPosition action))
                        this.clipPositionHistory.PushAction(HistoryManagerViewModel.Instance, action = new HistoryVideoClipPosition(this), "Edit media pos/duration");
                    action.MediaFrameOffset.SetCurrent(value);
                }

                this.Model.MediaFrameOffset = value;
                this.OnMediaFrameOffsetChanged(oldValue, value);
            }
        }

        /// <summary>
        /// Returns the parented timeline's play head, relative to this clip's <see cref="FrameBegin"/>. If the
        /// clip's begin is at 300 and the play head is at 325, this property returns 25
        /// </summary>
        public long RelativePlayHead {
            get {
                if (this.Track != null) {
                    return this.Track.Timeline.PlayHeadFrame - this.FrameBegin;
                }
                else {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Whether or not this clip is selected in the UI
        /// </summary>
        public bool IsSelected {
            get => this.isSelected;
            set => this.RaisePropertyChanged(ref this.isSelected, value);
        }

        /// <summary>
        /// The timeline associated with this clip. Clips should not really be modified when not inside a track
        /// </summary>
        public TimelineViewModel Timeline => this.Track?.Timeline;

        public ProjectViewModel Project => this.Track?.Timeline?.Project;

        public VideoEditorViewModel Editor => this.Project?.Editor;

        #region Automation

        public AutomationDataViewModel AutomationData { get; }

        public AutomationEngineViewModel AutomationEngine => this.Project?.AutomationEngine;

        IAutomatable IAutomatableViewModel.AutomationModel => this.Model;

        /// <summary>
        /// The automation sequence (either owned by the clip or an effect that this clip has) that is currently active
        /// </summary>
        public AutomationSequenceViewModel ActiveClipOrEffectSequence { get; private set; }

        /// <summary>
        /// Whether or not this clip's parameter properties are being refreshed
        /// </summary>
        public bool IsAutomationRefreshInProgress { get; set; }

        #endregion

        #region Drag Helpers

        public bool IsDraggingLeftThumb;
        public bool IsDraggingRightThumb;
        public bool IsDraggingClip;
        public bool IsDraggingAny => this.IsDraggingLeftThumb || this.IsDraggingRightThumb || this.IsDraggingClip;
        public ClipDragOperation drag;

        #endregion

        public AsyncRelayCommand EditDisplayNameCommand { get; }

        public RelayCommand RemoveClipCommand { get; }

        /// <summary>
        /// This clip's effects
        /// </summary>
        public ReadOnlyObservableCollection<BaseEffectViewModel> Effects { get; }

        public long LastSeekedFrame;

        /// <summary>
        /// Called when a
        /// </summary>
        public event ClipMovedOverPlayeHeadEventHandler ClipMovedOverPlayHead;

        /// <summary>
        /// An event fired when the user explicitly when the user seeks a frame that falls outside of the range of this clip.
        /// This may be fired during playback, but won't be called if the playback automatically leaves a clip
        /// </summary>
        public event PlayHeadLeaveClipEventHandler PlayHeadLeaveClip;

        protected ClipViewModel(Clip model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.AutomationData = new AutomationDataViewModel(this, model.AutomationData);
            this.AutomationData.ActiveSequenceChanged += this.AutomationDataOnActiveSequenceChanged;
            this.AutomationData.SetActiveSequenceFromModelDeserialisation();
            this.effects = new ObservableCollection<BaseEffectViewModel>();
            this.Effects = new ReadOnlyObservableCollection<BaseEffectViewModel>(this.effects);
            foreach (BaseEffect fx in model.Effects) {
                this.AddEffect(EffectRegistry.Instance.CreateViewModelFromModel(fx), false);
            }

            this.EditDisplayNameCommand = new AsyncRelayCommand(async () => {
                string name = await IoC.UserInput.ShowSingleInputDialogAsync("Input a new name", "Input a new display name for this clip", this.DisplayName);
                if (name != null) {
                    this.DisplayName = name;
                }
            });

            this.RemoveClipCommand = new RelayCommand(() => {
                this.Track?.DisposeAndRemoveItemsAction(new List<ClipViewModel>() {this});
            });
        }

        private void AutomationDataOnActiveSequenceChanged(AutomationDataViewModel sender, ActiveSequenceChangedEventArgs e) {
            this.SetActiveAutomationSequence(e.Sequence, false);
        }

        public void AddEffect(BaseEffectViewModel effect, bool addToModel = true) {
            this.InsertEffect(effect, this.Effects.Count, addToModel);
            if (effect.AutomationData.ActiveSequence != null) {
                this.SetActiveAutomationSequence(effect.AutomationData.ActiveSequence, true);
            }
        }

        public void InsertEffect(BaseEffectViewModel effect, int index, bool addToModel = true) {
            if (this.Effects.Contains(effect))
                throw new Exception("This clip already contains the effect");
            if (effect.OwnerClip != null)
                throw new Exception("Effect exists in another clip");

            if (addToModel)
                this.Model.InsertEffect(effect.Model, index);

            effect.OwnerClip = this;
            this.effects.Add(effect);
            effect.OnAddedToClip();
        }

        public bool RemoveEffect(BaseEffectViewModel effect, bool removeFromModel = true) {
            int index = this.Effects.IndexOf(effect);
            if (index < 0)
                return false;
            this.RemoveEffectAt(index, removeFromModel);
            return true;
        }

        public void RemoveEffectAt(int index, bool removeFromModel = true) {
            BaseEffectViewModel effect = this.Effects[index];
            if (effect.OwnerClip != this)
                throw new Exception("Internal error: effect is in our effect list but the effect's owner is not us");

            if (removeFromModel)
                this.Model.RemoveEffectAt(index);

            this.effects.Remove(effect);
            effect.OnRemovedFromClip();
            effect.OwnerClip = null;
        }

        // [ShortcutTarget("Application/RenameItem")]
        // public async Task OnShortcutActivated() {
        //     await Task.Run(async () => {
        //         await IoC.MessageDialogs.ShowDialogAsync("t", "ttt");
        //     });
        // }

        public virtual void OnFrameSpanChanged(FrameSpan oldSpan) {
            this.RaisePropertyChanged();
            this.RaisePropertyChanged(nameof(this.FrameBegin));
            this.RaisePropertyChanged(nameof(this.FrameDuration));
            this.RaisePropertyChanged(nameof(this.FrameEndIndex));
            this.RaisePropertyChanged(nameof(this.RelativePlayHead));
            if (this.Track != null) {
                this.Track.OnProjectModified();
                long frame = this.Track.Timeline.PlayHeadFrame;
                if (this.LastSeekedFrame != -1) {
                    if (!this.IntersectsFrameAt(frame)) {
                        this.OnPlayHeadLeaveClip(false);
                        this.LastSeekedFrame = -1;
                    }
                }
                else if (this.IntersectsFrameAt(frame)) {
                    this.LastSeekedFrame = frame;
                    this.OnClipMovedToPlayeHeadFrame(frame);
                }
            }
        }

        protected virtual void OnMediaFrameOffsetChanged(long oldFrame, long newFrame) {
            this.RaisePropertyChanged();
            this.Track?.OnProjectModified();
        }

        public void Dispose() {
            using (ErrorList stack = new ErrorList()) {
                try {
                    this.DisposeCore(stack);
                }
                catch (Exception e) {
                    stack.Add(new Exception(nameof(this.DisposeCore) + " method unexpectedly threw", e));
                }
            }
        }

        protected virtual void DisposeCore(ErrorList stack) {
            for (int i = this.effects.Count - 1; i >= 0; i--) {
                try {
                    this.RemoveEffectAt(i);
                }
                catch (Exception e) {
                    stack.Add(new Exception("Failed to remove effect", e));
                }
            }

            try {
                this.Model.Dispose();
            }
            catch (Exception e) {
                stack.Add(new Exception("Exception disposing model", e));
            }
        }

        public bool IntersectsFrameAt(long frame) => this.Model.IntersectsFrameAt(frame);

        /// <summary>
        /// Called when the user seeks a specific frame, and it intersects with this clip. The frame is relative to this clip's begin frame
        /// </summary>
        /// <param name="oldFrame">Previous frame (not relative to this clip)</param>
        /// <param name="newFrame">Current frame (relative to this clip)</param>
        public virtual void OnUserSeekedFrame(long oldFrame, long newFrame) {
            this.Model.OnFrameSeeked(oldFrame, newFrame);
        }

        /// <summary>
        /// Called when the user moves this clip such that it intersects with the play head
        /// </summary>
        /// <param name="frame">The play head frame, relative to this clip</param>
        public virtual void OnClipMovedToPlayeHeadFrame(long frame) {
            this.ClipMovedOverPlayHead?.Invoke(this, frame);
        }

        /// <summary>
        /// Called when the user seeks a frame that falls outside of the range of this clip
        /// </summary>
        /// <param name="isCausedByPlayHeadMovement">True when this is caused by the user moving the playhead, false when this is caused by the user moving the clip around</param>
        public virtual void OnPlayHeadLeaveClip(bool isCausedByPlayHeadMovement) {
            this.PlayHeadLeaveClip?.Invoke(this, isCausedByPlayHeadMovement);
        }

        private List<ClipViewModel> GetSelectedIncludingThis() {
            List<ClipViewModel> list = this.Timeline.GetSelectedClips().ToList();
            if (!list.Contains(this))
                list.Add(this);
            return list;
        }

        public void OnLeftThumbDragStart() {
            this.OnDragStart();
            this.IsDraggingLeftThumb = true;
        }

        public void OnRightThumbDragStart() {
            this.OnDragStart();
            this.IsDraggingRightThumb = true;
        }

        public void OnDragStart() {
            if (this.drag != null)
                throw new Exception("Drag already in progress");
            if (this.Timeline == null)
                throw new Exception("No timeline available");
            this.drag = ClipDragOperation.ForClip(this);
            this.drag.OnBegin();
            this.IsDraggingClip = true;
        }

        public void OnLeftThumbDragStop(bool cancelled) {
            this.OnDragStop(cancelled);
            this.IsDraggingLeftThumb = false;
        }

        public void OnRightThumbDragStop(bool cancelled) {
            this.OnDragStop(cancelled);
            this.IsDraggingRightThumb = false;
        }

        public void OnDragStop(bool cancelled) {
            if (this.drag != null) {
                this.drag.OnFinished(cancelled);
                this.drag = null;
            }

            this.IsDraggingClip = false;
        }

        public void OnLeftThumbDelta(long offset) {
            ClipDragOperation operation = this.drag;
            if (operation == null || offset == 0) {
                return;
            }

            foreach (ClipDragHandleInfo handle in operation.clips) {
                long newFrameBegin = handle.clip.FrameBegin + offset;
                if (newFrameBegin < 0) {
                    offset += -newFrameBegin;
                    newFrameBegin = 0;
                }

                long duration = handle.clip.FrameDuration - offset;
                if (duration < 1) {
                    newFrameBegin += (duration - 1);
                    duration = 1;
                    if (newFrameBegin < 0) {
                        continue;
                    }
                }

                handle.clip.MediaFrameOffset += (newFrameBegin - handle.clip.FrameBegin);
                handle.clip.FrameSpan = new FrameSpan(newFrameBegin, duration);
            }
        }

        public void OnRightThumbDelta(long offset) {
            ClipDragOperation operation = this.drag;
            if (operation == null || offset == 0) {
                return;
            }

            foreach (ClipDragHandleInfo handle in operation.clips) {
                FrameSpan span = handle.clip.FrameSpan;
                long newEndIndex = Math.Max(span.EndIndex + offset, span.Begin + 1);
                if (newEndIndex > operation.timeline.MaxDuration) {
                    operation.timeline.MaxDuration = newEndIndex + 300;
                }

                handle.clip.FrameSpan = span.WithEndIndex(newEndIndex);
            }
        }

        public void OnDragDelta(long offset) {
            ClipDragOperation operation = this.drag;
            if (operation == null || offset == 0) {
                return;
            }

            foreach (ClipDragHandleInfo handle in operation.clips) {
                FrameSpan span = handle.clip.FrameSpan;
                long begin = (span.Begin + offset) - handle.accumulator;
                handle.accumulator = 0L;
                if (begin < 0) {
                    handle.accumulator = -begin;
                    begin = 0;
                }

                long endIndex = begin + span.Duration;
                if (operation.timeline != null) {
                    if (endIndex > operation.timeline.MaxDuration) {
                        operation.timeline.MaxDuration = endIndex + 300;
                    }
                }

                handle.clip.FrameSpan = new FrameSpan(begin, span.Duration);
            }
        }

        public void OnDragToTrack(int index) {
            this.drag?.OnDragToTrack(index);
        }

        public static void RaiseTrackChanged(ClipViewModel clip) {
            clip.RaisePropertyChanged(nameof(Track));
            clip.RaisePropertyChanged(nameof(Timeline));
            clip.RaisePropertyChanged(nameof(Project));
            clip.RaisePropertyChanged(nameof(Editor));
            clip.RaisePropertyChanged(nameof(RelativePlayHead));
        }

        public async Task<bool> RenameAsync() {
            string result = await IoC.UserInput.ShowSingleInputDialogAsync("Rename clip", "Input a new clip name:", this.DisplayName ?? "", Validators.ForNonWhiteSpaceString());
            if (result != null) {
                this.DisplayName = result;
                return true;
            }

            return false;
        }

        private bool isUpdatingAutomationSequence;

        public void SetActiveAutomationSequence(AutomationSequenceViewModel sequence, bool isFromEffect) {
            if (this.isUpdatingAutomationSequence || ReferenceEquals(this.ActiveClipOrEffectSequence, sequence)) {
                return;
            }

            this.isUpdatingAutomationSequence = true;
            this.AutomationData.ActiveSequence = null;
            foreach (BaseEffectViewModel fx in this.effects) {
                if (!ReferenceEquals(fx.AutomationData.ActiveSequence, sequence))
                    fx.AutomationData.ActiveSequence = null;
            }

            this.ActiveClipOrEffectSequence = sequence;
            this.RaisePropertyChanged(nameof(this.ActiveClipOrEffectSequence));
            this.isUpdatingAutomationSequence = false;
        }
    }
}