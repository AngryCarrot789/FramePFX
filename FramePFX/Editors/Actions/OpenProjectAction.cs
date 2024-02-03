using System.IO;
using System.Threading.Tasks;
using System.Windows;
using FramePFX.Actions;
using FramePFX.Editors.Automation;
using FramePFX.Interactivity.DataContexts;
using FramePFX.Utils;
using FramePFX.Views;

namespace FramePFX.Editors.Actions {
    public class OpenProjectAction : AnAction {
        public override bool CanExecute(AnActionEventArgs e) {
            return e.DataContext.ContainsKey(DataKeys.VideoEditorKey);
        }

        public override Task ExecuteAsync(AnActionEventArgs e) {
            if (!e.DataContext.TryGetContext(DataKeys.VideoEditorKey, out VideoEditor editor)) {
                return Task.CompletedTask;
            }

            if (!NewProjectAction.CloseProject(editor)) {
                return Task.CompletedTask;
            }

            string filePath = IoC.FilePickService.OpenFile("Open a project file (.fpfx)", Filters.ProjectType);
            if (filePath == null) {
                return Task.CompletedTask;
            }

            if (!File.Exists(filePath)) {
                IoC.MessageService.ShowMessage("No such file", "That project file does not exist");
            }

            Project project = new Project();
            using (project.RenderManager.SuspendRenderInvalidation()) {
                try {
                    project.ReadFromFile(filePath);
                }
                catch (IOException ex) {
                    IoC.MessageService.ShowMessage("Read Error", "An exception occurred while reading the project", ex.GetToString());
                    try {
                        project.Destroy();
                    }
                    catch { /* ignored */
                    }

                    return Task.CompletedTask;
                }

                editor.SetProject(project);
            }

            AutomationEngine.UpdateValues(project.MainTimeline);
            project.RenderManager.InvalidateRender();

            return Task.CompletedTask;
        }
    }
}