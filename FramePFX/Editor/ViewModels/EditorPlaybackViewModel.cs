using System;
using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Editor.Timelines;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor.ViewModels {
    /// <summary>
    /// A view model responsible for handling the state of the playback (play, pause, etc)
    /// </summary>
    public class EditorPlaybackViewModel : BaseViewModel, IModifyProject, IDisposable {
        /// <summary>
        /// The playback model that this view model delegates to and from
        /// </summary>
        public EditorPlayback Model { get; }

        /// <summary>
        /// The video editor that owns this playback
        /// </summary>
        public VideoEditorViewModel Editor { get; }

        public ProjectViewModel Project => this.Editor.ActiveProject;

        public bool UsePrecisionTimingMode {
            get => this.Model.UsePrecisionTimingMode;
            set {
                this.Model.UsePrecisionTimingMode = value;
                this.RaisePropertyChanged();
                this.ProjectModified?.Invoke(this, nameof(this.UsePrecisionTimingMode));
            }
        }

        public bool ZoomToCursor {
            get => this.Model.ZoomToCursor;
            set {
                this.Model.ZoomToCursor = value;
                this.RaisePropertyChanged();
                this.ProjectModified?.Invoke(this, nameof(this.ZoomToCursor));
            }
        }

        public bool IsPlaying {
            get => this.Model.IsPlaying;
            private set {
                if (this.IsPlaying == value)
                    return;
                this.Model.IsPlaying = value;
                this.RaisePropertyChanged();
            }
        }

        public AsyncRelayCommand PlayCommand { get; }
        public AsyncRelayCommand PlayFromStartCommand { get; }
        public AsyncRelayCommand PauseCommand { get; }
        public AsyncRelayCommand StopCommand { get; }
        public AsyncRelayCommand TogglePlayCommand { get; }
        public AsyncRelayCommand SwitchPrecisionTimingModeCommand { get; }

        private bool wasPlayingBeforeSave;
        // private long lastPlayTime;

        public event ProjectModifiedEvent ProjectModified;

        public EditorPlaybackViewModel(VideoEditorViewModel editor) {
            this.Editor = editor ?? throw new ArgumentNullException(nameof(editor));
            this.Model = editor.Model.Playback;

            this.PlayCommand = new AsyncRelayCommand(this.PlayAction, () => this.Project != null && !this.Editor.IsProjectSaving && !this.IsPlaying);
            this.PlayFromStartCommand = new AsyncRelayCommand(this.PlayFromStart, () => this.Project != null && !this.Editor.IsProjectSaving);
            this.PauseCommand = new AsyncRelayCommand(this.PauseAction, () => this.Project != null && !this.Editor.IsProjectSaving && this.IsPlaying);
            this.StopCommand = new AsyncRelayCommand(this.StopAction, () => this.Project != null && !this.Editor.IsProjectSaving && this.IsPlaying);
            this.TogglePlayCommand = new AsyncRelayCommand(this.TogglePlayAction, () => this.Project != null && !this.Editor.IsProjectSaving);
            this.SwitchPrecisionTimingModeCommand = new AsyncRelayCommand(this.SwitchPrecisionMode);
        }

        private void UpdatePlaybackCommands() {
            this.PlayCommand.RaiseCanExecuteChanged();
            this.PauseCommand.RaiseCanExecuteChanged();
            this.StopCommand.RaiseCanExecuteChanged();
            this.TogglePlayCommand.RaiseCanExecuteChanged();
        }

        public void StartRenderTimer() {
            this.Model.PlaybackTimer.Start(this.UsePrecisionTimingMode);
            this.IsPlaying = true;
        }

        public void StopPlaybackForChangingTimeline() {
            this.StopRenderTimer().ContinueWith(x => {
                this.UpdatePlaybackCommands();
            });
        }

        public async Task StopRenderTimer() {
            await this.Model.PlaybackTimer.StopAsync();
            this.IsPlaying = false;
        }

        public Task PlayAction() {
            TimelineViewModel timeline = this.Editor.ActiveTimeline;
            if (timeline != null && this.Project != null && !this.IsPlaying) {
                timeline.InternalLastPlayHeadBeforePlaying = timeline.PlayHeadFrame;
                this.PlayInternal();
            }

            return Task.CompletedTask;
        }

        private void PlayInternal() {
            this.StartRenderTimer();
            this.UpdatePlaybackCommands();
        }

        private Task PlayFromStart() {
            TimelineViewModel timeline = this.Editor.ActiveTimeline;
            if (timeline != null && this.Project != null) {
                if (!this.IsPlaying) {
                    timeline.InternalLastPlayHeadBeforePlaying = timeline.PlayHeadFrame;
                    timeline.PlayHeadFrame = 0;
                    this.PlayInternal();
                }
                else {
                    timeline.PlayHeadFrame = 0;
                }
            }

            return Task.CompletedTask;
        }

        public async Task PauseAction() {
            if (this.Project != null && this.IsPlaying) {
                await this.StopRenderTimer();
                this.UpdatePlaybackCommands();
                TimelineViewModel timeline = this.Editor.ActiveTimeline;
                if (timeline != null) {
                    timeline.InternalLastPlayHeadBeforePlaying = timeline.PlayHeadFrame;
                }
            }
        }

        public async Task StopAction() {
            if (this.Project != null && this.IsPlaying) {
                await this.StopRenderTimer();
                this.UpdatePlaybackCommands();
                TimelineViewModel timeline = this.Editor.ActiveTimeline;
                if (timeline != null) {
                    timeline.PlayHeadFrame = timeline.InternalLastPlayHeadBeforePlaying;
                }
            }
        }

        public Task TogglePlayAction() {
            if (this.Project == null) {
                return Task.CompletedTask;
            }

            if (this.IsPlaying) {
                return ApplicationViewModel.Instance.Settings.StopOnTogglePlay ? this.StopAction() : this.PauseAction();
            }
            else {
                return this.PlayAction();
            }
        }

        public async Task OnProjectChanging(ProjectViewModel project) {
            await this.StopRenderTimer();
            this.UpdatePlaybackCommands();
        }

        public async Task OnProjectChanged(ProjectViewModel project) {
            await this.Model.PlaybackTimer.StopAsync();
            if (project != null) {
                this.SetTimerFrameRate(project.Settings.FrameRate);
            }

            this.UpdatePlaybackCommands();
            this.RaisePropertyChanged(nameof(this.Project));
        }

        public void SetTimerFrameRate(Rational frameRate) {
            if (frameRate.den <= 0 || frameRate.num <= 0)
                throw new Exception("Frame rate must be greater than zero");
            this.Model.PlaybackTimer.Interval = (long) Math.Round(1000d / frameRate.ToDouble);
        }

        private async Task SwitchPrecisionMode() {
            this.UsePrecisionTimingMode = !this.UsePrecisionTimingMode;
            if (this.IsPlaying) {
                await this.Model.PlaybackTimer.RestartAsync(this.UsePrecisionTimingMode);
            }
        }

        public void Dispose() {
            this.Model.PlaybackTimer.Dispose();
        }

        public async Task OnProjectSaving() {
            this.wasPlayingBeforeSave = this.IsPlaying;
            if (this.IsPlaying) {
                await this.StopRenderTimer();
            }

            this.UpdatePlaybackCommands();
        }

        public Task OnProjectSaved(bool canResumePlaying = true) {
            if (canResumePlaying && this.wasPlayingBeforeSave) {
                this.StartRenderTimer();
            }

            this.wasPlayingBeforeSave = false;
            this.UpdatePlaybackCommands();
            return Task.CompletedTask;
        }
    }
}