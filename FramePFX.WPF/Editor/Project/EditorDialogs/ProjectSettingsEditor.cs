using System.Threading.Tasks;
using FramePFX.Editor;

namespace FramePFX.WPF.Editor.Project.EditorDialogs
{
    [ServiceImplementation(typeof(IProjectSettingsEditor))]
    public class ProjectSettingsEditor : IProjectSettingsEditor
    {
        public Task<ProjectSettings> EditSettingsAsync(ProjectSettings settings)
        {
            ProjectSettingsEditorWindow window = new ProjectSettingsEditorWindow();
            ((ProjectSettingsEditorViewModel) window.DataContext).SetSettings(settings);
            if (window.ShowDialog() == true)
            {
                return Task.FromResult(((ProjectSettingsEditorViewModel) window.DataContext).ToSettings());
            }

            return Task.FromResult<ProjectSettings>(null);
        }
    }
}