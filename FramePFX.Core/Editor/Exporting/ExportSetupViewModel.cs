using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using FramePFX.Core.Editor.Exporting.Exporters;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs;
using FramePFX.Core.Views.Windows;

namespace FramePFX.Core.Editor.Exporting
{
    public class ExportSetupViewModel : BaseDialogViewModel
    {
        public ReadOnlyObservableCollection<ExporterViewModel> Exporters { get; }

        private ExporterViewModel selectedExporter;

        public ExporterViewModel SelectedExporter
        {
            get => this.selectedExporter;
            set
            {
                this.RaisePropertyChanged(ref this.selectedExporter, value);
                this.RunExportCommand.RaiseCanExecuteChanged();
            }
        }

        private string filePath;

        public string FilePath
        {
            get => this.filePath;
            set => this.RaisePropertyChanged(ref this.filePath, value);
        }

        private FrameSpan renderSpan;

        public FrameSpan RenderSpan
        {
            get => this.renderSpan;
            set
            {
                this.RaisePropertyChanged(ref this.renderSpan, value);
                this.RaisePropertyChanged(nameof(this.FrameBegin));
                this.RaisePropertyChanged(nameof(this.FrameEndIndex));
                this.RaisePropertyChanged(nameof(this.Duration));
            }
        }

        public long FrameBegin
        {
            get => this.RenderSpan.Begin;
            set => this.RenderSpan = this.RenderSpan.WithBeginIndex(Math.Min(value, this.FrameEndIndex));
        }

        public long FrameEndIndex
        {
            get => this.RenderSpan.EndIndex;
            set => this.RenderSpan = this.RenderSpan.WithEndIndex(Maths.Clamp(value, this.FrameBegin, this.MaxEndIndex));
        }

        public long Duration => this.RenderSpan.Duration;

        public long MaxEndIndex => this.Project.Timeline.MaxDuration - 1;

        public AsyncRelayCommand RunExportCommand { get; }

        public AsyncRelayCommand CancelSetupCommand { get; }

        public Project Project { get; }

        public ExportSetupViewModel(Project project)
        {
            this.Project = project ?? throw new ArgumentNullException(nameof(project));
            this.RunExportCommand = new AsyncRelayCommand(this.ExportActionAsync, () => this.SelectedExporter != null);
            this.CancelSetupCommand = new AsyncRelayCommand(this.CancelSetupAction);
            ObservableCollection<ExporterViewModel> collection = new ObservableCollection<ExporterViewModel>() {
                new FFmpegExportViewModel()
            };

            this.Exporters = new ReadOnlyObservableCollection<ExporterViewModel>(collection);
            foreach (ExporterViewModel e in this.Exporters)
            {
                e.LoadProjectDefaults(project);
            }

            this.SelectedExporter = collection[0];
            this.FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Video.mp4");
        }

        private async Task CancelSetupAction()
        {
            await this.Dialog.CloseDialogAsync(false);
        }

        public async Task ExportActionAsync()
        {
            ExporterViewModel exporter = this.SelectedExporter;
            if (exporter == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(this.FilePath))
            {
                await IoC.MessageDialogs.ShowMessageAsync("File Path", "No file path provided");
                return;
            }

            ExportProperties properties = new ExportProperties(this.RenderSpan, this.FilePath);
            ExportProgressViewModel export = new ExportProgressViewModel(properties);
            IWindow window = IoC.Provide<IExportViewService>().ShowExportWindow(export);

            // await IoC.MessageDialogs.ShowMessageExAsync("Export failed", "Failed to export video", e.GetToString())
#if DEBUG
            await Task.Run(() =>
            {
                exporter.Exporter.Export(this.Project, export, properties);
            });

            await window.CloseWindowAsync();
            await this.Dialog.CloseDialogAsync(true);
#else
            try {
                await Task.Run(() => {
                    exporter.Exporter.Export(this.Project, export, new ExportProperties(this.RenderSpan, this.FilePath));
                });
                await window.CloseWindowAsync();
            }
            catch (Exception e) {
                await window.CloseWindowAsync();
                await IoC.MessageDialogs.ShowMessageExAsync("Export failure", "Failed to export:", e.GetToString());
            }
            await this.Dialog.CloseDialogAsync(true);
#endif
        }
    }
}