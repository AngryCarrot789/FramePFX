using System;
using System.Threading.Tasks;
using FramePFX.Commands;
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

        public override Task ExecuteAsync(CommandEventArgs e) {
            if (!e.DataContext.TryGetContext(DataKeys.ProjectKey, out Project project)) {
                return Task.CompletedTask;
            }

            SaveProject(project);
            return Task.CompletedTask;
        }
    }

    public class SaveProjectAsCommand : Command {
        public override Task ExecuteAsync(CommandEventArgs e) {
            if (!e.DataContext.TryGetContext(DataKeys.ProjectKey, out Project project)) {
                return Task.CompletedTask;
            }

            SaveProjectCommand.SaveProjectAs(project);
            return Task.CompletedTask;
        }
    }
}