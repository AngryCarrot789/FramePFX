using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Automation;
using FramePFX.Automation.ViewModels;
using FramePFX.Commands;
using FramePFX.Editor.History;
using FramePFX.Editor.Registries;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.Timelines;
using FramePFX.History;
using FramePFX.History.Tasks;
using FramePFX.History.ViewModels;
using FramePFX.Utils;

namespace FramePFX.Editor.ViewModels.Timelines {
    /// <summary>
    /// The base view model for a timeline track. This could be a video or audio track (or others...)
    /// </summary>
    public abstract class TrackViewModel : BaseViewModel, IHistoryHolder, IDisplayName, ITrackResourceDropHandler, IAutomatableViewModel, IProjectViewModelBound, IRenameTarget {
        protected readonly HistoryBuffer<HistoryTrackDisplayName> displayNameHistory = new HistoryBuffer<HistoryTrackDisplayName>();
        private readonly ObservableCollectionEx<ClipViewModel> clips;
        private ClipViewModel selectedClip;

        public ReadOnlyObservableCollection<ClipViewModel> Clips { get; }

        public ObservableCollection<ClipViewModel> SelectedClips { get; }

        public ClipViewModel SelectedClip {
            get => this.selectedClip;
            set => this.RaisePropertyChanged(ref this.selectedClip, value);
        }

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
        public string TrackColour {
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

        protected TrackViewModel(Track model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.AutomationData = new AutomationDataViewModel(this, model.AutomationData);
            this.AutomationData.SetActiveSequenceFromModelDeserialisation();
            this.clips = new ObservableCollectionEx<ClipViewModel>();
            this.Clips = new ReadOnlyObservableCollection<ClipViewModel>(this.clips);
            this.SelectedClips = new ObservableCollection<ClipViewModel>();
            this.SelectedClips.CollectionChanged += (sender, args) => {
                this.RemoveSelectedClipsCommand.RaiseCanExecuteChanged();
            };

            this.RemoveSelectedClipsCommand = new AsyncRelayCommand(this.RemoveSelectedClipsAction, () => this.SelectedClips.Count > 0);
            this.RenameTrackCommand = new AsyncRelayCommand(this.RenameAsync);

            for (int i = 0; i < model.Clips.Count; i++) {
                this.InsertClip(i, ClipFactory.Instance.CreateViewModelFromModel(model.Clips[i]), false);
            }
        }

        private static Comparison<ClipViewModel> SortClip = (a, b) => a.FrameBegin.CompareTo(b.FrameBegin);

        public virtual void OnProjectModified() => this.Project?.OnProjectModified();

        public ClipViewModel CreateClip(Clip model, bool addToModel = true) {
            ClipViewModel vm = ClipFactory.Instance.CreateViewModelFromModel(model);
            this.AddClip(vm, addToModel);
            return vm;
        }

        public void AddClip(ClipViewModel clip, bool addToModel = true) {
            this.InsertClip(this.clips.Count, clip, addToModel);
        }

        public void InsertClip(int index, ClipViewModel clip, bool addToModel = true) {
            if (index < 0 || index > this.clips.Count)
                throw new IndexOutOfRangeException($"Index < 0 || Index > Count. Index = {index}, Count = {this.clips.Count}");
            if (ReferenceEquals(this, clip.Track))
                throw new InvalidOperationException("Attempted to add clip to a track it was already in");
            if (clip.Track != null)
                throw new InvalidOperationException("Clip was already added to another track");
            if (!this.IsClipTypeAcceptable(clip))
                throw new Exception("Invalid clip for this layer");

            if (addToModel) {
                this.Model.InsertClip(index, clip.Model);
            }

            ClipViewModel.PreSetTrack(clip, this);
            this.clips.Insert(index, clip);
            ClipViewModel.PostSetTrack(clip, this);
            this.OnProjectModified();
            if (addToModel) {
                this.TryRaiseLargestFrameInUseChanged();
            }
        }

        public bool RemoveClip(ClipViewModel clip) {
            int index = this.clips.IndexOf(clip);
            if (index < 0)
                return false;
            this.RemoveClipAt(index);
            return true;
        }

        public ClipViewModel RemoveClipAt(int index) {
            ClipViewModel clip = this.clips[index];
            if (!ReferenceEquals(this, clip.Track))
                throw new Exception($"Clip track does not match the current instance: {clip.Track} != {this}");
            if (!ReferenceEquals(this.Model.Clips[index], clip.Model))
                throw new Exception("Clip model clip list desynchronized");

            this.Model.RemoveClipAt(index);
            this.clips.RemoveAt(index);
            ClipViewModel.SetTrack(clip, null);
            this.OnProjectModified();
            this.TryRaiseLargestFrameInUseChanged();
            return clip;
        }

        public Task RemoveSelectedClipsAction() {
            return this.RemoveSelectedClipsAction(true);
        }

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

        public async Task DisposeAndRemoveItemsAction(IEnumerable<ClipViewModel> list) {
            try {
                this.DisposeAndRemoveItemsUnsafe(list as List<ClipViewModel> ?? list.ToList());
            }
            catch (Exception e) {
                await Services.DialogService.ShowMessageExAsync("Error", "An error occurred while removing clips", e.GetToString());
            }
        }

        public void DisposeAndRemoveItemsUnsafe(IList<ClipViewModel> list) {
            using (ErrorList stack = new ErrorList("Exception disposing clips")) {
                foreach (ClipViewModel clip in list) {
                    int index = this.clips.IndexOf(clip);
                    if (index < 0) {
                        continue;
                    }

                    Validate.Exception(index < this.Model.Clips.Count, "Model-ViewModel list desynchronized");
                    Validate.Exception(ReferenceEquals(clip.Model, this.Model.Clips[index]), "Model-ViewModel list desynchronized");
                    this.RemoveClipAt(index);
                    try {
                        clip.Dispose();
                    }
                    catch (Exception e) {
                        stack.Add(new Exception($"Failed to dispose {clip.GetType()} properly", e));
                    }
                }
            }
        }

        /// <summary>
        /// Clears all clips in this timeline and then disposes the timeline. This should be called after the track is removed from a timeline
        /// </summary>
        public void ClearAndDispose() {
            for (int i = this.clips.Count - 1; i >= 0; i--) {
                ClipViewModel clip = this.clips[i];
                this.RemoveClipAt(i);
                clip.Dispose();
            }
        }

        public IEnumerable<ClipViewModel> GetClipsAtFrame(long frame) {
            return this.Clips.Where(clip => clip.IntersectsFrameAt(frame));
        }

        public void MakeTopMost(ClipViewModel clip) {
            int endIndex = this.Clips.Count - 1;
            int index = this.Clips.IndexOf(clip);
            if (index == -1 || index == endIndex) {
                return;
            }

            this.Model.MakeTopMost(clip.Model, index, endIndex);
            this.clips.Move(index, endIndex);
            this.TryRaiseLargestFrameInUseChanged();
        }

        public abstract bool CanDropResource(ResourceItemViewModel resource);

        public abstract Task OnResourceDropped(ResourceItemViewModel resource, long frame);

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
            this.CreateClip(cloned);
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
                    clip.OnUserSeekedFrame(oldFrame, newFrame);
                    clip.LastSeekedFrame = newFrame;
                }
                else if (clip.LastSeekedFrame != -1) {
                    if (clip.IntersectsFrameAt(clip.LastSeekedFrame)) {
                        clip.OnPlayHeadLeaveClip(true);
                    }

                    clip.LastSeekedFrame = -1;
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

        public void AddSelected(ClipViewModel clip) {
            if (clip.Track != this) {
                throw new Exception("Clip is not placed in this track");
            }

            if (!clip.IsSelected)
                clip.IsSelected = true;
        }

        public void MoveClipToTrack(ClipViewModel clip, TrackViewModel newTrack) {
            if (newTrack == null) {
                if (!this.RemoveClip(clip))
                    throw new Exception("Clip was not present in the old track");
            }
            else if (clip.Track == null) {
                this.AddClip(clip);
            }
            else {
                int index = this.clips.IndexOf(clip);
                if (index < 0) {
                    throw new Exception("Clip was not present in the old track");
                }

                if (!ReferenceEquals(clip, this.clips[index]))
                    throw new Exception($"Clip does not reference equal other clip in track: {clip} != {this.clips[index]}");
                if (!ReferenceEquals(this, clip.Track))
                    throw new Exception($"Clip track does not match the current instance: {clip.Track} != {this}");
                if (!ReferenceEquals(this.Model.Clips[index], clip.Model))
                    throw new Exception("Clip model clip list desynchronized");

                this.Model.MoveClipToTrack(index, newTrack.Model);

                this.clips.RemoveAt(index);
                ClipViewModel.PreSetTrack(clip, newTrack);
                newTrack.clips.Add(clip);
                ClipViewModel.PostSetTrack(clip, newTrack);
                this.OnProjectModified();
                this.RaisePropertyChanged(nameof(this.LargestFrameInUse));
                newTrack.RaisePropertyChanged(nameof(newTrack.LargestFrameInUse));
            }
        }

        private void TryRaiseLargestFrameInUseChanged() {
            if (this.Model.LargestFrameInUse != this.Model.PreviousLargestFrameInUse) {
                this.RaisePropertyChanged(nameof(this.LargestFrameInUse));
            }
        }

        public bool IsRegionEmpty(FrameSpan span) {
            return !this.clips.Any(x => x.FrameSpan.Intersects(span));
        }
    }
}