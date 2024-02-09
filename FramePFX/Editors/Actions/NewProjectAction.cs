using System.Threading.Tasks;
using System.Windows;
using FramePFX.Actions;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.DataContexts;
using FramePFX.Views;

namespace FramePFX.Editors.Actions {
    public class NewProjectAction : AnAction {
        // true: project was already closed or is now closed
        // false: close was cancelled; cancel entire operation
        public static bool CloseProject(VideoEditor editor) {
            Project oldProject = editor.Project;
            if (oldProject == null) {
                return true;
            }

            MessageBoxResult result = IoC.MessageService.ShowMessage("Project already open", "A project is already open. Do you want to save it?", MessageBoxButton.YesNoCancel);
            switch (result) {
                case MessageBoxResult.Cancel: return false;
                case MessageBoxResult.Yes: {
                    bool? saveResult = SaveProjectAction.SaveProject(editor.Project);
                    if (!saveResult.HasValue) {
                        return false;
                    }

                    editor.CloseProject();
                    break;
                }
                default: {
                    editor.CloseProject();
                    break;
                }
            }

            return true;
        }

        public override Task ExecuteAsync(AnActionEventArgs e) {
            if (!e.DataContext.TryGetContext(DataKeys.VideoEditorKey, out VideoEditor editor)) {
                return Task.CompletedTask;
            }

            if (!CloseProject(editor)) {
                return Task.CompletedTask;
            }

            Project project = new Project();
            VideoTrack track = new VideoTrack() {
                DisplayName = "Video Track 1"
            };

            project.MainTimeline.AddTrack(track);
            editor.SetProject(project);
            return Task.CompletedTask;
        }
    }
}