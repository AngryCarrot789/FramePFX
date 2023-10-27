using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.AdvancedContextService;
using FramePFX.AdvancedContextService.NCSP;
using FramePFX.Automation;
using FramePFX.Automation.ViewModels;
using FramePFX.Commands;
using FramePFX.Editor.Registries;
using FramePFX.Editor.Timelines;
using FramePFX.Editor.Timelines.Tracks;
using FramePFX.Editor.ViewModels.Timelines.Tracks;
using FramePFX.Logger;
using FramePFX.PropertyEditing;
using FramePFX.ServiceManaging;
using FramePFX.Utils;
using FramePFX.Views.Dialogs.Message;
using FramePFX.Views.Dialogs.UserInputs;

namespace FramePFX.Editor.ViewModels.Timelines {
    public class TimelineViewModel : BaseViewModel, IAutomatableViewModel, IProjectViewModelBound {
        public static readonly MessageDialog DeleteTracksDialog;
        private readonly PropertyChangedEventHandler CachedTrackPropertyChangedHandler;
        private readonly RapidDispatchCallback rapidOnRenderCompleted;
        private readonly ObservableCollectionEx<TrackViewModel> tracks;
        private TrackViewModel primarySelectedTrack;
        private bool isRecordingKeyFrames;
        private long lastPlayHeadSeek;
        public long InternalLastPlayHeadBeforePlaying; // used for play/pause/stop

        public bool IsSeekingFrame;

        TimelineViewModel ITimelineViewModelBound.Timeline => this;
        IAutomatable IAutomatableViewModel.AutomationModel => this.Model;

        /// <summary>
        /// The project that this timeline was created in. This will only ever be set once (after the timeline constructor)
        /// </summary>
        public ProjectViewModel Project { get; private set; }

        public ReadOnlyObservableCollection<TrackViewModel> Tracks { get; }

        public ObservableCollection<TrackViewModel> SelectedTracks { get; }

        public TrackViewModel PreviouslySelectedTrack { get; private set; }

        public TrackViewModel PrimarySelectedTrack {
            get => this.primarySelectedTrack;
            set {
                bool flag = ReferenceEquals(this.PreviouslySelectedTrack, this.primarySelectedTrack);
                if (flag && ReferenceEquals(this.primarySelectedTrack, value)) {
                    return;
                }

                if (!flag) {
                    this.PreviouslySelectedTrack = this.primarySelectedTrack;
                }

                this.RaisePropertyChanged(ref this.primarySelectedTrack, value);
                if (!flag) {
                    this.RaisePropertyChanged(nameof(this.PreviouslySelectedTrack));
                }
            }
        }

        public AutomationDataViewModel AutomationData { get; }

        public long PlayHeadFrame {
            get => this.Model.PlayHeadFrame;
            set => this.SetPlayHead(value);
        }

        /// <summary>
        /// The frame at which the playhead was last seeked to
        /// </summary>
        public long LastPlayHeadSeek {
            get => this.lastPlayHeadSeek;
            set => this.RaisePropertyChanged(ref this.lastPlayHeadSeek, value);
        }

