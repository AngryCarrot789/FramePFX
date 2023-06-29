using System;
using System.Threading.Tasks;
using NAudio.Wave;

namespace FramePFX.Core.Editor.ViewModels {
    /// <summary>
    /// A view model responsible for handling the state of the playback (play, pause, etc)
    /// </summary>
    public class EditorPlaybackViewModel : BaseViewModel, IModifyProject, IDisposable {
        /// <summary>
        /// The playback model that this view model delegates to and from
        /// </summary>
        public EditorPlaybackModel Model { get; }

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
                this.Model.IsPlaying = value;
                this.RaisePropertyChanged();
            }
        }

        public AsyncRelayCommand PlayCommand { get; }
        public AsyncRelayCommand PauseCommand { get; }
        public AsyncRelayCommand StopCommand { get; }
        public AsyncRelayCommand TogglePlayCommand { get; }
        public AsyncRelayCommand SwitchPrecisionTimingModeCommand { get; }

        private bool wasPlayingBeforeSave;

        public event ProjectModifiedEvent ProjectModified;

        public EditorPlaybackViewModel(VideoEditorViewModel editor) {
            this.Editor = editor ?? throw new ArgumentNullException(nameof(editor));
            this.Model = editor.Model.Playback;

            this.PlayCommand = new AsyncRelayCommand(this.PlayAction, () => this.Project != null && !this.Editor.IsProjectSaving && !this.IsPlaying);
            this.PauseCommand = new AsyncRelayCommand(this.PauseAction, () => this.Project != null && !this.Editor.IsProjectSaving &&  this.IsPlaying);
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

        public async Task StopRenderTimer() {
            await this.Model.PlaybackTimer.StopAsync();
            this.IsPlaying = false;
        }

        public async Task PlayAction() {
            if (this.IsPlaying || this.Project == null) {
                return;
            }

            this.StartRenderTimer();
            this.UpdatePlaybackCommands();
        }

        public async Task PauseAction() {
            if (!this.IsPlaying || this.Project == null) {
                return;
            }

            await this.StopRenderTimer();
            this.UpdatePlaybackCommands();
        }

        public async Task StopAction() {
            if (!this.IsPlaying || this.Project == null) {
                return;
            }

            await this.StopRenderTimer();
            this.UpdatePlaybackCommands();
        }

        public Task TogglePlayAction() {
            if (this.Project == null) {
                return Task.CompletedTask;
            }

            if (this.IsPlaying) {
                return IoC.App.Settings.StopOnTogglePlay ? this.StopAction() : this.PauseAction();
            }
            else {
                return this.PlayAction();
            }
        }

        public async Task OnProjectChanging(ProjectViewModel project) {
            if (this.Project !=null) {
                this.Project.Model.AudioEngine.Stop();
            }

            if (this.IsPlaying) {
                await this.StopRenderTimer();
                this.UpdatePlaybackCommands();
            }

            if (project == null) {
                await this.Model.PlaybackTimer.StopAsync();
            }
        }

        public async Task OnProjectChanged(ProjectViewModel project) {
            await this.Model.PlaybackTimer.StopAsync();
            if (project != null) {
                this.SetTimerFrameRate(project.Settings.FrameRate.AsDouble);
                ProjectSettingsModel settings = project.Settings.Model;
                project.Model.AudioEngine.Start(new WaveFormat(settings.SampleRate, settings.BitRate, settings.Channels));
            }

            this.UpdatePlaybackCommands();
            this.RaisePropertyChanged(nameof(this.Project));
        }

        public void SetTimerFrameRate(double frameRate) {
            if (frameRate <= 0d)
                throw new Exception("Frame rate must be non-zero");

            if (frameRate < 0.0001d)
                frameRate = 0.0001d; // ??????????

            this.Model.PlaybackTimer.Interval = (long) Math.Round(1000d / frameRate);
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

        public async Task OnProjectSaved(bool canResumePlaying = true) {
            if (canResumePlaying && this.wasPlayingBeforeSave) {
                this.StartRenderTimer();
            }

            this.wasPlayingBeforeSave = false;
            this.UpdatePlaybackCommands();
        }
    }
}