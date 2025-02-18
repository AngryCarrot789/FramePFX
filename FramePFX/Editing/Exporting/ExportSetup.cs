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

using FramePFX.Editing.Timelines;
using PFXToolKitUI.Utils;

namespace FramePFX.Editing.Exporting;

public delegate void ExportSetupSpanChangedEventHandler(ExportSetup sender, FrameSpan oldSpan, FrameSpan newSpan);

public delegate void ExportSetupFilePathChangedEventHandler(ExportSetup sender, string? oldFilePath, string? newFilePath);

public delegate void ExportSetupExporterChangedEventHandler(ExportSetup sender, BaseExporterInfo? oldExporter, BaseExporterInfo? newExporter);

/// <summary>
/// Information about preparing to export. This is created when the user opens the export dialog
/// </summary>
public sealed class ExportSetup {
    private FrameSpan span;
    private string? filePath;
    private BaseExporterInfo? exporter;

    /// <summary>
    /// Gets the video editor that owns the timeline being exported
    /// </summary>
    public VideoEditor Editor { get; }

    /// <summary>
    /// Gets the project that owns the timeline being exported
    /// </summary>
    public Project Project { get; }

    /// <summary>
    /// Gets the timeline being exported
    /// </summary>
    public Timeline Timeline { get; }

    /// <summary>
    /// Gets or sets the region of the timeline that is to be exported
    /// </summary>
    public FrameSpan Span {
        get => this.span;
        set {
            FrameSpan oldSpan = this.span;
            if (oldSpan == value)
                return;

            this.span = value;
            this.SpanChanged?.Invoke(this, oldSpan, value);
        }
    }

    /// <summary>
    /// Gets or sets the destination file path for the export. May be a folder if the current exporter requires that
    /// </summary>
    public string? FilePath {
        get => this.filePath;
        set {
            string? oldFilePath = this.filePath;
            if (oldFilePath == value)
                return;

            this.filePath = value;
            this.FilePathChanged?.Invoke(this, oldFilePath, value);
        }
    }

    /// <summary>
    /// Gets or sets the exporter registration that will be used for actually exporting
    /// </summary>
    public BaseExporterInfo? Exporter {
        get => this.exporter;
        set {
            BaseExporterInfo? oldExporter = this.exporter;
            if (oldExporter == value)
                return;

            oldExporter?.Deselect();
            value?.OnSelected(this);
            this.exporter = value;
            this.ExporterChanged?.Invoke(this, oldExporter, value);
        }
    }

    public event ExportSetupSpanChangedEventHandler? SpanChanged;
    public event ExportSetupFilePathChangedEventHandler? FilePathChanged;
    public event ExportSetupExporterChangedEventHandler? ExporterChanged;

    public ExportSetup(VideoEditor editor, Timeline timeline) {
        Validate.NotNull(editor);
        Validate.NotNull(timeline);
        if (editor.Project == null)
            throw new InvalidOperationException("Editor has no projet");
        if (timeline.Project?.Editor != editor)
            throw new InvalidOperationException("Timeline's owner editor is not the editor provided");

        this.Project = editor.Project!;
        this.Editor = editor;
        this.Timeline = timeline;
    }
}