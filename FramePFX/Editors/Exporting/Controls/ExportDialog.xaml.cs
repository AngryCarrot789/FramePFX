using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FramePFX.Logger;
using FramePFX.Utils;
using FramePFX.Views;

namespace FramePFX.Editors.Exporting.Controls {
    /// <summary>
    /// Interaction logic for ExportDialog.xaml
    /// </summary>
    public partial class ExportDialog : WindowEx {
        public static readonly DependencyProperty ExportSetupProperty = DependencyProperty.Register("ExportSetup", typeof(ExportSetup), typeof(ExportDialog), new PropertyMetadata(null, (d, e) => ((ExportDialog) d).OnSetupChanged((ExportSetup) e.OldValue, (ExportSetup) e.NewValue)));
        private bool isUpdatingControl;

        public ExportSetup ExportSetup {
            get => (ExportSetup) this.GetValue(ExportSetupProperty);
            set => this.SetValue(ExportSetupProperty, value);
        }

        public ExportDialog() {
            this.InitializeComponent();
            this.PART_BeginFrameDragger.ValueChanged += this.BeginFrameDraggerOnValueChanged;
            this.PART_EndFrameDragger.ValueChanged += this.EndFrameDraggerOnValueChanged;
        }

        private void BeginFrameDraggerOnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (this.isUpdatingControl)
                return;

            this.isUpdatingControl = true;
            ExportProperties props = this.ExportSetup.Properties;
            props.Span = props.Span.MoveBegin((long) e.NewValue);
            this.isUpdatingControl = false;
        }

        private void EndFrameDraggerOnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (this.isUpdatingControl)
                return;

            this.isUpdatingControl = true;
            ExportProperties props = this.ExportSetup.Properties;
            props.Span = props.Span.MoveEndIndex((long) e.NewValue);
            this.isUpdatingControl = false;
        }

        private void UpdateBeginFrameDragger() {
            ExportSetup setup = this.ExportSetup;
            ExportProperties props = setup.Properties;
            this.PART_EndFrameDragger.Maximum = setup.Project.MainTimeline.MaxDuration;
            this.PART_EndFrameDragger.Minimum = props.Span.Begin;
            this.PART_EndFrameDragger.Value = props.Span.EndIndex;
            this.PART_DurationTextBlock.Text = props.Span.Duration.ToString();
        }

        private void UpdateEndFrameDragger() {
            ExportProperties props = this.ExportSetup.Properties;
            this.PART_BeginFrameDragger.Maximum = props.Span.EndIndex;
            this.PART_BeginFrameDragger.Minimum = 0;
            this.PART_BeginFrameDragger.Value = props.Span.Begin;
            this.PART_DurationTextBlock.Text = props.Span.Duration.ToString();
        }

        private void OnSetupChanged(ExportSetup oldSetup, ExportSetup newSetup) {
            if (oldSetup != null) {
                oldSetup.Properties.SpanChanged -= this.PropertiesOnSpanChanged;
                oldSetup.Properties.FilePathChanged -= this.PropertiesOnFilePathChanged;
                this.PART_ComboBox.Items.Clear();
            }

            if (newSetup != null) {
                newSetup.Properties.SpanChanged += this.PropertiesOnSpanChanged;
                newSetup.Properties.FilePathChanged += this.PropertiesOnFilePathChanged;
                foreach (Exporter exporter in newSetup.Exporters) {
                    this.PART_ComboBox.Items.Add(exporter.DisplayName);
                }

                this.PART_ComboBox.SelectedIndex = 0;
                this.PART_FilePathTextBox.Text = newSetup.Properties.FilePath;
                this.UpdateBeginFrameDragger();
                this.UpdateEndFrameDragger();
            }
        }

        private void PropertiesOnSpanChanged(ExportProperties sender) {
            this.UpdateBeginFrameDragger();
            this.UpdateEndFrameDragger();
        }

        private void PropertiesOnFilePathChanged(ExportProperties sender) {
            this.PART_FilePathTextBox.Text = sender.FilePath;
        }

        private bool isRendering;
        private CancellationTokenSource exportToken;

        private async void Export_Click(object sender, RoutedEventArgs e) {
            if (this.isRendering) {
                return;
            }

            int index = this.PART_ComboBox.SelectedIndex;
            if (index == -1) {
                return;
            }

            this.isRendering = true;
            ((Button) sender).IsEnabled = false;
            ExportSetup setup = this.ExportSetup;
            Exporter exporter = setup.Exporters[index];
            setup.Project.Editor.Playback.Pause();

            bool isCancelled = false;
            this.exportToken = new CancellationTokenSource();

            ExportProgressDialog progressDialog = new ExportProgressDialog(setup.Properties.Span, this.exportToken);
            progressDialog.Show();

            try {
                setup.Project.IsExporting = true;
                // Export will most likely be using unsafe code, meaning async won't work
                await Task.Factory.StartNew(() => {
                    exporter.Export(setup.Project, progressDialog, setup.Properties, this.exportToken.Token);
                }, TaskCreationOptions.LongRunning);
            }
            catch (TaskCanceledException) {
                isCancelled = true;
            }
            catch (Exception ex) {
                string err = ex.GetToString();
                AppLogger.Instance.WriteLine("Error exporting: " + err);
                MessageBox.Show("An error occurred while exporting: " + err, "Export failure");
            }
            finally {
                setup.Project.IsExporting = false;
            }

            if (isCancelled && File.Exists(setup.Properties.FilePath)) {
                try {
                    File.Delete(setup.Properties.FilePath);
                }
                catch (Exception ex) {
                    AppLogger.Instance.WriteLine("Failed to delete cancelled export's file: " + ex.GetToString());
                }
            }

            this.isRendering = false;
            setup.Project.RenderManager.InvalidateRender();
            ((Button) sender).IsEnabled = true;
            progressDialog.Close();
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            this.exportToken?.Cancel();
            this.Close();
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            this.ExportSetup = null;
            this.exportToken?.Dispose();
        }
    }
}
