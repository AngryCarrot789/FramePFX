using System;
using System.IO;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editors.Automation;
using FramePFX.Interactivity.DataContexts;
using FramePFX.Utils;

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

            OpenProjectAt(editor, filePath);
            return Task.CompletedTask;
        }

        public static bool OpenProjectAt(VideoEditor editor, string filePath) {
            Project project;
            try {
                project = Project.ReadProjectAt(filePath);
            }
            catch (Exception ex) {
                IoC.MessageService.ShowMessage("Read Error", "An exception occurred while reading the project", ex.GetToString());
                return false;
            }

            editor.SetProject(project);

            AutomationEngine.UpdateValues(project.MainTimeline);
            project.RenderManager.InvalidateRender();
            return true;
        }
    }
}