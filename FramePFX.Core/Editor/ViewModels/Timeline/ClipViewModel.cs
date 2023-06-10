using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Core.Editor.History;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.Timeline;
using FramePFX.Core.History.Tasks;
using FramePFX.Core.History.ViewModels;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels.Timeline {
    /// <summary>
    /// The base view model for all types of clips (video, audio, etc)
    /// </summary>
    public abstract class ClipViewModel : BaseViewModel, IUserRenameable, IDropClipResource, IClipDragHandler, IDisposable {
        private readonly HistoryBuffer<HistoryClipDisplayName> displayNameHistory = new HistoryBuffer<HistoryClipDisplayName>();
        private readonly HistoryBuffer<HistoryVideoClipPosition> clipPositionHistory = new HistoryBuffer<HistoryVideoClipPosition>();

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
                }
            }
        }

        public TimelineViewModel Timeline => this.Layer?.Timeline;

        public ProjectViewModel Project => this.Layer?.Timeline.Project;

        public VideoEditorViewModel Editor => this.Layer?.Timeline.Project.Editor;

        public HistoryManagerViewModel HistoryManager => this.Editor?.HistoryManager;

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
            this.IsDraggingLeftThumb = true;
        }

        public virtual void OnLeftThumbDragStop(bool cancelled) {
            this.IsDraggingLeftThumb = false;
        }

        public virtual void OnRightThumbDragStart() {
            this.IsDraggingRightThumb = true;
        }

        public virtual void OnRightThumbDragStop(bool cancelled) {
            this.IsDraggingRightThumb = false;
        }

        public virtual void OnDragStart() {
            this.IsDraggingClip = true;
        }

        public virtual void OnDragStop(bool cancelled) {
            this.IsDraggingClip = false;
        }

        public virtual void OnLeftThumbDelta(long offset) {

        }

        public virtual void OnRightThumbDelta(long offset) {

        }

        public virtual void OnDragDelta(long offset) {

        }
    }
}