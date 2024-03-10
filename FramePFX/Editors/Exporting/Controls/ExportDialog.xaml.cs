//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FramePFX.Logger;
using FramePFX.Utils;
using FramePFX.Views;

namespace FramePFX.Editors.Exporting.Controls
{
    /// <summary>
    /// Interaction logic for ExportDialog.xaml
    /// </summary>
    public partial class ExportDialog : WindowEx
    {
        public static readonly DependencyProperty ExportSetupProperty = DependencyProperty.Register("ExportSetup", typeof(ExportSetup), typeof(ExportDialog), new PropertyMetadata(null, (d, e) => ((ExportDialog) d).OnSetupChanged((ExportSetup) e.OldValue, (ExportSetup) e.NewValue)));
        private bool isProcessingFrameSpanControls;
        private bool isProcessingFilePathControl;
        private bool isUpdatingComboBox;

        public ExportSetup ExportSetup
        {
            get => (ExportSetup) this.GetValue(ExportSetupProperty);
            set => this.SetValue(ExportSetupProperty, value);
        }

        public ExportDialog()
        {
            this.InitializeComponent();
            this.PART_BeginFrameDragger.ValueChanged += this.BeginFrameDraggerOnValueChanged;
            this.PART_EndFrameDragger.ValueChanged += this.EndFrameDraggerOnValueChanged;
            this.PART_FilePathTextBox.TextChanged += this.FilePathTextBoxOnTextChanged;
            this.PART_ComboBox.SelectionChanged += this.PART_ComboBoxOnSelectionChanged;
        }

        private void PART_ComboBoxOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.isUpdatingComboBox || !(this.ExportSetup is ExportSetup setup))
            {
                return;
            }

