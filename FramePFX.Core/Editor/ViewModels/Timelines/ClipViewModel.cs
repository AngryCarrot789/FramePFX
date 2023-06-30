using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Core.Automation;
using FramePFX.Core.Automation.ViewModels;
using FramePFX.Core.Editor.History;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.Timelines;
using FramePFX.Core.History;
using FramePFX.Core.History.Tasks;
using FramePFX.Core.History.ViewModels;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels.Timelines {
    /// <summary>
    /// The base view model for all types of clips (video, audio, etc)
    /// </summary>
    public abstract class ClipViewModel : BaseViewModel, IHistoryHolder, IAutomatableViewModel, IDisplayName, IAcceptResourceDrop, IClipDragHandler, IDisposable {
        protected readonly HistoryBuffer<HistoryClipDisplayName> displayNameHistory = new HistoryBuffer<HistoryClipDisplayName>();
        protected readonly HistoryBuffer<HistoryVideoClipPosition> clipPositionHistory = new HistoryBuffer<HistoryVideoClipPosition>();
        protected HistoryVideoClipPosition lastDragHistoryAction;

        /// <summary>
        /// Whether or not this clip's history is being changed, and therefore, no changes should be pushed to the history manager
        /// </summary>
        public bool IsHistoryChanging { get; set; }

        /// <summary>
        /// Whether or not this clip's parameter properties are being refreshed
        /// </summary>
        public bool IsAutomationRefreshInProgress { get; set; }

        public bool IsDraggingLeftThumb { get; private set; }
        public bool IsDraggingRightThumb { get; private set; }
        public bool IsDraggingClip { get; private set; }

        public bool IsDraggingAny => this.IsDraggingLeftThumb || this.IsDraggingRightThumb || this.IsDraggingClip;

        /// <summary>
        /// The clip's display/readable name, editable by a user
        /// </summary>
        public string DisplayName {
            get => this.Model.DisplayName;
            set {
                if (!this.IsHistoryChanging && this.Track != null) {
                    if (!this.displayNameHistory.TryGetAction(out HistoryClipDisplayName action))
                        this.displayNameHistory.PushAction(this.HistoryManager, action = new HistoryClipDisplayName(this), "Edit media duration");
                    action.DisplayName.SetCurrent(value);
                }

                this.Model.DisplayName = value;
                this.RaisePropertyChanged();
                this.Track?.OnProjectModified(this);
            }
        }

        /// <summary>
        /// The track this clip is located in
        /// </summary>
        private TrackViewModel track;
        public TrackViewModel Track {
            get => this.track;
            set {
                if (!ReferenceEquals(this.track, value)) {
                    this.RaisePropertyChanged(ref this.track, value);
                    this.RaisePropertyChanged(nameof(this.Timeline));
                    this.RaisePropertyChanged(nameof(this.Project));
                    this.RaisePropertyChanged(nameof(this.Editor));
                    this.RaisePropertyChanged(nameof(this.HistoryManager));
                }
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
                        this.clipPositionHistory.PushAction(this.HistoryManager, action = new HistoryVideoClipPosition(this), "Edit media pos/duration");
                    action.Span.SetCurrent(value);
                }

                this.Model.FrameSpan = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.FrameBegin));
                this.RaisePropertyChanged(nameof(this.FrameDuration));
                this.RaisePropertyChanged(nameof(this.FrameEndIndex));
                this.OnFrameSpanChanged(oldSpan, value);
                this.Track?.OnProjectModified(this);
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
                        this.clipPositionHistory.PushAction(this.HistoryManager, action = new HistoryVideoClipPosition(this), "Edit media pos/duration");
                    action.MediaFrameOffset.SetCurrent(value);
                }

                this.Model.MediaFrameOffset = value;
                this.RaisePropertyChanged();
                this.OnMediaFrameOffsetChanged(oldValue, value);
                this.Track?.OnProjectModified(this);
            }
        }

        public TimelineViewModel Timeline => this.Track?.Timeline;

        public ProjectViewModel Project => this.Track?.Timeline.Project;

        public VideoEditorViewModel Editor => this.Track?.Timeline.Project.Editor;

        public HistoryManagerViewModel HistoryManager => this.Track?.Timeline.Project.Editor.HistoryManager;

        public AutomationDataViewModel AutomationData { get; }

        public AutomationEngineViewModel AutomationEngine => this.Track?.Timeline.Project.AutomationEngine;

        public AsyncRelayCommand EditDisplayNameCommand { get; }

        public RelayCommand RemoveClipCommand { get; }

        public Clip Model { get; }

        IAutomatable IAutomatableViewModel.AutomationModel => this.Model;

        protected ClipViewModel(Clip model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.AutomationData = new AutomationDataViewModel(this, model.AutomationData);
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

        public static void SetTrack(ClipViewModel viewModel, TrackViewModel track, bool fireTrackChangedEvent = true) {
            Track oldTrack = viewModel.Model.Track;
            Track newTrack = track?.Model;
            if (!ReferenceEquals(oldTrack, newTrack)) {
                Clip.SetTrack(viewModel.Model, track?.Model, fireTrackChangedEvent);
            }

            viewModel.Track = track;
        }

        protected virtual void OnFrameSpanChanged(FrameSpan oldSpan, FrameSpan newSpan) {

        }

        protected virtual void OnMediaFrameOffsetChanged(long oldFrame, long newFrame) {

        }

        public void Dispose() {
            using (ExceptionStack stack = new ExceptionStack()) {
                try {
                    this.DisposeCore(stack);
                }
                catch (Exception e) {
                    stack.Add(new Exception(nameof(this.DisposeCore) + " method unexpectedly threw", e));
                }
            }
        }

        protected virtual void DisposeCore(ExceptionStack stack) {
            try {
                this.Model.Dispose();
            }
            catch (Exception e) {
                stack.Add(new Exception("Exception disposing model", e));
            }
        }

        public bool IntersectsFrameAt(long frame) => this.Model.IntersectsFrameAt(frame);

        public virtual void OnTimelinePlayBegin() {

        }

        public virtual void OnTimelinePlayEnd() {

        }

        public virtual bool CanDropResource(BaseResourceObjectViewModel resource) {
            return ReferenceEquals(resource.Manager, this.Track?.Timeline.Project.ResourceManager);
        }

        public virtual Task OnDropResource(BaseResourceObjectViewModel resource) {
            return IoC.MessageDialogs.ShowMessageAsync("Resource dropped", "This clip can't do anything with that resource!");
        }

        public virtual void OnLeftThumbDragStart() {
            if (this.IsDraggingLeftThumb)
                throw new Exception("Already dragging left thumb");
            this.IsDraggingLeftThumb = true;
            this.CreateClipDragHistoryAction();
        }

        public virtual void OnLeftThumbDragStop(bool cancelled) {
            if (!this.IsDraggingLeftThumb)
                throw new Exception("Not dragging left thumb");
            this.IsDraggingLeftThumb = false;
            this.PushClipDragHistoryAction(cancelled);
        }

        public virtual void OnRightThumbDragStart() {
            if (this.IsDraggingRightThumb)
                throw new Exception("Already dragging right thumb");
            this.IsDraggingRightThumb = true;
            this.CreateClipDragHistoryAction();
        }

        public virtual void OnRightThumbDragStop(bool cancelled) {
            if (!this.IsDraggingRightThumb)
                throw new Exception("Not dragging right thumb");
            this.IsDraggingRightThumb = false;
            this.PushClipDragHistoryAction(cancelled);
        }

        public virtual void OnDragStart() {
            if (this.IsDraggingClip)
                throw new Exception("Already dragging");
            this.IsDraggingClip = true;
            this.CreateClipDragHistoryAction();
            if (this.Timeline is TimelineViewModel timeline) {
                if (timeline.IsGloballyDragging) {
                    return;
                }

                List<ClipViewModel> selected = timeline.GetSelectedClips().ToList();
                if (selected.Count > 1) {
                    timeline.IsGloballyDragging = true;
                    timeline.DraggingClips = selected;
                    timeline.ProcessingDragEventClip = this;
                    foreach (ClipViewModel clip in selected) {
                        if (clip != this) {
                            clip.OnDragStart();
                        }
                    }

                    timeline.ProcessingDragEventClip = null;
                }
            }
        }

        public virtual void OnDragStop(bool cancelled) {
            if (!this.IsDraggingClip)
                throw new Exception("Not dragging");

            this.IsDraggingClip = false;
            if (this.Timeline is TimelineViewModel timeline && timeline.IsGloballyDragging) {
                if (timeline.ProcessingDragEventClip == null) {
                    timeline.DragStopHistoryList = new List<HistoryVideoClipPosition>();
                }

                if (cancelled) {
                    this.lastDragHistoryAction.Undo();
                }
                else {
                    timeline.DragStopHistoryList.Add(this.lastDragHistoryAction);
                }

                this.lastDragHistoryAction = null;
                if (timeline.ProcessingDragEventClip != null) {
                    return;
                }

                timeline.ProcessingDragEventClip = this;
                foreach (ClipViewModel clip in timeline.DraggingClips) {
                    if (this != clip) {
                        clip.OnDragStop(cancelled);
                    }
                }

                timeline.IsGloballyDragging = false;
                timeline.ProcessingDragEventClip = null;
                timeline.DraggingClips = null;
                timeline.Project.Editor.HistoryManager.AddAction(new MultiHistoryAction(new List<IHistoryAction>(timeline.DragStopHistoryList)));
                timeline.DragStopHistoryList = null;
            }
            else {
                this.PushClipDragHistoryAction(cancelled);
            }
        }

        private long addedOffset;

        public virtual void OnLeftThumbDelta(long offset) {
            if (this.Timeline == null) {
                return;
            }

            // limit frame begin such that media frame offset is always greater than or equal to 0
            // long temp = this.MediaFrameOffset + offset;
            // if (temp < 0) {
            //     offset += -temp;
            // }
            // if (offset == 0) {
            //     return;
            // }

            long newFrameBegin = this.FrameBegin + offset;

            if (newFrameBegin < 0) {
                offset += -newFrameBegin;
                newFrameBegin = 0;
            }

            long duration = this.FrameDuration - offset;
            if (duration < 1) {
                newFrameBegin += (duration - 1);
                duration = 1;
                if (newFrameBegin < 0) {
                    return;
                }
            }

            this.MediaFrameOffset += (newFrameBegin - this.FrameBegin);
            this.FrameSpan = new FrameSpan(newFrameBegin, duration);
            this.lastDragHistoryAction.Span.SetCurrent(this.FrameSpan);
        }

        public virtual void OnRightThumbDelta(long offset) {
            FrameSpan span = this.FrameSpan;
            long newEndIndex = Math.Max(span.EndIndex + offset, span.Begin + 1);
            if (this.Timeline is TimelineViewModel timeline) {
                if (newEndIndex > timeline.MaxDuration) {
                    timeline.MaxDuration = newEndIndex + 300;
                }
            }

            this.FrameSpan = span.WithEndIndex(newEndIndex);
            this.lastDragHistoryAction.Span.SetCurrent(this.FrameSpan);
        }

        public virtual void OnDragDelta(long offset) {
            FrameSpan span = this.FrameSpan;
            long begin = (span.Begin + offset) - this.addedOffset;
            this.addedOffset = 0L;
            if (begin < 0) {
                this.addedOffset = -begin;
                begin = 0;
            }

            long endIndex = begin + span.Duration;
            TimelineViewModel timeline = this.Timeline;
            if (timeline != null) {
                if (endIndex > timeline.MaxDuration) {
                    timeline.MaxDuration = endIndex + 300;
                }
            }

            this.FrameSpan = new FrameSpan(begin, span.Duration);

            if (timeline != null && timeline.IsGloballyDragging) {
                if (timeline.ProcessingDragEventClip == null) {
                    timeline.ProcessingDragEventClip = this;
                    foreach (ClipViewModel clip in timeline.DraggingClips) {
                        if (this != clip) {
                            clip.OnDragDelta(offset);
                        }
                    }

                    timeline.ProcessingDragEventClip = null;
                }
            }

            this.lastDragHistoryAction.Span.SetCurrent(this.FrameSpan);
        }

        public virtual void OnDragToTrack(int index) {
            if (!(this.Track is TrackViewModel track)) {
                return;
            }

            TimelineViewModel timeline = track.Timeline;
            if (timeline.IsGloballyDragging && timeline.IsAboutToDragAcrossTracks) {
                return;
            }

            int target = Maths.Clamp(index, 0, timeline.Tracks.Count - 1);
            TrackViewModel targetTrack = timeline.Tracks[target];
            if (ReferenceEquals(track, targetTrack)) {
                return;
            }

            if (!targetTrack.IsClipTypeAcceptable(this)) {
                return;
            }

            if (timeline.IsGloballyDragging) {
                if (timeline.DraggingClips.All(x => ReferenceEquals(x.Track, track) && track.IsClipTypeAcceptable(x))) {
                    timeline.IsAboutToDragAcrossTracks = true;
                    foreach (ClipViewModel clip in timeline.DraggingClips) {
                        timeline.MoveClip(clip, track, targetTrack);
                    }

                    timeline.IsAboutToDragAcrossTracks = false;
                }
                else {
                    return;
                }
            }
            else {
                timeline.MoveClip(this, track, targetTrack);
            }
        }

        protected void CreateClipDragHistoryAction() {
            if (this.lastDragHistoryAction != null) {
                throw new Exception("Drag history was non-null, which means a drag was started before another drag was completed");
            }

            this.lastDragHistoryAction = new HistoryVideoClipPosition(this);
        }

        protected void PushClipDragHistoryAction(bool cancelled) {
            // throws if this.lastDragHistoryAction is null. It should not be null if there's no bugs in the drag start/end calls
            if (cancelled) {
                this.lastDragHistoryAction.Undo();
            }
            else {
                this.HistoryManager?.AddAction(this.lastDragHistoryAction, "Drag clip");
            }

            this.lastDragHistoryAction = null;
        }

        protected virtual void RaiseAutomationPropertyUpdated(string propertyName, in RefreshAutomationValueEventArgs e) {
            this.RaisePropertyChanged(propertyName);
        }
    }
}