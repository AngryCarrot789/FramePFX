using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.AdvancedContextService;
using FramePFX.AdvancedContextService.NCSP;
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
using FramePFX.Interactivity;
using FramePFX.PropertyEditing;
using FramePFX.Utils;
using FramePFX.Views.Dialogs.UserInputs;

namespace FramePFX.Editor.ViewModels.Timelines {
    /// <summary>
    /// The base view model for all types of clips (video, audio, etc)
    /// </summary>
    public abstract class ClipViewModel : BaseViewModel, IHistoryHolder, IAutomatableViewModel, IDisplayName, IProjectViewModelBound, IDisposable, IRenameTarget, IStrictFrameRange {
        protected readonly HistoryBuffer<HistoryVideoClipPosition> clipPositionHistory = new HistoryBuffer<HistoryVideoClipPosition>();
        private readonly ObservableCollection<BaseEffectViewModel> effects;
        private bool skipUpdatePropertyEditor;
        private bool isSelected;

        public static DragDropRegistry<ClipViewModel> DropRegistry { get; }

        public Clip Model { get; }

        /// <summary>
        /// The track this clip is located in
        /// </summary>
        public TrackViewModel Track { get; private set; }

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

                this.Model.SetFrameSpan(value);
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
                long? playhead = this.Track?.Timeline?.PlayHeadFrame;
                return playhead.HasValue ? this.ConvertTimelineToRelativeFrame(playhead.Value, out _) : 0;
            }
        }

        /// <summary>
        /// Whether or not this clip is selected in the UI
        /// </summary>
        public bool IsSelected {
            get => this.isSelected;
            set => this.RaisePropertyChanged(ref this.isSelected, value);
        }

        public bool IsClipActive => this.Model.IsRenderingEnabled;

        /// <summary>
        /// The timeline associated with this clip. Clips should not really be modified when not inside a track
        /// </summary>
        public TimelineViewModel Timeline => this.Track?.Timeline;

        public ProjectViewModel Project => this.Track?.Timeline?.Project;

        public VideoEditorViewModel Editor => this.Project?.Editor;

        #region Automation

        public AutomationDataViewModel AutomationData { get; }

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
            this.effects = new ObservableCollection<BaseEffectViewModel>();
            this.Effects = new ReadOnlyObservableCollection<BaseEffectViewModel>(this.effects);
            this.skipUpdatePropertyEditor = true;
            foreach (BaseEffect fx in model.Effects) {
                this.AddEffect(EffectFactory.Instance.CreateViewModelFromModel(fx), false);
            }

            this.skipUpdatePropertyEditor = false;

            this.EditDisplayNameCommand = new AsyncRelayCommand(async () => {
                string name = await Services.UserInput.ShowSingleInputDialogAsync("Input a new name", "Input a new display name for this clip", this.DisplayName);
                if (name != null) {
                    this.DisplayName = name;
                }
            });

            this.RemoveClipCommand = new RelayCommand(() => {
                this.Track?.DisposeAndRemoveItemsAction(new List<ClipViewModel>() {this});
            });

            this.AutomationData.SetActiveSequenceFromModelDeserialisation();
        }

        static ClipViewModel() {
            DropRegistry = new DragDropRegistry<ClipViewModel>();
            DropRegistry.Register<ClipViewModel, EffectProviderViewModel>((clip, x, dt, ctx) => {
                if (clip.Model.IsEffectTypeAllowed(x.EffectType))
                    return EnumDropType.Copy;
                return EnumDropType.None;
            }, async (clip, x, dt, ctx) => {
                BaseEffect effect;
                try {
                    effect = EffectFactory.Instance.CreateModel(x.EffectFactoryId);
                }
                catch (Exception e) {
                    await Services.DialogService.ShowMessageExAsync("Error", "Failed to create effect from the dropped effect", e.GetToString());
                    return;
                }

                clip.AddEffect(EffectFactory.Instance.CreateViewModelFromModel(effect));
            });

            IContextRegistration reg = ContextRegistry.Instance.RegisterType(typeof(ClipViewModel));
            reg.AddEntry(new ActionContextEntry(null, "actions.general.RenameItem", "Rename Clip"));
            reg.AddEntry(new ActionContextEntry(null, "actions.automation.AddKeyFrame", "Add key frame", "Adds a key frame to the active sequence"));
            reg.AddEntry(new ActionContextEntry(null, "actions.editor.timeline.CreateCompositionFromSelection", "Create composition from selection", "Creates a composition clip from the selected clips"));
            reg.AddEntry(SeparatorEntry.Instance);
            reg.AddEntry(new ActionContextEntry(null, "actions.editor.timeline.DeleteSelectedClips", "Delete Clip(s)!!!"));
        }

        public static void SetSelectedAndShowPropertyEditor(ClipViewModel clip) {
            clip.IsSelected = true;
            TimelineViewModel timeline = clip.Timeline;
            List<ClipViewModel> list = timeline != null ? timeline.GetSelectedClips().ToList() : CollectionUtils.SingleItem(clip);
            PFXPropertyEditorRegistry.Instance.OnClipSelectionChanged(list);
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
            if (effect == null)
                throw new ArgumentNullException(nameof(effect));

            if (this.Effects.Contains(effect))
                throw new Exception("This clip already contains the effect");
            if (effect.OwnerClip != null)
                throw new Exception("Effect exists in another clip");

            if (addToModel) {
                this.Model.InsertEffect(effect.Model, index);
            }
            else if (!this.IsEffectTypeAllowed(effect)) {
                throw new Exception($"Effect type '{effect.GetType()}' is not applicable to the clip '{this.GetType()}'");
            }

            effect.OwnerClip = this;
            BaseEffectViewModel.OnAddingToClip(effect);
            this.effects.Add(effect);
            BaseEffectViewModel.OnAddedToClip(effect);
            if (!this.skipUpdatePropertyEditor) {
                this.UpdateEffectPropertyEditor();
            }
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

            BaseEffectViewModel.OnRemovingFromClip(effect);
            effect.OwnerClip = null;
            this.effects.Remove(effect);
            BaseEffectViewModel.OnRemovedFromClip(effect, this);
            if (!this.skipUpdatePropertyEditor) {
                this.UpdateEffectPropertyEditor();
            }
        }

        // TODO: feels weird referencing the property editor here... maybe switch to using events?
        protected virtual void UpdateEffectPropertyEditor() {
            PFXPropertyEditorRegistry.Instance.OnEffectCollectionChanged();
        }

        // [ShortcutTarget("Application/RenameItem")]
        // public async Task OnShortcutActivated() {
        //     await Task.Run(async () => {
        //         await Services.DialogService.ShowDialogAsync("t", "ttt");
        //     });
        // }

        public virtual void OnFrameSpanChanged(FrameSpan oldSpan) {
            this.RaisePropertyChanged();
            this.RaisePropertyChanged(nameof(this.FrameBegin));
            this.RaisePropertyChanged(nameof(this.FrameDuration));
            this.RaisePropertyChanged(nameof(this.FrameEndIndex));
            this.RaisePropertyChanged(nameof(this.RelativePlayHead));
            if (this.Track != null) {
                this.Track.RaisePropertyChanged(nameof(this.Track.LargestFrameInUse));
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

        public void ClearEffects(bool updatePropertyPages = true) {
            using (ErrorList list = new ErrorList()) {
                this.skipUpdatePropertyEditor = true;
                for (int i = this.effects.Count - 1; i >= 0; i--) {
                    try {
                        this.RemoveEffectAt(i);
                    }
                    catch (Exception e) {
                        list.Add(new Exception("Failed to remove effect", e));
                    }
                }

                this.skipUpdatePropertyEditor = false;
                if (updatePropertyPages) {
                    this.UpdateEffectPropertyEditor();
                }
            }
        }

        /// <summary>
        /// Disposes the model and also things that this clip has (e.g. event handlers registered to models)
        /// </summary>
        public virtual void Dispose() {
            this.Model.Dispose();
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

        public async Task<bool> RenameAsync() {
            string result = await Services.UserInput.ShowSingleInputDialogAsync("Rename clip", "Input a new clip name:", this.DisplayName ?? "", Validators.ForNonWhiteSpaceString());
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
            if (isFromEffect && this.AutomationData.ActiveSequence != sequence) {
                this.AutomationData.ActiveSequence = null;
            }

            foreach (BaseEffectViewModel fx in this.effects) {
                if (!ReferenceEquals(fx.AutomationData.ActiveSequence, sequence))
                    fx.AutomationData.ActiveSequence = null;
            }

            this.ActiveClipOrEffectSequence = sequence;
            this.RaisePropertyChanged(nameof(this.ActiveClipOrEffectSequence));
            this.AutomationData.ActiveSequence = sequence;
            this.isUpdatingAutomationSequence = false;
        }

        public long ConvertRelativeToTimelineFrame(long relative) => this.Model.ConvertRelativeToTimelineFrame(relative);

        public long ConvertTimelineToRelativeFrame(long timeline, out bool inRange) => this.Model.ConvertTimelineToRelativeFrame(timeline, out inRange);

        public bool IsTimelineFrameInRange(long timeline) => this.Model.IsTimelineFrameInRange(timeline);

        public static void PreSetTrack(ClipViewModel clip, TrackViewModel track) {
            clip.Track = track;
        }

        public static void PostSetTrack(ClipViewModel clip, TrackViewModel track) {
            clip.RaisePropertyChanged(nameof(Track));
            clip.RaisePropertyChanged(nameof(Timeline));
            clip.RaisePropertyChanged(nameof(Project));
            clip.RaisePropertyChanged(nameof(Editor));
            clip.RaisePropertyChanged(nameof(RelativePlayHead));
        }

        public static void SetTrack(ClipViewModel clip, TrackViewModel track) {
            PreSetTrack(clip, track);
            PostSetTrack(clip, track);
        }

        /// <summary>
        /// Delegates a call to our model's <see cref="Clip.IsEffectTypeAllowed"/>
        /// </summary>
        /// <param name="effect">The non-null effect to check</param>
        /// <returns>True if the effect can be added, otherwise false</returns>
        public bool IsEffectTypeAllowed(BaseEffectViewModel effect) {
            if (effect == null)
                throw new ArgumentNullException(nameof(effect));
            return this.Model.IsEffectTypeAllowed(effect.Model);
        }
    }
}