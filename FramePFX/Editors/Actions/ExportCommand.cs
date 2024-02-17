using System;
using System.IO;
using FramePFX.CommandSystem;
using FramePFX.Editors.Exporting;
using FramePFX.Editors.Exporting.Controls;
using FramePFX.Editors.Timelines;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Actions {
    public class ExportCommand : Command {
        public bool ExportActiveTimeline { get; }

        public ExportCommand(bool exportActiveTimeline) {
            this.ExportActiveTimeline = exportActiveTimeline;
        }

        public override bool CanExecute(CommandEventArgs e) {
            if (this.ExportActiveTimeline) {
                return TryGetTimeline(e.DataContext, out Timeline timeline) && timeline.Project != null && !timeline.Project.IsExporting;
            }
            else {
                return TryGetProject(e.DataContext, out Project project) && !project.IsExporting;
            }
        }

        public override void Execute(CommandEventArgs e) {
            Project project;
            Timeline timeline;
            if (this.ExportActiveTimeline) {
                if (!TryGetTimeline(e.DataContext, out timeline)) {
                    return;
                }
            }
            else {
                if (!TryGetProject(e.DataContext, out project)) {
                    return;
                }

                timeline = project.MainTimeline;
            }

            if ((project = timeline.Project) == null) {
                return;
            }

            project.Editor.Playback.Pause();
            ExportDialog dialog = new ExportDialog {
                ExportSetup = new ExportSetup(project, timeline) {
                    Properties = {
                        Span = new FrameSpan(0, timeline.LargestFrameInUse),
                        FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "OutputVideo.mp4")
                    }
                }
            };

            dialog.ShowDialog();
        }

        public static bool TryGetProject(IDataContext ctx, out Project project) {
            if (DataKeys.ProjectKey.TryGetContext(ctx, out project))
                return true;
            return DataKeys.VideoEditorKey.TryGetContext(ctx, out VideoEditor editor) && (project = editor.Project) != null;
        }

        public static bool TryGetTimeline(IDataContext ctx, out Timeline timeline) {
            if (DataKeys.TimelineKey.TryGetContext(ctx, out timeline))
                return true;
            if (DataKeys.ProjectKey.TryGetContext(ctx, out Project project)) {
                timeline = project.ActiveTimeline;
                return true;
            }

            if (DataKeys.VideoEditorKey.TryGetContext(ctx, out VideoEditor editor) && (project = editor.Project) != null) {
                timeline = project.ActiveTimeline;
                return true;
            }

            return false;
        }
    }
}