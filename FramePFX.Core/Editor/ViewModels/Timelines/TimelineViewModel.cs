using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Core.Automation;
using FramePFX.Core.Automation.ViewModels;
using FramePFX.Core.Editor.History;
using FramePFX.Core.Editor.Registries;
using FramePFX.Core.Editor.Timelines;
using FramePFX.Core.Editor.Timelines.Tracks;
using FramePFX.Core.Editor.ViewModels.Timelines.Tracks;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs.UserInputs;

namespace FramePFX.Core.Editor.ViewModels.Timelines {
    public class TimelineViewModel : BaseViewModel, IAutomatableViewModel, IDisposable {
        private readonly ObservableCollectionEx<TrackViewModel> tracks;
        public ReadOnlyObservableCollection<TrackViewModel> Tracks { get; }

        public ObservableCollectionEx<TrackViewModel> SelectedTracks { get; }

        private TrackViewModel primarySelectedTrack;
        private volatile bool ignorePlayHeadPropertyChange;
        private volatile bool isFramePropertyChangeScheduled;

        public TrackViewModel PrimarySelectedTrack {
            get => this.primarySelectedTrack;
            set => this.RaisePropertyChanged(ref this.primarySelectedTrack, value);
        }

        public long PlayHeadFrame {
            get => this.Model.PlayHeadFrame;
            set {
                long oldValue = this.Model.PlayHeadFrame;
                if (oldValue == value) {
                    return;
                }

                if (value >= this.MaxDuration) {
                    value = this.MaxDuration - 1;
                }

                if (value < 0) {
                    value = 0;
                }

                this.Model.PlayHeadFrame = value;
                if (!this.ignorePlayHeadPropertyChange) {
                    this.RaisePropertyChanged();
                    this.OnPlayHeadMoved(oldValue, value, true);
                }
            }
        }

        public long MaxDuration {
            get => this.Model.MaxDuration;
            set {
                this.Model.MaxDuration = value;
                this.RaisePropertyChanged();
            }
        }

        public AsyncRelayCommand RemoveSelectedTracksCommand { get; }
        public RelayCommand MoveSelectedUpCommand { get; }
        public RelayCommand MoveSelectedDownCommand { get; }

        public AsyncRelayCommand AddVideoTrackCommand { get; }
        public AsyncRelayCommand AddAudioTrackCommand { get; }

        public Timeline Model { get; }

        IAutomatable IAutomatableViewModel.AutomationModel => this.Model;

        public AutomationDataViewModel AutomationData { get; }

        public ProjectViewModel Project { get; }

        public AutomationEngineViewModel AutomationEngine => this.Project?.AutomationEngine;

        public bool IsAutomationChangeInProgress {
            get => this.Model.IsAutomationChangeInProgress;
            set => this.Model.IsAutomationChangeInProgress = value;
        }

        public InputValidator TrackNameValidator { get; }

        /// <summary>
        /// A flag used when handing clip drag events so that other clips know if they are being dragged by a source clip (multi-clip drag)
        /// </summary>
        public bool IsGloballyDragging { get; set; }

        public bool IsAboutToDragAcrossTracks { get; set; }

        public ClipViewModel ProcessingDragEventClip { get; set; }

        public List<ClipViewModel> DraggingClips { get; set; }

        public List<HistoryVideoClipPosition> DragStopHistoryList { get; set; }

