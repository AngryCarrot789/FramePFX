using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Core.Editor.History;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.Timeline;
using FramePFX.Core.History;
using FramePFX.Core.History.Tasks;
using FramePFX.Core.History.ViewModels;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels.Timeline {
    /// <summary>
    /// The base view model for all types of clips (video, audio, etc)
    /// </summary>
    public abstract class ClipViewModel : BaseViewModel, IHistoryHolder, IDisplayName, IDropClipResource, IClipDragHandler, IDisposable {
        protected readonly HistoryBuffer<HistoryClipDisplayName> displayNameHistory = new HistoryBuffer<HistoryClipDisplayName>();
        protected readonly HistoryBuffer<HistoryVideoClipPosition> clipPositionHistory = new HistoryBuffer<HistoryVideoClipPosition>();
        protected HistoryVideoClipPosition lastDragHistoryAction;

        public bool IsHistoryChanging { get; set; }

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
                if (!this.IsHistoryChanging && this.Layer != null) {
                    if (!this.displayNameHistory.TryGetAction(out HistoryClipDisplayName action))
                        this.displayNameHistory.PushAction(this.HistoryManager, action = new HistoryClipDisplayName(this), "Edit media duration");
                    action.DisplayName.SetCurrent(value);
                }

                this.Model.DisplayName = value;
                this.RaisePropertyChanged();
                this.Layer?.OnProjectModified(this);
            }
        }

        /// <summary>
        /// The layer this clip is located in
        /// </summary>
        private LayerViewModel layer;
        public LayerViewModel Layer {
            get => this.layer;
            set {
                if (!ReferenceEquals(this.layer, value)) {
                    this.RaisePropertyChanged(ref this.layer, value);
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
                FrameSpan oldSpan = this.Model.FrameSpan;
                if (oldSpan != value) {
                    if (!this.IsHistoryChanging && !this.IsDraggingAny && this.Layer != null) {
                        if (!this.clipPositionHistory.TryGetAction(out HistoryVideoClipPosition action))
                            this.clipPositionHistory.PushAction(this.HistoryManager, action = new HistoryVideoClipPosition(this), "Edit media pos/duration");
                        action.Span.SetCurrent(value);
                    }

                    this.Model.FrameSpan = value;
                    this.RaisePropertyChanged(nameof(this.FrameBegin));
                    this.RaisePropertyChanged(nameof(this.FrameDuration));
                    this.RaisePropertyChanged(nameof(this.FrameEndIndex));
                    this.OnFrameSpanChanged(oldSpan, value);
                    this.Layer?.OnProjectModified(this);
                }
            }
        }

        public long FrameBegin {
            get => this.FrameSpan.Begin;
            set => this.FrameSpan = this.FrameSpan.SetBegin(value);
        }

        public long FrameDuration {
            get => this.FrameSpan.Duration;
            set => this.FrameSpan = this.FrameSpan.SetDuration(value);
        }

        public long FrameEndIndex {
            get => this.FrameSpan.EndIndex;
            set => this.FrameSpan = this.FrameSpan.SetEndIndex(value);
        }

        /// <summary>
        /// The number of frames that are skipped relative to <see cref="ClipStart"/>. This will be positive if the
        /// left grip of the clip is dragged to the right, and will be 0 when dragged to the left
        /// <para>
        /// Alternative name: MediaBegin
        /// </para>
        /// </summary>
        public long MediaFrameOffset {
            get => this.Model.MediaFrameOffset;
            set {
                long oldValue = this.Model.MediaFrameOffset;
                if (oldValue != value) {
                    if (!this.IsHistoryChanging && !this.IsDraggingAny && this.Layer != null) {
                        if (!this.clipPositionHistory.TryGetAction(out HistoryVideoClipPosition action))
                            this.clipPositionHistory.PushAction(this.HistoryManager, action = new HistoryVideoClipPosition(this), "Edit media pos/duration");
                        action.MediaFrameOffset.SetCurrent(value);
                    }

                    this.Model.MediaFrameOffset = value;
                    this.RaisePropertyChanged();
                    this.OnMediaFrameOffsetChanged(oldValue, value);
                    this.Layer?.OnProjectModified(this);
                }
            }
        }

        public TimelineViewModel Timeline => this.Layer?.Timeline;

        public ProjectViewModel Project => this.Layer?.Timeline.Project;

        public VideoEditorViewModel Editor => this.Layer?.Timeline.Project.Editor;

        public HistoryManagerViewModel HistoryManager => this.Layer?.Timeline.Project.Editor.HistoryManager;

        public AsyncRelayCommand EditDisplayNameCommand { get; }

        public RelayCommand RemoveClipCommand { get; }

        public ClipModel Model { get; }

        protected ClipViewModel(ClipModel model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.EditDisplayNameCommand = new AsyncRelayCommand(async () => {
                string name = await IoC.UserInput.ShowSingleInputDialogAsync("Input a new name", "Input a new display name for this clip", this.DisplayName);
                if (name != null) {
                    this.DisplayName = name;
                }
            });

            this.RemoveClipCommand = new RelayCommand(() => {
                this.Layer?.DisposeAndRemoveItemsAction(new List<ClipViewModel>() {this});
            });
        }

        public static void SetLayer(ClipViewModel viewModel, LayerViewModel layer, bool fireLayerChangedEvent = true) {
            LayerModel oldLayer = viewModel.Model.Layer;
            LayerModel newLayer = layer?.Model;
            if (!ReferenceEquals(oldLayer, newLayer)) {
                ClipModel.SetLayer(viewModel.Model, layer?.Model, fireLayerChangedEvent);
            }

            viewModel.Layer = layer;
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
                    stack.Push(new Exception(nameof(this.DisposeCore) + " method unexpectedly threw", e));
                }
            }
        }

        protected virtual void DisposeCore(ExceptionStack stack) {
            try {
                this.Model.Dispose();
            }
            catch (Exception e) {
                stack.Push(new Exception("Exception disposing model", e));
            }
        }

        public bool IntersectsFrameAt(long frame) => this.Model.IntersectsFrameAt(frame);

        public virtual void OnTimelinePlayBegin() {

        }

        public virtual void OnTimelinePlayEnd() {

        }

        public virtual bool CanDropResource(ResourceItem resource) {
            return ReferenceEquals(resource.Manager, this.Layer?.Timeline.Project.ResourceManager.Model);
        }

        public virtual Task OnDropResource(ResourceItem resource) {
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

            long begin = this.FrameBegin + offset;
            if (begin < 0) {
                offset += -begin;
                begin = 0;
            }

            long duration = this.FrameDuration - offset;
            if (duration < 1) {
                begin += (duration - 1);
                duration = 1;
                if (begin < 0) {
                    return;
                }
            }

            this.FrameSpan = new FrameSpan(begin, duration);
            this.lastDragHistoryAction.Span.SetCurrent(this.FrameSpan);
        }

        public virtual void OnRightThumbDelta(long offset) {
            if (!(this.Timeline is TimelineViewModel timeline)) {
                return;
            }

            FrameSpan span = this.FrameSpan;
            long newEndIndex = Math.Max(span.EndIndex + offset, span.Begin + 1);
            if (newEndIndex > timeline.MaxDuration) {
                timeline.MaxDuration = newEndIndex + 300;
            }

            this.FrameSpan = span.SetEndIndex(newEndIndex);
            this.lastDragHistoryAction.Span.SetCurrent(this.FrameSpan);
        }

        public virtual void OnDragDelta(long offset) {
            if (!(this.Timeline is TimelineViewModel timeline)) {
                return;
            }

            FrameSpan span = this.FrameSpan;
            long begin = (span.Begin + offset) - this.addedOffset;
            this.addedOffset = 0L;
            if (begin < 0) {
                this.addedOffset = -begin;
                begin = 0;
            }

            long endIndex = begin + span.Duration;
            if (endIndex > timeline.MaxDuration) {
                timeline.MaxDuration = endIndex + 300;
            }

            this.FrameSpan = new FrameSpan(begin, span.Duration);

            if (timeline.IsGloballyDragging) {
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

        public virtual void OnDragToLayer(int index) {
            if (!(this.Layer is LayerViewModel layer)) {
                return;
            }

            TimelineViewModel timeline = layer.Timeline;
            if (timeline.IsGloballyDragging && timeline.IsAboutToDragAcrossLayers) {
                return;
            }

            int target = Maths.Clamp(index, 0, timeline.Layers.Count - 1);
            LayerViewModel targetLayer = timeline.Layers[target];
            if (ReferenceEquals(layer, targetLayer)) {
                return;
            }

            if (!targetLayer.CanAccept(this)) {
                return;
            }

            if (timeline.IsGloballyDragging) {
                if (timeline.DraggingClips.All(x => ReferenceEquals(x.Layer, layer))) {
                    timeline.IsAboutToDragAcrossLayers = true;
                    foreach (ClipViewModel clip in timeline.DraggingClips) {
                        if (targetLayer.CanAccept(this)) {
                            timeline.MoveClip(clip, layer, targetLayer);
                        }
                    }

                    timeline.IsAboutToDragAcrossLayers = false;
                }
                else {
                    return;
                }
            }
            else {
                timeline.MoveClip(this, layer, targetLayer);
            }
        }

        public virtual void OnDragToLayerOffset(int offset) {

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
    }
}