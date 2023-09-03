using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Automation;
using FramePFX.Automation.ViewModels;
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
    public abstract class TrackViewModel : BaseViewModel, IHistoryHolder, IDisplayName, IResourceItemDropHandler, IAutomatableViewModel, IProjectViewModelBound, IRenameTarget {
        protected readonly HistoryBuffer<HistoryTrackDisplayName> displayNameHistory = new HistoryBuffer<HistoryTrackDisplayName>();

        private readonly ObservableCollectionEx<ClipViewModel> clips;
        public ReadOnlyObservableCollection<ClipViewModel> Clips { get; }

        private List<ClipViewModel> selectedClips;

        public List<ClipViewModel> SelectedClips {
            get => this.selectedClips;
            set {
                this.RaisePropertyChanged(ref this.selectedClips, value ?? new List<ClipViewModel>());
                this.RemoveSelectedClipsCommand.RaiseCanExecuteChanged();
                this.Timeline.Project.Editor?.View.UpdateClipSelection();
            }
        }

        private ClipViewModel primarySelectedClip;

        public ClipViewModel PrimarySelectedClip {
            get => this.primarySelectedClip;
            set => this.RaisePropertyChanged(ref this.primarySelectedClip, value);
        }

        public string DisplayName {
            get => this.Model.DisplayName;
            set {
                if (!this.IsHistoryChanging) {
                    if (!this.displayNameHistory.TryGetAction(out HistoryTrackDisplayName action))
                        this.displayNameHistory.PushAction(this.HistoryManager, action = new HistoryTrackDisplayName(this), "Edit media duration");
                    action.DisplayName.SetCurrent(value);
                }

                this.Model.DisplayName = value;
                this.RaisePropertyChanged();
            }
        }

        public double MinHeight {
            get => this.Model.MinHeight;
            set {
                this.Model.MinHeight = value;
                this.RaisePropertyChanged();
            }
        }

        public double MaxHeight {
            get => this.Model.MaxHeight;
            set {
                this.Model.MaxHeight = value;
                this.RaisePropertyChanged();
            }
        }

        public double Height {
            get => this.Model.Height;
            set {
                this.Model.Height = Math.Max(Math.Min(value, this.MaxHeight), this.MinHeight);
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

        public AsyncRelayCommand RenameTrackCommand { get; }

        public AsyncRelayCommand RemoveSelectedClipsCommand { get; }

        public TimelineViewModel Timeline { get; set; }

        public ProjectViewModel Project => this.Timeline?.Project;

        public VideoEditorViewModel Editor => this.Project?.Editor;

        public HistoryManagerViewModel HistoryManager => this.Project?.Editor.HistoryManager;

        public AutomationDataViewModel AutomationData { get; }

        public Track Model { get; }

        IAutomatable IAutomatableViewModel.AutomationModel => this.Model;

        public AutomationEngineViewModel AutomationEngine => this.Project?.AutomationEngine;

        protected TrackViewModel(Track model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            model.viewModel = this;

            this.AutomationData = new AutomationDataViewModel(this, model.AutomationData);
            this.clips = new ObservableCollectionEx<ClipViewModel>();
            this.Clips = new ReadOnlyObservableCollection<ClipViewModel>(this.clips);
            this.selectedClips = new List<ClipViewModel>();
            this.RemoveSelectedClipsCommand = new AsyncRelayCommand(this.RemoveSelectedClipsAction, () => this.SelectedClips.Count > 0);
            this.RenameTrackCommand = new AsyncRelayCommand(this.RenameAsync);

            for (int i = 0; i < model.Clips.Count; i++) {
                this.InsertClip(i, ClipRegistry.Instance.CreateViewModelFromModel(model.Clips[i]), false);
            }
        }

        public virtual void OnProjectModified() {
            this.Timeline?.OnProjectModified();
        }

        public ClipViewModel CreateClip(Clip model, bool addToModel = true) {
            ClipViewModel vm = ClipRegistry.Instance.CreateViewModelFromModel(model);
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
            if (!this.IsClipTypeAcceptable(clip))
                throw new Exception("Invalid clip for this layer");

            if (addToModel) {
                this.Model.InsertClip(index, clip.Model);
            }

            clip.Track = this;
            this.clips.Insert(index, clip);
            ClipViewModel.RaiseTrackChanged(clip);
        }

        public bool RemoveClipFromTrack(ClipViewModel clip) {
            int index = this.clips.IndexOf(clip);
            if (index < 0)
                return false;
            this.RemoveClipFromTrack(index);
            return true;
        }

        public ClipViewModel RemoveClipFromTrack(int index) {
            ClipViewModel clip = this.clips[index];
            if (!ReferenceEquals(this, clip.Track))
                throw new Exception($"Clip track does not match the current instance: {clip.Track} != {this}");
            if (!ReferenceEquals(this.Model.Clips[index], clip.Model))
                throw new Exception($"Clip model clip list desynchronized");

            this.Model.RemoveClipAt(index);
            this.clips.RemoveAt(index);
            clip.Track = null;
            ClipViewModel.RaiseTrackChanged(clip);
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

            if (confirm && !await IoC.MessageDialogs.ShowYesNoDialogAsync($"Delete clip{Lang.S(list.Count)}?", $"Are you sure you want to delete {(list.Count == 1 ? "1 clip" : $"{list.Count} clips")}?")) {
                return;
            }

            await this.DisposeAndRemoveItemsAction(list);
        }

        public async Task DisposeAndRemoveItemsAction(IEnumerable<ClipViewModel> list) {
            try {
                this.DisposeAndRemoveItemsUnsafe(list.ToList());
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageExAsync("Error", "An error occurred while removing clips", e.GetToString());
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
                    this.RemoveClipFromTrack(index);
                    if (clip is IDisposable disposable) {
                        try {
                            disposable.Dispose();
                        }
                        catch (Exception e) {
                            stack.Add(new Exception($"Failed to dispose {clip.GetType()} properly", e));
                        }
                    }
                }
            }
        }

        public void Dispose() {
            using (ErrorList stack = new ErrorList("Exception disposing track")) {
                try {
                    this.DisposeCore(stack);
                }
                catch (Exception e) {
                    stack.Add(new Exception(nameof(this.DisposeCore) + " method unexpectedly threw", e));
                }
            }
        }

        protected virtual void DisposeCore(ErrorList stack) {
            using (ErrorList innerStack = new ErrorList("Exception disposing a clip", false)) {
                for (int i = this.clips.Count - 1; i >= 0; i--) {
                    ClipViewModel clip = this.clips[i];

                    try {
                        this.RemoveClipFromTrack(i);
                    }
                    catch (Exception e) {
                        innerStack.Add(new Exception("Failed to remove clip from track", e));
                    }

                    try {
                        clip.Dispose();
                    }
                    catch (Exception e) {
                        innerStack.Add(new Exception($"Failed to dispose clip: {clip}", e));
                    }
                }

                this.clips.Clear();
                this.Model.Clips.Clear();
                if (innerStack.TryGetException(out Exception ex)) {
                    stack.Add(ex);
                }
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

            this.Model.Clips.MoveItem(index, endIndex);
            this.clips.Move(index, endIndex);
        }

        public abstract bool CanDropResource(ResourceItemViewModel resource);

        public abstract Task OnResourceDropped(ResourceItemViewModel resource, long frameBegin);

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

        public static void RaiseTimelineChanged(TrackViewModel track) {
            track.RaisePropertyChanged(nameof(Timeline));
            track.RaisePropertyChanged(nameof(Project));
            track.RaisePropertyChanged(nameof(Editor));
            track.RaisePropertyChanged(nameof(HistoryManager));
            track.RaisePropertyChanged(nameof(AutomationEngine));
        }

        public async Task<bool> RenameAsync() {
            string result = await IoC.UserInput.ShowSingleInputDialogAsync("Change track name", "Input a new track name:", this.DisplayName ?? "", this.Timeline.TrackNameValidator);
            if (result != null) {
                this.DisplayName = result;
                return true;
            }

            return false;
        }
    }
}