        public TimelineViewModel(ProjectViewModel project, Timeline model) {
            this.Project = project ?? throw new ArgumentNullException(nameof(project));
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.AutomationData = new AutomationDataViewModel(this, model.AutomationData);
            this.tracks = new ObservableCollectionEx<TrackViewModel>();
            this.Tracks = new ReadOnlyObservableCollection<TrackViewModel>(this.tracks);
            this.SelectedTracks = new ObservableCollectionEx<TrackViewModel>();
            this.SelectedTracks.CollectionChanged += (sender, args) => {
                this.RemoveSelectedTracksCommand.RaiseCanExecuteChanged();
            };
            this.RemoveSelectedTracksCommand = new AsyncRelayCommand(this.RemoveSelectedTracksAction, () => this.SelectedTracks.Count > 0);
            this.MoveSelectedUpCommand = new RelayCommand(this.MoveSelectedItemUpAction);
            this.MoveSelectedDownCommand = new RelayCommand(this.MoveSelectedItemDownAction);
            this.AddVideoTrackCommand = new AsyncRelayCommand(this.AddVideoTrackAction);
            this.AddAudioTrackCommand = new AsyncRelayCommand(this.AddAudioTrackAction, () => false);
            this.TrackNameValidator = InputValidator.FromFunc((x) => string.IsNullOrEmpty(x) ? "Clip name cannot be empty" : null);
            foreach (Track track in this.Model.Tracks) {
                this.tracks.Add(TrackRegistry.Instance.CreateViewModelFromModel(this, track));
            }
        }

        public void AddTrack(TrackViewModel track, bool addToModel = true) {
            if (addToModel)
                this.Model.AddTrack(track.Model);
            this.tracks.Add(track);
            this.OnProjectModified();
        }

        public void OnPlayHeadMoved(long oldFrame, long newFrame, bool? schedule) {
            if (oldFrame == newFrame || !(schedule is bool b)) {
                return;
            }

            this.Project.AutomationEngine.TickAndRefreshProjectAtFrame(false, newFrame);
            this.Project.Editor.View.Render(b);
        }

        // TODO: Could optimise this, maybe create "chunks" of clips that span 10 frame sections across the entire timeline
        public IEnumerable<ClipViewModel> GetClipsAtPlayHead() {
            return this.GetClipsAtFrame(this.PlayHeadFrame);
        }

        public IEnumerable<ClipViewModel> GetClipsAtFrame(long frame) {
            return this.Tracks.SelectMany(track => track.GetClipsAtFrame(frame));
        }

        public IEnumerable<ClipViewModel> GetSelectedClips() {
            return this.tracks.SelectMany(x => x.SelectedClips);
        }

        public async Task<VideoTrackViewModel> AddVideoTrackAction() {
            VideoTrackViewModel track = new VideoTrackViewModel(this, new VideoTrack(this.Model));
            this.AddTrack(track);
            this.DoRender(true);
            return track;
        }

        public async Task<AudioTrackViewModel> AddAudioTrackAction() {
            AudioTrackViewModel track = new AudioTrackViewModel(this, new AudioTrack(this.Model));
            this.AddTrack(track);
            return track;
        }

        public void DoRender(bool schedule = false) {
            this.Project.Editor?.DoRender(schedule);
        }

        public Task RemoveSelectedTracksAction() {
            return this.RemoveSelectedTracksAction(true);
        }

        public async Task RemoveSelectedTracksAction(bool confirm) {
            IList<TrackViewModel> list = this.SelectedTracks;
            if (list.Count < 1) {
                return;
            }

            string msg = list.Count == 1 ? "1 track" : $"{list.Count} tracks";
            if (confirm && !await IoC.MessageDialogs.ShowYesNoDialogAsync("Delete tracks?", $"Are you sure you want to delete {msg}?")) {
                return;
            }

            this.RemoveTracks(list.ToList());
        }

        public void RemoveTracks(IEnumerable<TrackViewModel> list) {
            foreach (TrackViewModel item in list) {
                this.Model.RemoveTrack(item.Model);
                this.tracks.Remove(item);
            }

            this.OnProjectModified();
        }

