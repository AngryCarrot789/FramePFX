using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FramePFX.Automation;
using FramePFX.Automation.ViewModels;
using FramePFX.Commands;
using FramePFX.Editor.Registries;
using FramePFX.Editor.Timelines;
using FramePFX.Editor.Timelines.Tracks;
using FramePFX.Editor.ViewModels.Timelines.Tracks;
using FramePFX.Utils;
using FramePFX.Views.Dialogs.Message;
using FramePFX.Views.Dialogs.UserInputs;

namespace FramePFX.Editor.ViewModels.Timelines {
    public class TimelineViewModel : BaseViewModel, IAutomatableViewModel, IProjectViewModelBound, IDisposable {
        public static readonly MessageDialog DeleteTracksDialog;
        private volatile bool isRendering;

        private readonly ObservableCollectionEx<TrackViewModel> tracks;
        public ReadOnlyObservableCollection<TrackViewModel> Tracks { get; }

        public ObservableCollection<TrackViewModel> SelectedTracks { get; }

        private TrackViewModel primarySelectedTrack;
        private volatile int isPlayBackUiUpdateScheduledState;

        public TrackViewModel PrimarySelectedTrack {
            get => this.primarySelectedTrack;
            set => this.RaisePropertyChanged(ref this.primarySelectedTrack, value);
        }

        public long PlayHeadFrame {
            get => this.Model.PlayHeadFrame;
            set {
                if (this.isRendering) {
                    this.RaisePropertyChanged();
                    return;
                }

                this.OnUserSeekedPlayHead(this.Model.PlayHeadFrame, value, true);
            }
        }

        public Rational FPS => this.Project.Settings.FrameRate;

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

        public bool IsAutomationRefreshInProgress { get; set; }

        public ProjectViewModel Project { get; set; }

        public bool IsAutomationChangeInProgress {
            get => this.Model.IsAutomationChangeInProgress;
            set => this.Model.IsAutomationChangeInProgress = value;
        }

        public InputValidator TrackNameValidator { get; }

        static TimelineViewModel() {
            DeleteTracksDialog = Dialogs.YesCancelDialog.Clone();
            DeleteTracksDialog.ShowAlwaysUseNextResultOption = true;
        }

        private readonly Action CachedDoRenderAndScheduleUpdatePlayHead;

        TimelineViewModel ITimelineViewModelBound.Timeline => this;

        public TimelineViewModel(Timeline model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.AutomationData = new AutomationDataViewModel(this, model.AutomationData);
            this.AutomationData.SetActiveSequenceFromModelDeserialisation();
            this.tracks = new ObservableCollectionEx<TrackViewModel>();
            this.Tracks = new ReadOnlyObservableCollection<TrackViewModel>(this.tracks);
            this.SelectedTracks = new ObservableCollection<TrackViewModel>();
            this.SelectedTracks.CollectionChanged += (sender, args) => {
                this.RemoveSelectedTracksCommand.RaiseCanExecuteChanged();
            };

            this.RemoveSelectedTracksCommand = new AsyncRelayCommand(this.RemoveSelectedTracksAction, () => this.SelectedTracks.Count > 0);
            this.MoveSelectedUpCommand = new RelayCommand(this.MoveSelectedItemUpAction);
            this.MoveSelectedDownCommand = new RelayCommand(this.MoveSelectedItemDownAction);
            this.AddVideoTrackCommand = new AsyncRelayCommand(this.AddNewVideoTrackAction);
            this.AddAudioTrackCommand = new AsyncRelayCommand(this.AddNewAudioTrackAction, () => false);
            this.TrackNameValidator = InputValidator.FromFunc((x) => string.IsNullOrEmpty(x) ? "Clip name cannot be empty" : null);
            this.CachedDoRenderAndScheduleUpdatePlayHead = this.DoFullRenderAndScheduleUIUpdate;
            foreach (Track track in this.Model.Tracks) {
                TrackViewModel trackVm = TrackRegistry.Instance.CreateViewModelFromModel(track);
                trackVm.Timeline = this;
                this.tracks.Add(trackVm);
                TrackViewModel.RaiseTimelineChanged(trackVm);
            }
        }

        /// <summary>
        /// Initialises this timeline
        /// </summary>
        public void Initialise() {

        }

        public void AddTrack(TrackViewModel track) => this.InsertTrack(this.tracks.Count, track);

        public void InsertTrack(int index, TrackViewModel track) {
            this.Model.InsertTrack(index, track.Model);
            track.Timeline = this;
            this.tracks.Insert(index, track);
            TrackViewModel.RaiseTimelineChanged(track);
            this.OnProjectModified();
        }

