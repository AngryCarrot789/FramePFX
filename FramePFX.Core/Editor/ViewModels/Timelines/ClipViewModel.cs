using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using FramePFX.Core.Views.Dialogs.UserInputs;

namespace FramePFX.Core.Editor.ViewModels.Timelines
{
    /// <summary>
    /// The base view model for all types of clips (video, audio, etc)
    /// </summary>
    public abstract class ClipViewModel : BaseViewModel, IHistoryHolder, IAutomatableViewModel, IDisplayName, IAcceptResourceDrop, IClipDragHandler, IProjectViewModelBound, IDisposable, IRenameTarget
    {
        protected readonly HistoryBuffer<HistoryVideoClipPosition> clipPositionHistory = new HistoryBuffer<HistoryVideoClipPosition>();
        protected HistoryVideoClipPosition lastDragHistoryAction;

        public long LastSeekedFrame { get; set; }

        /// <summary>
        /// Whether or not this clip's history is being changed, and therefore, no changes should be pushed to the history manager
        /// </summary>
        public bool IsHistoryChanging { get; set; }

        public HistoryManagerViewModel HistoryManager => this.Editor?.HistoryManager;

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
        public string DisplayName
        {
            get => this.Model.DisplayName;
            set
            {
                this.Model.DisplayName = value;
                this.RaisePropertyChanged();
                this.Track?.OnProjectModified();
            }
        }

        /// <summary>
        /// The track this clip is located in
        /// </summary>
        public TrackViewModel Track { get; set; }

        public FrameSpan FrameSpan
        {
            get => this.Model.FrameSpan;
            set
            {
                FrameSpan oldSpan = this.FrameSpan;
                if (oldSpan == value)
                {
                    return;
                }

                if (!this.IsHistoryChanging && !this.IsDraggingAny && this.Track != null)
                {
                    if (!this.clipPositionHistory.TryGetAction(out HistoryVideoClipPosition action) && this.GetHistoryManager(out HistoryManagerViewModel m))
                        this.clipPositionHistory.PushAction(m, action = new HistoryVideoClipPosition(this), "Edit media pos/duration");
                    action.Span.SetCurrent(value);
                }

                this.Model.FrameSpan = value;
                this.OnFrameSpanChanged(oldSpan);
            }
        }

        public long FrameBegin
        {
            get => this.FrameSpan.Begin;
            set => this.FrameSpan = this.FrameSpan.WithBegin(value);
        }

        public long FrameDuration
        {
            get => this.FrameSpan.Duration;
            set => this.FrameSpan = this.FrameSpan.WithDuration(value);
        }

        public long FrameEndIndex
        {
            get => this.FrameSpan.EndIndex;
            set => this.FrameSpan = this.FrameSpan.WithEndIndex(value);
        }