        /// <summary>
        /// The maximum duration of this timeline. Automatically adjusted when required (e.g. dragging clips around)
        /// </summary>
        public long MaxDuration {
            get => this.Model.MaxDuration;
            set {
                this.Model.MaxDuration = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Whether or not to add new key frames when a parameter is modified during playback. Default is false
        /// </summary>
        public bool IsRecordingKeyFrames {
            get => this.isRecordingKeyFrames;
            set => this.RaisePropertyChanged(ref this.isRecordingKeyFrames, value);
        }

        public string DisplayName {
            get => this.Model.DisplayName;
            set {
                this.Model.DisplayName = value;
                this.RaisePropertyChanged();
            }
        }

        private bool autoScrollOnClipDrag;

        public bool AutoScrollOnClipDrag {
            get => this.autoScrollOnClipDrag;
            set => this.RaisePropertyChanged(ref this.autoScrollOnClipDrag, value);
        }

        private bool autoScrollDuringPlayback;

        public bool AutoScrollDuringPlayback {
            get => this.autoScrollDuringPlayback;
            set => this.RaisePropertyChanged(ref this.autoScrollDuringPlayback, value);
        }

        public long LargestFrameInUse => this.Model.LargestFrameInUse;

        public bool IsAutomationRefreshInProgress { get; set; }

        public double UnitZoom { get; set; } = 1d;

        public double LastRenderMillis { get; private set; }

        public AsyncRelayCommand RemoveSelectedTracksCommand { get; }
        public RelayCommand MoveSelectedUpCommand { get; }
        public RelayCommand MoveSelectedDownCommand { get; }
        public AsyncRelayCommand AddVideoTrackCommand { get; }
        public AsyncRelayCommand AddAudioTrackCommand { get; }

        public InputValidator TrackNameValidator { get; }

        public Timeline Model { get; }

        public Rational FPS => this.Project.Settings.FrameRate;

        public bool IsHistoryChanging { get; set; }

        public event TimelineEventHandler ClipSelectionChanged;

        private Task updateAndRenderTask;

        public TimelineViewModel(Timeline model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.CachedTrackPropertyChangedHandler = this.OnTrackPropertyChanged;
            this.rapidOnRenderCompleted = new RapidDispatchCallback(this.RefreshAutomationAndPlayhead, ExecutionPriority.Normal, "TimelineRapidCallback");
            this.AutomationData = new AutomationDataViewModel(this, model.AutomationData);
            this.AutomationData.SetActiveSequenceFromModelDeserialisation();
            this.tracks = new ObservableCollectionEx<TrackViewModel>();
            this.Tracks = new ReadOnlyObservableCollection<TrackViewModel>(this.tracks);
            this.SelectedTracks = new ObservableCollection<TrackViewModel>();
            this.SelectedTracks.CollectionChanged += (sender, args) => {
                this.RemoveSelectedTracksCommand.RaiseCanExecuteChanged();
                PFXPropertyEditorRegistry.Instance.OnTrackSelectionChanged(this.SelectedTracks.ToList());
            };

            this.RemoveSelectedTracksCommand = new AsyncRelayCommand(this.RemoveSelectedTracksAction, () => this.SelectedTracks.Count > 0);
            this.MoveSelectedUpCommand = new RelayCommand(this.MoveSelectedTrackUp);
            this.MoveSelectedDownCommand = new RelayCommand(this.MoveSelectedTrackDown);
            this.AddVideoTrackCommand = new AsyncRelayCommand(this.AddNewVideoTrackAction);
            this.AddAudioTrackCommand = new AsyncRelayCommand(this.AddNewAudioTrackAction, () => false);
            this.TrackNameValidator = Validators.ForNonEmptyString("Track name cannot be an empty string");
            foreach (Track track in this.Model.Tracks) {
                TrackViewModel trackVm = TrackFactory.Instance.CreateViewModelFromModel(track);
                trackVm.Timeline = this;
                this.tracks.Add(trackVm);
                this.InternalOnTrackAdded(trackVm);
                TrackViewModel.OnTimelineChanged(trackVm);
            }

            this.UpdateAndRefreshLargestFrame();
        }

        static TimelineViewModel() {
            DeleteTracksDialog = Dialogs.YesCancelDialog.Clone();
            DeleteTracksDialog.ShowAlwaysUseNextResultOption = true;

            IContextRegistration reg = ContextRegistry.Instance.RegisterType(typeof(TimelineViewModel));
            reg.AddEntry(new ActionContextEntry(null, "actions.editor.NewVideoTrack", "Add Video track"));
            reg.AddEntry(new ActionContextEntry(null, "actions.editor.NewAudioTrack", "Add Audio Track"));
        }

        public void SetPlayHead(long frame, bool setLastSeekFrame = true) {
            if (setLastSeekFrame)
                this.LastPlayHeadSeek = frame;
            this.OnUserSeekedPlayHead(this.Model.PlayHeadFrame, frame);
        }

        public void SetProject(ProjectViewModel project) {
            ProjectViewModel oldProject = this.Project;
            if (ReferenceEquals(oldProject, project)) {
                return;
            }

            this.OnProjectChanging(project);
            this.Project = project;
            foreach (TrackViewModel track in this.Tracks) {
                TrackViewModel.OnTimelineProjectChanged(track, oldProject, project);
            }

            this.OnProjectChanged(oldProject);
        }

        protected virtual void OnProjectChanging(ProjectViewModel newProject) {
        }

        protected virtual void OnProjectChanged(ProjectViewModel oldProject) {
        }

        public void AddTrack(TrackViewModel track) => this.InsertTrack(this.tracks.Count, track);

        public void InsertTrack(int index, TrackViewModel track) {
            this.Model.InsertTrack(index, track.Model);
            track.Timeline = this;
            this.tracks.Insert(index, track);
            this.InternalOnTrackAdded(track);
            TrackViewModel.OnTimelineChanged(track);
        }

        private async void OnUserSeekedPlayHead(long oldFrame, long newFrame) {
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

            this.IsSeekingFrame = true;
            try {
                await this.UpdateAndRenderTimelineToEditor(true);
            }
            catch (TaskCanceledException) {
                // do nothing
            }
            finally {
                this.IsSeekingFrame = false;
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

        public IEnumerable<ClipViewModel> GetSelectedClipsAtFrame(long frame) {
            return this.tracks.SelectMany(x => x.GetSelectedClipsAtFrame(frame));
        }

        public Task<VideoTrackViewModel> AddNewVideoTrackAction() => this.InsertNewVideoTrackAction(this.tracks.Count);
        public Task<VideoTrackViewModel> AddNewVideoTrackAction(bool render) => this.InsertNewVideoTrackAction(this.tracks.Count, render);

        public Task<AudioTrackViewModel> AddNewAudioTrackAction() => this.InsertNewAudioTrackAction(this.tracks.Count);

        public Task<VideoTrackViewModel> InsertNewVideoTrackAction(int index, bool render = true) {
            VideoTrackViewModel track = new VideoTrackViewModel(new VideoTrack() {DisplayName = "Video Track " + (this.tracks.Count + 1)});
            this.InsertTrack(index, track);
            if (render && this.Project?.Editor != null) {
                this.InvalidateAutomationAndRender();
            }

            return Task.FromResult(track);
        }

        public Task<AudioTrackViewModel> InsertNewAudioTrackAction(int index) {
            AudioTrackViewModel track = new AudioTrackViewModel(new AudioTrack() {DisplayName = "Audio Track " + (this.tracks.Count + 1)});
            this.InsertTrack(index, track);
            return Task.FromResult(track);
        }

        /// <summary>
        /// Schedules an automation update and render at some point in the future (during a nearby application tick)
        /// </summary>
        public void InvalidateAutomationAndRender() {
            Task task = this.updateAndRenderTask;
            if (task != null && !task.IsCompleted || Services.Application.IsDispatcherSuspended) {
                return;
            }

            this.updateAndRenderTask = null;
            if (this.GetEditor(out VideoEditorViewModel editor, out _)) {
                this.updateAndRenderTask = Services.Application.Invoke(() => this.UpdateAndRenderTimelineToEditorInternal(editor), ExecutionPriority.Normal);
            }
        }

        /// <summary>
        /// Performs an automation engine tick and then renders the state of the timeline to the editor,
        /// optionally allowing the render to be scheduled for the future or to happen right now
        /// </summary>
        /// <param name="shouldScheduleRender">
        /// Schedules the rendering to be done at some point in the future, instead of immediately
        /// </param>
        public Task UpdateAndRenderTimelineToEditor(bool shouldScheduleRender = false) {
            Task task = this.updateAndRenderTask;
            if (task != null && !task.IsCompleted)
                return task;
            if (this.GetEditor(out VideoEditorViewModel editor, out _))
                return this.UpdateAndRenderTimelineToEditorInternal(editor, shouldScheduleRender);
            return Task.CompletedTask;
        }

        private async Task UpdateAndRenderTimelineToEditorInternal(VideoEditorViewModel editor, bool shouldScheduleRender = false) {
            AutomationEngine.UpdateTimeline(this.Model, this.PlayHeadFrame);
            try {
                await editor.DoDrawRenderFrame(this, shouldScheduleRender);
            }
            catch (TaskCanceledException) {
                // do nothing
            }

            this.RefreshAutomationAndPlayhead();
        }

        public Task RemoveSelectedTracksAction() {
            return this.RemoveSelectedTracksAction(true);
        }

        public async Task RemoveSelectedTracksAction(bool confirm) {
            IList<TrackViewModel> list = this.SelectedTracks;
            if (list.Count < 1) {
                return;
            }

            string s = Lang.S(list.Count);
            int totalClips = list.Sum(x => x.Clips.Count);
            string msg = $"Are you sure you want to delete {list.Count} track{s}{(totalClips > 0 ? $" with {totalClips} clips" : "")}?";
            if (confirm && await DeleteTracksDialog.ShowAsync($"Delete track{s}?", msg) != "yes") {
                return;
            }

            this.RemoveTracks(list.ToList());
        }

        public async Task RemoveTracksAction(IEnumerable<TrackViewModel> enumerable, bool confirm) {
            List<TrackViewModel> list = enumerable as List<TrackViewModel> ?? enumerable.ToList();
            if (list.Count > 0) {
                string s = Lang.S(list.Count);
                int totalClips = list.Sum(x => x.Clips.Count);
                string msg = $"Are you sure you want to delete {list.Count} track{s}{(totalClips > 0 ? $" with {totalClips} clips" : "")}?";
                if (confirm && await DeleteTracksDialog.ShowAsync($"Delete track{s}?", msg) != "yes") {
                    return;
                }

                this.RemoveTracks(list);
            }
        }

        public void RemoveTracks(IEnumerable<TrackViewModel> enumerable) {
            List<TrackViewModel> list = enumerable as List<TrackViewModel> ?? enumerable.ToList();
            foreach (TrackViewModel item in list) {
                if (this.Model.RemoveTrack(item.Model)) {
                    item.Timeline = null;
                    if (this.tracks.Remove(item)) {
                        this.InternalOnTrackRemoved(item);
                        TrackViewModel.OnTimelineChanged(item);
                    }
                    else {
                        AppLogger.WriteLine("Unexpected failure to remove track from timeline");
                        AppLogger.WriteLine(Environment.StackTrace);
                    }
                }
            }
        }

        public bool RemoveTrack(TrackViewModel track) {
            int index = this.tracks.IndexOf(track);
            if (index == -1)
                return false;
            this.RemoveTrackAt(index);
            return true;
        }

        public void RemoveTrackAt(int index) {
            TrackViewModel track = this.tracks[index];
            this.Model.RemoveTrackAt(index);
            track.Timeline = null;
            this.tracks.RemoveAt(index);
            this.InternalOnTrackRemoved(track);
            TrackViewModel.OnTimelineChanged(track);
            this.InvalidateAutomationAndRender();
        }

        public void MoveSelectedTrack(int offset) {
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

            this.InvalidateAutomationAndRender();
        }

        public void MoveSelectedTrackUp() => this.MoveSelectedTrack(-1);

        public void MoveSelectedTrackDown() => this.MoveSelectedTrack(1);

        public void OnStepFrameCallback() {
            VideoEditorViewModel editor = this.Project.Editor;
            if (editor == null) {
                AppLogger.WriteLine("[FATAL] Attempted to render a timeline without a video editor associated with it");
                AppLogger.WriteLine(Environment.StackTrace);
                return;
            }

            this.Model.PlayHeadFrame = Periodic.Add(this.PlayHeadFrame, 1, 0L, this.MaxDuration);
            AutomationEngine.UpdateTimeline(this.Model, this.PlayHeadFrame);
            editor.DoDrawRenderFrame(this).ContinueWith(t => this.rapidOnRenderCompleted.Invoke());
        }

        public void RefreshAutomationAndPlayhead() {
            AutomationEngine.RefreshTimeline(this, this.PlayHeadFrame);
            this.RaisePropertyChanged(nameof(this.PlayHeadFrame));

            this.LastRenderMillis = Math.Round(this.Model.LastRenderDurationTicks / Time.TICK_PER_MILLIS_D, 3);
            this.RaisePropertyChanged(nameof(this.LastRenderMillis));
        }

        public virtual void ClearAndDispose() {
            for (int i = this.tracks.Count - 1; i >= 0; i--) {
                TrackViewModel track = this.tracks[i];
                this.RemoveTrackAt(i);
                track.ClearAndDispose();
            }
        }

        private void InternalOnTrackAdded(TrackViewModel track) {
            track.PropertyChanged += this.CachedTrackPropertyChangedHandler;
            this.UpdateAndRefreshLargestFrame();
        }

        private void InternalOnTrackRemoved(TrackViewModel track) {
            track.PropertyChanged -= this.CachedTrackPropertyChangedHandler;
            this.UpdateAndRefreshLargestFrame();
        }

        private void UpdateAndRefreshLargestFrame() {
            this.Model.UpdateLargestFrame();
            this.RaisePropertyChanged(nameof(this.LargestFrameInUse));
        }

        private void OnTrackPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(TrackViewModel.LargestFrameInUse)) {
                this.Model.UpdateLargestFrame();
                this.RaisePropertyChanged(nameof(this.LargestFrameInUse));
            }
        }

        public override string ToString() => $"{this.GetType().Name} ({this.Model})";

        public TrackViewModel GetTrackByModel(Track track) {
            int index = this.Model.IndexOfTrack(track);
            return index == -1 ? null : this.tracks[index];
        }

        public void OnSelectionChanged() {
            this.ClipSelectionChanged?.Invoke(this);
        }

        private bool GetEditor(out VideoEditorViewModel editor, out ProjectViewModel project) {
            if ((project = this.Project) == null) {
                AppLogger.WriteLine("Tried to tick and render timeline without an associated project:\n" + Environment.StackTrace);
                editor = null;
                return false;
            }

            if ((editor = project.Editor) == null) {
                AppLogger.WriteLine("Tried to tick and render timeline whose project has no editor associated:\n" + Environment.StackTrace);
                return false;
            }

            return true;
        }
    }
}