            this.isUpdatingComboBox = true;
            setup.SelectedExporterIndex = this.PART_ComboBox.SelectedIndex;
            this.isUpdatingComboBox = false;
        }

        private void BeginFrameDraggerOnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.isProcessingFrameSpanControls)
                return;

            this.isProcessingFrameSpanControls = true;
            ExportProperties props = this.ExportSetup.Properties;
            props.Span = props.Span.MoveBegin((long) e.NewValue);
            this.isProcessingFrameSpanControls = false;
        }

        private void EndFrameDraggerOnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.isProcessingFrameSpanControls)
                return;

            this.isProcessingFrameSpanControls = true;
            ExportProperties props = this.ExportSetup.Properties;
            props.Span = props.Span.MoveEndIndex((long) e.NewValue);
            this.isProcessingFrameSpanControls = false;
        }

        private void FilePathTextBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.isProcessingFilePathControl)
                return;

            this.isProcessingFilePathControl = true;
            ExportProperties props = this.ExportSetup.Properties;
            props.FilePath = this.PART_FilePathTextBox.Text;
            this.isProcessingFilePathControl = false;
        }

        private void UpdateBeginFrameDragger()
        {
            ExportSetup setup = this.ExportSetup;
            ExportProperties props = setup.Properties;
            this.PART_EndFrameDragger.Maximum = setup.Timeline.MaxDuration;
            this.PART_EndFrameDragger.Minimum = props.Span.Begin;
            this.PART_EndFrameDragger.Value = props.Span.EndIndex;
            this.PART_DurationTextBlock.Text = props.Span.Duration.ToString();
        }

        private void UpdateEndFrameDragger()
        {
            ExportProperties props = this.ExportSetup.Properties;
            this.PART_BeginFrameDragger.Maximum = props.Span.EndIndex;
            this.PART_BeginFrameDragger.Minimum = 0;
            this.PART_BeginFrameDragger.Value = props.Span.Begin;
            this.PART_DurationTextBlock.Text = props.Span.Duration.ToString();
        }

        private void OnSetupChanged(ExportSetup oldSetup, ExportSetup newSetup)
        {
            if (oldSetup != null)
            {
                oldSetup.Properties.SpanChanged -= this.PropertiesOnSpanChanged;
                oldSetup.Properties.FilePathChanged -= this.PropertiesOnFilePathChanged;
                oldSetup.SelectedExporterIndexChanged -= this.OnSelectedExporterChanged;
                this.SetCurrentExporterContent(null);
                this.PART_ComboBox.Items.Clear();
            }

            if (newSetup != null)
            {
                newSetup.Properties.SpanChanged += this.PropertiesOnSpanChanged;
                newSetup.Properties.FilePathChanged += this.PropertiesOnFilePathChanged;
                newSetup.SelectedExporterIndexChanged += this.OnSelectedExporterChanged;
                foreach (Exporter exporter in newSetup.Exporters)
                {
                    this.PART_ComboBox.Items.Add(exporter.DisplayName);
                }

                this.PART_ComboBox.SelectedIndex = 0;
                this.PART_FilePathTextBox.Text = newSetup.Properties.FilePath;
                this.UpdateBeginFrameDragger();
                this.UpdateEndFrameDragger();
                this.SetCurrentExporterContent(newSetup.SelectedExporter);
                if (newSetup.Timeline == newSetup.Project.MainTimeline || !(newSetup.Timeline is CompositionTimeline composition))
                {
                    this.Title = "Export Project";
                }
                else
                {
                    string title = composition.Resource.DisplayName;
                    this.Title = string.IsNullOrWhiteSpace(title) ? "Export Composition Timeline" : $"Export '{title}' timeline";
                }
            }
        }

        private void OnSelectedExporterChanged(ExportSetup sender)
        {
            this.SetCurrentExporterContent(sender.SelectedExporter);
            this.isUpdatingComboBox = true;
            this.PART_ComboBox.SelectedIndex = sender.SelectedExporterIndex;
            this.isUpdatingComboBox = false;
        }

        private void SetCurrentExporterContent(Exporter newExporter)
        {
            if (this.Content is ExporterContent oldContent)
            {
                oldContent.Disconnected();
                this.PART_ExportContentPresenter.Content = null;
            }

            if (newExporter != null)
            {
                ExporterContent content = ExporterContent.NewInstance(newExporter.GetType());
                this.PART_ExportContentPresenter.Content = content;
                content.Measure(this.DesiredSize);
                content.InvalidateMeasure();
                content.Connected(newExporter);
            }
        }

        private void PropertiesOnSpanChanged(ExportProperties sender)
        {
            this.UpdateBeginFrameDragger();
            this.UpdateEndFrameDragger();
        }

        private void PropertiesOnFilePathChanged(ExportProperties sender)
        {
            this.PART_FilePathTextBox.Text = sender.FilePath;
        }

        private bool isRendering;
        private CancellationTokenSource exportToken;

        private async void Export_Click(object sender, RoutedEventArgs e)
        {
            if (this.isRendering)
            {
                return;
            }

            int index = this.PART_ComboBox.SelectedIndex;
            if (index == -1)
            {
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

            try
            {
                setup.Project.IsExporting = true;
                // Export will most likely be using unsafe code, meaning async won't work
                await Task.Factory.StartNew(() =>
                {
                    exporter.Export(setup, progressDialog, setup.Properties, this.exportToken.Token);
                }, TaskCreationOptions.LongRunning);
            }
            catch (TaskCanceledException)
            {
                isCancelled = true;
            }
            catch (OperationCanceledException)
            {
                isCancelled = true;
            }
            catch (Exception ex)
            {
                string err = ex.GetToString();
                AppLogger.Instance.WriteLine("Error exporting: " + err);
                IoC.MessageService.ShowMessage("Export failure", "An exception occurred while exporting video", err);
            }
            finally
            {
                setup.Project.IsExporting = false;
            }

            if (isCancelled && File.Exists(setup.Properties.FilePath))
            {
                try
                {
                    File.Delete(setup.Properties.FilePath);
                }
                catch (Exception ex)
                {
                    AppLogger.Instance.WriteLine("Failed to delete cancelled export's file: " + ex.GetToString());
                }
            }

            this.isRendering = false;
            setup.Timeline.RenderManager.InvalidateRender();
            ((Button) sender).IsEnabled = true;
            progressDialog.Close();
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.exportToken?.Cancel();
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            this.ExportSetup = null;
            this.exportToken?.Dispose();
        }
    }
}