        public long MediaFrameOffset
        {
            get => this.Model.MediaFrameOffset;
            set
            {
                long oldValue = this.MediaFrameOffset;
                if (oldValue == value)
                {
                    return;
                }

                if (!this.IsHistoryChanging && !this.IsDraggingAny && this.Track != null && this.GetHistoryManager(out HistoryManagerViewModel m))
                {
                    if (!this.clipPositionHistory.TryGetAction(out HistoryVideoClipPosition action))
                        this.clipPositionHistory.PushAction(m, action = new HistoryVideoClipPosition(this), "Edit media pos/duration");
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
        public long RelativePlayHead
        {
            get
            {
                if (this.Track != null)
                {
                    return this.Track.Timeline.PlayHeadFrame - this.FrameBegin;
                }
                else
                {
                    return this.FrameBegin;
                }
            }
        }

        public TimelineViewModel Timeline => this.Track?.Timeline;

        public ProjectViewModel Project => this.Track?.Timeline?.Project;

        public VideoEditorViewModel Editor => this.Project?.Editor;

        public AutomationDataViewModel AutomationData { get; }

        public AutomationEngineViewModel AutomationEngine => this.Project?.AutomationEngine;

        public AsyncRelayCommand EditDisplayNameCommand { get; }

        public RelayCommand RemoveClipCommand { get; }

        public Clip Model { get; }

        IAutomatable IAutomatableViewModel.AutomationModel => this.Model;

        public ObservableCollection<ClipGroupViewModel> ConnectedGroups { get; }

        private ClipDragData drag;

        protected ClipViewModel(Clip model)
        {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            model.viewModel = this;

            this.AutomationData = new AutomationDataViewModel(this, model.AutomationData);
            this.EditDisplayNameCommand = new AsyncRelayCommand(async () =>
            {
                string name = await IoC.UserInput.ShowSingleInputDialogAsync("Input a new name", "Input a new display name for this clip", this.DisplayName);
                if (name != null)
                {
                    this.DisplayName = name;
                }
            });

            this.RemoveClipCommand = new RelayCommand(() =>
            {
                this.Track?.DisposeAndRemoveItemsAction(new List<ClipViewModel>() {this});
            });

            this.ConnectedGroups = new ObservableCollection<ClipGroupViewModel>();
        }

        public bool GetHistoryManager(out HistoryManagerViewModel manager)
        {
            return (manager = this.Editor?.HistoryManager) != null;
        }

        public virtual void OnFrameSpanChanged(FrameSpan oldSpan)
        {
            this.RaisePropertyChanged();
            this.RaisePropertyChanged(nameof(this.FrameBegin));
            this.RaisePropertyChanged(nameof(this.FrameDuration));
            this.RaisePropertyChanged(nameof(this.FrameEndIndex));
            this.RaisePropertyChanged(nameof(this.RelativePlayHead));
            if (this.Track != null)
            {
                this.Track.OnProjectModified();
                long frame = this.Track.Timeline.PlayHeadFrame;
                if (this.LastSeekedFrame != -1)
                {
                    if (!this.IntersectsFrameAt(frame))
                    {
                        this.OnPlayHeadLeaveClip(false);
                        this.LastSeekedFrame = -1;
                    }
                }
                else if (this.IntersectsFrameAt(frame))
                {
                    this.LastSeekedFrame = frame;
                    this.OnClipMovedToPlayeHeadFrame(frame);
                }
            }
        }

        protected virtual void OnMediaFrameOffsetChanged(long oldFrame, long newFrame)
        {
            this.RaisePropertyChanged();
            this.Track?.OnProjectModified();
        }

        public void Dispose()
        {
            using (ErrorList stack = new ErrorList())
            {
                try
                {
                    this.DisposeCore(stack);
                }
                catch (Exception e)
                {
                    stack.Add(new Exception(nameof(this.DisposeCore) + " method unexpectedly threw", e));
                }
            }
        }

        protected virtual void DisposeCore(ErrorList stack)
        {
            try
            {
                this.Model.Dispose();
            }
            catch (Exception e)
            {
                stack.Add(new Exception("Exception disposing model", e));
            }
        }

        public bool IntersectsFrameAt(long frame) => this.Model.IntersectsFrameAt(frame);

        public virtual bool CanDropResource(BaseResourceObjectViewModel resource)
        {
            return ReferenceEquals(resource.Manager, this.Track?.Timeline.Project.ResourceManager);
        }

        public virtual Task OnDropResource(BaseResourceObjectViewModel resource)
        {
            return IoC.MessageDialogs.ShowMessageAsync("Resource dropped", "This clip can't do anything with that resource!");
        }

        protected void CreateClipDragHistoryAction()
        {
            if (this.lastDragHistoryAction != null)
            {
                throw new Exception("Drag history was non-null, which means a drag was started before another drag was completed");
            }

            this.lastDragHistoryAction = new HistoryVideoClipPosition(this);
        }

        protected void PushClipDragHistoryAction(bool cancelled)
        {
            // throws if this.lastDragHistoryAction is null. It should not be null if there's no bugs in the drag start/end calls
            if (cancelled)
            {
                this.lastDragHistoryAction.Undo();
            }
            else if (this.GetHistoryManager(out HistoryManagerViewModel m))
            {
                m.AddAction(this.lastDragHistoryAction, "Drag clip");
            }

            this.lastDragHistoryAction = null;
        }

        /// <summary>
        /// Called when the user seeks a specific frame, and it intersects with this clip. The frame is relative to this clip's begin frame
        /// </summary>
        /// <param name="oldFrame">Previous frame (not relative to this clip)</param>
        /// <param name="newFrame">Current frame (relative to this clip)</param>
        public virtual void OnUserSeekedFrame(long oldFrame, long newFrame)
        {
        }

        /// <summary>
        /// Called when the user moves this clip such that it intersects with the play head
        /// </summary>
        /// <param name="frame">The play head frame, relative to this clip</param>
        public virtual void OnClipMovedToPlayeHeadFrame(long frame)
        {
        }

        public virtual void OnPlayHeadLeaveClip(bool isPlayheadLeaveClip)
        {
        }

        private List<ClipViewModel> GetSelectedIncludingThis()
        {
            List<ClipViewModel> list = this.Timeline.GetSelectedClips().ToList();
            if (!list.Contains(this))
                list.Add(this);
            return list;
        }

        public void OnLeftThumbDragStart()
        {
            this.OnDragStart();
            this.IsDraggingLeftThumb = true;
        }

        public void OnRightThumbDragStart()
        {
            this.OnDragStart();
            this.IsDraggingRightThumb = true;
        }

        public void OnDragStart()
        {
            if (this.drag != null)
                throw new Exception("Drag already in progress");
            if (this.Timeline == null)
                throw new Exception("No timeline available");
            this.drag = new ClipDragData(this.Timeline, this.GetSelectedIncludingThis());
            this.drag.OnBegin();
            this.IsDraggingClip = true;
        }

        public void OnLeftThumbDragStop(bool cancelled)
        {
            this.OnDragStop(cancelled);
            this.IsDraggingLeftThumb = false;
        }

        public void OnRightThumbDragStop(bool cancelled)
        {
            this.OnDragStop(cancelled);
            this.IsDraggingRightThumb = false;
        }

        public void OnDragStop(bool cancelled)
        {
            if (this.drag != null)
            {
                this.drag.OnFinished(cancelled);
                this.drag = null;
            }

            this.IsDraggingClip = false;
        }

        public void OnLeftThumbDelta(long offset)
        {
            this.drag?.OnLeftThumbDelta(offset);
        }

        public void OnRightThumbDelta(long offset)
        {
            this.drag?.OnRightThumbDelta(offset);
        }

        public void OnDragDelta(long offset)
        {
            this.drag?.OnDragDelta(offset);
        }

        public void OnDragToTrack(int index)
        {
            this.drag?.OnDragToTrack(index);
        }

        public static void RaiseTrackChanged(ClipViewModel clip)
        {
            clip.RaisePropertyChanged(nameof(Track));
            clip.RaisePropertyChanged(nameof(Timeline));
            clip.RaisePropertyChanged(nameof(Project));
            clip.RaisePropertyChanged(nameof(Editor));
            clip.RaisePropertyChanged(nameof(RelativePlayHead));
        }

        public async Task<bool> RenameAsync()
        {
            string result = await IoC.UserInput.ShowSingleInputDialogAsync("Rename clip", "Input a new clip name:", this.DisplayName ?? "", Validators.ForNonWhiteSpaceString());
            if (result != null)
            {
                this.DisplayName = result;
                return true;
            }

            return false;
        }
    }
}