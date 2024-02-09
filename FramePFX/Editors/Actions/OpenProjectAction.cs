using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editors.Automation;
using FramePFX.Editors.ResourceManaging.Autoloading.Controls;
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

            if (!ResourceLoaderDialog.TryLoadResources(project.ResourceManager.RootContainer)) {
                try {
                    editor.CloseProject();
                }
                catch (Exception e) {
                    IoC.MessageService.ShowMessage("Close Error", "An exception occurred while closing the project", e.GetToString());
                }

                return false;
            }

            project.SetUnModified();
            AutomationEngine.UpdateValues(project.MainTimeline);
            project.MainTimeline.RenderManager.InvalidateRender();
            Debug.Assert(project.IsModified == false, "Expected automation update and render invalidation to not mark project as modified");

            return true;
        }
    }
}