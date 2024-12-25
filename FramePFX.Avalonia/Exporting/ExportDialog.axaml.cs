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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using FramePFX.Avalonia.Bindings;
using FramePFX.Avalonia.Themes.Controls;
using FramePFX.Editing.Exporting;
using FramePFX.Editing.Timelines;
using FramePFX.Services.Messaging;
using FramePFX.Tasks;
using FramePFX.Utils;

namespace FramePFX.Avalonia.Exporting;

public partial class ExportDialog : WindowEx {
    public static readonly DirectProperty<ExportDialog, bool> IsExportingProperty = AvaloniaProperty.RegisterDirect<ExportDialog, bool>(nameof(IsExporting), o => o.IsExporting);

    public bool IsExporting {
        get => this.myIsExporting;
        private set => this.SetAndRaise(IsExportingProperty, ref this.myIsExporting, value);
    }

    public ExportSetup Setup { get; }

    private bool myIsExporting;
    private readonly List<ExporterKey> myKeyList;

    private readonly IBinder<ExportSetup> filePathBinder = new AutoUpdateAndEventPropertyBinder<ExportSetup>(TextBox.TextProperty, nameof(ExportSetup.FilePathChanged), (b) => b.Control.SetValue(TextBox.TextProperty, b.Model.FilePath), (b) => b.Model.FilePath = b.Control.GetValue(TextBox.TextProperty));
    private bool isProcessingFrameSpanControls;
    private CancellationTokenSource? exportToken;

    public ExportDialog(ExportSetup setup) {
        Validate.NotNull(setup);

        this.Setup = setup;
        this.InitializeComponent();

        this.myKeyList = ExporterRegistry.Instance.Keys.ToList();
        foreach (ExporterKey key in this.myKeyList) {
            // Reset to prepare for new info... or maybe just leave it???
            ExporterRegistry.Instance.Exporters[key].Reset();

            this.PART_ComboBox.Items.Add(new ComboBoxItem() {
                Content = key.DisplayName
            });
        }

        this.PART_ComboBox.SelectionChanged += this.OnComboBoxSelectionChanged;
        this.PART_BeginFrameDragger.ValueChanged += this.BeginFrameDraggerOnValueChanged;
        this.PART_EndFrameDragger.ValueChanged += this.EndFrameDraggerOnValueChanged;
        this.Setup.SpanChanged += this.OnSetupSpanChanged;
        this.Setup.ExporterChanged += this.SetupOnExporterChanged;
        this.OnIsExportingChanged(false);
    }

    static ExportDialog() {
        IsExportingProperty.Changed.AddClassHandler<ExportDialog, bool>((d, e) => d.OnIsExportingChanged(e.NewValue.GetValueOrDefault()));
    }

    private void OnIsExportingChanged(bool isExporting) {
        this.PART_ExportButton.IsEnabled = !isExporting;
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        this.ThePropertyEditor.ApplyTemplate();
        this.filePathBinder.Attach(this.PART_FilePathTextBox, this.Setup);
        this.UpdateBeginFrameDragger();
        this.UpdateEndFrameDragger();

        // Select the first exporter for convenience
        this.PART_ComboBox.SelectedIndex = 0;
    }

    // TODO: combobox can change exporter, but changing exporter externally will not change combo box. Too lazy to implement :-)

    private void SetupOnExporterChanged(ExportSetup sender, BaseExporterInfo? oldExporter, BaseExporterInfo? newExporter) {
        this.ThePropertyEditor.PropertyEditor = newExporter?.PropertyEditor;
    }

