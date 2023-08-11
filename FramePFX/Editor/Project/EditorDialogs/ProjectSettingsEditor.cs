using System.Threading.Tasks;
using FramePFX.Core;
using FramePFX.Core.Editor;

namespace FramePFX.Editor.Project.EditorDialogs {
    [ServiceImplementation(typeof(IProjectSettingsEditor))]
    public class ProjectSettingsEditor : IProjectSettingsEditor {
        public async Task<ProjectSettings> EditSettingsAsync(ProjectSettings settings) {
            ProjectSettingsEditorWindow window = new ProjectSettingsEditorWindow();
            ((ProjectSettingsEditorViewModel) window.DataContext).SetSettings(settings);
            if (window.ShowDialog() == true) {
                return ((ProjectSettingsEditorViewModel) window.DataContext).ToSettings();
            }

            return null;
        }
    }
}