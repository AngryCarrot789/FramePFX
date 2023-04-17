using System.Security.RightsManagement;
using System.Threading.Tasks;

namespace FramePFX.Project.EditorDialogs {
    public class ProjectSettingsDialogService {
        public static ProjectSettingsDialogService Instance { get; } = new ProjectSettingsDialogService();

        public Task<ProjectSettingsViewModel> EditSettings(EditorProject project) {
            ProjectSettingsEditorWindow window = new ProjectSettingsEditorWindow();
            ProjectSettingsEditorViewModel vm = (ProjectSettingsEditorViewModel) window.DataContext;
            vm.Settings.Width = project.Resolution.Width;
            vm.Settings.Height = project.Resolution.Height;
            vm.Settings.FrameRate = project.FrameRate;
            return Task.FromResult(window.ShowDialog() == true && vm.Settings != null ? vm.Settings : null);
        }
    }
}