    private void OnComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        this.Setup.Exporter = ExporterRegistry.Instance.Exporters[this.myKeyList[this.PART_ComboBox.SelectedIndex]];
    }

    private async void PART_ExportButton_OnClick(object? sender, RoutedEventArgs e) {
        if (this.IsExporting)
            return;

        ExportSetup setup = this.Setup;
        BaseExporterInfo? exporter = setup.Exporter;
        if (exporter == null) {
            return;
        }

        BaseExportContext context = exporter.CreateContext(setup);

        this.IsExporting = true;

        this.exportToken = new CancellationTokenSource();

        ExportProgressDialog progressDialog = new ExportProgressDialog(setup.Span, this.exportToken);
        progressDialog.Topmost = true;
        progressDialog.Show();

        setup.Editor.IsExporting = true;
        ActivityTask exportTask = TaskManager.Instance.RunTask(() => {
            progressDialog.ActivityTask = TaskManager.Instance.CurrentTask;
            progressDialog.ActivityTask.Progress.Caption = "Export Task";
            progressDialog.ActivityTask.Progress.Text = "Exporting...";

            // Export will most likely be using unsafe code, meaning async won't work
            context.Export(progressDialog, this.exportToken.Token);
            return Task.CompletedTask;
        }, TaskCreationOptions.LongRunning);

        await exportTask;
        setup.Editor.IsExporting = false;

        if (exportTask.Exception != null) {
            await IMessageDialogService.Instance.ShowMessage("Export failure", "An exception occurred while exporting video", exportTask.Exception.ToString());
        }

        if (exportTask.IsCancelled && File.Exists(setup.FilePath)) {
            try {
                File.Delete(setup.FilePath);
            }
            catch (Exception ex) {
                // AppLogger.Instance.WriteLine("Failed to delete cancelled export's file: " + ex.GetToString());
            }
        }

        this.IsExporting = false;
        setup.Timeline.UpdateAutomation(setup.Timeline.PlayHeadPosition, true);
        progressDialog.Close();
        this.Close();
    }

    private void PART_CancelButton_OnClick(object? sender, RoutedEventArgs e) {
        this.exportToken?.Cancel();
        this.Close();
    }

    protected override void OnClosing(WindowClosingEventArgs e) {
        if (this.IsExporting) {
            e.Cancel = true;
        }

        base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e) {
        // We need to deselect the exporter so that it detaches events
        this.Setup.SpanChanged -= this.OnSetupSpanChanged;
        this.Setup.ExporterChanged -= this.SetupOnExporterChanged;
        this.PART_ComboBox.SelectionChanged -= this.OnComboBoxSelectionChanged;
        this.PART_BeginFrameDragger.ValueChanged -= this.BeginFrameDraggerOnValueChanged;
        this.PART_EndFrameDragger.ValueChanged -= this.EndFrameDraggerOnValueChanged;
        this.Setup.Exporter = null;
        this.myKeyList.Clear();
        this.PART_ComboBox.Items.Clear();

        base.OnClosed(e);
    }

    #region Span Editors

    private void BeginFrameDraggerOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e) {
        if (this.isProcessingFrameSpanControls)
            return;

        this.isProcessingFrameSpanControls = true;
        this.Setup.Span = this.Setup.Span.MoveBegin((long) e.NewValue);
        this.isProcessingFrameSpanControls = false;
    }

    private void EndFrameDraggerOnValueChanged(object? sender, RangeBaseValueChangedEventArgs e) {
        if (this.isProcessingFrameSpanControls)
            return;

        this.isProcessingFrameSpanControls = true;
        this.Setup.Span = this.Setup.Span.WithEndIndex((long) e.NewValue);
        this.isProcessingFrameSpanControls = false;
    }

    private void UpdateBeginFrameDragger() {
        ExportSetup setup = this.Setup;
        this.PART_EndFrameDragger.Maximum = setup.Timeline.MaxDuration;
        this.PART_EndFrameDragger.Minimum = setup.Span.Begin;
        this.PART_EndFrameDragger.Value = setup.Span.EndIndex;
        this.PART_DurationTextBlock.Text = setup.Span.Duration.ToString();
    }

    private void UpdateEndFrameDragger() {
        ExportSetup setup = this.Setup;
        this.PART_BeginFrameDragger.Maximum = setup.Span.EndIndex;
        this.PART_BeginFrameDragger.Minimum = 0;
        this.PART_BeginFrameDragger.Value = setup.Span.Begin;
        this.PART_DurationTextBlock.Text = setup.Span.Duration.ToString();
    }

    private void OnSetupSpanChanged(ExportSetup sender, FrameSpan oldspan, FrameSpan newspan) {
        this.UpdateBeginFrameDragger();
        this.UpdateEndFrameDragger();
    }

    #endregion
}