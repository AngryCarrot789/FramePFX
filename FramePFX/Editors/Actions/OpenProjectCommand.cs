using System;
using System.Diagnostics;
using System.IO;
using FramePFX.CommandSystem;
using FramePFX.Editors.Automation;
using FramePFX.Editors.ResourceManaging.Autoloading.Controls;
using FramePFX.Interactivity.DataContexts;
using FramePFX.Utils;

namespace FramePFX.Editors.Actions {
    public class OpenProjectCommand : Command {
        public override bool CanExecute(CommandEventArgs e) {
            return e.DataContext.ContainsKey(DataKeys.VideoEditorKey);
        }

        public override void Execute(CommandEventArgs e) {
            if (!DataKeys.VideoEditorKey.TryGetContext(e.DataContext, out VideoEditor editor)) {
                return;
            }

            if (!NewProjectCommand.CloseProject(editor)) {
                return;
            }

            string filePath = IoC.FilePickService.OpenFile("Open a project file (.fpfx)", Filters.ProjectType);
            if (filePath == null) {
                return;
            }

            if (!File.Exists(filePath)) {
                IoC.MessageService.ShowMessage("No such file", "That project file does not exist");
            }

            OpenProjectAt(editor, filePath);
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