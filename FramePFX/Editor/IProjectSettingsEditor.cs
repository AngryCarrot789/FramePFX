using System.Threading.Tasks;

namespace FramePFX.Editor
{
    public interface IProjectSettingsEditor
    {
        Task<ProjectSettings> EditSettingsAsync(ProjectSettings settings);
    }
}