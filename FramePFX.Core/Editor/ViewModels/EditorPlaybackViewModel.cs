using System;
using System.Threading.Tasks;

namespace FramePFX.Core.Editor.ViewModels {
    /// <summary>
    /// A view model responsible for handling the state of the playback (play, pause, etc)
    /// </summary>
    public class EditorPlaybackViewModel : BaseViewModel, IDisposable {
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

        public EditorPlaybackViewModel(VideoEditorViewModel editor) {
            this.Editor = editor ?? throw new ArgumentNullException(nameof(editor));
            this.Model = new EditorPlaybackModel(editor.Model);

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
            this.Model.FrameStepTimer.Start(this.UsePrecisionTimingMode);
        }

        public Task StopRenderTimer() {
            return this.Model.FrameStepTimer.StopAsync();
        }

        public async Task PlayAction() {
            if (this.IsPlaying || this.Project == null) {
                return;
            }

            this.StartRenderTimer();
            this.IsPlaying = true;
            this.UpdatePlaybackCommands();
        }

        public async Task PauseAction() {
            if (!this.IsPlaying || this.Project == null) {
                return;
            }

            await this.StopRenderTimer();
            this.IsPlaying = false;
            this.UpdatePlaybackCommands();
        }

        public async Task StopAction() {
            if (!this.IsPlaying || this.Project == null) {
                return;
            }

            await this.StopRenderTimer();
            this.IsPlaying = false;
            this.UpdatePlaybackCommands();
        }

        public Task TogglePlayAction() {
            if (this.Project == null) {
                return Task.CompletedTask;
            }

            if (this.IsPlaying) {
                return IoC.App.UserSettings.StopOnTogglePlay ? this.StopAction() : this.PauseAction();
            }
            else {
                return this.PlayAction();
            }
        }

        public async Task OnProjectChanging(ProjectViewModel project) {
            if (this.IsPlaying) {
                await this.StopRenderTimer();
                this.IsPlaying = false;
                this.UpdatePlaybackCommands();
            }

            if (project == null) {
                await this.Model.FrameStepTimer.StopAsync();
            }

            if (this.Project != null) {
                await this.Project.DisposeAsync();
            }
        }

        public async Task OnProjectChanged(ProjectViewModel project) {
            if (project != null) {
                this.SetTimerFrameRate(project.Settings.FrameRate);
            }
            else {
                await this.Model.FrameStepTimer.StopAsync();
            }

            this.UpdatePlaybackCommands();
        }

        public void SetTimerFrameRate(double frameRate) {
            if (frameRate < 1)
                frameRate = 1;
            this.Model.FrameStepTimer.Interval = (long) Math.Round(1000d / frameRate);
        }

        private async Task SwitchPrecisionMode() {
            this.UsePrecisionTimingMode = !this.UsePrecisionTimingMode;
            await this.Model.FrameStepTimer.RestartAsync(this.UsePrecisionTimingMode);
        }

        public void Dispose() {
            this.Model.FrameStepTimer.Dispose();
        }

        public async Task OnProjectSaving() {
            await this.StopRenderTimer();
            this.UpdatePlaybackCommands();
        }

        public async Task OnProjectSaved() {
            this.StartRenderTimer();
            this.UpdatePlaybackCommands();
        }
    }
}