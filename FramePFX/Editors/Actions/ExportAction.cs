using System;
using System.IO;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editors.Exporting;
using FramePFX.Editors.Exporting.Controls;
using FramePFX.Editors.Timelines;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Actions {
    public class ExportAction : AnAction {
        public override bool CanExecute(AnActionEventArgs e) {
            return TryGetProject(e.DataContext, out Project project) && !project.IsExporting;
        }

        public override Task ExecuteAsync(AnActionEventArgs e) {
            if (TryGetProject(e.DataContext, out Project project)) {
                project.Editor.Playback.Pause();
                ExportDialog dialog = new ExportDialog {
                    ExportSetup = new ExportSetup(project) {
                        Properties = {
                            Span = new FrameSpan(0, project.MainTimeline.LargestFrameInUse),
                            FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "OutputVideo.mp4")
                        }
                    }
                };

                dialog.ShowDialog();
            }

            return Task.CompletedTask;
        }

        public static bool TryGetProject(IDataContext ctx, out Project project) {
            if (ctx.TryGetContext(DataKeys.ProjectKey, out project))
                return true;
            return ctx.TryGetContext(DataKeys.VideoEditorKey, out VideoEditor editor) && (project = editor.Project) != null;
        }
    }
}