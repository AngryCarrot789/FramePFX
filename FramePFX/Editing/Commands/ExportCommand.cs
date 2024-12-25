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

using FramePFX.CommandSystem;
using FramePFX.Editing.Exporting;
using FramePFX.Editing.Timelines;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editing.Commands;

public class ExportCommand : AsyncCommand {
    protected override Executability CanExecuteOverride(CommandEventArgs e) {
        VideoEditor? theEditor;
        if (DataKeys.TimelineKey.TryGetContext(e.ContextData, out Timeline? theTimeline)) {
            theEditor = theTimeline.Project?.Editor;
        }
        else if (DataKeys.VideoEditorKey.TryGetContext(e.ContextData, out theEditor)) {
            theTimeline = theEditor.Project?.ActiveTimeline;
        }
        else {
            return Executability.Invalid;
        }

        if (theEditor == null || theTimeline == null || theEditor.Project == null) {
            return Executability.ValidButCannotExecute;
        }

        return Executability.Valid;
    }

    protected override Task ExecuteAsync(CommandEventArgs e) {
        VideoEditor? theEditor;
        if (DataKeys.TimelineKey.TryGetContext(e.ContextData, out Timeline? theTimeline)) {
            theEditor = theTimeline.Project?.Editor;
        }
        else if (DataKeys.VideoEditorKey.TryGetContext(e.ContextData, out theEditor)) {
            theTimeline = theEditor.Project?.ActiveTimeline;
        }
        else {
            return Task.CompletedTask;
        }

        if (theEditor == null || theTimeline == null || theEditor.Project == null) {
            return Task.CompletedTask;
        }

        if (theEditor.IsExporting) {
            return Task.CompletedTask;
        }

        theEditor.Playback.Pause();
        IExportDialogService dialogService = Application.Instance.ServiceManager.GetService<IExportDialogService>();

        ExportSetup setup = new ExportSetup(theEditor, theTimeline) {
            Span = new FrameSpan(0, theTimeline.LargestFrameInUse),
            FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "video.mp4")
        };
        return dialogService.ShowExportDialog(setup);
    }
}