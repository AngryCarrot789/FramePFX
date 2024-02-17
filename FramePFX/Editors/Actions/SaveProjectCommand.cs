using System;
using FramePFX.CommandSystem;
using FramePFX.Interactivity.DataContexts;
using FramePFX.Utils;

namespace FramePFX.Editors.Actions {
    public class SaveProjectCommand : Command {
        public static bool? SaveProject(Project project) {
            if (project.HasSavedOnce && !string.IsNullOrEmpty(project.ProjectFilePath)) {
                return SaveProjectInternal(project, project.ProjectFilePath);
            }
            else {
                return SaveProjectAs(project);
            }
        }

        public static bool? SaveProjectAs(Project project) {
            const string message = "Specify a file path for the project file. Any project data will be stored in the same folder, so it's best to create a project-specific folder";
            string filePath = IoC.FilePickService.SaveFile(message, Filters.ProjectType, project.ProjectFilePath);
            if (filePath == null) {
                return null;
            }

            return SaveProjectInternal(project, filePath);
        }

        private static bool SaveProjectInternal(Project project, string filePath) {
            project.Editor?.Playback.Pause();

            try {
                project.WriteToFile(filePath);
                return true;
            }
            catch (Exception e) {
                IoC.MessageService.ShowMessage("Save Error", "An exception occurred while saving project", e.GetToString());
                return false;
            }
        }

        public override void Execute(CommandEventArgs e) {
            if (!DataKeys.ProjectKey.TryGetContext(e.DataContext, out Project project)) {
                return;
            }

            SaveProject(project);
        }
    }

    public class SaveProjectAsCommand : Command {
        public override void Execute(CommandEventArgs e) {
            if (!DataKeys.ProjectKey.TryGetContext(e.DataContext, out Project project)) {
                return;
            }

            SaveProjectCommand.SaveProjectAs(project);
        }
    }
}