        public async void OnUserSeekedPlayHead(long oldFrame, long newFrame, bool? schedule) {
            if (newFrame >= this.MaxDuration) {
                newFrame = this.MaxDuration - 1;
            }

            if (newFrame < 0) {
                newFrame = 0;
            }

            if (oldFrame == newFrame) {
                return;
            }

            this.Model.PlayHeadFrame = newFrame;
            this.RaisePropertyChanged(nameof(this.PlayHeadFrame));

            foreach (TrackViewModel track in this.tracks) {
                track.OnUserSeekedFrame(oldFrame, newFrame);
            }

            if (schedule is bool b) {
                this.isRendering = true;
                try {
                    await this.DoRender(b);
                }
                catch (TaskCanceledException) {
                    // do nothing
                }
                finally {
                    this.isRendering = false;
                }
            }
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

        public Task<VideoTrackViewModel> AddNewVideoTrackAction() => this.InsertNewVideoTrackAction(this.tracks.Count);

        public Task<AudioTrackViewModel> AddNewAudioTrackAction() => this.InsertNewAudioTrackAction(this.tracks.Count);

        public async Task<VideoTrackViewModel> InsertNewVideoTrackAction(int index) {
            VideoTrackViewModel track = new VideoTrackViewModel(new VideoTrack() {DisplayName = "Video Track " + (this.tracks.Count + 1)});
            this.InsertTrack(index, track);
            await this.DoRender(true);
            return track;
        }

        public Task<AudioTrackViewModel> InsertNewAudioTrackAction(int index) {
            AudioTrackViewModel track = new AudioTrackViewModel(new AudioTrack() {DisplayName = "Audio Track " + (this.tracks.Count + 1)});
            this.InsertTrack(index, track);
            return Task.FromResult(track);
        }

        public Task DoRender(bool schedule = false) {
            AutomationEngine.UpdateAndRefreshProject(this.Project, this.PlayHeadFrame);
            return this.Project.Editor.DoRenderFrame(this, schedule);
        }

        public Task RemoveSelectedTracksAction() {
            return this.RemoveSelectedTracksAction(true);
        }

        public async Task RemoveSelectedTracksAction(bool confirm) {
            IList<TrackViewModel> list = this.SelectedTracks;
            if (list.Count < 1) {
                return;
            }

            if (confirm && await DeleteTracksDialog.ShowAsync("Delete tracks?", $"Are you sure you want to delete {list.Count} track{Lang.S(list.Count)}?") != "yes") {
                return;
            }

            this.RemoveTracks(list.ToList());
        }

        public void RemoveTracks(IEnumerable<TrackViewModel> list) {
            foreach (TrackViewModel item in list) {
                this.Model.RemoveTrack(item.Model);
                item.Timeline = null;
                this.tracks.Remove(item);
                TrackViewModel.RaiseTimelineChanged(item);
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
                this.Model.MoveTrackIndex(selection[i], target);
                selection[i] = target;
            }

            this.OnProjectModified();
        }

        public virtual void MoveSelectedItemUpAction() => this.MoveSelectedItems(-1);

        public virtual void MoveSelectedItemDownAction() => this.MoveSelectedItems(1);

        protected virtual void OnSelectionChanged() {
            this.RemoveSelectedTracksCommand.RaiseCanExecuteChanged();
        }

        public void OnStepFrameCallback() {
            this.CachedDoRenderAndScheduleUpdatePlayHead();
            // IoC.Dispatcher.Invoke(this.CachedDoRenderAndScheduleUpdatePlayHead);
        }

        private void DoFullRenderAndScheduleUIUpdate() {
            VideoEditorViewModel editor = this.Project.Editor;
            if (editor == null) {
                return;
            }

            this.Model.PlayHeadFrame = Periodic.Add(this.PlayHeadFrame, 1, 0L, this.MaxDuration);
            AutomationEngine.UpdateProject(this.Project.Model, this.PlayHeadFrame);
            editor.DoRenderFrame(this).ContinueWith(t => {
                if (Interlocked.CompareExchange(ref this.isPlayBackUiUpdateScheduledState, 1, 0) != 0) {
                    return;
                }

                Services.Application.InvokeAsync(() => {
                    AutomationEngine.RefreshTimeline(this, this.PlayHeadFrame);
                    this.RaisePropertyChanged(nameof(this.PlayHeadFrame));
                    this.isPlayBackUiUpdateScheduledState = 0;
                });
            });
        }

        public void Dispose() {
            using (ErrorList stack = new ErrorList("Exception disposing timeline")) {
                this.DisposeCore(stack);
            }
        }

        protected virtual void DisposeCore(ErrorList list) {
            using (ErrorList innerList = ErrorList.NoAutoThrow) {
                foreach (TrackViewModel track in this.tracks) {
                    try {
                        track.Dispose();
                    }
                    catch (Exception e) {
                        innerList.Add(e);
                    }
                }

                this.tracks.Clear();
                this.Model.ClearTracks();
                if (innerList.TryGetException(out Exception ex)) {
                    list.Add(ex);
                }
            }
        }

        public TrackViewModel GetPrevious(TrackViewModel track) {
            int index = this.tracks.IndexOf(track);
            return index > 0 ? this.tracks[index - 1] : null;
        }

        public static void MoveClip(ClipViewModel clip, TrackViewModel oldTrack, TrackViewModel newTrack) {
            oldTrack.MoveClipToTrack(clip, newTrack);
        }

        public void OnProjectModified() {
        }
    }
}