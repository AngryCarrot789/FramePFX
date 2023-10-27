using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.AdvancedContextService;
using FramePFX.AdvancedContextService.NCSP;
using FramePFX.Automation;
using FramePFX.Automation.ViewModels;
using FramePFX.Commands;
using FramePFX.Editor.History;
using FramePFX.Editor.Registries;
using FramePFX.Editor.Timelines;
using FramePFX.History;
using FramePFX.History.Tasks;
using FramePFX.History.ViewModels;
using FramePFX.Interactivity;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editor.ViewModels.Timelines {
    /// <summary>
    /// The base view model for a timeline track. This could be a video or audio track (or others...)
    /// </summary>
    public abstract class TrackViewModel : BaseViewModel, IHistoryHolder, IDisplayName, IAutomatableViewModel, IProjectViewModelBound, IRenameTarget {
        protected readonly HistoryBuffer<HistoryTrackDisplayName> displayNameHistory = new HistoryBuffer<HistoryTrackDisplayName>();
        private readonly ObservableCollectionEx<ClipViewModel> clips;
        private ClipViewModel selectedClip;

        public ReadOnlyObservableCollection<ClipViewModel> Clips { get; }

        public ObservableCollection<ClipViewModel> SelectedClips { get; }

        public ClipViewModel SelectedClip {
            get => this.selectedClip;
            set => this.RaisePropertyChanged(ref this.selectedClip, value);
        }

        /// <summary>
        /// Gets the last clip in this track, by index, not frame
        /// </summary>
        public ClipViewModel LastClip => this.clips[this.clips.Count - 1];

        public long LargestFrameInUse => this.Model.LargestFrameInUse;

        public string DisplayName {
            get => this.Model.DisplayName;
            set {
                if (!this.IsHistoryChanging) {
                    if (!this.displayNameHistory.TryGetAction(out HistoryTrackDisplayName action))
                        this.displayNameHistory.PushAction(HistoryManagerViewModel.Instance, action = new HistoryTrackDisplayName(this), "Edit display name");
                    action.DisplayName.SetCurrent(value);
                }

                this.Model.DisplayName = value;
                this.RaisePropertyChanged();
            }
        }

        public double Height {
            get => this.Model.Height;
            set {
                this.Model.Height = Math.Max(Math.Min(value, 500), 24);
                this.RaisePropertyChanged();
            }
        }

        // Feels so wrong having colours here for some reason... should be done in a converter with enums maybe?
        public SKColor TrackColour {
            get => this.Model.TrackColour;
            set {
                this.Model.TrackColour = value;
                this.RaisePropertyChanged();
            }
        }

        public bool IsHistoryChanging { get; set; }

        public bool IsAutomationRefreshInProgress { get; set; }

        public TimelineViewModel Timeline { get; set; }

        public ProjectViewModel Project => this.Timeline?.Project;

        public VideoEditorViewModel Editor => this.Project?.Editor;

        public AutomationDataViewModel AutomationData { get; }

        public AsyncRelayCommand RenameTrackCommand { get; }
        public AsyncRelayCommand RemoveSelectedClipsCommand { get; }

        IAutomatable IAutomatableViewModel.AutomationModel => this.Model;

        public Track Model { get; }

        public static DragDropRegistry<TrackViewModel> DropRegistry { get; }
        public const string DroppedFrameKey = "TrackDropFrameLocation";

        protected TrackViewModel(Track model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.AutomationData = new AutomationDataViewModel(this, model.AutomationData);
            this.AutomationData.SetActiveSequenceFromModelDeserialisation();
            this.clips = new ObservableCollectionEx<ClipViewModel>();
            this.Clips = new ReadOnlyObservableCollection<ClipViewModel>(this.clips);
            this.SelectedClips = new ObservableCollection<ClipViewModel>();
            this.SelectedClips.CollectionChanged += (sender, args) => {
                this.RemoveSelectedClipsCommand.RaiseCanExecuteChanged();
                this.Timeline?.OnSelectionChanged();
            };

            this.RemoveSelectedClipsCommand = new AsyncRelayCommand(this.RemoveSelectedClipsAction, () => this.SelectedClips.Count > 0);
            this.RenameTrackCommand = new AsyncRelayCommand(this.RenameAsync);
            this.Model.ClipInserted += this.OnModelClipInserted;
            this.Model.ClipRemoved += this.OnModelClipRemoved;
            this.Model.ClipMovedToTrack += this.OnModelClipMovedToTrack;

            Clip[] array = model.Clips.items;
            int count = model.Clips.size;
            for (int i = 0; i < count; i++) {
                ClipViewModel vm = ClipFactory.Instance.CreateViewModelFromModel(array[i]);
                ClipViewModel.PreSetTrack(vm, this);
                this.clips.Insert(i, vm);
                ClipViewModel.PostSetTrack(vm, this);
            }
        }

        static TrackViewModel() {
            DropRegistry = new DragDropRegistry<TrackViewModel>();
            DropRegistry.RegisterNative<TrackViewModel>(NativeDropTypes.FileDrop, (handler, objekt, type, c) => {
                return objekt.GetData(NativeDropTypes.FileDrop) is string[] files && files.Length > 0 ? EnumDropType.Copy : EnumDropType.None;
            }, async (model, objekt, type, c) => {
                string[] files = (string[]) objekt.GetData(NativeDropTypes.FileDrop);
                await Services.DialogService.ShowDialogAsync("TODO", $"Dropping files directly into the timeline is not implemented yet.\nYou dropped: {string.Join(", ", files)}");
            });

            IContextRegistration reg = ContextRegistry.Instance.RegisterType(typeof(TrackViewModel));
            reg.AddEntry(new ActionContextEntry(null, "actions.general.RenameItem", "Rename track"));
            reg.AddEntry(new ActionContextEntry(null, "actions.timeline.track.ChangeTrackColour", "Change colour"));
            List<IContextEntry> newClipList = new List<IContextEntry> {
                new ActionContextEntry(null, "actions.timeline.NewAdjustmentClip", "New adjustment clip")
            };

            reg.AddEntry(new GroupContextEntry("New clip...", newClipList));

            reg.AddEntry(SeparatorEntry.Instance);
            reg.AddEntry(new ActionContextEntry(null, "actions.editor.NewVideoTrack", "Insert video track below"));
            reg.AddEntry(new ActionContextEntry(null, "actions.editor.NewAudioTrack", "Insert audio track below"));
            reg.AddEntry(SeparatorEntry.Instance);
            reg.AddEntry(new ActionContextEntry(null, "actions.editor.timeline.DeleteSelectedTracks", "Delete Track(s)"));
        }

        private void OnModelClipInserted(Track track, Clip clip, int index) {
            ClipViewModel vm = ClipFactory.Instance.CreateViewModelFromModel(clip);
            ClipViewModel.PreSetTrack(vm, this);
            this.clips.Insert(index, vm);
            ClipViewModel.PostSetTrack(vm, this);

            this.OnProjectModified();
            this.TryRaiseLargestFrameInUseChanged();
        }

        private void OnModelClipRemoved(Track track, Clip clip, int index) {
            ClipViewModel vm = this.clips[index];
            if (!ReferenceEquals(vm.Model, clip))
                throw new Exception("Model-ViewModel list desynchronized");
            ClipViewModel.PreSetTrack(vm, null);
            this.clips.RemoveAt(index);
            ClipViewModel.PostSetTrack(vm, null);

            this.OnProjectModified();
            this.TryRaiseLargestFrameInUseChanged();
        }

        private void OnModelClipMovedToTrack(ClipMovedEventArgs e) {
            ClipViewModel clip;
            if (ReferenceEquals(e.OldTrack, this.Model)) { // we are the source track; remove the clip internally
                e.Parameter = clip = this.clips[e.OldIndex];
                ClipViewModel.PreSetTrack(clip, null);
                this.clips.RemoveAt(e.OldIndex);
                ClipViewModel.PostSetTrack(clip, null);
                this.TryRaiseLargestFrameInUseChanged();
            }
            else if (ReferenceEquals(e.NewTrack, this.Model)) { // we are the target track; add the clip internally
                if ((clip = e.Parameter as ClipViewModel) == null) {
                    throw new InvalidOperationException("Expected parameter to be a ClipViewModel");
                }

                ClipViewModel.PreSetTrack(clip, this);
                this.clips.Insert(e.NewIndex, clip);
                ClipViewModel.PostSetTrack(clip, this);
                this.TryRaiseLargestFrameInUseChanged();
                this.OnProjectModified();
            }
            else {
                throw new Exception("???");
            }
        }

        public virtual void OnProjectModified() => this.Project?.OnProjectModified();

        public Task RemoveSelectedClipsAction() => this.RemoveSelectedClipsAction(true);

        public async Task RemoveSelectedClipsAction(bool confirm) {
            IList<ClipViewModel> list = this.SelectedClips;
            if (list.Count < 1) {
                return;
            }

            if (confirm && !await Services.DialogService.ShowYesNoDialogAsync($"Delete clip{Lang.S(list.Count)}?", $"Are you sure you want to delete {(list.Count == 1 ? "1 clip" : $"{list.Count} clips")}?")) {
                return;
            }

            await this.DisposeAndRemoveItemsAction(list);
        }

        public async Task DisposeAndRemoveItemsAction(IEnumerable<ClipViewModel> items) {
            List<ClipViewModel> list = items as List<ClipViewModel> ?? items.ToList();
            using (ErrorList stack = new ErrorList("One or more exceptions occurred while removing clips", false, true)) {
                foreach (ClipViewModel clip in list) {
                    if (!this.Model.RemoveClip(clip.Model)) {
                        continue;
                    }

                    try {
                        clip.Model.Dispose();
                    }
                    catch (Exception e) {
                        stack.Add(new Exception($"Failed to dispose {clip.GetType()} properly", e));
                    }
                }

                if (stack.TryGetException(out Exception exception)) {
                    await Services.DialogService.ShowMessageExAsync("Error", "An error occurred while removing clips", exception.GetToString());
                }
            }
        }

        /// <summary>
        /// Clears all clips in this timeline and then disposes the timeline. This should be called after the track is removed from a timeline
        /// </summary>
        public void ClearAndDispose() {
            for (int i = this.clips.Count - 1; i >= 0; i--) {
                ClipViewModel clip = this.clips[i];
                this.Model.RemoveClipAt(i);
                clip.Model.Dispose();
            }
        }

        // TODO: implement an optimised way of mapping Model->ViewModel and vice-versa,
        // so that view models can sort of use the Track model's clip cache for more optimised iterations
        public IEnumerable<ClipViewModel> GetClipsAtFrame(long frame) {
            return this.clips.Where(clip => clip.IntersectsFrameAt(frame));
        }

        public IEnumerable<ClipViewModel> GetSelectedClipsAtFrame(long frame) {
            return this.clips.Where(clip => clip.IsSelected && clip.IntersectsFrameAt(frame));
        }

        /// <summary>
        /// Slices the given clip at the given frame
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        public Task SliceClipAction(ClipViewModel clip, long frame) {
            FrameSpan span = clip.Model.FrameSpan;
            if (frame <= span.Begin || frame >= span.EndIndex) {
                return Task.CompletedTask;
            }

            Clip cloned = clip.Model.Clone();
            cloned.FrameSpan = FrameSpan.FromIndex(frame, span.EndIndex);
            cloned.MediaFrameOffset = frame - span.Begin;
            clip.FrameSpan = span.WithEndIndex(frame);
            this.Model.AddClip(cloned);
            return Task.CompletedTask;
        }

        public bool IsClipTypeAcceptable(ClipViewModel clip) {
            return this.Model.IsClipTypeAcceptable(clip.Model);
        }

        /// <summary>
        /// Called when the user seeks a specific frame
        /// </summary>
        /// <param name="oldFrame">Previous frame</param>
        /// <param name="newFrame">Current frame</param>
        public virtual void OnUserSeekedFrame(long oldFrame, long newFrame) {
            foreach (ClipViewModel clip in this.clips) {
                FrameSpan span = clip.FrameSpan;
                long relative = newFrame - span.Begin;
                if (relative >= 0 && relative < span.Duration) {
                    clip.Model.OnFrameSeeked(oldFrame, newFrame);
                    clip.Model.LastSeekedFrame = newFrame;
                }
                else if (clip.Model.LastSeekedFrame != -1) {
                    if (clip.IntersectsFrameAt(clip.Model.LastSeekedFrame)) {
                        clip.OnPlayHeadLeaveClip(true);
                    }

                    clip.Model.LastSeekedFrame = -1;
                }
            }
        }

        public static void OnTimelineChanged(TrackViewModel track) {
            track.RaisePropertyChanged(nameof(Timeline));
            track.RaisePropertyChanged(nameof(Project));
            track.RaisePropertyChanged(nameof(Editor));
        }

        public static void OnTimelineProjectChanged(TrackViewModel track, ProjectViewModel oldProject, ProjectViewModel newProject) {
            track.RaisePropertyChanged(nameof(track.Project));
            track.RaisePropertyChanged(nameof(track.Editor));
        }

        public async Task<bool> RenameAsync() {
            string result = await Services.UserInput.ShowSingleInputDialogAsync("Change track name", "Input a new track name:", this.DisplayName ?? "", this.Timeline.TrackNameValidator);
            if (result != null) {
                this.DisplayName = result;
                return true;
            }

            return false;
        }

        private void TryRaiseLargestFrameInUseChanged() {
            if (this.Model.LargestFrameInUse != this.Model.PreviousLargestFrameInUse) {
                this.RaisePropertyChanged(nameof(this.LargestFrameInUse));
            }
        }

        public ClipViewModel GetClipByModel(Clip clip) {
            return this.Model.TryGetIndexOfClip(clip, out int index) ? this.clips[index] : null;
        }

        public override string ToString() {
            return $"{this.GetType().Name} -> {this.Model}";
        }
    }
}