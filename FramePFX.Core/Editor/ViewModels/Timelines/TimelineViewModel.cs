using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FramePFX.Core.Automation;
using FramePFX.Core.Automation.ViewModels;
using FramePFX.Core.Editor.Registries;
using FramePFX.Core.Editor.Timelines;
using FramePFX.Core.Editor.Timelines.Tracks;
using FramePFX.Core.Editor.ViewModels.Timelines.Tracks;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs.Message;
using FramePFX.Core.Views.Dialogs.UserInputs;

namespace FramePFX.Core.Editor.ViewModels.Timelines
{
    public class TimelineViewModel : BaseViewModel, IAutomatableViewModel, IProjectViewModelBound, IDisposable
    {
        public static readonly MessageDialog DeleteTracksDialog;

        private readonly ObservableCollectionEx<TrackViewModel> tracks;
        public ReadOnlyObservableCollection<TrackViewModel> Tracks { get; }

        private List<TrackViewModel> selectedTracks;

        public List<TrackViewModel> SelectedTracks
        {
            get => this.selectedTracks;
            set
            {
                this.RaisePropertyChanged(ref this.selectedTracks, value);
                this.RemoveSelectedTracksCommand.RaiseCanExecuteChanged();
            }
        }

        private TrackViewModel primarySelectedTrack;
        private volatile int isPlayBackUiUpdateScheduledState;

        public TrackViewModel PrimarySelectedTrack
        {
            get => this.primarySelectedTrack;
            set => this.RaisePropertyChanged(ref this.primarySelectedTrack, value);
        }

        public long PlayHeadFrame
        {
            get => this.Model.PlayHeadFrame;
            set
            {
                long oldValue = this.Model.PlayHeadFrame;
                if (oldValue == value)
                {
                    return;
                }

                if (value >= this.MaxDuration)
                {
                    value = this.MaxDuration - 1;
                }

                if (value < 0)
                {
                    value = 0;
                }

                this.Model.PlayHeadFrame = value;
                this.RaisePropertyChanged();
                this.OnUserSeekedPlayHead(oldValue, value, true);
            }
        }

