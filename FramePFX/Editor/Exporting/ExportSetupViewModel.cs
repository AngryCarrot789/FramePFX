using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Editor.Exporting.Exporters;
using FramePFX.Editor.ViewModels;
using FramePFX.Logger;
using FramePFX.Utils;
using FramePFX.Views.Dialogs;
using FramePFX.Views.Windows;

namespace FramePFX.Editor.Exporting {
    public class ExportSetupViewModel : BaseDialogViewModel {
        public ReadOnlyObservableCollection<ExporterViewModel> Exporters { get; }

        private ExporterViewModel selectedExporter;

        public ExporterViewModel SelectedExporter {
            get => this.selectedExporter;
            set {
                this.RaisePropertyChanged(ref this.selectedExporter, value);
                this.RunExportCommand.RaiseCanExecuteChanged();
            }
        }

        private string filePath;

        public string FilePath {
            get => this.filePath;
            set => this.RaisePropertyChanged(ref this.filePath, value);
        }

        private FrameSpan renderSpan;

        public FrameSpan RenderSpan {
            get => this.renderSpan;
            set {
                this.RaisePropertyChanged(ref this.renderSpan, value);
                this.RaisePropertyChanged(nameof(this.FrameBegin));
                this.RaisePropertyChanged(nameof(this.FrameEndIndex));
                this.RaisePropertyChanged(nameof(this.Duration));
            }
        }

        public long FrameBegin {
            get => this.RenderSpan.Begin;
            set => this.RenderSpan = this.RenderSpan.WithBeginIndex(Math.Min(value, this.FrameEndIndex));
        }

        public long FrameEndIndex {
            get => this.RenderSpan.EndIndex;
            set => this.RenderSpan = this.RenderSpan.WithEndIndex(Maths.Clamp(value, this.FrameBegin, this.MaxEndIndex));
        }

        public long Duration => this.RenderSpan.Duration;

        public long MaxEndIndex => this.Project.Timeline.MaxDuration - 1;

        public AsyncRelayCommand RunExportCommand { get; }

        public AsyncRelayCommand CancelSetupCommand { get; }

        public ProjectViewModel Project { get; }

        public ExportSetupViewModel(ProjectViewModel project) {
            this.Project = project ?? throw new ArgumentNullException(nameof(project));
            this.RunExportCommand = new AsyncRelayCommand(this.ExportActionAsync, () => this.SelectedExporter != null);
            this.CancelSetupCommand = new AsyncRelayCommand(this.CancelSetupAction);
            ObservableCollection<ExporterViewModel> collection = new ObservableCollection<ExporterViewModel>() {
                new FFmpegExportViewModel()
            };

            this.Exporters = new ReadOnlyObservableCollection<ExporterViewModel>(collection);
            foreach (ExporterViewModel e in this.Exporters) {
                e.LoadProjectDefaults(project.Model);
            }

            this.SelectedExporter = collection[0];
            string folder;
            if (project.Model.IsTempDataFolder || !Directory.Exists(project.TheProjectDataFolder)) {
                folder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            else {
                folder = project.TheProjectDataFolder;
            }

            string defaultPath = Path.Combine(folder, "Video.mp4");
            if (TextIncrement.GetIncrementableString(x => !File.Exists(x), defaultPath, out string fp))
                fp = defaultPath;
            this.FilePath = fp;
        }

        private async Task CancelSetupAction() {
            await this.Dialog.CloseDialogAsync(false);
        }

        public async Task ExportActionAsync() {
            ExporterViewModel exporter = this.SelectedExporter;
            if (exporter == null) {
                return;
            }

            if (string.IsNullOrEmpty(this.FilePath)) {
                await Services.DialogService.ShowMessageAsync("File Path", "No file path provided");
                return;
            }

            VideoEditorViewModel editor = this.Project.Editor;
            EditorPlaybackViewModel playback = editor.Playback;
            if (playback.IsPlaying) {
                await playback.PauseAction();
            }

            editor.View.OnExportBegin(true);

            CancellationTokenSource source = new CancellationTokenSource();
            this.Project.IsExporting = true;
            bool isCancelled = false;
            try {
                AppLogger.PushHeader("Begin Export");
                ExportProperties properties = new ExportProperties(this.RenderSpan, this.FilePath);
                ExportProgressViewModel export = new ExportProgressViewModel(properties, source);
                IWindow window = Services.GetService<IExportViewService>().ShowExportWindow(export);

                try {
                    // Export will most likely be using unsafe code, meaning async won't work
                    await Task.Factory.StartNew(
                        () => {
                            try {
                                editor.View.OnExportBegin(false);
                                exporter.Exporter.Export(this.Project.Model, export, new ExportProperties(this.RenderSpan, this.FilePath), source.Token);
                            }
                            finally {
                                try {
                                    editor.View.OnExportEnd();
                                }
                                catch {
                                    // ignored
                                }
                            }
                        },
                        TaskCreationOptions.LongRunning);
                }
                catch (TaskCanceledException) {
                    isCancelled = true;
                }
                catch (Exception e) {
                    string err = e.GetToString();
                    AppLogger.WriteLine("Error exporting: " + err);
                    await Services.DialogService.ShowMessageExAsync("Export failure", "An error occurred while exporting: ", err);
                }

                if (isCancelled && File.Exists(this.FilePath)) {
                    try {
                        File.Delete(this.FilePath);
                    }
                    catch (Exception e) {
                        AppLogger.WriteLine("Failed to delete cancelled export's file: " + e.GetToString());
                    }
                }

                await window.CloseWindowAsync();
                await this.Dialog.CloseDialogAsync(true);
            }
            finally {
                AppLogger.PopHeader();
                this.Project.IsExporting = false;
                source.Dispose();
            }
        }
    }
}