        public virtual void MoveSelectedItems(int offset) {
            if (offset == 0 || this.SelectedTracks.Count < 1) {
                return;
            }

            List<int> selection = new List<int>();
            foreach (TrackViewModel item in this.SelectedTracks) {
                int index = this.tracks.IndexOf(item);
                if (index < 0) {
                    continue;
                }

                selection.Add(index);
            }

            if (offset > 0) {
                selection.Sort((a, b) => b.CompareTo(a));
            }
            else {
                selection.Sort((a, b) => a.CompareTo(b));
            }

            for (int i = 0; i < selection.Count; i++) {
                int target = selection[i] + offset;
                if (target < 0 || target >= this.tracks.Count || selection.Contains(target)) {
                    continue;
                }

                this.tracks.Move(selection[i], target);
                this.Model.Tracks.MoveItem(selection[i], target);
                selection[i] = target;
            }

            this.OnProjectModified();
        }

        public virtual void MoveSelectedItemUpAction() {
            this.MoveSelectedItems(-1);
        }

        public virtual void MoveSelectedItemDownAction() {
            this.MoveSelectedItems(1);
        }

        protected virtual void OnSelectionChanged() {
            this.RemoveSelectedTracksCommand.RaiseCanExecuteChanged();
        }

        public void OnStepFrameCallback() {
            this.StepFrameAsync().Wait();
        }

        public void StepFrame(long change = 1L, bool schedule = false) {
            this.ignorePlayHeadPropertyChange = true;
            long oldFrame = this.PlayHeadFrame;
            this.PlayHeadFrame = Periodic.Add(oldFrame, change, 0L, this.MaxDuration);

            this.Project.AutomationEngine.TickAndRefreshProjectAtFrame(false, this.PlayHeadFrame);
            this.Project.Editor.View.Render(schedule);

            if (!this.isFramePropertyChangeScheduled) {
                this.isFramePropertyChangeScheduled = true;
                IoC.Dispatcher.Invoke(() => {
                    this.RaisePropertyChanged(nameof(this.PlayHeadFrame));
                    this.isFramePropertyChangeScheduled = false;

                });
            }

            this.ignorePlayHeadPropertyChange = false;
        }

        public async Task StepFrameAsync(long change = 1L) {
            this.ignorePlayHeadPropertyChange = true;
            long oldFrame = this.PlayHeadFrame;
            this.PlayHeadFrame = Periodic.Add(oldFrame, change, 0L, this.MaxDuration);

            this.Project.AutomationEngine.TickAndRefreshProjectAtFrame(false, this.PlayHeadFrame);
            await this.Project.Editor.View.RenderAsync();

            if (!this.isFramePropertyChangeScheduled) {
                this.isFramePropertyChangeScheduled = true;
                IoC.Dispatcher.InvokeAsync(() => {
                    this.RaisePropertyChanged(nameof(this.PlayHeadFrame));
                    this.isFramePropertyChangeScheduled = false;
                });
            }

            this.ignorePlayHeadPropertyChange = false;
        }

        public void Dispose() {
            using (ExceptionStack stack = new ExceptionStack("Exception disposing timeline")) {
                try {
                    this.DisposeCore(stack);
                }
                catch (Exception e) {
                    stack.Add(new Exception(nameof(this.DisposeCore) + " method unexpectedly threw", e));
                }
            }
        }

        protected virtual void DisposeCore(ExceptionStack stack) {
            using (ExceptionStack innerStack = new ExceptionStack(false)) {
                foreach (TrackViewModel clip in this.tracks) {
                    try {
                        clip.Dispose();
                    }
                    catch (Exception e) {
                        innerStack.Add(e);
                    }
                }

                this.tracks.Clear();
                this.Model.ClearTracks();
                if (innerStack.TryGetException(out Exception ex)) {
                    stack.Add(ex);
                }
            }
        }

        public TrackViewModel GetPrevious(TrackViewModel track) {
            int index = this.tracks.IndexOf(track);
            return index > 0 ? this.tracks[index - 1] : null;
        }

        public void MoveClip(ClipViewModel clip, TrackViewModel oldTrack, TrackViewModel newTrack) {
            oldTrack.RemoveClipFromTrack(clip);
            newTrack.AddClipToTrack(clip);
        }

        public void OnProjectModified() {
        }
    }
}