        public long MaxDuration
        {
            get => this.Model.MaxDuration;
            set
            {
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

        public bool IsAutomationChangeInProgress
        {
            get => this.Model.IsAutomationChangeInProgress;
            set => this.Model.IsAutomationChangeInProgress = value;
        }

        public InputValidator TrackNameValidator { get; }

        static TimelineViewModel()
        {
            DeleteTracksDialog = Dialogs.YesCancelDialog.Clone();
            DeleteTracksDialog.ShowAlwaysUseNextResultOption = true;
        }

        private readonly Action CachedDoRenderAndScheduleUpdatePlayHead;

        public TimelineViewModel(ProjectViewModel project, Timeline model)
        {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.Project = project ?? throw new ArgumentNullException(nameof(project));
            this.AutomationData = new AutomationDataViewModel(this, model.AutomationData);
            this.tracks = new ObservableCollectionEx<TrackViewModel>();
            this.Tracks = new ReadOnlyObservableCollection<TrackViewModel>(this.tracks);
            this.selectedTracks = new List<TrackViewModel>();
            this.RemoveSelectedTracksCommand = new AsyncRelayCommand(this.RemoveSelectedTracksAction, () => this.SelectedTracks.Count > 0);
            this.MoveSelectedUpCommand = new RelayCommand(this.MoveSelectedItemUpAction);
            this.MoveSelectedDownCommand = new RelayCommand(this.MoveSelectedItemDownAction);
            this.AddVideoTrackCommand = new AsyncRelayCommand(this.AddNewVideoTrackAction);
            this.AddAudioTrackCommand = new AsyncRelayCommand(this.AddNewAudioTrackAction, () => false);
            this.TrackNameValidator = InputValidator.FromFunc((x) => string.IsNullOrEmpty(x) ? "Clip name cannot be empty" : null);
            this.CachedDoRenderAndScheduleUpdatePlayHead = this.DoFullRenderAndScheduleUIUpdate;
            foreach (Track track in this.Model.Tracks)
            {
                TrackViewModel trackVm = TrackRegistry.Instance.CreateViewModelFromModel(track);
                trackVm.Timeline = this;
                this.tracks.Add(trackVm);
                TrackViewModel.RaiseTimelineChanged(trackVm);
            }
        }

        public void AddTrack(TrackViewModel track) => this.InsertTrack(this.tracks.Count, track);

        public void InsertTrack(int index, TrackViewModel track)
        {
            this.Model.InsertTrack(index, track.Model);
            track.Timeline = this;
            this.tracks.Insert(index, track);
            TrackViewModel.RaiseTimelineChanged(track);
            this.OnProjectModified();
        }

        public void OnUserSeekedPlayHead(long oldFrame, long newFrame, bool? schedule)
        {
            if (oldFrame == newFrame || !(schedule is bool b))
            {
                return;
            }

            foreach (TrackViewModel track in this.tracks)
            {
                track.OnUserSeekedFrame(oldFrame, newFrame);
            }

            this.Project.AutomationEngine.UpdateAndRefreshAt(true, newFrame);
            this.Project.Editor.View.Render(b);
        }

        // TODO: Could optimise this, maybe create "chunks" of clips that span 10 frame sections across the entire timeline
        public IEnumerable<ClipViewModel> GetClipsAtPlayHead()
        {
            return this.GetClipsAtFrame(this.PlayHeadFrame);
        }

        public IEnumerable<ClipViewModel> GetClipsAtFrame(long frame)
        {
            return this.Tracks.SelectMany(track => track.GetClipsAtFrame(frame));
        }

        public IEnumerable<ClipViewModel> GetSelectedClips()
        {
            return this.tracks.SelectMany(x => x.SelectedClips);
        }

        public Task<VideoTrackViewModel> AddNewVideoTrackAction() => this.InsertNewVideoTrackAction(this.tracks.Count);

        public Task<AudioTrackViewModel> AddNewAudioTrackAction() => this.InsertNewAudioTrackAction(this.tracks.Count);

        public async Task<VideoTrackViewModel> InsertNewVideoTrackAction(int index)
        {
            VideoTrackViewModel track = new VideoTrackViewModel(new VideoTrack());
            this.InsertTrack(index, track);
            await this.DoRenderAsync(true);
            return track;
        }

        public async Task<AudioTrackViewModel> InsertNewAudioTrackAction(int index)
        {
            AudioTrackViewModel track = new AudioTrackViewModel(new AudioTrack());
            this.InsertTrack(index, track);
            return track;
        }

        public void DoRender(bool schedule = false)
        {
            this.Project.AutomationEngine.UpdateAndRefreshAt(true, this.PlayHeadFrame);
            this.Project.Editor.DoRenderFrame(schedule);
        }

        public async Task DoRenderAsync(bool schedule)
        {
            this.Project.AutomationEngine.UpdateAndRefreshAt(true, this.PlayHeadFrame);
            await this.Project.Editor.DoRenderFrame(schedule);
        }

        public Task RemoveSelectedTracksAction()
        {
            return this.RemoveSelectedTracksAction(true);
        }

        public async Task RemoveSelectedTracksAction(bool confirm)
        {
            IList<TrackViewModel> list = this.SelectedTracks;
            if (list.Count < 1)
            {
                return;
            }

            if (confirm && await DeleteTracksDialog.ShowAsync("Delete tracks?", $"Are you sure you want to delete {list.Count} track{Lang.S(list.Count)}?") != "yes")
            {
                return;
            }

            this.RemoveTracks(list.ToList());
        }

        public void RemoveTracks(IEnumerable<TrackViewModel> list)
        {
            foreach (TrackViewModel item in list)
            {
                this.Model.RemoveTrack(item.Model);
                item.Timeline = null;
                this.tracks.Remove(item);
                TrackViewModel.RaiseTimelineChanged(item);
            }

            this.OnProjectModified();
        }

        public virtual void MoveSelectedItems(int offset)
        {
            if (offset == 0 || this.SelectedTracks.Count < 1)
            {
                return;
            }

            List<int> selection = new List<int>();
            foreach (TrackViewModel item in this.SelectedTracks)
            {
                int index = this.tracks.IndexOf(item);
                if (index < 0)
                {
                    continue;
                }

                selection.Add(index);
            }

            if (offset > 0)
            {
                selection.Sort((a, b) => b.CompareTo(a));
            }
            else
            {
                selection.Sort((a, b) => a.CompareTo(b));
            }

            for (int i = 0; i < selection.Count; i++)
            {
                int target = selection[i] + offset;
                if (target < 0 || target >= this.tracks.Count || selection.Contains(target))
                {
                    continue;
                }

                this.tracks.Move(selection[i], target);
                this.Model.Tracks.MoveItem(selection[i], target);
                selection[i] = target;
            }

            this.OnProjectModified();
        }

        public virtual void MoveSelectedItemUpAction() => this.MoveSelectedItems(-1);

        public virtual void MoveSelectedItemDownAction() => this.MoveSelectedItems(1);

        protected virtual void OnSelectionChanged()
        {
            this.RemoveSelectedTracksCommand.RaiseCanExecuteChanged();
        }

        public void OnStepFrameCallback() => IoC.Dispatcher.Invoke(this.CachedDoRenderAndScheduleUpdatePlayHead);

        private void DoFullRenderAndScheduleUIUpdate()
        {
            VideoEditorViewModel editor = this.Project.Editor;
            if (editor == null)
            {
                return;
            }

            this.Model.PlayHeadFrame = Periodic.Add(this.PlayHeadFrame, 1, 0L, this.MaxDuration);
            this.Project.AutomationEngine.Model.UpdateAt(this.PlayHeadFrame);

            editor.DoRenderFrame().ContinueWith((t) =>
            {
                if (Interlocked.CompareExchange(ref this.isPlayBackUiUpdateScheduledState, 1, 0) != 0)
                {
                    return;
                }

                IoC.Dispatcher.InvokeLaterAsync(() =>
                {
                    this.Project.AutomationEngine.RefreshTimeline(this.Project.AutomationEngine.Project.Timeline, this.PlayHeadFrame);
                    this.RaisePropertyChanged(nameof(this.PlayHeadFrame));
                    this.isPlayBackUiUpdateScheduledState = 0;
                });
            });
        }

        public void Dispose()
        {
            using (ErrorList stack = new ErrorList("Exception disposing timeline"))
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
            using (ErrorList innerStack = new ErrorList(false))
            {
                foreach (TrackViewModel track in this.tracks)
                {
                    try
                    {
                        track.Dispose();
                    }
                    catch (Exception e)
                    {
                        innerStack.Add(e);
                    }
                }

                this.tracks.Clear();
                this.Model.ClearTracks();
                if (innerStack.TryGetException(out Exception ex))
                {
                    stack.Add(ex);
                }
            }
        }

        public TrackViewModel GetPrevious(TrackViewModel track)
        {
            int index = this.tracks.IndexOf(track);
            return index > 0 ? this.tracks[index - 1] : null;
        }

        public void MoveClip(ClipViewModel clip, TrackViewModel oldTrack, TrackViewModel newTrack)
        {
            if (!oldTrack.RemoveClipFromTrack(clip))
                throw new Exception("Clip was not present in the old track");
            newTrack.AddClip(clip);
        }

        public void OnProjectModified()
        {
        }
    }
}