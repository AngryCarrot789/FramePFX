using System;
using System.IO;
using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Editors.Exporting;
using FramePFX.Editors.Exporting.Controls;
using FramePFX.Editors.Timelines;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Actions {
    public class ExportCommand : Command {
        public bool ExportContextualTimeline { get; }

        public ExportCommand(bool exportContextualTimeline) {
            this.ExportContextualTimeline = exportContextualTimeline;
        }

        public override bool CanExecute(CommandEventArgs e) {
            if (this.ExportContextualTimeline) {
                return TryGetTimeline(e.DataContext, out Timeline timeline) && timeline.Project != null && !timeline.Project.IsExporting;
            }
            else {
                return TryGetProject(e.DataContext, out Project project) && !project.IsExporting;
            }
        }

        public override Task ExecuteAsync(CommandEventArgs e) {
            Project project;
            Timeline timeline;
            if (this.ExportContextualTimeline) {
                if (!TryGetTimeline(e.DataContext, out timeline)) {
                    return Task.CompletedTask;
                }
            }
            else {
                if (!TryGetProject(e.DataContext, out project)) {
                    return Task.CompletedTask;
                }

                timeline = project.MainTimeline;
            }

            if ((project = timeline.Project) == null) {
                return Task.CompletedTask;
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
            return Task.CompletedTask;
        }

        public static bool TryGetProject(IDataContext ctx, out Project project) {
            if (ctx.TryGetContext(DataKeys.ProjectKey, out project))
                return true;
            return ctx.TryGetContext(DataKeys.VideoEditorKey, out VideoEditor editor) && (project = editor.Project) != null;
        }

        public static bool TryGetTimeline(IDataContext ctx, out Timeline timeline) {
            if (ctx.TryGetContext(DataKeys.TimelineKey, out timeline))
                return true;
            if (ctx.TryGetContext(DataKeys.ProjectKey, out Project project)) {
                timeline = project.ActiveTimeline;
                return true;
            }

            if (ctx.TryGetContext(DataKeys.VideoEditorKey, out VideoEditor editor) && (project = editor.Project) != null) {
                timeline = project.ActiveTimeline;
                return true;
            }

            return false;
        }
    }
}